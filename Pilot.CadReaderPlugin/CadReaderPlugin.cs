using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ascon.Pilot.SDK.CadReader.Analyzer;
using Ascon.Pilot.SDK.CadReader.Form;
using Ascon.Pilot.SDK.CadReader.Model;
using Ascon.Pilot.SDK.CadReader.Spc;
using Ascon.Pilot.SDK.Menu;

// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.CadReader
{
    [Export(typeof(IMenu<MainViewContext>))]
    [Export(typeof(IMenu<ObjectsViewContext>))]
    public class CadReaderPlugin : IMenu<MainViewContext>, IMenu<ObjectsViewContext>
    {
        private readonly IObjectModifier _objectModifier;
        private readonly IObjectsRepository _objectsRepository;
        private List<Specification> _listSpec;
        private const string ADD_INFORMATION_TO_PILOT = "ADD_INFORMATION_TO_PILOT";
        private const string ADD_DOC_TO_PILOT = "ADD_DOC_TO_PILOT";
        private const string ABOUT_PROGRAM_MENU = "ABOUT_PROGRAM_MENU";
        private const string SOURCE_DOC_EXT = ".cdw";
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected;

        [ImportingConstructor]
        public CadReaderPlugin(IObjectModifier modifier, IObjectsRepository repository, IPersonalSettings personalSettings)
        {
            _objectModifier = modifier;
            _objectsRepository = repository;
            _listSpec = new List<Specification>();
        }

        /// <summary>
        /// Очистка строки полученной из спецификации от служибных символов: $| и @/
        /// </summary>
        /// <param name="str">Строка которую нужно очистить</param>
        /// <returns>Очищенная строка</returns>
        private static string ValueTextClear(string str)
        {
            return str?.Replace("$|", "").Replace(" @/", " ").Replace("@/", " ");
        }

        /// <summary>
        /// "Умный" поиск секции в спецификации, возвращает True если передана название секции спецификации по ГОСТ
        /// </summary>
        /// <param name="sectionName">Наименование секции в спецификации, например: Сборочные единицы</param>
        /// <param name="pattern">Шаблон для поиска секции</param>
        /// <returns>True or false</returns>
        private static bool ParsingSectionName(string sectionName, string pattern)
        {
            sectionName = sectionName.ToLower();
            return sectionName.Contains(pattern);
        }

        private static bool IsFileExtension(string name, string ext)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var theExt = Path.GetExtension(name).ToLower();
            return theExt == ext;
        }

        private void SetDocOnMenuClick()
        {
            var loader = new ObjectLoader(_objectsRepository);
            if (_listSpec == null) return;
            // прикрепляем xps'ки
            foreach (var spc in _listSpec)
            {
                if (spc.GlobalId == Guid.Empty) continue;
                loader.Load(spc.GlobalId, obj =>
                {
                    var builder = _objectModifier.Edit(obj);
                    builder.AddFile(spc.PreviewDocument);
                    _objectModifier.Apply();
                });
            }
        }

        private void SetInformationOnMenuClick(IDataObject selected)
        {
            _listSpec.Clear();
            var loader = new ObjectLoader(_objectsRepository);
            var storage = new StorageAnalayzer(_objectsRepository);
            var path = storage.GetProjectFolderByPilotStorage(selected);
            if (path == null) return;
            var filesSpw = storage.GetFilesSpw(path);
            foreach (var fileSpw in filesSpw)
            {
                var spc = GetInformationFromKompas(fileSpw);
                if (spc == null)
                {
                    MessageBox.Show("Информация не может быть получена. Cкорее всего спецификация сделана в версии ниже КОМПАС-3D V16");
                    continue;
                }
                _listSpec.Add(spc);
            }

            if (_listSpec.Count > 1)
            {
                _listSpec = SpecificationAnalyzer.CreateTree(_listSpec);
            }

            // для объектов спецификациий формируем pdf, для спецификаций формируем xps
            // формируем вторичное представление для спецификаций
            var kompasConverterTask = new Task<KompasConverter>(() =>
            {
                using (var k = new KompasConverter())
                {
                    k.KompasConvertToXps(_listSpec);
                    foreach (var spc in _listSpec)
                    {
                        k.KompasConvertToPdf(spc.ListSpcObjects);
                    }
                    return k;
                }
            });
            kompasConverterTask.Start();
            kompasConverterTask.Wait();

            loader.Load(selected.Id, projectId =>
            {
                if (projectId == null) return;
                // создаём только корневые объекты
                // затем создаём только спецификации
                // а потом к ним прилинковываем остальные объекты линейно
                CreateAndUpdateSpcToPilot(projectId, _listSpec.Where(i => i.Parent == null));

                //SynchronizeCheck(projectId, _listSpec.Where(i => i.Parent == null), b =>
                //{
                //    //CreateAndUpdateSpcToPilot(projectId, _listSpec.Where(i => i.Parent == null));
                //    //foreach (var l in _listSpec)
                //    //{
                //    //    foreach (var ob in l.ListSpcObjects)
                //    //    {
                //    //        _loader.Load(l.GlobalId, o =>
                //    //        {
                //    //            CreateNewSpcObjToPilot(o, ob);
                //    //        });
                //    //    }
                //    //}
                //});
            });
        }

        /// <summary>
        /// Метод получает информацию из спецификации и возвращает экземпляр класса Specification
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <returns>Объект спецификации</returns>
        private static Specification GetInformationFromKompas(string filename)
        {
            using (var inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var ms = new MemoryStream();
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(ms);
                ms.Position = 0;
                var taskOpenSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(ms));
                taskOpenSpwFile.Start();
                taskOpenSpwFile.Wait();
                if (!taskOpenSpwFile.Result.IsCompleted)
                    return null;
                var spc = taskOpenSpwFile.Result.GetSpecification;
                spc.FileName = filename;
                return spc;
            }
        }


        private void SynchronizeCheck(IDataObject parent, IEnumerable<IGeneralDocEntity> listDoc, Action<bool> onLoadedAction)
        {
            var children = parent.TypesByChildren;
            var loader = new LoaderOfObjects(_objectsRepository);
            loader.Load(children.Keys, objects =>
            {
                var generalDocEntities = listDoc as IGeneralDocEntity[] ?? listDoc.ToArray();
                foreach (var obj in objects)
                {
                    var attrMarkValue = string.Empty;
                    foreach (var a in obj.Attributes)
                    {
                        if (a.Key == "mark")
                            attrMarkValue = a.Value.ToString();
                    }
                    foreach (var doc in generalDocEntities)
                    {
                        var colunmValue = ValueTextClear(doc.GetDesignation());
                        doc.SetGlobalId(colunmValue == attrMarkValue ? obj.Id : Guid.Empty);
                    }
                }
                onLoadedAction(true);
            });
        }

        private void UpdateSpcByPilot(Specification spc)
        {
            var loader = new ObjectLoader(_objectsRepository);
            loader.Load(spc.GlobalId, obj =>
            {
                var needToChange = false;

                var builder = _objectModifier.Edit(obj);

                var spcName = ValueTextClear(spc.Name);
                var spcDesignation = ValueTextClear(spc.Designation);
                // проверка нужно ли изменять объект
                foreach (var attrObj in obj.Attributes)
                {
                    if (attrObj.Key == "name")
                    {
                        if (attrObj.Value.ToString() != spcName)
                        {
                            builder.SetAttribute(attrObj.Key, spcName);
                            needToChange = true;
                        }
                    }
                    if (attrObj.Key == "mark")
                    {
                        if (attrObj.Value.ToString() != spcDesignation)
                        {
                            builder.SetAttribute(attrObj.Key, spcDesignation);
                            needToChange = true;
                        }
                    }
                }

                // получаем xps файл из Обозревателя
                var fileFromPilot = obj.Files.FirstOrDefault(f => IsFileExtension(f.Name, ".xps"));
                var xpsFile = spc.PreviewDocument;
                if (xpsFile != null && fileFromPilot != null)
                {
                    // md5 в нижнем регистре расчитывается и возвращается пилотом
                    var fileNameMd5 = CalculatorMd5Checksum.Go(xpsFile);
                    if (!string.IsNullOrEmpty(fileNameMd5) && fileFromPilot.Md5 != fileNameMd5)
                    {
                        builder.CreateFileSnapshot("").AddFile(xpsFile);
                        needToChange = true;
                    }
                }
                if (needToChange) _objectModifier.Apply();
            });
        }

        private void UpdateSpcObjByPilot(SpcObject spcObject)
        {
            var needToChange = false;
            var loader = new ObjectLoader(_objectsRepository);
            loader.Load(spcObject.GlobalId, obj =>
            {
                if (obj == null)
                    return;
                var builder = _objectModifier.Edit(obj);
                foreach (var spcColumn in spcObject.Columns)
                {
                    var spcColVal = ValueTextClear(spcColumn.Value);
                    // проверка нужно ли изменять объект
                    foreach (var attrObj in obj.Attributes)
                    {
                        if (attrObj.Key != spcColumn.TypeName) continue;
                        if (attrObj.Value.ToString() == spcColVal) continue;
                        needToChange = true;
                        if (int.TryParse(spcColVal, out var i))
                            builder.SetAttribute(spcColumn.TypeName, i);
                        else
                            builder.SetAttribute(spcColumn.TypeName, spcColVal);
                    }
                }
                // получаем pdf файл из Обозревателя
                var fileFromPilot = obj.Files.FirstOrDefault(f => IsFileExtension(f.Name, ".pdf"));
                var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SOURCE_DOC_EXT));
                if (doc != null && fileFromPilot != null)
                {
                    var pdfFile = spcObject.PreviewDocument;
                    // md5 в нижнем регистре расчитывается и возвращается пилотом
                    var fileNameMd5 = CalculatorMd5Checksum.Go(pdfFile);
                    if (!string.IsNullOrEmpty(fileNameMd5) && fileFromPilot.Md5 != fileNameMd5)
                    {
                        needToChange = true;
                        builder.CreateFileSnapshot("").AddFile(pdfFile);
                    }
                }
                if (needToChange) _objectModifier.Apply();
            });
        }

        private void CreateNewSpcToPilot(IDataObject parent, Specification spc)
        {
            var t = GetTypeSpecification();
            var builder = _objectModifier.Create(parent, t);
            spc.GlobalId = builder.DataObject.Id;

            builder.SetAttribute("name", spc.Name);
            builder.SetAttribute("mark", ValueTextClear(spc.Designation));

            //if (File.Exists(spc.PreviewDocument))
            //{
            //    builder.AddFile(spc.PreviewDocument);
            //}
            _objectModifier.Apply();

            Thread.Sleep(2000);

            var loader = new ObjectLoader(_objectsRepository);
            loader.Load(spc.GlobalId, obj =>
            {
                if (!File.Exists(spc.FileName)) return;
                string[] paths = { spc.FileName };
                var storageObjects = _objectsRepository.GetStorageObjects(paths);
                var storageObject = storageObjects.FirstOrDefault();

                if (storageObject == null) return;

                foreach (var relation in obj.Relations.Where(x => x.Type == ObjectRelationType.SourceFiles))
                {
                    _objectModifier.RemoveLink(obj, relation);
                }

                var relationId = Guid.NewGuid();
                var relationName = relationId.ToString();
                const ObjectRelationType relationType = ObjectRelationType.SourceFiles;
                var selectedRelation = new Relation
                {
                    Id = relationId,
                    Type = relationType,
                    Name = storageObject.DataObject.DisplayName,
                    TargetId = storageObject.DataObject.Id
                };
                var chosenRelation = new Relation
                {
                    Id = relationId,
                    Type = relationType,
                    Name = relationName,
                    TargetId = spc.GlobalId
                };
                _objectModifier.CreateLink(selectedRelation, chosenRelation);
                _objectModifier.Apply();

                foreach (var spcObject in spc.ListSpcObjects)
                {
                    if (spcObject.SectionNumber == 15) continue;
                    CreateNewSpcObjToPilot(obj, spcObject);
                }

                if (spc.Children == null) return;

                foreach (var cSpc in spc.Children)
                {
                    CreateNewSpcToPilot(obj, cSpc);
                }
            });
        }

        private void CreateNewSpcObjToPilot(IDataObject parent, SpcObject spcObject)
        {
            var t = GetTypeBySectionName(spcObject.SectionName);
            if (t == null) return;
            var builder = _objectModifier.Create(parent, t);
            spcObject.GlobalId = builder.DataObject.Id;
            foreach (var attr in spcObject.Columns)
            {
                var val = attr.Value;
                if (string.IsNullOrEmpty(attr.TypeName) || string.IsNullOrEmpty(val)) continue;
                // очишаем значение от служебных символов и выражений
                val = ValueTextClear(val);
                // в качестве наименование передаётся внутренее имя (а не то которое отображается)
                if (int.TryParse(val, out var i))
                    builder.SetAttribute(attr.TypeName, i);
                else
                    builder.SetAttribute(attr.TypeName, val);
            }
            var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SOURCE_DOC_EXT));

            if (doc == null) return;

            var fileName = doc.FileName;
            string[] paths = { fileName };
            var storageObjects = _objectsRepository.GetStorageObjects(paths);
            var storageObject = storageObjects.FirstOrDefault();

            if (File.Exists(spcObject.PreviewDocument))
            {
                builder.AddFile(spcObject.PreviewDocument);
            }
            _objectModifier.Apply();

            if (storageObject == null) return;

            if (storageObject.DataObject.Id == Guid.Empty) return;

            var loader = new ObjectLoader(_objectsRepository);
            loader.Load(spcObject.GlobalId, obj =>
            {
                foreach (var relation in obj.Relations.Where(x => x.Type == ObjectRelationType.SourceFiles))
                {
                    _objectModifier.RemoveLink(obj, relation);
                }
                var relationId = Guid.NewGuid();
                var relationName = relationId.ToString();
                const ObjectRelationType relationType = ObjectRelationType.SourceFiles;
                var selectedRelation = new Relation
                {
                    Id = relationId,
                    Type = relationType,
                    Name = storageObject.DataObject.DisplayName,
                    TargetId = storageObject.DataObject.Id
                };
                var chosenRelation = new Relation
                {
                    Id = relationId,
                    Type = relationType,
                    Name = relationName,
                    TargetId = builder.DataObject.Id
                };
                _objectModifier.CreateLink(selectedRelation, chosenRelation);
                _objectModifier.Apply();
            });
        }

        private void CreateAndUpdateSpcToPilot(IDataObject parent, IEnumerable<IGeneralDocEntity> listDoc)
        {
            foreach (var docEntity in listDoc)
            {
                if (!(docEntity is Specification spc)) continue;

                if (spc.GlobalId == Guid.Empty)
                {
                    CreateNewSpcToPilot(parent, spc);
                }
                else
                {
                    UpdateSpcByPilot(spc);
                }
            }
        }

        private IType GetTypeSpecification()
        {
            var pilotTypes = _objectsRepository.GetTypes();
            return pilotTypes.FirstOrDefault(itype => itype.Name == "specification");
        }


        private IType GetTypeBySectionName(string sectionName)
        {
            // ReSharper disable once RedundantAssignment
            var name = string.Empty;
            var pilotTypes = _objectsRepository.GetTypes();
            foreach (var itype in pilotTypes)
            {
                name = itype.Name;

                if (ParsingSectionName(sectionName, "документ") && name == "drawing")
                    return itype;
                if (ParsingSectionName(sectionName, "комплекс") && name == "drawing")
                    return itype;
                if (ParsingSectionName(sectionName, "сборочн") && name == "specification")
                    return itype;
                if (ParsingSectionName(sectionName, "детал") && name == "drawing")
                    return itype;
                if (ParsingSectionName(sectionName, "стандарт") && name == "stdpart")
                    return itype;
                if (ParsingSectionName(sectionName, "проч") && name == "other")
                    return itype;
                if (ParsingSectionName(sectionName, "материал") && name == "material")
                    return itype;
                if (ParsingSectionName(sectionName, "комплект") && name == "complect")
                    return itype;
            }
            return null;
        }

        public void Build(IMenuBuilder builder, ObjectsViewContext context)
        {
            if (context.SelectedObjects.Count() != 1)
                return;

            var dataObjects = context.SelectedObjects.ToArray();
            if (dataObjects.Length != 1)
                return;

            _selected = dataObjects.FirstOrDefault();
            if (_selected == null)
                return;
            if (!_selected.Type.IsMountable)
                return;

            var icon = IconLoader.GetIcon(@"/Resources/menu_icon.svg");
            builder.AddItem(ADD_INFORMATION_TO_PILOT, 1)
                   .WithHeader("Д_обавить информацию из файлов")
                   .WithIcon(icon);
            builder.AddItem(ADD_DOC_TO_PILOT, 1)
                .WithHeader("Д_обавить документы")
                .WithIcon(icon);
        }

        public void OnMenuItemClick(string name, ObjectsViewContext context)
        {
            if (name == ADD_INFORMATION_TO_PILOT)
                SetInformationOnMenuClick(context.SelectedObjects.FirstOrDefault());
            if (name == ADD_DOC_TO_PILOT)
                SetDocOnMenuClick();

        }

        public void Build(IMenuBuilder builder, MainViewContext context)
        {
            var menuItem = builder.ItemNames.First();
            builder.GetItem(menuItem).AddItem(ABOUT_PROGRAM_MENU, 1).WithHeader("О интеграции с КОМПАС");
        }

        public void OnMenuItemClick(string name, MainViewContext context)
        {
            if (name == ABOUT_PROGRAM_MENU)
                new AboutPluginBox().Show();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.SDK.Menu;
using Ascon.Pilot.SDK.SpwReader.Spc;
// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.SpwReader
{
    [Export(typeof(IMenu<MainViewContext>))]
    [Export(typeof(IMenu<ObjectsViewContext>))]
    public class SpwReaderPlugin : IMenu<MainViewContext>, IMenu<ObjectsViewContext>
    {
        private readonly IObjectModifier _objectModifier;
        private readonly IObjectsRepository _objectsRepository;
        private readonly IFileProvider _fileProvider;
        private readonly IEnumerable<IType> _pilotTypes;
        private readonly ObjectLoader _loader;
        private readonly List<IDataObject> _dataObjects;
        private const string ADD_INFORMATION_TO_PILOT = "ADD_INFORMATION_TO_PILOT";
        private const string ABOUT_PROGRAM_MENU = "ABOUT_PROGRAM_MENU";
        private const string SOURCE_DOC_EXT = ".cdw";
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected;
       
        [ImportingConstructor]
        public SpwReaderPlugin(IObjectModifier modifier, IObjectsRepository repository, IPersonalSettings personalSettings, IFileProvider fileProvider)
        {
            _objectModifier = modifier;
            _objectsRepository = repository;
            //todo: fixme
            //_pilotTypes = _objectsRepository.GetTypes();
            _loader = new ObjectLoader(repository);
            _dataObjects = new List<IDataObject>();
            _fileProvider = fileProvider;
        }
        
        /// <summary>
        /// Очистка строки полученной из спецификации от служибных символов: $| и @/
        /// </summary>
        /// <param name="str">Строка которую нужно очистить</param>
        /// <returns>Очищенная строка</returns>
        private static string ValueTextClear(string str)
        {
            return str.Replace("$|", "").Replace(" @/", " ").Replace("@/", " ");
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

        private void SetInformationOnMenuClick(IDataObject selected)
        {
            var listSpec = new List<Specification>();
            var storage = new StorageAnalayzer(_objectsRepository);
            var path = storage.GetProjectFolderByPilotStorage(selected);
            if (path == null) return;
            var filesSpw = storage.GetFilesSpw(path);
            foreach (var fileSpw in filesSpw)
            {
                var spc = GetInformationFromKompas(fileSpw);
                if (spc == null) continue;
                var kompasConverterTask = new Task<KompasConverter>(() =>
                {
                    var k = new KompasConverter(spc.ListSpcObjects);
                    k.KompasConvertToPdf();
                    return k;
                });
                kompasConverterTask.Start();
                kompasConverterTask.Wait();
                listSpec.Add(spc);
            }
            IDataObject parent = null;
            _loader.Load(selected.ParentId, o =>
            {
                parent = o;
            });
            if (parent == null) return;

            foreach (var spc in listSpec)
            {
                SynchronizeCheck(parent, spc.ListSpcObjects);
                AddInformationToPilot(parent, spc.ListSpcObjects);
            }
        }

        private IFile GetFileFromPilotStorage(IDataObject selected, string ext)
        {
            if (selected == null)
                return null;
            IFile file = null;
            _loader.Load(selected.RelatedSourceFiles.FirstOrDefault(), obj =>
            {
                file = obj.Files.FirstOrDefault(f => IsFileExtension(f.Name, ext));
            });
            return file;
        }

        /// <summary>
        /// Метод получает информацию из спецификации и возвращает экземпляр класса Specification
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <returns>Объект спецификации</returns>
        private Specification GetInformationFromKompas(string filename)
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
                spc.CurrentPath = filename;
                return spc;
            }
        }

        private Specification GetInformationFromKompas(IFile file)
        {
            var inputStream = _fileProvider.OpenRead(file);
            if (inputStream == null)
                return null;
            if (!_fileProvider.Exists(file.Id))
                return null;
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
            spc.File = file;
            return spc;
        }

        private void SynchronizeCheck(IDataObject parent, List<SpcObject> listSpcObject)
        {
            var children = parent.TypesByChildren;
            var loader = new LoaderOfObjects(_objectsRepository);
            _dataObjects.Clear();
            loader.Load(children.Keys, objects =>
            {
                foreach (var obj in objects)
                {
                    if (obj.Id == _selected.Id)
                        continue;
                    var attrNameValue = string.Empty;
                    var attrMarkValue = string.Empty;
                    foreach (var a in obj.Attributes)
                    {
                        if (a.Key == "name")
                            attrNameValue = a.Value.ToString();
                        if (a.Key == "mark")
                            attrMarkValue = a.Value.ToString();
                    }
                    foreach (var spcObj in listSpcObject)
                    {
                        bool isName = false, isMark = false;
                        foreach (var column in spcObj.Columns)
                        {
                            var colunmValue = ValueTextClear(column.Value);
                            if ((column.TypeName == "name") && (colunmValue == attrNameValue))
                                isName = true;
                            // TODO: здесь может быть проблема с объектами без обозначения и с дублирующими объектами необходимо тестирование и исследование
                            if ((column.TypeName == "mark") && (colunmValue == attrMarkValue) || attrMarkValue == string.Empty)
                                isMark = true;
                        }
                        if (isName && isMark)
                        {
                            spcObj.IsSynchronized = true;
                            spcObj.GlobalId = obj.Id;
                            _dataObjects.Add(obj);
                        }
                    }
                }
            });
        }

        private void UpdatePilotObject(SpcObject spcObject)
        {
            if (_dataObjects.Count == 0)
                return;
            var needToChange = false;
            var obj = _dataObjects.FirstOrDefault(o => spcObject.GlobalId == o.Id);
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
                    if (attrObj.Value.ToString() != spcColVal)
                    {
                        needToChange = true;
                        int i;
                        if (int.TryParse(spcColVal, out i))
                            builder.SetAttribute(spcColumn.TypeName, i);
                        else
                            builder.SetAttribute(spcColumn.TypeName, spcColVal);
                    }
                }
            }
            // получаем pdf файл из Обозревателя
            var fileFromPilot = obj.Files.FirstOrDefault(f => IsFileExtension(f.Name, ".pdf"));
            var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SOURCE_DOC_EXT));
            if (doc != null && fileFromPilot != null)
            {
                var pdfFile = spcObject.PdfDocument;
                // md5 в нижнем регистре расчитывается и возвращается пилотом
                var fileNameMd5 = CalculatorMd5Checksum.Go(pdfFile);
                if (!string.IsNullOrEmpty(fileNameMd5) && fileFromPilot.Md5 != fileNameMd5)
                {
                    needToChange = true;
                    builder.AddFile(pdfFile);
                }
            }
            //TODO: внесмотря на провекрку выдаётся ошибка, если изменился только чертёж
            if (needToChange) _objectModifier.Apply();
        }

        private void CreateNewObjectsToPilot(IDataObject parent, SpcObject spcObject)
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
            if (doc != null)
            {
                var fileName = doc.FileName;
                string[] paths = { fileName };
                var storageObjects = _objectsRepository.GetStorageObjects(paths);
                var storageObject = storageObjects.FirstOrDefault();

                if (storageObject != null)
                {
                    //Create relations
                    var relationId = Guid.NewGuid();
                    var relationName = relationId.ToString();
                    const ObjectRelationType relationType = ObjectRelationType.SourceFiles;
                    var selectedRealtion = new Relation
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
                    _objectModifier.CreateLink(selectedRealtion, chosenRelation);
                    _objectModifier.Apply();
                }
                if (File.Exists(spcObject.PdfDocument))
                {
                    builder.AddFile(spcObject.PdfDocument);
                };
            }
            _objectModifier.Apply();
        }

        private void AddInformationToPilot(IDataObject parent, List<SpcObject> _listSpcObject)
        {
            foreach (var spcObject in _listSpcObject)
            {
                if (string.IsNullOrEmpty(spcObject.SectionName)) continue;
                if (!spcObject.IsSynchronized)
                {
                    CreateNewObjectsToPilot(parent, spcObject);
                }
                else
                {
                    UpdatePilotObject(spcObject);
                }
            }
        }

        private IType GetTypeBySectionName(string sectionName)
        {
            // ReSharper disable once RedundantAssignment
            var title = string.Empty;
            foreach (var itype in _pilotTypes)
            {
                title = itype.Title;
                if (ParsingSectionName(sectionName, "документ") && title == "Документ")
                    return itype;
                if (ParsingSectionName(sectionName, "комплекс") && title == "Комплекс")
                    return itype;
                if (ParsingSectionName(sectionName, "сборочн") && title == "Сборочная единица")
                    return itype;
                if (ParsingSectionName(sectionName, "детал") && title == "Деталь")
                    return itype;
                if (ParsingSectionName(sectionName, "стандарт") && title == "Стандартное изделие")
                    return itype;
                if (ParsingSectionName(sectionName, "проч") && title == "Прочее изделие")
                    return itype;
                if (ParsingSectionName(sectionName, "материал") && title == "Материал")
                    return itype;
                if (ParsingSectionName(sectionName, "комплект") && title == "Комплект")
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
                   .WithHeader("Д_обавить информацию с диска")
                   .WithIcon(icon);
        }

        public void OnMenuItemClick(string name, ObjectsViewContext context)
        {
            if (name == ADD_INFORMATION_TO_PILOT)
                SetInformationOnMenuClick(context.SelectedObjects.FirstOrDefault());
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

using System;
using NLog;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Uln.KompasShell;


namespace Ascon.Pilot.SDK.SpwReader
{
    [Export(typeof(IMainMenu))]
    [Export(typeof(IObjectContextMenu))]
    public class SpwReaderPlugin : IObjectContextMenu, IMainMenu
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IObjectModifier _objectModifier;
        private readonly IObjectsRepository _objectsRepository;
        private readonly IFileProvider _fileProvider;
        private readonly IEnumerable<IType> _pilotTypes;
        private readonly ObjectLoader _loader;
        private const string ADD_INFORMATION_TO_PILOT = "ADD_INFORMATION_TO_PILOT";
        private const string ABOUT_PROGRAM_MENU = "ABOUT_PROGRAM_MENU";
        private const string SpwExt = ".spw";
        private const string SourceDocExt = ".cdw";
        private const string PilotTypeAssembly = "assembly";
        private const string PilotTypeDocument = "document";
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected;
        // задача для открытия и анализа файла спецификации
        private Task<SpwAnalyzer> _taskOpenSpwFile;
        // список объктов спецификации полученных в ходе парсинга
        private List<SpcObject> _listSpcObject;
        private KomapsShell _komaps;
        private bool _isKompasInit;
        private List<IDataObject> _dataObjects;


        [ImportingConstructor]
        public SpwReaderPlugin(IObjectModifier modifier, IObjectsRepository repository, IPersonalSettings personalSettings, IFileProvider fileProvider)
        {
            _objectModifier = modifier;
            _objectsRepository = repository;
            _fileProvider = fileProvider;
            _pilotTypes = _objectsRepository.GetTypes();
            _loader = new ObjectLoader(repository);
            _dataObjects = new List<IDataObject>();
#if DEBUG
            Logger.Info("Start plugin. Logger work!");
#endif
        }

        public void BuildMenu(IMenuHost menuHost)
        {
            var menuItem = menuHost.GetItems().First();
            menuHost.AddSubItem(menuItem, ABOUT_PROGRAM_MENU, "О интеграции с КОМПАС", null, 0);
            menuHost.AddItem(ABOUT_PROGRAM_MENU, "О интеграции с КОМПАС", null, 1);
        }

        public void OnMenuItemClick(string itemName)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (itemName)
            {
                case ADD_INFORMATION_TO_PILOT:
                    SetInformationOnMenuClick(_selected);
                    break;
                case ABOUT_PROGRAM_MENU:
                    new MessageBox().Show();
                    break;
            }
        }

        public void BuildContextMenu(IMenuHost menuHost, IEnumerable<IDataObject> selection, bool isContext)
        {
            if (isContext)
                return;

            var dataObjects = selection.ToArray();
            if (dataObjects.Count() != 1)
                return;

            var itemNames = menuHost.GetItems().ToList();
            const string indexItemName = "SetInfo";
            var insertIndex = itemNames.IndexOf(indexItemName) + 1;

            _selected = dataObjects.FirstOrDefault();
            if (_selected == null)
                return;
            if (!CheckObjectsType(_selected))
                return; ;

            var icon = IconLoader.GetIcon(@"/Resources/menu_icon.svg");
            menuHost.AddItem(ADD_INFORMATION_TO_PILOT, "Д_обавить информацию из спецификации", icon, insertIndex);
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

        private bool CheckObjectsType(IDataObject selected)
        {
            var isCheck = false;
            if (selected.Type.Name != PilotTypeDocument) return false;
            _loader.Load(selected.ParentId, parent =>
            {
                isCheck = parent.Type.Name == PilotTypeAssembly;
            });
            return isCheck;
        }

        private void SetInformationOnMenuClick(IDataObject selected)
        {
            if (!CheckObjectsType(selected))
                return;

            var file = GetFileFromPilotStorage(selected, SpwExt);
            if (file == null) return;
            var info = GetInformationFromKompas(file);
            if (info == null) return;
            if (!info.Result.IsCompleted) return;
            _komaps = new KomapsShell();
            string message;
            _isKompasInit = _komaps.InitKompas(out message);
            if (!_isKompasInit) Logger.Error(message);
            _loader.Load(_selected.ParentId, parent =>
            {
                SynchronizeCheck(parent);
                AddInformationToPilot(parent);
            });
            _komaps.ExitKompas();
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

        private IFile GetFileFromPilotStorage(Guid id, string ext)
        {
            IFile file = null;
            _loader.Load(id, obj =>
            {
                file = obj.Files.FirstOrDefault(f => IsFileExtension(f.Name, ext));
            });
            return file;
        }

        private Task<SpwAnalyzer> GetInformationFromKompas(IFile file)
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

            _taskOpenSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(ms));
            _taskOpenSpwFile.Start();
            _taskOpenSpwFile.Wait();
            if (!_taskOpenSpwFile.Result.IsCompleted)
                return null;
            _listSpcObject = _taskOpenSpwFile.Result.GetListSpcObject;
            return _taskOpenSpwFile;
        }

        private void SynchronizeCheck(IDataObject parent)
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
                    foreach (var spcObj in _listSpcObject)
                    {
                        bool isName = false, isMark = false;
                        foreach (var column in spcObj.Columns)
                        {
                            var colunmValue = ValueTextClear(column.Value);
                            if ((column.TypeName == "name") && (colunmValue == attrNameValue))
                                isName = true;
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

        private void AddPdfFileToPilotObject(IObjectBuilder builder, string fileName)
        {
            if (!_isKompasInit) return;
            if (!File.Exists(fileName)) return;
            var pdfFile = Path.GetTempFileName() + ".pdf";
            string message;
            var isConvert = _komaps.ConvertToPdf(fileName, pdfFile, out message);
            if (!isConvert)
            {
                Logger.Error(message);
                return;
            }
            builder.AddFile(pdfFile);
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
            var fileFromPilot = GetFileFromPilotStorage(obj.RelatedSourceFiles.FirstOrDefault(), SourceDocExt);
            var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SourceDocExt));
            if (doc != null)
            {
                var fileName = doc.FileName;
                if (fileFromPilot != null)
                {
                    // md5 в нижнем регистре расчитывается и возвращается пилотом
                    var fileNameMd5 = CalculatorMd5Checksum.Go(fileName);
                    if (fileFromPilot.Md5 != fileNameMd5)
                    {
                        needToChange = true;
                        string[] paths = { fileName };
                        var storageObjects = _objectsRepository.GetStorageObjects(paths);
                        var storageObject = storageObjects.FirstOrDefault();
                        if (storageObject != null)
                            builder.AddSourceFileRelation(storageObject.DataObject.Id);
                        AddPdfFileToPilotObject(builder, fileName);
                    }
                }
            }
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
                int i;
                if (int.TryParse(val, out i))
                    builder.SetAttribute(attr.TypeName, i);
                else
                    builder.SetAttribute(attr.TypeName, val);
            }
            var doc = spcObject.Documents.FirstOrDefault(f => IsFileExtension(f.FileName, SourceDocExt));
            if (doc != null)
            {
                var fileName = doc.FileName;
                string[] paths = { fileName };
                var storageObjects = _objectsRepository.GetStorageObjects(paths);
                var storageObject = storageObjects.FirstOrDefault();
                if (storageObject != null)
                    builder.AddSourceFileRelation(storageObject.DataObject.Id);
                AddPdfFileToPilotObject(builder, fileName);
            }
            _objectModifier.Apply();
        }

        private void AddInformationToPilot(IDataObject parent)
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
                if (ParsingSectionName(sectionName, "прочи") && title == "Прочее изделие")
                    return itype;
                if (ParsingSectionName(sectionName, "материал") && title == "Материал")
                    return itype;
                if (ParsingSectionName(sectionName, "комплект") && title == "Комплект")
                    return itype;
            }
            return null;
        }
    }
}

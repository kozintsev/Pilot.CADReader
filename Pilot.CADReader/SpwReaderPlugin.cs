using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Ascon.Pilot.SDK.SpwReader
{
    [Export(typeof(IStorageContextMenu))]
    [Export(typeof(IObjectContextMenu))]
    [Export(typeof(IMainMenu))]
    public class SpwReaderPlugin : IStorageContextMenu, IObjectContextMenu, IMainMenu
    {

        private readonly IObjectModifier _objectModifier;
        private readonly IObjectsRepository _objectsRepository;
        private readonly IPersonalSettings _personalSettings;
        private readonly IFileProvider _fileProvider;
        private readonly IEnumerable<IType> _pilotTypes;
        private const string ADD_INFORMATION_TO_PILOT = "ADD_INFORMATION_TO_PILOT";
        private const string GET_INFORMATION_BY_FILE = "GET_INFORMATION_BY_FILE";
        private const string ABOUT_PROGRAM_MENU = "ABOUT_PROGRAM_MENU";
        // путь к файлу выбранному на Pilot Storage
        private string _path;
        private SpwReaderSettings _settings;
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected;
        
        // задача для открытия и анализа файла спецификации
        private Task<SpwAnalyzer> _taskOpenSpwFile;
        // список объктов спецификации полученных в ходе парсинга
        private List<SpcObject> _listSpcObject;
        // список секций спецификации
        private List<SpcSection> _spcSections;


        [ImportingConstructor]
        public SpwReaderPlugin(IObjectModifier modifier, IObjectsRepository repository, IPersonalSettings personalSettings, IFileProvider fileProvider)
        {
            _objectModifier = modifier;
            _objectsRepository = repository;
            _personalSettings = personalSettings;
            _fileProvider = fileProvider;
            _pilotTypes = _objectsRepository.GetTypes();

            _settings = new SpwReaderSettings(personalSettings, repository);
        }

        public void BuildMenu(IMenuHost menuHost)
        {
            var menuItem = menuHost.GetItems().First();
            menuHost.AddSubItem(menuItem, ABOUT_PROGRAM_MENU, "О интеграции с КОМПАС", null, 0);
            menuHost.AddItem(ABOUT_PROGRAM_MENU, "О интеграции с КОМПАС", null, 1);
        }


        public void OnMenuItemClick(string itemName)
        {
            switch (itemName)
            {
                case ADD_INFORMATION_TO_PILOT:
                    SetInformationOnMenuClick(_selected);
                    break;
                case GET_INFORMATION_BY_FILE:
                    GetInformationByKompas(_path);
                    break;
                case ABOUT_PROGRAM_MENU:
                    new MessageBox().Show();
                    break;
            }
        }


        public void BuildContextMenu(IMenuHost menuHost, IEnumerable<IStorageDataObject> selection)
        {
            var icon = IconLoader.GetIcon(@"/Resources/icon.png");
            var itemNames = menuHost.GetItems().ToList();
            const string indexItemName = "mniShowProjectsExplorerCommand";
            var insertIndex = itemNames.IndexOf(indexItemName) + 1;

            menuHost.AddItem(GET_INFORMATION_BY_FILE, "Получить информацию", icon, insertIndex);
            var item = selection.FirstOrDefault();
            if (item == null)
                return;
            _path = item.Path;
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
            if (_selected.Type.Name == "assembly" || _selected.Type.Name == "document")
            {
                var icon = IconLoader.GetIcon(@"/Resources/menu_icon.svg");
                menuHost.AddItem(ADD_INFORMATION_TO_PILOT, "Д_обавить информацию из спецификации", icon, insertIndex);
            }
        }

        private static string ValueTextClear(string str)
        {
            return str.Replace("$|", "").Replace(" @/", " ").Replace("@/", " ");
        }

        private static string CreateOpenFileDialog()
        {
            var filename = string.Empty;
            var dlg = new OpenFileDialog {Filter = "Компас-спецификация|;*.spw"};
            dlg.FileOk += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                filename = dlg.FileName;
            };
            dlg.ShowDialog();
            return filename;
        }

        private void SetInformationOnMenuClick(IDataObject selected)
        {
            IDataObject parent;
            switch (selected.Type.Name)
            {
                case "assembly":
                    parent = _objectsRepository.GetCachedObject(selected.Id);
                    break;
                case "document":
                    parent = _objectsRepository.GetCachedObject(selected.ParentId);
                    break;
                default:
                    return;
            }
            var file = GetFileByPilotStorage();
            if (file != null)
            {
                var info = GetInformationByKompas(file);
                if (info != null)
                {
                    if (info.Result.IsCompleted)
                    {
                        AddInformationByPilot(parent);
                        return;
                    }
                }
            }
            if (_taskOpenSpwFile != null)
            {
                if (_taskOpenSpwFile.Result.IsCompleted)
                {
                    AddInformationByPilot(parent);
                    return;
                }
            }
            if (!UserTakeFile())
                return; ;
            GetInformationByKompas(_path);
            if (_taskOpenSpwFile == null)
                return;
            if (_taskOpenSpwFile.Result.IsCompleted)
                AddInformationByPilot(parent);

        }

        private bool IsCorrectFileExtension(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            var ext = Path.GetExtension(name).ToLower();
            if (ext == ".spw" || ext == ".zip")
                return true;

            return false;
        }

        private IFile GetFileByPilotStorage()
        {
            if (_selected == null)
                return null;
            var obj = _objectsRepository.GetCachedObject(_selected.RelatedSourceFiles.FirstOrDefault());
            var file = obj.Files.FirstOrDefault(f => IsCorrectFileExtension(f.Name));
            return file;
        }


        private Task<SpwAnalyzer> GetInformationByKompas(IFile file)
        {
            if (!_fileProvider.Exists(file.Id))
                return null;

            var inputStream = _fileProvider.OpenRead(file);
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
            _spcSections = _taskOpenSpwFile.Result.GetListSpcSection;
            return _taskOpenSpwFile;
        }

        private Task<SpwAnalyzer> GetInformationByKompas(string filename)
        {
            var fInfo = new FileInfo(filename);
            if (!fInfo.Exists) // file does not exist; do nothing
                return null;

            var ext = fInfo.Extension.ToLower();
            if (ext == ".spw" || ext == ".zip")
            {
                _taskOpenSpwFile = new Task<SpwAnalyzer>(() => new SpwAnalyzer(filename));
                _taskOpenSpwFile.Start();
                _taskOpenSpwFile.Wait();
                if (!_taskOpenSpwFile.Result.IsCompleted)
                    return null;
                _listSpcObject = _taskOpenSpwFile.Result.GetListSpcObject;
                _spcSections = _taskOpenSpwFile.Result.GetListSpcSection;
                return _taskOpenSpwFile;
            }
            return null;
        }


        

        private bool UserTakeFile()
        {
            var path = CreateOpenFileDialog();
            if (string.IsNullOrEmpty(path))
                return false;
            _path = path;
            return true;
        }


        private void SynchronizeCheck(IDataObject parent)
        {
            bool isName = false, isMark = false;
            var linkobjects = parent.TypesByChildren;
            foreach (var obj in linkobjects)
            {
                var key = obj.Key;
                var currentObj =_objectsRepository.GetCachedObject(key);
                var attrName = string.Empty;
                var attrMark = string.Empty;
                foreach (var a in currentObj.Attributes)
                {
                    if (a.Key == "name")
                        attrName = a.Value.ToString();
                    if (a.Key == "mark")
                        attrMark = a.Value.ToString();

                }
                if (string.IsNullOrEmpty(attrName) && string.IsNullOrEmpty(attrMark))
                    return;
                foreach (var spcObj in _listSpcObject)
                {
                    foreach (var column in spcObj.Columns.Select(col => ValueTextClear(col.Value)))
                    {
                        if (column == attrName)
                            isName = true;
                        if (column == attrMark)
                            isMark = true;
                    }
                    if (isName && isMark)
                        spcObj.IsSynchronized = true;
                }
            }
        }

        private void AddInformationByPilot(IDataObject parent)
        {
            //var parent = _repository.GetCachedObject(parentId);
            SynchronizeCheck(parent);
            foreach (var spcObject in _listSpcObject)
            {
                if (!string.IsNullOrEmpty(spcObject.SectionName))
                {
                    var t = GetTypeBySectionName(spcObject.SectionName);
                    if (t != null && !spcObject.IsSynchronized)
                    {
                        var builder = _objectModifier.Create(parent, t);
                        foreach (var attr in spcObject.Columns)
                        {
                            var val = attr.Value;
                            // очишаем значение от служебных символов и выражений
                            val = ValueTextClear(val);
                            // в качестве наименование передаётся внутренее имя (а не то которое отображается)
                            if (string.IsNullOrEmpty(attr.TypeName))
                                continue;
                            if (!string.IsNullOrEmpty(val))
                                builder.SetAttribute(attr.TypeName, val);
                        }
                        _objectModifier.Apply();
                    }

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
                if (sectionName == "Документация" && title == "Документ")
                    return itype;
                if (sectionName == "Комплексы" && title == "Комплекс")
                    return itype;
                if (sectionName == "Сборочные единицы" && title == "Сборочная единица")
                    return itype;
                if (sectionName == "Детали" && title == "Деталь")
                    return itype;
                if (sectionName == "Стандартные изделия" && title == "Стандартное изделие")
                    return itype;
                if (sectionName == "Прочие изделия" && title == "Прочее изделие")
                    return itype;
                if (sectionName == "Материалы" && title == "Материал")
                    return itype;
                if (sectionName == "Комплекты" && title == "Комплект")
                    return itype;
            }
            return null;
        }
    }
}

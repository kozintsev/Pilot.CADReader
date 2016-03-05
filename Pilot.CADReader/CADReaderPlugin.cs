using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Ascon.Pilot.SDK.CADReader
{
    [Export(typeof(IStorageContextMenu))]
    [Export(typeof(IObjectContextMenu))]
    public class CADReaderPlugin : IStorageContextMenu, IObjectContextMenu
    {

        private readonly IObjectModifier _objectModifier;
        private readonly IObjectsRepository _objectsRepository;
        private readonly IPersonalSettings _personalSettings;
        private readonly IFileProvider _fileProvider;
        private const string ADD_INFORMATION_TO_PILOT = "ADD_INFORMATION_TO_PILOT";
        private const string GET_INFORMATION_BY_FILE = "GET_INFORMATION_BY_FILE";
        // путь к файлу выбранному на Pilot Storage
        private string _path;
        private CADReaderSettings settings;
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected;
        IEnumerable<IType> pilotTypes;
        // задача для открытия и анализа файла спецификации
        Task<SpwAnalyzer> taskOpenSpwFile;
        // список объктов спецификации полученных в ходе парсинга
        private List<SpcObject> listSpcObject;
        // список секций спецификации
        private List<SpcSection> spcSections;


        [ImportingConstructor]
        public CADReaderPlugin(IObjectModifier modifier, IObjectsRepository repository, IPersonalSettings personalSettings, IFileProvider fileProvider)
        {
            _objectModifier = modifier;
            _objectsRepository = repository;
            _personalSettings = personalSettings;
            _fileProvider = fileProvider;
            pilotTypes = _objectsRepository.GetTypes();

            settings = new CADReaderSettings(personalSettings, repository);
        }


        public void OnMenuItemClick(string itemName)
        {
            switch (itemName)
            {
                // если выбрано меню в клиенте
                case ADD_INFORMATION_TO_PILOT:
                    SetInformationOnMenuClick(_selected);
                    break;
                // если выбрано меню на Pilot Storage
                case GET_INFORMATION_BY_FILE:
                    GetInformationByKompas(_path);
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
            if (_selected.Type.Name == "assembly" || _selected.Type.Name == "document")
            {
                menuHost.AddItem(ADD_INFORMATION_TO_PILOT, "Д_обавить информацию из спецификации", null, insertIndex);
            }
        }

        private void SetInformationOnMenuClick(IDataObject selected)
        {
            IDataObject parent;
            if (selected.Type.Name == "assembly")
            {
                parent = _objectsRepository.GetCachedObject(selected.Id);
            }
            else if (selected.Type.Name == "document")
            {
                parent = _objectsRepository.GetCachedObject(selected.ParentId);
            }
            else
            {
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
            if (taskOpenSpwFile != null)
            {
                if (taskOpenSpwFile.Result.IsCompleted)
                {
                    AddInformationByPilot(parent);
                    return;
                }
            }
            if (UserTakeFile())
            {
                GetInformationByKompas(_path);
                if (taskOpenSpwFile.Result.IsCompleted)
                    AddInformationByPilot(parent);
            }
        }

        private bool IsCorrectFileExtension(string name)
        {
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
            IFile file = obj.Files.FirstOrDefault(f => IsCorrectFileExtension(f.Name));
            return file;
        }


        private Task<SpwAnalyzer> GetInformationByKompas(IFile file)
        {
            if (!_fileProvider.Exists(file.Id))
                return null;

            var inputStream = _fileProvider.OpenRead(file);
            MemoryStream ms = new MemoryStream();
            inputStream.Seek(0, SeekOrigin.Begin);
            inputStream.CopyTo(ms);
            ms.Position = 0;

            taskOpenSpwFile = new Task<SpwAnalyzer>(() =>
            {
                return new SpwAnalyzer(ms);
            });
            taskOpenSpwFile.Start();
            taskOpenSpwFile.Wait();
            if (taskOpenSpwFile.Result.IsCompleted)
            {
                listSpcObject = taskOpenSpwFile.Result.GetListSpcObject;
                spcSections = taskOpenSpwFile.Result.GetListSpcSection;
            }
            return taskOpenSpwFile;
        }

        private Task<SpwAnalyzer> GetInformationByKompas(string filename)
        {
            var fInfo = new FileInfo(filename);
            if (!fInfo.Exists) // file does not exist; do nothing
                return null;

            var ext = fInfo.Extension.ToLower();
            if (ext == ".spw" || ext == ".zip")
            {
                taskOpenSpwFile = new Task<SpwAnalyzer>(() =>
                {
                    return new SpwAnalyzer(filename);
                });
                taskOpenSpwFile.Start();
                taskOpenSpwFile.Wait();
                if (taskOpenSpwFile.Result.IsCompleted)
                {
                    listSpcObject = taskOpenSpwFile.Result.GetListSpcObject;
                    spcSections = taskOpenSpwFile.Result.GetListSpcSection;
                }
                return taskOpenSpwFile;
            }
            return null;
        }


        private string CreateOpenFileDialog()
        {
            string filename = String.Empty;
            var dlg = new OpenFileDialog();
            dlg.Filter = "Компас-спецификация|;*.spw"; // Filter files by extension
            dlg.FileOk += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (dlg != null)
                {
                    filename = dlg.FileName;
                }
            };
            dlg.ShowDialog();
            return filename;
        }

        private bool UserTakeFile()
        {
            string path = CreateOpenFileDialog();
            if (String.IsNullOrEmpty(path))
                return false;
            _path = path;
            return true;
        }

        private void AddInformationByPilot(IDataObject parent)
        {
            //var parent = _repository.GetCachedObject(parentId);
            foreach (var spcObject in listSpcObject)
            {
                if (!String.IsNullOrEmpty(spcObject.SectionName))
                {
                    var t = GetTypeBySectionName(spcObject.SectionName);
                    if (t != null)
                    {
                        var builder = _objectModifier.Create(parent, t);
                        foreach (var attr in spcObject.Columns)
                        {
                            string val = attr.Value;
                            // очишаем значение от служебных символов и выражений
                            val = ValueTextClear(val);
                            // в качестве наименование передаётся внутренее имя (а не то которое отображается)
                            if (String.IsNullOrEmpty(attr.TypeName))
                                continue;
                            if (!String.IsNullOrEmpty(val))
                                builder.SetAttribute(attr.TypeName, val);
                        }
                        _objectModifier.Apply();
                    }

                }
            }
        }

        private IType GetTypeBySectionName(string sectionName)
        {
            string title;
            foreach (var itype in pilotTypes)
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
            return null;//_repository.GetTypes().Where(t => t.Title == sectionName).FirstOrDefault();
        }

        private string ValueTextClear(string str)
        {
            return str.Replace("$|", "").Replace(" @/", " ").Replace("@/", " ");
        }
    }
}

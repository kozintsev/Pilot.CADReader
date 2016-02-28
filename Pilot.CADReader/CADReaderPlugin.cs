using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Ascon.Pilot.SDK.CADReader
{
    [Export(typeof(IStorageContextMenu))]
    [Export(typeof(IObjectContextMenu))]
    [Export(typeof(IMainMenu))]
    public class CADReaderPlugin : IStorageContextMenu, IObjectContextMenu, IMainMenu
    {

        private readonly IObjectModifier _modifier;
        private readonly IObjectsRepository _repository;
        private const string CreateCopyItemName = "InsertObjects";
        private const string _command_name = "CopyPathCommand";
        private const string ServiceMenu = "CADReaderSetting";
        // путь к файлу выбранному на Pilot Storage
        private string _path;
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected; 
        // поток для открытия и анализа файла спецификации
        private BackgroundWorker _worker;
        // список объктов спецификации полученных в ходе парсинга
        private List<SpcObject> listSpcObject = null;
        // список секций спецификации
        private List<SpcSection> spcSections = null;
        // 


        [ImportingConstructor]
        public CADReaderPlugin(IObjectModifier modifier, IObjectsRepository repository)
        {
            _modifier = modifier;
            _repository = repository;
            
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
        }

        private IType GetTypeBySectionName(string sectionName)
        {
            string title;
            foreach (var itype in _repository.GetTypes())
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

        public void OnMenuItemClick(string itemName)
        {
            // если выбрано меню в клиенте
            if (itemName == CreateCopyItemName)
            {
                var parent = _repository.GetCachedObject(_selected.Id);
                foreach (var spcObject in listSpcObject)
                {
                    // определяем наименование секции спецификации
                    // в будущем необходимо доработать 
                    foreach (var spcSection in spcSections)
                    {
                        if (spcObject.SectionNumber == spcSection.Number)
                            spcObject.SectionName = spcSection.Name;
                    }
                    if (!String.IsNullOrEmpty(spcObject.SectionName))
                    {
                        var t = GetTypeBySectionName(spcObject.SectionName);
                        if (t != null)
                        {
                            var builder = _modifier.Create(parent, t);
                        }
                        
                    }
                        
                    
                }
                //var t = GetTypeByTitle("Сборочная единица");
                string s = t.Name.ToString();
                Debug.WriteLine(s);
                //Метод позволяет получить список всех типов.
                //IEnumerable< IType > IObjectsRepository.GetTypes();
                //var builder = _modifier.Create(parent, _selected.Type);
                //foreach (var attribute in _selected.Attributes)
                //{
                //    if (attribute.Value is string)
                //        builder.SetAttribute(attribute.Key, (string)attribute.Value);
                //    if (attribute.Value is int)
                //        builder.SetAttribute(attribute.Key, (int)attribute.Value);
                //    if (attribute.Value is double)
                //        builder.SetAttribute(attribute.Key, (double)attribute.Value);
                //    if (attribute.Value is DateTime)
                //        builder.SetAttribute(attribute.Key, (DateTime)attribute.Value);
                //}
                //_modifier.Apply();
            }
            // если выбрано меню на Pilot Storage
            if (itemName.Equals(_command_name, StringComparison.InvariantCultureIgnoreCase))
            {
                var fInfo = new FileInfo(_path);
                if (!fInfo.Exists) // file does not exist; do nothing
                    return;

                var ext = fInfo.Extension.ToLower();
                if (ext == ".spw" || ext == ".zip")
                {
                    _worker.DoWork += openSpwFile;
                    _worker.RunWorkerAsync(_path);
                }
            }
            // вызов окна с настройками 
            if (itemName == ServiceMenu)
            {
                Debug.WriteLine("\"My menu in service\" was clicked");
            }
                
        }

        private void openSpwFile(object sender, DoWorkEventArgs args)
        {
            var worker = sender as BackgroundWorker;
            var fileName = args.Argument as string;
            ZFile z = new ZFile();
            if (z.IsZip(fileName))
            {
                z.ExtractZipToMemoryStream(fileName, "MetaInfo");
                SpwAnalyzer x = new SpwAnalyzer(z.OutputMemStream);
                // событие вызываемое после парсинга спецификации
                x.ParsingCompletedEvent += delegate (object s, EventArgs e)
                {
                    spcSections   = x.GetListSpcSection();
                    listSpcObject = x.GetListSpcObject();
                };
                if (x.Opened)
                {
                    x.Run();
                }
            }
        }

        public void BuildContextMenu(IMenuHost menuHost, IEnumerable<IStorageDataObject> selection)
        {
            var icon = IconLoader.GetIcon(@"/Resources/icon.png");
            var itemNames = menuHost.GetItems().ToList();
            const string indexItemName = "mniShowProjectsExplorerCommand";
            var insertIndex = itemNames.IndexOf(indexItemName) + 1;

            menuHost.AddItem(_command_name, "Get information", icon, insertIndex);
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
            menuHost.AddItem(CreateCopyItemName, "S_et information", null, insertIndex);
        }

        public void BuildMenu(IMenuHost menuHost)
        {
            var menuItem = menuHost.GetItems().First();
            //menuHost.AddSubItem(menuItem, ServiceMenu, "CAD Reader", null, 0);

            //menuHost.AddItem("Setting", "Setting", null, 1);
            //menuHost.AddSubItem("MyMenu", MySubMenu, "Submenu item", null, 0);
        }
    }
}

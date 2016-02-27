using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Ascon.Pilot.SDK.CADReader
{
    [Export(typeof(IStorageContextMenu))]
    [Export(typeof(IObjectContextMenu))]
    public class CADReaderPlugin : IStorageContextMenu, IObjectContextMenu
    {

        private readonly IObjectModifier _modifier;
        private readonly IObjectsRepository _repository;
        private const string CreateCopyItemName = "InsertObjects";
        private const string _command_name = "CopyPathCommand";
        // путь к файлу выбранному на Pilot Storage
        private string _path;
        // выбранный с помощью контекстного меню клиента объект
        private IDataObject _selected; 
        // поток для открытия и анализа файла спецификации
        private BackgroundWorker _worker;

       
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

        public void OnMenuItemClick(string itemName)
        {
            // если выбрано меню в клиенте
            if (itemName == CreateCopyItemName)
            {
                var parent = _repository.GetCachedObject(_selected.ParentId);
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
    }
}

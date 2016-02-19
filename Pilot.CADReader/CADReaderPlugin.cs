using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Ascon.Pilot.SDK.CADReader
{
    [Export(typeof(IStorageContextMenu))]
    public class CADReaderPlugin : IStorageContextMenu
    {

        private const string _command_name = "CopyPathCommand";
        private string _path;
        private BackgroundWorker _worker;

  

        public CADReaderPlugin()
        {
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
        }

        public void OnMenuItemClick(string itemName)
        {
            if (!itemName.Equals(_command_name, StringComparison.InvariantCultureIgnoreCase))
                return;
            var fInfo = new FileInfo(_path);
            if (!fInfo.Exists) // file does not exist; do nothing
                return;

            var ext = fInfo.Extension.ToLower();
            if (ext == ".spw" || ext == ".zip")
            {
                _worker.DoWork += openSpwFile;
                _worker.RunWorkerAsync(_path);
            }

            //var thread = new Thread(() => Clipboard.SetText(_path));
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
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
                x.Run();
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
    }
}

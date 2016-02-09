using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Ascon.Pilot.SDK.CADReader
{
    [Export(typeof(IStorageContextMenu))]
    public class CADReaderPlugin : IStorageContextMenu
    {

        private const string _command_name = "CopyPathCommand";
        private string _path;

        public void OnMenuItemClick(string itemName)
        {
            if (!itemName.Equals(_command_name, StringComparison.InvariantCultureIgnoreCase))
                return;

            //var thread = new Thread(() => Clipboard.SetText(_path));
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
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

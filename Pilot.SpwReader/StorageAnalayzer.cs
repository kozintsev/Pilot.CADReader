using System.IO;

// ReSharper disable InconsistentNaming

namespace Ascon.Pilot.SDK.CadReader
{
    internal class StorageAnalayzer
    {
        private const string SPW_EXT = "*.spw";
        private readonly string _storagePath;
        private readonly IObjectsRepository _repository;

        public StorageAnalayzer(IObjectsRepository repository)
        {
            _storagePath = repository.GetStoragePath();
            _repository = repository;
        }

        public string GetProjectFolderByPilotStorage(IDataObject dataObject)
        {
            var dirs = Directory.GetDirectories(_storagePath);
            var objs = _repository.GetStorageObjects(dirs);
            foreach (var obj in objs)
            {
                if (obj.DataObject.Id == dataObject.Id)
                    return obj.Path;
            }
            return null;
        }

        public string[] GetFilesSpw(string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return null;
            var files = Directory.GetFiles(folder, SPW_EXT, SearchOption.AllDirectories);
            return files;
        }
    }
}

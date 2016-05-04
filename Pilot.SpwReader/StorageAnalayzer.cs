using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.SpwReader
{
    class StorageAnalayzer
    {
        private readonly string _storagePath;
        private readonly IObjectsRepository _repository;

        public StorageAnalayzer(IObjectsRepository repository)
        {
            _storagePath = repository.GetStoragePath();
            _repository = repository;
            GetInformation();
        }

        private void GetInformation()
        {
            var dirs = Directory.GetDirectories(_storagePath);
            var objs = _repository.GetStorageObjects(dirs);
            foreach (var obj in objs)
            {
                // проекты
                var wr = new DataObjectWrapper(obj.DataObject, _repository);  
            }
            foreach (var dir in dirs)
            {
                Directory.GetDirectories(dir);
                Directory.GetFiles(dir);
                Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            }
        }
    }
}

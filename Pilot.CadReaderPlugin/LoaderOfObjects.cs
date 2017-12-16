using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.SDK.CadReader
{
    class LoaderOfObjects : IObserver<IDataObject>, IObserver<IStorageDataObject>
    {
        private readonly IObjectsRepository _repository;
        private  int _count;
        private Action<List<IDataObject>> _onLoadedAction;
        private IDisposable _subscription;
        private List<IDataObject> _list;

        public LoaderOfObjects(IObjectsRepository repository)
        {
            _repository = repository;
        }

        public void Load(ICollection<Guid> ids , Action<List<IDataObject>> onLoadedAction)
        {
            _onLoadedAction = onLoadedAction;
            _count = ids.Count();
            _list = new List<IDataObject>();
            _subscription = _repository.SubscribeObjects(ids).Subscribe(this);
            _subscription.Dispose();
        }
        public void OnNext(IDataObject value)
        {
            if (value.State != DataState.Loaded)
                return;
            _list.Add(value);
            if (_list.Count != _count)
                return;
            _onLoadedAction(_list);
        }

        public void OnNext(IStorageDataObject value)
        {
            
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ascon.Pilot.SDK.SpwReader
{
    class LoaderOfObjects : IObserver<IDataObject>
    {
        private readonly IObjectsRepository _repository;
        private  int _count;
        private Action<List<IDataObject>> _onLoadedAction;
        private bool _sent;
        private IDisposable _subscription;
        private List<IDataObject> _list;

        public LoaderOfObjects(IObjectsRepository repository)
        {
            _repository = repository;
        }

        public void Load(ICollection<Guid> ids , Action<List<IDataObject>> onLoadedAction)
        {
            _sent = false;
            _onLoadedAction = onLoadedAction;
            _count = ids.Count();
            _list = new List<IDataObject>();
            _subscription = _repository.SubscribeObjects(ids).Subscribe(this);
        }
        public void OnNext(IDataObject value)
        {
            if (_sent)
            {
                if (_subscription != null)
                    _subscription.Dispose();
                return;
            }

            if (value.State != DataState.Loaded)
                return;

            _list.Add(value);

            if (_list.Count != _count)
                return;

            _onLoadedAction(_list);
            _sent = true;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }
    }
}

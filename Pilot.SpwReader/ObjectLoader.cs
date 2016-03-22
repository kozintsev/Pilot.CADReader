using System;

namespace Ascon.Pilot.SDK.SpwReader
{
    class ObjectLoader : IObserver<IDataObject>
    {
        private readonly IObjectsRepository _repository;
        private Action<IDataObject> _onLoadedAction;
        private bool _sent;
        private IDisposable _subscription;

        public ObjectLoader(IObjectsRepository repository)
        {
            _repository = repository;
        }

        public void Load(Guid id, Action<IDataObject> onLoadedAction)
        {
            _sent = false;
            _onLoadedAction = onLoadedAction;
            _subscription = _repository.SubscribeObjects(new[] {id}).Subscribe(this);
            //_repository.SubscribeObjects(new[] {id});
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
            
            _onLoadedAction(value);
            _sent = true;
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}

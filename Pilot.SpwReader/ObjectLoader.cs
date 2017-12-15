using System;

namespace Ascon.Pilot.SDK.CadReader
{
    class ObjectLoader : IObserver<IDataObject>
    {
        private readonly IObjectsRepository _repository;
        private Action<IDataObject> _onLoadedAction;
        private IDisposable _subscription;

        public ObjectLoader(IObjectsRepository repository)
        {
            _repository = repository;
        }

        public void Load(Guid id, Action<IDataObject> onLoadedAction)
        {
            _onLoadedAction = onLoadedAction;
            _subscription = _repository.SubscribeObjects(new[] {id}).Subscribe(this);
            _subscription.Dispose();
        }

        public void OnNext(IDataObject value)
        {
            if (value.State != DataState.Loaded) 
                return;
            
            _onLoadedAction(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}

using System;

namespace Ascon.Pilot.SDK.CadReader
{
    public class ObjectLoader : IObserver<IDataObject>
    {
        private readonly IObjectsRepository _repository;
        private Action<IDataObject> _onLoadedAction;
        private IDisposable _subscription;
        private bool _loaded;

        public ObjectLoader(IObjectsRepository repository)
        {
            _repository = repository;
        }

        public void Load(Guid id, Action<IDataObject> onLoadedAction)
        {
            _loaded = false;
            _onLoadedAction = onLoadedAction;
            _subscription = _repository.SubscribeObjects(new[] { id }).Subscribe(this);
        }


        public void OnNext(IDataObject value)
        {
            if (value.State != DataState.Loaded)
                return;

            if (_loaded)
                return;

            _loaded = true;

            SubscriptionDispose();
            _onLoadedAction(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void SubscriptionDispose()
        {
            try
            {
                _subscription?.Dispose();
            }
            catch
            {
                //ignor
            }

        }
    }
}

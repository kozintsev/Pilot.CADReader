using System;

namespace Ascon.Pilot.SDK.CadReader
{
    internal class CreateDocRreview : IObserver<IDataObject>
    {
        private readonly IObjectsRepository _repository;
        private Action<IDataObject> _onLoadedAction;
        private IDisposable _subscription;
        private bool _created;

        public CreateDocRreview(IObjectsRepository repository)
        {
            _repository = repository;
        }

        public void Create(Guid id, Action<IDataObject> onLoadedAction)
        {
            _created = false;
            _onLoadedAction = onLoadedAction;
            _subscription = _repository.SubscribeObjects(new[] { id }).Subscribe(this);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(IDataObject value)
        {
            if (_created)
                return;

            if (value.State == DataState.Loaded)
            {
                _created = true;
                SubscriptionDispose();
                _onLoadedAction(value);
            }
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

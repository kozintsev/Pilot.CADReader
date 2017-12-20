using System;
using Ascon.Pilot.SDK.CadReader.Spc;

namespace Ascon.Pilot.SDK.CadReader
{
    internal class CreateDocRreview : IObserver<IDataObject>
    {
        private readonly IObjectsRepository _repository;
        private readonly IObjectModifier _modifier;
        private Action<IDataObject> _onLoadedAction;
        private Specification _spc;
        private IDisposable _subscription;
        private bool _created;
        private Guid _parentId;

        public CreateDocRreview(IObjectsRepository repository, IObjectModifier modifier)
        {
            _repository = repository;
            _modifier = modifier;
        }

        public void Create(Specification spc, Guid parentId, Action<IDataObject> onLoadedAction)
        {
            _created = false;
            _spc = spc;
            _parentId = parentId;
            _onLoadedAction = onLoadedAction;
            _subscription = _repository.SubscribeObjects(new[] { spc.GlobalId }).Subscribe(this);
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

using System;

namespace Ascon.Pilot.SDK.CadReader
{
    class CreateDocRreview : IObserver<IDataObject>
    {
        private readonly IObjectsRepository _repository;
        private readonly IObjectModifier _modifier;
        private Action<Guid> _onLoadedAction;
        private IObjectBuilder _builder;
        private IDisposable _subscription;
        private bool _created;
        private Guid _parentId;

        public CreateDocRreview(IObjectsRepository repository, IObjectModifier modifier)
        {
            _repository = repository;
            _modifier = modifier;
        }

        public void Create(Guid parentId, Action<Guid> onLoadedAction)
        {
            _created = false;
            _parentId = parentId;
            _onLoadedAction = onLoadedAction;
            //_subscription = _repository.SubscribeObjects(new[] { _bimObject.GlobalId }).Subscribe(this);
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

            if (value.State == DataState.Loaded && value.ObjectStateInfo.State == ObjectState.DeletedPermanently)
            {
                _onLoadedAction(value.Id);
                return;
            }
            if (value.State == DataState.Loaded && value.ObjectStateInfo.State == ObjectState.InRecycleBin)
            {
                _created = true;
                _onLoadedAction(value.Id);
                return;
            }

            if (value.State == DataState.Loaded && value.ObjectStateInfo.State != ObjectState.DeletedPermanently && value.ObjectStateInfo.State != ObjectState.InRecycleBin)
            {
                _created = true;
                _onLoadedAction(value.Id);
                return;
            }
            if (value.State == DataState.NonExistent)
            {
                _created = true;
                _onLoadedAction(value.Id);
            }
        }
    }
}

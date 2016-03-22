using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascon.Pilot.SDK.SpwReader
{
    class LoaderOfObjects : IObserver<IDataObject[]>
    {
        private readonly IObjectsRepository _repository;
        private Action<IDataObject[]> _onLoadedAction;
        private bool _sent;

        public LoaderOfObjects(IObjectsRepository repository)
        {
            _repository = repository;
        }

        public void Load(IEnumerable<Guid> ids , Action<IDataObject[]> onLoadedAction)
        {
            _sent = false;
            _onLoadedAction = onLoadedAction;
            _repository.SubscribeObjects(ids);
        }
        public void OnNext(IDataObject[] values)
        {
            if (_sent)
            {
                return;
            }
            
            if (values == null)
                return;
            var count = values.Count();
            var i = values.Count(value => value.State == DataState.Loaded);

            if (i != count)
                return;

            _onLoadedAction(values);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
namespace Ascon.Pilot.SDK.CadReader
{
    public interface IAsyncMethods
    {
        Task<IEnumerable<T>> GetObjectsAsync<T>(IEnumerable<Guid> ids, Func<IDataObject, T> converter, CancellationToken ct);
        Task<IEnumerable<T>> GetTasksAsync<T>(IEnumerable<Guid> ids, Func<ITaskObject, T> converter, CancellationToken ct);
    }

    public class AsyncMethods : IAsyncMethods
    {
        private readonly IObjectsRepository _repository;

        public AsyncMethods(IObjectsRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns objects by identifiers as an asynchronous operation
        /// </summary>
        /// <typeparam name="T">Type of extension-side IDataObject projection</typeparam>
        /// <param name="ids">Object identifiers to be loaded</param>
        /// <param name="converter">Сonverts IDataObject to the extension-side projection. 
        /// IDataObject belongs to the application AppDomain and can be used for 2 minutes only after being created, 
        /// so it should be converted to the extension-side projection immedeately after being received.</param>
        /// <param name="ct">CancellationToken to cancel objects loading. The OperationCanceledException will be thrown.</param>
        /// <returns>The task object representing the asynchronous operation</returns>
        public Task<IEnumerable<T>> GetObjectsAsync<T>(IEnumerable<Guid> ids, Func<IDataObject, T> converter, CancellationToken ct)
        {
            // Creates an observable that fires notification when cancelling CancellationToken
            var cancel = Observable.Create<T>(o => ct.Register(o.OnCompleted));

            var loading = ids.ToList();
            var observableList = _repository
                .SubscribeObjects(loading)                          // Subscribing on interested objects
                .TakeUntil(cancel)                                  // Stopping subscription on cancel
                .ObserveOnDispatcher(DispatcherPriority.Background) // Forcing notifications to be raised on UI thread with Background priority
                .Where(o => o.State == DataState.Loaded)            // Filtering "NoData" notifications
                .Distinct(o => o.Id)                                // Filtering already emitted notifications
                .Select(converter)                                  // Converting IDataObject to extension-side projection
                .Take(loading.Count)                                // Wait for all objects to be loaded
                .ToList();

            return Task<IEnumerable<T>>.Factory.StartNew(() =>
            {
                var lazy = observableList.Wait();
                // Forces the "lazy" enumerable to be immedeately iterated in background thread
                return lazy.ToList();
            }, ct);
        }

        /// <summary>
        /// Returns tasks by identifiers as an asynchronous operation
        /// </summary>
        /// <typeparam name="T">Type of extension-side ITaskObject projection</typeparam>
        /// <param name="ids">Task identifiers to be loaded</param>
        /// <param name="converter">Сonverts ITaskObject to the extension-side projection. 
        /// ITaskObject belongs to the application AppDomain and can be used for 2 minutes only after being created, 
        /// so it should be converted to the extension-side projection immedeately after being received.</param>
        /// <param name="ct">CancellationToken to cancel tasks loading. The OperationCanceledException will be thrown.</param>
        /// <returns>The task object representing the asynchronous operation</returns>
        public Task<IEnumerable<T>> GetTasksAsync<T>(IEnumerable<Guid> ids, Func<ITaskObject, T> converter, CancellationToken ct)
        {
            // Creates an observable that fires notification when cancelling CancellationToken
            var cancel = Observable.Create<T>(o => ct.Register(o.OnCompleted));

            var loading = ids.ToList();
            var observableList = _repository
                .SubscribeTasks(loading)                            // Subscribing on interested tasks
                .TakeUntil(cancel)                                  // Stopping subscription on cancel
                .ObserveOnDispatcher(DispatcherPriority.Background) // Forcing notifications to be raised on UI thread with Background priority
                .Distinct(o => o.Id)                                // Filtering already emitted notifications
                .Select(converter)                                  // Converting ITaskObject to extension-side projection
                .Take(loading.Count)                                // Wait for all objects to be loaded
                .ToList();

            return Task<IEnumerable<T>>.Factory.StartNew(() =>
            {
                var lazy = observableList.Wait();
                // Forces the "lazy" enumerable to be immedeately iterated in background thread
                return lazy.ToList();
            }, ct);
        }
    }
}

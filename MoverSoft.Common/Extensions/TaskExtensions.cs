namespace MoverSoft.Common.Extensions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static Task<T[]> WhenAllForAwait<T>(this IEnumerable<Task<T>> tasks)
        {
            return Task.WhenAll(tasks).WrapMultipleExceptionsForAwait();
        }

        public static Task WhenAllForAwait(this IEnumerable<Task> tasks)
        {
            return Task.WhenAll(tasks).WrapMultipleExceptionsForAwait();
        }

        public static Task WrapMultipleExceptionsForAwait(this Task task)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            task.ContinueWith(
                continuationAction: ignored => TaskExtensions.CompleteTaskAndWrapMultipleExceptions(task, tcs),
                continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
                cancellationToken: CancellationToken.None,
                scheduler: TaskScheduler.Default);

            return tcs.Task;
        }

        public static Task<T> WrapMultipleExceptionsForAwait<T>(this Task<T> task)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            task.ContinueWith(
                continuationAction: ignored => TaskExtensions.CompleteTaskAndWrapMultipleExceptions(task, tcs),
                continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
                cancellationToken: CancellationToken.None,
                scheduler: TaskScheduler.Default);

            return tcs.Task;
        }

        private static void CompleteTaskAndWrapMultipleExceptions<T>(Task task, TaskCompletionSource<T> completionSource)
        {
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    completionSource.SetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    var genericTask = task as Task<T>;
                    completionSource.SetResult(genericTask != null ? genericTask.Result : default(T));
                    break;
                case TaskStatus.Faulted:
                    completionSource.SetException(task.Exception.InnerExceptions.Count > 1 ? task.Exception : task.Exception.InnerException);
                    break;
            }
        }
    }
}

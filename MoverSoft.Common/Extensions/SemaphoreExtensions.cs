namespace MoverSoft.Common.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class SemaphoreExtensions
    {
        /// <summary>
        /// Performs the action inside the locking of a semaphore.
        /// </summary>
        /// <param name="semaphore">The semaphore used for locking.</param>
        /// <param name="action">The action to perform.</param>
        public static async Task ThrottleAction(this SemaphoreSlim semaphore, Func<Task> action)
        {
            await semaphore.WaitAsync();

            await action();

            semaphore.Release();
        }

        /// <summary>
        /// Performs the action inside the locking of a semaphore.
        /// </summary>
        /// <typeparam name="T">The return type of the action.</typeparam>
        /// <param name="semaphore">The semaphore used for locking.</param>
        /// <param name="action">The action to perform.</param>
        public static async Task<T> ThrottleAction<T>(this SemaphoreSlim semaphore, Func<Task<T>> action)
        {
            await semaphore.WaitAsync();

            var result = await action();

            semaphore.Release();

            return result;
        }
    }
}

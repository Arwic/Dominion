// Dominion - Copyright (C) Timothy Ings
// AsyncExtensions.cs
// This file defines extension methods to async related classes

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArwicEngine.Core
{
    public static class AsyncExtensions
    {
        /// <summary>
        /// Runs a task with a timeout
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static Task<T> WithTimeout<T>(this Task<T> task, int duration)
        {
            return Task.Factory.StartNew(() =>
            {
                bool b = task.Wait(duration);
                if (b) return task.Result;
                return default(T);
            });
        }

        /// <summary>
        /// Runs a task with a cancelation token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                        s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            return await task;
        }
    }
}

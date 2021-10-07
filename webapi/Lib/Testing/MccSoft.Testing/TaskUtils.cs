using System;
using System.Threading.Tasks;

namespace MccSoft.Testing
{
    public static class TaskUtils
    {
        /// <summary>
        /// Runs the asynchronous callback synchronously. Exists to work around xUnit deadlocks in
        /// calls like <c>task.Wait()</c> or <c>task.Result</c>.
        /// </summary>
        /// <remarks>
        /// xUnit uses its own SynchronizationContext that allows a maximum thread count
        /// equal to the logical CPU count. So when we're trying to call something like
        /// <c>task.Wait()</c> or <c>task.Result</c>,
        /// the task gets scheduled to the xUnit's synchronization context, which may be already
        /// at its limit running the test thread, so we end up in a deadlock.
        /// Blocking calls should be avoided where possible. Sync over async (blocking a thread)
        /// is a bad practice.
        /// Use this workaround only if alternative implementation is difficult (e.g. to await
        /// a task in a constructor).
        /// </remarks>
        /// <typeparam name="T">The type of the task result.</typeparam>
        /// <param name="work">The callback that needs to be executed synchronously.</param>
        /// <returns>The result of the task returned by the callback.</returns>
        public static T RunSynchronously<T>(Func<Task<T>> work)
        {
            // The workaround for xUnit deadlock is to explicitly schedule the task
            // to the thread pool, avoiding xUnit's sync context.
            return Task.Run(work).GetAwaiter().GetResult();
        }
    }
}

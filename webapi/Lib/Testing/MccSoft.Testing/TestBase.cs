using MccSoft.Testing.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MccSoft.Testing
{
    /// <summary>
    /// A helper test class. Inherit from it to use <see cref="MotherFactory"/>.
    /// </summary>
    public class TestBase
    {
        /// <summary>
        /// The entity factory.
        /// The non-conventional name is chosen to read as an English article:
        /// a.DomainClass(...args...)
        /// For classes starting with a vowel use <see cref="an"/>.
        /// </summary>
        public static MotherFactory a = null;

        /// <summary>
        /// The entity factory.
        /// The non-conventional name is chosen to read as an English article:
        /// an.Entity(...args...)
        /// For classes starting with a consonant use <see cref="a"/>.
        /// </summary>
        public static MotherFactory an = null;

        /// <summary>
        /// Waits that message was consumed
        /// </summary>
        /// <param name="taskCompletionSource">CompletionSource which definies then the message was consumed</param>
        /// <param name="timeout">Timeout in ms</param>
        protected virtual async Task WaitForResultConsumed(
            TaskCompletionSource<bool> taskCompletionSource,
            int timeout = 1000
        )
        {
            var taskList = new List<Task> { taskCompletionSource.Task, Task.Delay(timeout) };
            var finishedTask = await Task.WhenAny(taskList);
            if (!(finishedTask is Task<bool>))
            {
                throw new Exception("Timeout: Message was not consumed");
            }
        }
    }
}

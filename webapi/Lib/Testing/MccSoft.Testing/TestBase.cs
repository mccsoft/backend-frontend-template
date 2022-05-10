using MccSoft.Testing.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.Testing
{
    /// <summary>
    /// A helper test class. Inherit from it to use <see cref="MotherFactory"/> and DbContext utilities.
    /// </summary>
    public abstract class TestBase<TDbContext> where TDbContext : DbContext
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

        #region WithDbContext

        protected abstract TDbContext CreateDbContext();

        protected void WithDbContext(Action<TDbContext> action)
        {
            var db = CreateDbContext();
            action(db);
        }

        protected T WithDbContext<T>(Func<TDbContext, T> action)
        {
            var db = CreateDbContext();
            var result = action(db);

            return result;
        }

        protected async Task<T> WithDbContext<T>(Func<TDbContext, Task<T>> action)
        {
            var db = CreateDbContext();
            return await action(db);
        }

        protected async Task WithDbContext(Func<TDbContext, Task> action)
        {
            var db = CreateDbContext();
            await action(db);
        }

        #endregion


    }
}

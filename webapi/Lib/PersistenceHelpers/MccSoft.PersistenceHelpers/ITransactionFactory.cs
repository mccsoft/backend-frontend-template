using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace MccSoft.PersistenceHelpers
{
    /// <summary>
    /// Represents an object that can start a transaction.
    /// </summary>
    public interface ITransactionFactory
    {
        /// <summary>
        /// Starts a new transaction.
        /// </summary>
        /// <returns>
        /// A <see cref="IDbContextTransaction" /> that represents the started transaction.
        /// </returns>
        IDbContextTransaction BeginTransaction();

        /// <summary>
        /// Asynchronously starts a new transaction.
        /// </summary>
        /// <returns>
        /// A <see cref="IDbContextTransaction" /> that represents the started transaction.
        /// </returns>
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}

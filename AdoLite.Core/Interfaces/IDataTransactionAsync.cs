using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    /// <summary>
    /// Defines asynchronous data transaction operations.
    /// </summary>
    public interface IDataTransactionAsync
    {
        /// <summary>
        /// Executes a list of SQL command patterns asynchronously within a transaction.
        /// If any command fails, the entire transaction is rolled back.
        /// </summary>
        /// <param name="queryPatterns">
        /// A list of SQL command patterns to be executed. Each pattern includes the query and its parameters.
        /// </param>
        /// <param name="commandTimeoutSeconds">
        /// Optional command timeout in seconds for each SQL command. Default is 30 seconds.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The result is true if all commands executed and committed successfully; otherwise, false.
        /// </returns>
        Task<bool> SaveChangesAsync(
            List<IQueryPattern> queryPatterns,
             int commandTimeoutSeconds = 30,
            CancellationToken cancellationToken = default
           );
    }
}

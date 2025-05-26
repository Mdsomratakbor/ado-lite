using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataTransactionAsync
    {
        /// <summary>
        /// Executes a list of SQL command patterns asynchronously in a transaction. Rolls back if any fail.
        /// </summary>
        /// <param name="queryPatterns">A list of SQL command patterns.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates success or failure.</returns>
        Task<bool> SaveChangesAsync(List<IQueryPattern> queryPatterns);
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataQueryAsync
    {/// <summary>
     /// Executes a SQL query asynchronously and returns the result as a DataTable.
     /// </summary>
     /// <param name="query">The SQL query to execute.</param>
     /// <param name="parameters">Optional query parameters.</param>
     /// <returns>A task that represents the asynchronous operation. The task result contains a DataTable.</returns>
        Task<DataTable> GetDataTableAsync(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query asynchronously and returns the result as a DataSet.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DataSet.</returns>
        Task<DataSet> GetDataSetAsync(string query, Dictionary<string, string> parameters);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a single DataRow.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DataRow.</returns>
        Task<DataRow> GetDataRowAsync(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a single value of type T.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a value of type T.</returns>
        Task<T> GetSingleValueAsync<T>(string query, Dictionary<string, string> parameters = null);
    }
}

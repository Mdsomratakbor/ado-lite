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


        /// <summary>
        /// Executes a SQL query asynchronously and maps the first row of the result to an object of type T.
        /// </summary>
        Task<T> GetSingleRecordAsync<T>(string query, Dictionary<string, string> parameters = null) where T : new();

        /// <summary>
        /// Executes a SQL query asynchronously and returns a list of values from a single column.
        /// </summary>
        Task<List<T>> GetListAsync<T>(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query asynchronously and returns the count of rows.
        /// </summary>
        Task<int> GetCountAsync(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query asynchronously and checks if any row exists.
        /// </summary>
        Task<bool> ExistsAsync(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a paged subset of data as a DataTable.
        /// </summary>
        Task<DataTable> GetPagedDataTableAsync(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a dictionary of key-value pairs.
        /// </summary>
        Task<Dictionary<TKey, TValue>> GetDictionaryAsync<TKey, TValue>(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a list of objects using a mapping function.
        /// </summary>
        Task<List<T>> GetMappedListAsync<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null);

    }
}

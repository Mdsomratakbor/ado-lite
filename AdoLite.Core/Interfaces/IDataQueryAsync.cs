using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataQueryAsync
    {
        /// <summary>
        /// Executes a SQL query asynchronously and returns the result as a DataTable.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DataTable with the query results.</returns>
        Task<DataTable> GetDataTableAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and returns the result as a DataSet.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DataSet with one or more DataTables.</returns>
        Task<DataSet> GetDataSetAsync(string query, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a single DataRow.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a single DataRow from the result set.</returns>
        Task<DataRow> GetDataRowAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a single scalar value of type T.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a single value of type T.</returns>
        Task<T> GetSingleValueAsync<T>(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and maps the first row of the result to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an object of type T, or default if no data is found.</returns>
        Task<T> GetSingleRecordAsync<T>(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default) where T : new();

        /// <summary>
        /// Executes a SQL query asynchronously and returns a list of values from a single column.
        /// </summary>
        /// <typeparam name="T">The type of the values to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of values of type T.</returns>
        Task<List<T>> GetListAsync<T>(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and returns the count of rows.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of rows returned by the query.</returns>
        Task<int> GetCountAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and checks if any row exists.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if at least one row exists, otherwise false.</returns>
        Task<bool> ExistsAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a paged subset of data as a DataTable.
        /// </summary>
        /// <param name="query">The SQL query to execute with paging applied.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="pageNumber">Page number starting from 1.</param>
        /// <param name="pageSize">Number of rows per page.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DataTable with the paged result.</returns>
        Task<DataTable> GetPagedDataTableAsync(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a dictionary of key-value pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
        /// <param name="query">The SQL query expected to return two columns (key and value).</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary of key-value pairs.</returns>
        Task<Dictionary<TKey, TValue>> GetDictionaryAsync<TKey, TValue>(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query asynchronously and returns a list of objects using a mapping function.
        /// </summary>
        /// <typeparam name="T">The type of the objects to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="mapFunc">A function that maps a DataRow to an object of type T.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of mapped objects.</returns>
        Task<List<T>> GetMappedListAsync<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default);
    }
}

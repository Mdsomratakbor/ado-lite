using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataQuery
    {
        /// <summary>
        /// Executes a SQL query and returns the result as a DataTable.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>A DataTable containing the result set.</returns>
        DataTable GetDataTable(string query, Dictionary<string, string> parameters = null);


        /// <summary>
        /// Executes a SQL query and maps the first row of the result to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the model to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>An object of type T representing the first row of the result, or default(T) if no data is found.</returns>
        T GetSingleRecord<T>(string query, Dictionary<string, string> parameter = null) where T : new();

        /// <summary>
        /// Executes a SQL query and returns the result as a DataSet.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <returns>A DataSet containing one or more DataTables.</returns>
        DataSet GetDataSet(string query, Dictionary<string, string> parameters);

        /// <summary>
        /// Executes a SQL query and maps the result set to a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of the model to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>A list of objects of type T representing the result set.</returns>
        List<T> GetRecordList<T>(string query, Dictionary<string, string> parameter = null);
        /// <summary>
        /// Executes a SQL query and returns a single DataRow.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>A single DataRow.</returns>
        DataRow GetDataRow(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query and returns a single value of type T.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>A single value of type T.</returns>
        T GetSingleValue<T>(string query, Dictionary<string, string> parameters = null);


        /// <summary>
        /// Gets a list of values of type T from a single column query.
        /// </summary>
        /// <typeparam name="T">The type of values to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>List of values of type T.</returns>
        List<T> GetList<T>(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Gets the count of rows returned by the query.
        /// </summary>
        /// <param name="query">The SQL query to count rows.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>Number of rows.</returns>
        int GetCount(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Checks if any row exists matching the query.
        /// </summary>
        /// <param name="query">The SQL query to check existence.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>True if at least one row exists, otherwise false.</returns>
        bool Exists(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Gets a paged subset of data as a DataTable.
        /// </summary>
        /// <param name="query">The SQL query to execute with paging applied.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="pageNumber">Page number starting from 1.</param>
        /// <param name="pageSize">Number of rows per page.</param>
        /// <returns>DataTable with the paged result.</returns>
        DataTable GetPagedDataTable(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize);

        /// <summary>
        /// Gets a dictionary where each row is represented as key-value pairs, with the key column and value column specified.
        /// </summary>
        /// <typeparam name="TKey">Type of dictionary key.</typeparam>
        /// <typeparam name="TValue">Type of dictionary value.</typeparam>
        /// <param name="query">The SQL query expected to return two columns (key and value).</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>Dictionary of key-value pairs.</returns>
        Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Gets a list of strongly typed objects from the query result using a mapper function.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="mapFunc">A function that maps a DataRow to an object of type T.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>List of mapped objects.</returns>
        List<T> GetMappedList<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null);
    }
}

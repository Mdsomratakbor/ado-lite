using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataTransaction
    {
        /// <summary>
        /// Creates a query pattern for a SQL command with parameters.
        /// </summary>
        /// <param name="query">The SQL command.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>An instance of IQueryPattern.</returns>
        IQueryPattern AddQuery(string query, Dictionary<string, object> parameters);

        /// <summary>
        /// Generates a dictionary of SQL parameters from an array of values.
        /// </summary>
        /// <param name="values">An array of parameter values.</param>
        /// <returns>A dictionary of parameter names and values.</returns>
        Dictionary<string, object> AddParameters(string[] values = null);

        /// <summary>
        /// Executes a list of SQL command patterns in a transaction. Rolls back if any fail.
        /// </summary>
        /// <param name="queryPatterns">A list of SQL command patterns.</param>
        /// <returns>True if all commands succeed, otherwise false.</returns>
        bool SaveChanges(List<IQueryPattern> queryPatterns);
        /// <summary>
        /// Executes a raw SQL command that does not return a result set (e.g., DDL or setup scripts).
        /// </summary>
        /// <param name="query">The raw SQL command to execute.</param>
        void ExecuteRawSql(string query);
        /// <summary>
        /// Performs a bulk insert into the specified table using a DataTable.
        /// </summary>
        /// <param name="tableName">Target table name.</param>
        /// <param name="dataTable">DataTable containing rows to insert.</param>
        void BulkInsert(string tableName, DataTable dataTable);

        /// <summary>
        /// Performs a bulk insert into the specified table using a list of typed objects.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="tableName">Target table name.</param>
        /// <param name="dataList">List of objects to insert.</param>
        void BulkInsert<T>(string tableName, List<T> dataList);

        /// <summary>
        /// Performs a bulk insert into the specified table from a JSON file.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="tableName">Target table name.</param>
        /// <param name="jsonFilePath">Path to the JSON file.</param>
        void BulkInsertFromJson<T>(string tableName, string jsonFilePath);

        /// <summary>
        /// Performs a bulk insert into the specified table from a CSV file.
        /// </summary>
        /// <param name="tableName">Target table name.</param>
        /// <param name="csvFilePath">Path to the CSV file.</param>
        void BulkInsertFromCsv(string tableName, string csvFilePath);
    }
}

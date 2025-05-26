using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataQuery
    {/// <summary>
     /// Executes a SQL query and returns the result as a DataTable.
     /// </summary>
     /// <param name="query">The SQL query to execute.</param>
     /// <param name="parameters">Optional query parameters.</param>
     /// <returns>A DataTable containing the result set.</returns>
        DataTable GetDataTable(string query, Dictionary<string, string> parameters = null);

        /// <summary>
        /// Executes a SQL query and returns the result as a DataSet.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Query parameters.</param>
        /// <returns>A DataSet containing one or more DataTables.</returns>
        DataSet GetDataSet(string query, Dictionary<string, string> parameters);

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
    }
}

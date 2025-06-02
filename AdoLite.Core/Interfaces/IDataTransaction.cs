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
    }
}

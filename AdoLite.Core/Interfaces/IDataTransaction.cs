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
        IQueryPattern CreateQuery(string query, Dictionary<string, object> parameters);

        /// <summary>
        /// Generates a dictionary of SQL parameters from an array of values.
        /// </summary>
        /// <param name="values">An array of parameter values.</param>
        /// <returns>A dictionary of parameter names and values.</returns>
        Dictionary<string, string> CreateParameters(string[] values = null);

        /// <summary>
        /// Executes a list of SQL command patterns in a transaction. Rolls back if any fail.
        /// </summary>
        /// <param name="queryPatterns">A list of SQL command patterns.</param>
        /// <returns>True if all commands succeed, otherwise false.</returns>
        bool SaveChanges(List<IQueryPattern> queryPatterns);
    }
}

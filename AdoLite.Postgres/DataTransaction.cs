using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Base;
using AdoLite.Core.Interfaces;
using Npgsql;

namespace AdoLite.Postgres
{
    public partial class DataQuery : IDataTransaction
    {
        public IQueryPattern _queryPattern;
        public Dictionary<string, string> AddParameters(string[] values = null)
        {
            var parameters = new Dictionary<string, string>();

            if (values != null && values.Length > 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    // prefers parameters like @param1, @param2
                    parameters.Add($"@param{i + 1}", values[i]);
                }
            }

            return parameters;
        }


        public IQueryPattern AddQuery(string query, Dictionary<string, object> parameters)
        {
            _queryPattern = new QueryPattern();
            _queryPattern.Query = query;
            _queryPattern.Parameters.Add(parameters);
            return _queryPattern;
        }

        /// <summary>
        /// Saves changes to the database using a list of query patterns.
        /// </summary>
        /// <param name="queryPatterns">List of query patterns to be executed.</param>
        /// <returns>A boolean indicating whether the operation was successful.</returns>
        public bool SaveChanges(List<IQueryPattern> queryPatterns)
        {
            try
            {
                    NpgsqlTransaction transaction;
                    transaction = _connection.BeginTransaction(); // Begin transaction
                    try
                    {
                        using (NpgsqlCommand cmd = _connection.CreateCommand()) // PostgreSQL command
                        {
                            cmd.Transaction = transaction;
                            foreach (var data in queryPatterns)
                            {
                                cmd.CommandText = data.Query;
                                if (data.Parameters.Count > 0 && data.Parameters != null)
                                {
                                    cmd.Parameters.Clear();
                                    foreach (var parameter in data.Parameters)
                                    {
                                        foreach (var item in parameter)
                                        {
                                            cmd.Parameters.AddWithValue(item.Key, item.Value); // Add parameters to the command
                                        }
                                    }
                                }

                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit(); // Commit the transaction
                        }
                    }
                    catch (Exception ex1)
                    {
                        transaction.Rollback(); // Rollback in case of an error
                        throw ex1;
                    }
                
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
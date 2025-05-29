using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using AdoLite.Core.Base;
using AdoLite.Core.Interfaces;

namespace AdoLite.SqlServer
{
    public partial class DataQuery : IDataTransaction
    {
        public IQueryPattern _queryPattern;

        public Dictionary<string, string> AddParameters(string[] values = null)
        {
            var parameter = new Dictionary<string, string>();
            int i = 1;
            if (values != null && values.Length > 0)
            {
                foreach (var data in values)
                {
                    parameter.Add($"@param{i}", data); // SQL Server also uses '@' for parameters
                    i++;
                }
            }
            return parameter;
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
        
                    using (SqlTransaction transaction = _connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = _connection.CreateCommand())
                            {
                                cmd.Transaction = transaction;

                                foreach (var data in queryPatterns)
                                {
                                    cmd.CommandText = data.Query;
                                    cmd.Parameters.Clear();

                                    if (data.Parameters != null && data.Parameters.Count > 0)
                                    {
                                        foreach (var parameterDict in data.Parameters)
                                        {
                                            foreach (var param in parameterDict)
                                            {
                                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                                            }
                                        }
                                    }

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

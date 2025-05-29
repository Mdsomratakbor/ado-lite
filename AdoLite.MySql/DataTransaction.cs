using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using AdoLite.Core.Base;
using AdoLite.Core.Interfaces;

namespace AdoLite.MySql
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
                    parameter.Add($"@param{i}", data); // MySQL also uses '@' for named parameters
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

        public bool SaveChanges(List<IQueryPattern> queryPatterns)
        {
            try
            {
                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = _connection.CreateCommand())
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

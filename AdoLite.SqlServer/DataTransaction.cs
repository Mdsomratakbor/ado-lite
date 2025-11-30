using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AdoLite.Core.Base;
using AdoLite.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AdoLite.SqlServer
{
    public partial class DataQuery : IDataTransaction
    {
        public IQueryPattern? _queryPattern;

        public Dictionary<string, object> AddParameters(string[] values = null)
        {
            var parameter = new Dictionary<string, object>();
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
            if (queryPatterns == null) throw new ArgumentNullException(nameof(queryPatterns));
            using var connection = CreateAndOpenConnection();
            using SqlTransaction transaction = connection.BeginTransaction();
            var sw = Stopwatch.StartNew();
            try
            {
                using SqlCommand cmd = connection.CreateCommand();
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
                                object value = param.Value;

                                if (value is SqlParameter sqlParam)
                                {
                                    var p = cmd.Parameters.Add(sqlParam.ParameterName ?? param.Key, sqlParam.SqlDbType);
                                    if (!string.IsNullOrWhiteSpace(sqlParam.TypeName))
                                    {
                                        p.TypeName = sqlParam.TypeName;
                                    }
                                    p.Direction = sqlParam.Direction;
                                    p.Size = sqlParam.Size;
                                    p.Value = sqlParam.Value ?? DBNull.Value;
                                    continue;
                                }

                                cmd.Parameters.AddWithValue(param.Key, value ?? DBNull.Value);
                            }
                        }
                    }

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                sw.Stop();
                _logger?.LogInformation(
                    "{Operation} executed {Count} statements in {ElapsedMs}ms | SqlBatch={@Batch}",
                    nameof(SaveChanges),
                    queryPatterns.Count,
                    sw.ElapsedMilliseconds,
                    queryPatterns.Select(q => TrimSqlForLog(q.Query)).ToList());

                return true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    _logger?.LogError(rollbackEx, "Failed to rollback transaction for {Operation}", nameof(SaveChanges));
                }

                _logger?.LogError(
                    ex,
                    "{Operation} failed in {ElapsedMs}ms | SqlBatch={@Batch}",
                    nameof(SaveChanges),
                    sw.ElapsedMilliseconds,
                    queryPatterns?.Select(q => TrimSqlForLog(q.Query)).ToList());
                throw;
            }
        }


        public void ExecuteRawSql(string query)
        {
            using var connection = CreateAndOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            var sw = Stopwatch.StartNew();
            try
            {
                cmd.ExecuteNonQuery();
                sw.Stop();
                LogSuccess(nameof(ExecuteRawSql), query, null, sw.ElapsedMilliseconds, 0);
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(ExecuteRawSql), query, null, sw.ElapsedMilliseconds, ex);
                throw;
            }
        }


        public void BulkInsert(string tableName, DataTable dataTable)
        {
            using var connection = CreateAndOpenConnection();
            using var bulkCopy = new SqlBulkCopy(connection);
            bulkCopy.DestinationTableName = tableName;
            bulkCopy.WriteToServer(dataTable);
        }

    
        public void BulkInsert<T>(string tableName, List<T> dataList)
        {
            var table = ToDataTable(dataList);
            BulkInsert(tableName, table);
        }

     

        public void BulkInsertFromJson<T>(string tableName, string jsonFilePath)
        {
            var jsonData = File.ReadAllText(jsonFilePath);
            var dataList = JsonConvert.DeserializeObject<List<T>>(jsonData);
            BulkInsert(tableName, ToDataTable(dataList));
        }

    
        public void BulkInsertFromCsv(string tableName, string csvFilePath)
        {
            var table = CsvToDataTable(csvFilePath);
            BulkInsert(tableName, table);
        }

      

        private DataTable ToDataTable<T>(List<T> data)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in data)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        private DataTable CsvToDataTable(string csvFilePath)
        {
            var dt = new DataTable();
            using (var reader = new StreamReader(csvFilePath))
            {
                bool isHeader = true;
                string[] headers = null;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (isHeader)
                    {
                        headers = values;
                        foreach (var header in headers)
                        {
                            dt.Columns.Add(header);
                        }
                        isHeader = false;
                    }
                    else
                    {
                        dt.Rows.Add(values);
                    }
                }
            }
            return dt;
        }
    }
}

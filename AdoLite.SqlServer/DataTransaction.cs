using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using AdoLite.Core.Base;
using AdoLite.Core.Interfaces;
using Newtonsoft.Json;

namespace AdoLite.SqlServer
{
    public partial class DataQuery : IDataTransaction
    {
        public IQueryPattern _queryPattern;

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
                                    //foreach (var parameterDict in data.Parameters)
                                    //{
                                    //    foreach (var param in parameterDict)
                                    //    {
                                    //        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                                    //    }
                                    //}

                                    foreach (var parameterDict in data.Parameters)
                                    {
                                        foreach (var param in parameterDict)
                                        {
                                            object value = param.Value;

                                            // If the value itself is a SqlParameter, unwrap its actual value
                                            if (value is SqlParameter sqlParam)
                                            {
                                                value = sqlParam.Value ?? DBNull.Value;
                                            }

                                            cmd.Parameters.AddWithValue(param.Key, value ?? DBNull.Value);
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


        public void ExecuteRawSql(string query)
        {
            using (SqlCommand cmd = _connection.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
        }


        public void BulkInsert(string tableName, DataTable dataTable)
        {
            using (var bulkCopy = new SqlBulkCopy(_connection))
            {
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.WriteToServer(dataTable);
            }
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

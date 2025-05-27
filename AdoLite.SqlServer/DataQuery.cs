using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using AdoLite.Core.Interfaces;
using System.Reflection;

namespace AdoLite.SqlServer
{
    /// <summary>
    /// Provides SQL Server-specific implementations of data querying operations.
    /// </summary>
    public partial class DataQuery(string connectionString) : IDataQuery
    {
        private readonly string _databaseConnection = connectionString;

        /// <summary>
        /// Gets a single row of data from the database.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>A DataRow object representing the retrieved row.</returns>
        public virtual DataRow GetDataRow(string query, Dictionary<string, string> parameter = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_databaseConnection)) // SQL Server connection
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection)) // SQL Server data adapter
                    {
                        if (parameter != null && parameter.Count > 0)
                        {
                            foreach (var item in parameter)
                            {
                                dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }

                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                            return dataTable.Rows[0];
                        else
                            return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Executes a SQL query and maps the first row of the result to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the model to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>An object of type T representing the first row of the result, or default(T) if no data is found.</returns>
        public virtual T GetSingleRecord<T>(string query, Dictionary<string, string> parameter = null) where T : new()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_databaseConnection))
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
                    {
                        if (parameter != null && parameter.Count > 0)
                        {
                            foreach (var item in parameter)
                            {
                                dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }

                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        if (dataTable.Rows.Count == 0)
                            return default;

                        DataRow row = dataTable.Rows[0];
                        T result = new T();

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            PropertyInfo prop = typeof(T).GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (prop != null && row[column] != DBNull.Value)
                            {
                                prop.SetValue(result, Convert.ChangeType(row[column], prop.PropertyType));
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a DataSet from the database for the given query.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>A DataSet object containing the data.</returns>
        public virtual DataSet GetDataSet(string query, Dictionary<string, string> parameter)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_databaseConnection)) // SQL Server connection
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection)) // SQL Server data adapter
                    {
                        if (parameter != null && parameter.Count > 0)
                        {
                            foreach (var item in parameter)
                            {
                                dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }

                        DataSet dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Executes a SQL query and maps the result set to a list of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of the model to return.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>A list of objects of type T representing the result set.</returns>
        public virtual List<T> GetRecordList<T>(string query, Dictionary<string, string> parameter = null) where T : new()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_databaseConnection))
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
                    {
                        if (parameter != null && parameter.Count > 0)
                        {
                            foreach (var item in parameter)
                            {
                                dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }

                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        List<T> resultList = new List<T>();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            T obj = new T();
                            foreach (DataColumn column in dataTable.Columns)
                            {
                                PropertyInfo prop = typeof(T).GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                                if (prop != null && row[column] != DBNull.Value)
                                {
                                    prop.SetValue(obj, Convert.ChangeType(row[column], prop.PropertyType));
                                }
                            }
                            resultList.Add(obj);
                        }

                        return resultList;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a DataTable from the database for the given query.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>A DataTable object containing the data.</returns>
        public virtual DataTable GetDataTable(string query, Dictionary<string, string> parameter = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_databaseConnection)) // SQL Server connection
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection)) // SQL Server data adapter
                    {
                        if (parameter != null && parameter.Count > 0)
                        {
                            foreach (var item in parameter)
                            {
                                dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }

                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a single value from the database query.
        /// </summary>
        /// <typeparam name="T">The type of data to return.</typeparam>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameter">Parameters for the query.</param>
        /// <returns>A single value of type T.</returns>
        public virtual T GetSingleValue<T>(string query, Dictionary<string, string> parameter = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_databaseConnection)) // SQL Server connection
                {
                    using (SqlCommand command = new SqlCommand(query, connection)) // SQL Server command
                    {
                        if (parameter != null && parameter.Count > 0)
                        {
                            foreach (var item in parameter)
                            {
                                command.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }

                        connection.Open();
                        object result = command.ExecuteScalar();
                        connection.Close();

                        if (result != null && result != DBNull.Value)
                            return (T)Convert.ChangeType(result, typeof(T));
                        else
                            return default;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public List<T> GetList<T>(string query, Dictionary<string, string> parameters = null)
        {
            try
            {
                var list = new List<T>();
                var dt = GetDataTable(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    // Assume single column in result for conversion
                    object val = row[0];
                    if (val == DBNull.Value)
                        list.Add(default);
                    else
                        list.Add((T)Convert.ChangeType(val, typeof(T)));
                }

                return list;
            }
            catch
            {
                throw;
            }
        }

        public int GetCount(string query, Dictionary<string, string> parameters = null)
        {
            // We can use GetSingleValue<int> because count(*) returns an int
            return GetSingleValue<int>(query, parameters);
        }

        public bool Exists(string query, Dictionary<string, string> parameters = null)
        {
            try
            {
                using var connection = new SqlConnection(_databaseConnection);
                using var command = new SqlCommand(query, connection);

                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var item in parameters)
                        command.Parameters.AddWithValue(item.Key, item.Value);
                }

                connection.Open();
                using var reader = command.ExecuteReader();
                bool exists = reader.HasRows;
                connection.Close();
                return exists;
            }
            catch
            {
                throw;
            }
        }

        public DataTable GetPagedDataTable(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize)
        {
            try
            {
                // Validate pageNumber & pageSize
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                int offset = (pageNumber - 1) * pageSize;

                // Append paging to query for SQL Server 2012+
                string pagedQuery = $"{query} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                if (parameters == null)
                    parameters = new Dictionary<string, string>();

                // Use object dictionary for parameter names but values as object (int) instead of string
                var paramObj = parameters.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                paramObj["@Offset"] = offset;
                paramObj["@PageSize"] = pageSize;

                using var connection = new SqlConnection(_databaseConnection);
                using var command = new SqlCommand(pagedQuery, connection);

                foreach (var param in paramObj)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
            catch
            {
                throw;
            }
        }

        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string query, Dictionary<string, string> parameters = null)
        {
            try
            {
                var dict = new Dictionary<TKey, TValue>();
                var dt = GetDataTable(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    object key = row[0];
                    object value = row[1];

                    TKey typedKey = key == DBNull.Value ? default : (TKey)Convert.ChangeType(key, typeof(TKey));
                    TValue typedValue = value == DBNull.Value ? default : (TValue)Convert.ChangeType(value, typeof(TValue));

                    dict[typedKey] = typedValue;
                }

                return dict;
            }
            catch
            {
                throw;
            }
        }

        public List<T> GetMappedList<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null)
        {
            if (mapFunc == null)
                throw new ArgumentNullException(nameof(mapFunc));

            try
            {
                var dt = GetDataTable(query, parameters);
                var list = new List<T>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(mapFunc(row));
                }

                return list;
            }
            catch
            {
                throw;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Microsoft.Data.SqlClient;  // Preferred over System.Data.SqlClient for .NET Core/8+

namespace AdoLite.SqlServer
{
    public partial class DataQuery : IDataQueryAsync
    {
        /// <summary>
        /// Retrieves a single row from the database as a DataRow asynchronously.
        /// </summary>
        public virtual async Task<DataRow> GetDataRowAsync(string query, Dictionary<string, string> parameter = null)
        {
            try
            {
               
                    using (SqlCommand command = new SqlCommand(query, _connection))
                    {
                        if (parameter != null)
                        {
                            foreach (var item in parameter)
                            {
                                command.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }


                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                                return dt.Rows[0];

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
        /// Retrieves a DataSet asynchronously from the database.
        /// </summary>
        public virtual async Task<DataSet> GetDataSetAsync(string query, Dictionary<string, string> parameter = null)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(query, _connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    if (parameter != null)
                    {
                        foreach (var item in parameter)
                        {
                            command.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }

                    DataSet ds = new DataSet();

                    // SqlDataAdapter.Fill is synchronous, so use Task.Run to avoid blocking.
                    await Task.Run(() => adapter.Fill(ds));

                    return ds;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a DataTable asynchronously from the database.
        /// </summary>
        public virtual async Task<DataTable> GetDataTableAsync(string query, Dictionary<string, string> parameter = null)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(query, _connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    if (parameter != null)
                    {
                        foreach (var item in parameter)
                        {
                            command.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }

                    DataTable dt = new DataTable();
                    await Task.Run(() => adapter.Fill(dt));
                    return dt;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single scalar value asynchronously.
        /// </summary>
        public virtual async Task<T> GetSingleValueAsync<T>(string query, Dictionary<string, string> parameter = null)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    if (parameter != null)
                    {
                        foreach (var item in parameter)
                        {
                            command.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }

                    object result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                        return (T)Convert.ChangeType(result, typeof(T));
                    else
                        return default;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Retrieves a single record asynchronously, mapped to type T.
        /// </summary>
        public virtual async Task<T> GetSingleRecordAsync<T>(string query, Dictionary<string, string> parameters = null) where T : new()
        {
            var dt = await GetDataTableAsync(query, parameters);
            if (dt.Rows.Count == 0)
                return default;

            var row = dt.Rows[0];
            T obj = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                {
                    prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                }
            }
            return obj;
        }

        /// <summary>
        /// Retrieves a list of values from a single column asynchronously.
        /// </summary>
        public virtual async Task<List<T>> GetListAsync<T>(string query, Dictionary<string, string> parameters = null)
        {
            var dt = await GetDataTableAsync(query, parameters);
            var list = new List<T>();
            if (dt.Columns.Count == 0) return list;

            foreach (DataRow row in dt.Rows)
            {
                if (row[0] != DBNull.Value)
                    list.Add((T)Convert.ChangeType(row[0], typeof(T)));
            }
            return list;
        }

        /// <summary>
        /// Retrieves count asynchronously.
        /// </summary>
        public virtual async Task<int> GetCountAsync(string query, Dictionary<string, string> parameters = null)
        {
            return await GetSingleValueAsync<int>(query, parameters);
        }

        /// <summary>
        /// Checks existence asynchronously.
        /// </summary>
        public virtual async Task<bool> ExistsAsync(string query, Dictionary<string, string> parameters = null)
        {
            var count = await GetSingleValueAsync<int>(query, parameters);
            return count > 0;
        }

        /// <summary>
        /// Retrieves a paged DataTable asynchronously.
        /// </summary>
        public virtual async Task<DataTable> GetPagedDataTableAsync(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize)
        {
            // Example of applying paging logic in SQL Server using OFFSET-FETCH
            string pagedQuery = $@"
                {query}
                OFFSET {(pageNumber - 1) * pageSize} ROWS
                FETCH NEXT {pageSize} ROWS ONLY";

            return await GetDataTableAsync(pagedQuery, parameters);
        }

        /// <summary>
        /// Retrieves a dictionary asynchronously.
        /// </summary>
        public virtual async Task<Dictionary<TKey, TValue>> GetDictionaryAsync<TKey, TValue>(string query, Dictionary<string, string> parameters = null)
        {
            var dt = await GetDataTableAsync(query, parameters);
            var dict = new Dictionary<TKey, TValue>();

            if (dt.Columns.Count < 2)
                return dict;

            foreach (DataRow row in dt.Rows)
            {
                if (row[0] != DBNull.Value && row[1] != DBNull.Value)
                {
                    TKey key = (TKey)Convert.ChangeType(row[0], typeof(TKey));
                    TValue value = (TValue)Convert.ChangeType(row[1], typeof(TValue));
                    dict[key] = value;
                }
            }
            return dict;
        }

        /// <summary>
        /// Retrieves a list of mapped objects asynchronously.
        /// </summary>
        public virtual async Task<List<T>> GetMappedListAsync<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null)
        {
            var dt = await GetDataTableAsync(query, parameters);
            var list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(mapFunc(row));
            }

            return list;
        }
    }
}

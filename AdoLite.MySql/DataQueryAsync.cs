using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using MySql.Data.MySqlClient;

namespace AdoLite.MySql
{
    public partial class DataQuery : IDataQueryAsync
    {
       

        public virtual async Task<DataRow> GetDataRowAsync(string query, Dictionary<string, string> parameter = null)
        {
            using (var command = new MySqlCommand(query, _connection))
            {
                if (parameter != null)
                {
                    foreach (var item in parameter)
                    {
                        command.Parameters.AddWithValue(item.Key, item.Value);
                    }
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
        }

        public virtual async Task<DataSet> GetDataSetAsync(string query, Dictionary<string, string> parameter = null)
        {
            using (var command = new MySqlCommand(query, _connection))
            using (var adapter = new MySqlDataAdapter(command))
            {
                if (parameter != null)
                {
                    foreach (var item in parameter)
                    {
                        command.Parameters.AddWithValue(item.Key, item.Value);
                    }
                }

                var ds = new DataSet();
                await Task.Run(() => adapter.Fill(ds));
                return ds;
            }
        }

        public virtual async Task<DataTable> GetDataTableAsync(string query, Dictionary<string, string> parameter = null)
        {
            using (var command = new MySqlCommand(query, _connection))
            using (var adapter = new MySqlDataAdapter(command))
            {
                if (parameter != null)
                {
                    foreach (var item in parameter)
                    {
                        command.Parameters.AddWithValue(item.Key, item.Value);
                    }
                }

                var dt = new DataTable();
                await Task.Run(() => adapter.Fill(dt));
                return dt;
            }
        }

        public virtual async Task<T> GetSingleValueAsync<T>(string query, Dictionary<string, string> parameter = null)
        {
            using (var command = new MySqlCommand(query, _connection))
            {
                if (parameter != null)
                {
                    foreach (var item in parameter)
                    {
                        command.Parameters.AddWithValue(item.Key, item.Value);
                    }
                }

                object result = await command.ExecuteScalarAsync();
                return (result != null && result != DBNull.Value)
                    ? (T)Convert.ChangeType(result, typeof(T))
                    : default;
            }
        }

        /// <summary>
        /// Retrieves a single record mapped to type T asynchronously.
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
        /// Retrieves a paged DataTable asynchronously using LIMIT and OFFSET for MySQL.
        /// </summary>
        public virtual async Task<DataTable> GetPagedDataTableAsync(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize)
        {
            string pagedQuery = $@"
                {query}
                LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";

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

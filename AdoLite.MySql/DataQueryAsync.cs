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


        public virtual async Task<DataRow> GetDataRowAsync(string query, Dictionary<string, string> parameter = null, CancellationToken cancellationToken = default)
        {
            using var command = new MySqlCommand(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }
            }

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var dt = new DataTable();
            dt.Load(reader);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public virtual async Task<DataSet> GetDataSetAsync(string query, Dictionary<string, string> parameter = null, CancellationToken cancellationToken = default)
        {
            var dataSet = new DataSet();

            using var command = new MySqlCommand(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }
            }

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            int tableIndex = 0;
            do
            {
                var dt = new DataTable($"Table{tableIndex++}");
                dt.Load(reader);
                dataSet.Tables.Add(dt);
            }
            while (!reader.IsClosed && await reader.NextResultAsync(cancellationToken));

            return dataSet;
        }

        public virtual async Task<DataTable> GetDataTableAsync(string query, Dictionary<string, string> parameter = null, CancellationToken cancellationToken = default)
        {
            var dt = new DataTable();

            using var command = new MySqlCommand(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }
            }

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            dt.Load(reader);

            return dt;
        }

        public virtual async Task<T> GetSingleValueAsync<T>(string query, Dictionary<string, string> parameter = null, CancellationToken cancellationToken = default)
        {
            using var command = new MySqlCommand(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }
            }

            object result = await command.ExecuteScalarAsync(cancellationToken);
            return (result != null && result != DBNull.Value)
                ? (T)Convert.ChangeType(result, typeof(T))
                : default;
        }

        public virtual async Task<T> GetSingleRecordAsync<T>(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default) where T : new()
        {
            var dt = await GetDataTableAsync(query, parameters, cancellationToken);
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

        public virtual async Task<List<T>> GetListAsync<T>(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            var dt = await GetDataTableAsync(query, parameters, cancellationToken);
            var list = new List<T>();

            if (dt.Columns.Count == 0) return list;

            foreach (DataRow row in dt.Rows)
            {
                if (row[0] != DBNull.Value)
                    list.Add((T)Convert.ChangeType(row[0], typeof(T)));
            }
            return list;
        }

        public virtual async Task<int> GetCountAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            return await GetSingleValueAsync<int>(query, parameters, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            var count = await GetSingleValueAsync<int>(query, parameters, cancellationToken);
            return count > 0;
        }

        public virtual async Task<DataTable> GetPagedDataTableAsync(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            string pagedQuery = $@"
                {query}
                LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";

            return await GetDataTableAsync(pagedQuery, parameters, cancellationToken);
        }

        public virtual async Task<Dictionary<TKey, TValue>> GetDictionaryAsync<TKey, TValue>(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            var dt = await GetDataTableAsync(query, parameters, cancellationToken);
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

        public virtual async Task<List<T>> GetMappedListAsync<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            var dt = await GetDataTableAsync(query, parameters, cancellationToken);
            var list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(mapFunc(row));
            }

            return list;
        }
    }
}

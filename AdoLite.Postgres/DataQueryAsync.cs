using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Npgsql;

namespace AdoLite.Postgres
{
    public partial class DataQuery : IDataQueryAsync
    {
        /// <summary>
        /// Retrieves a single row from the database as a DataRow.
        /// </summary>
        public virtual async Task<DataRow> GetDataRowAsync(string query, Dictionary<string, string> parameter = null, CancellationToken cancellationToken = default)
        {
            var dt = await GetDataTableAsync(query, parameter, cancellationToken);
            if (dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }

        /// <summary>
        /// Retrieves a DataSet from the database.
        /// Note: Cancellation token support is limited due to NpgsqlDataAdapter.Fill lacking async overloads.
        /// </summary>
        public virtual async Task<DataSet> GetDataSetAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            var dataSet = new DataSet();
            using var cmd = new NpgsqlCommand(query, _connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            do
            {
                var dt = new DataTable();
                dt.Load(reader);
                dataSet.Tables.Add(dt);
            } while (!reader.IsClosed && await reader.NextResultAsync(cancellationToken));

            return dataSet;
        }


        /// <summary>
        /// Retrieves a DataTable from the database.
        /// </summary>
        public virtual async Task<DataTable> GetDataTableAsync(string query, Dictionary<string, string> parameter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var cmd = new NpgsqlCommand(query, _connection))
                {
                    if (parameter != null && parameter.Count > 0)
                    {
                        foreach (var item in parameter)
                        {
                            cmd.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }

                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw; // Propagate cancellation
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single column value from the database as a specific type (T).
        /// </summary>
        public virtual async Task<T> GetSingleValueAsync<T>(string query, Dictionary<string, string> parameter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var cmd = new NpgsqlCommand(query, _connection))
                {
                    if (parameter != null && parameter.Count > 0)
                    {
                        foreach (var item in parameter)
                        {
                            cmd.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }

                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var data = reader.IsDBNull(0) ? default(T) : (T)Convert.ChangeType(reader[0], typeof(T));
                            return data;
                        }
                    }
                }
                return default;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single record asynchronously, mapped to type T.
        /// </summary>
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

        /// <summary>
        /// Retrieves a list of values from a single column asynchronously.
        /// </summary>
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

        /// <summary>
        /// Retrieves count asynchronously.
        /// </summary>
        public virtual async Task<int> GetCountAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            return await GetSingleValueAsync<int>(query, parameters, cancellationToken);
        }

        /// <summary>
        /// Checks existence asynchronously.
        /// </summary>
        public virtual async Task<bool> ExistsAsync(string query, Dictionary<string, string> parameters = null, CancellationToken cancellationToken = default)
        {
            var count = await GetSingleValueAsync<int>(query, parameters, cancellationToken);
            return count > 0;
        }

        /// <summary>
        /// Retrieves a paged DataTable asynchronously.
        /// </summary>
        public virtual async Task<DataTable> GetPagedDataTableAsync(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            // For PostgreSQL, use LIMIT and OFFSET syntax
            string pagedQuery = $@"
                {query}
                LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";

            return await GetDataTableAsync(pagedQuery, parameters, cancellationToken);
        }

        /// <summary>
        /// Retrieves a dictionary asynchronously.
        /// </summary>
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

        /// <summary>
        /// Retrieves a list of mapped objects asynchronously.
        /// </summary>
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

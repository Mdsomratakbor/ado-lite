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
    }
}

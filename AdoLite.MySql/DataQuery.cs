using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using AdoLite.Core.Interfaces;
using System.Reflection;

namespace AdoLite.MySql
{
    public partial class DataQuery : IDataQuery, IDisposable
    {
        private readonly string _databaseConnection;
        private readonly MySqlConnection _connection;
        private bool _disposed;

        public DataQuery(string connectionString)
        {
            _databaseConnection = connectionString;
            _connection = new MySqlConnection(_databaseConnection);
            _connection.Open();
        }

        public virtual DataRow GetDataRow(string query, Dictionary<string, string> parameter = null)
        {
            using var dataAdapter = new MySqlDataAdapter(query, _connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }

        public virtual T GetSingleRecord<T>(string query, Dictionary<string, string> parameter = null) where T : new()
        {
            using var dataAdapter = new MySqlDataAdapter(query, _connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            if (dataTable.Rows.Count == 0) return default;

            DataRow row = dataTable.Rows[0];
            T result = new T();

            foreach (DataColumn column in dataTable.Columns)
            {
                PropertyInfo prop = typeof(T).GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null && row[column] != DBNull.Value)
                    prop.SetValue(result, Convert.ChangeType(row[column], prop.PropertyType));
            }

            return result;
        }

        public virtual DataSet GetDataSet(string query, Dictionary<string, string> parameter)
        {
            using var dataAdapter = new MySqlDataAdapter(query, _connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            return dataSet;
        }

        public virtual DataTable GetDataTable(string query, Dictionary<string, string> parameter = null)
        {
            using var dataAdapter = new MySqlDataAdapter(query, _connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public virtual T GetSingleValue<T>(string query, Dictionary<string, string> parameter = null)
        {
            using var command = new MySqlCommand(query, _connection);
            AddParameters(command, parameter);

            object result = command.ExecuteScalar();
            return (result != null && result != DBNull.Value)
                ? (T)Convert.ChangeType(result, typeof(T))
                : default;
        }

        public List<T> GetList<T>(string query, Dictionary<string, string> parameters = null)
        {
            var list = new List<T>();
            var dt = GetDataTable(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                object val = row[0];
                list.Add(val == DBNull.Value ? default : (T)Convert.ChangeType(val, typeof(T)));
            }

            return list;
        }

        public int GetCount(string query, Dictionary<string, string> parameters = null)
            => GetSingleValue<int>(query, parameters);

        public bool Exists(string query, Dictionary<string, string> parameters = null)
        {
            using var command = new MySqlCommand(query, _connection);
            AddParameters(command, parameters);

            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public DataTable GetPagedDataTable(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            int offset = (pageNumber - 1) * pageSize;
            string pagedQuery = $"{query} LIMIT @PageSize OFFSET @Offset";

            var paramObj = parameters?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) ?? new();
            paramObj["@PageSize"] = pageSize;
            paramObj["@Offset"] = offset;

            using var command = new MySqlCommand(pagedQuery, _connection);
            foreach (var param in paramObj)
                command.Parameters.AddWithValue(param.Key, param.Value);

            using var adapter = new MySqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string query, Dictionary<string, string> parameters = null)
        {
            var dict = new Dictionary<TKey, TValue>();
            var dt = GetDataTable(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                TKey key = row[0] == DBNull.Value ? default : (TKey)Convert.ChangeType(row[0], typeof(TKey));
                TValue value = row[1] == DBNull.Value ? default : (TValue)Convert.ChangeType(row[1], typeof(TValue));
                dict[key] = value;
            }

            return dict;
        }

        public List<T> GetMappedList<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null)
        {
            if (mapFunc == null)
                throw new ArgumentNullException(nameof(mapFunc));

            var dt = GetDataTable(query, parameters);
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
                list.Add(mapFunc(row));

            return list;
        }

        private void AddParameters(MySqlCommand command, Dictionary<string, string> parameters)
        {
            if (parameters != null)
            {
                foreach (var param in parameters)
                    command.Parameters.AddWithValue(param.Key, param.Value);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Close();
                _connection?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}

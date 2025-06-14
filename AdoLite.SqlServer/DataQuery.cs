﻿using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using AdoLite.Core.Interfaces;
using System.Reflection;

namespace AdoLite.SqlServer
{
    public partial class DataQuery : IDataQuery, IDisposable
    {
        private readonly string _databaseConnection;
        private readonly SqlConnection _connection;
        private bool _disposed;

        public DataQuery(string connectionString)
        {
            _databaseConnection = connectionString;
            _connection = new SqlConnection(_databaseConnection);
            _connection.Open(); // Open once and share
        }

        public virtual DataRow GetDataRow(string query, Dictionary<string, string> parameter = null)
        {
            using SqlDataAdapter dataAdapter = new SqlDataAdapter(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                    dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
            }

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }

        public virtual T GetSingleRecord<T>(string query, Dictionary<string, string> parameter = null) where T : new()
        {
            using SqlDataAdapter dataAdapter = new SqlDataAdapter(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                    dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
            }

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
            using SqlDataAdapter dataAdapter = new SqlDataAdapter(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                    dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
            }

            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            return dataSet;
        }

        public virtual DataTable GetDataTable(string query, Dictionary<string, string> parameter = null)
        {
            using SqlDataAdapter dataAdapter = new SqlDataAdapter(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                    dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
            }

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public virtual T GetSingleValue<T>(string query, Dictionary<string, string> parameter = null)
        {
            using SqlCommand command = new SqlCommand(query, _connection);
            if (parameter != null)
            {
                foreach (var item in parameter)
                    command.Parameters.AddWithValue(item.Key, item.Value);
            }

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
            using SqlCommand command = new SqlCommand(query, _connection);
            if (parameters != null)
            {
                foreach (var item in parameters)
                    command.Parameters.AddWithValue(item.Key, item.Value);
            }

            using SqlDataReader reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public DataTable GetPagedDataTable(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            int offset = (pageNumber - 1) * pageSize;
            string pagedQuery = $"{query} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var paramObj = parameters?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) ?? new();
            paramObj["@Offset"] = offset;
            paramObj["@PageSize"] = pageSize;

            using SqlCommand command = new SqlCommand(pagedQuery, _connection);
            foreach (var param in paramObj)
                command.Parameters.AddWithValue(param.Key, param.Value);

            using SqlDataAdapter adapter = new SqlDataAdapter(command);
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

        /// <summary>
        /// Dispose and close shared connection
        /// </summary>
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

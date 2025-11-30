using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AdoLite.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AdoLite.SqlServer
{
    public partial class DataQuery : IDataQuery, IDisposable
    {
        private readonly string _connectionString;
        private readonly ILogger<DataQuery>? _logger;
        private bool _disposed;

        public DataQuery(string connectionString, ILogger<DataQuery>? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger;
        }

        public virtual DataRow GetDataRow(string query, Dictionary<string, string> parameter = null)
        {
            using var connection = CreateAndOpenConnection();
            using var dataAdapter = new SqlDataAdapter(query, connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            var sw = Stopwatch.StartNew();
            try
            {
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                sw.Stop();
                LogSuccess(nameof(GetDataRow), query, parameter, sw.ElapsedMilliseconds, dataTable.Rows.Count);
                return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(GetDataRow), query, parameter, sw.ElapsedMilliseconds, ex);
                throw;
            }
        }

        public virtual T GetSingleRecord<T>(string query, Dictionary<string, string> parameter = null) where T : new()
        {
            using var connection = CreateAndOpenConnection();
            using var dataAdapter = new SqlDataAdapter(query, connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            var sw = Stopwatch.StartNew();
            try
            {
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                if (dataTable.Rows.Count == 0)
                {
                    sw.Stop();
                    LogSuccess(nameof(GetSingleRecord), query, parameter, sw.ElapsedMilliseconds, 0);
                    return default;
                }

                DataRow row = dataTable.Rows[0];
                T result = new T();

                foreach (DataColumn column in dataTable.Columns)
                {
                    PropertyInfo prop = typeof(T).GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null && row[column] != DBNull.Value)
                        prop.SetValue(result, Convert.ChangeType(row[column], prop.PropertyType));
                }

                sw.Stop();
                LogSuccess(nameof(GetSingleRecord), query, parameter, sw.ElapsedMilliseconds, 1);
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(GetSingleRecord), query, parameter, sw.ElapsedMilliseconds, ex);
                throw;
            }
        }

        public virtual DataSet GetDataSet(string query, Dictionary<string, string> parameter)
        {
            using var connection = CreateAndOpenConnection();
            using var dataAdapter = new SqlDataAdapter(query, connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            var sw = Stopwatch.StartNew();
            try
            {
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                sw.Stop();
                LogSuccess(nameof(GetDataSet), query, parameter, sw.ElapsedMilliseconds, dataSet.Tables.Count);
                return dataSet;
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(GetDataSet), query, parameter, sw.ElapsedMilliseconds, ex);
                throw;
            }
        }

        public virtual DataTable GetDataTable(string query, Dictionary<string, string> parameter = null)
        {
            using var connection = CreateAndOpenConnection();
            using var dataAdapter = new SqlDataAdapter(query, connection);
            AddParameters(dataAdapter.SelectCommand, parameter);

            var sw = Stopwatch.StartNew();
            try
            {
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                sw.Stop();
                LogSuccess(nameof(GetDataTable), query, parameter, sw.ElapsedMilliseconds, dataTable.Rows.Count);
                return dataTable;
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(GetDataTable), query, parameter, sw.ElapsedMilliseconds, ex);
                throw;
            }
        }

        public virtual T GetSingleValue<T>(string query, Dictionary<string, string> parameter = null)
        {
            using var connection = CreateAndOpenConnection();
            using var command = new SqlCommand(query, connection);
            AddParameters(command, parameter);

            var sw = Stopwatch.StartNew();
            try
            {
                object result = command.ExecuteScalar();
                sw.Stop();
                LogSuccess(nameof(GetSingleValue), query, parameter, sw.ElapsedMilliseconds, result == null || result == DBNull.Value ? 0 : 1);
                return ConvertResult<T>(result);
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(GetSingleValue), query, parameter, sw.ElapsedMilliseconds, ex);
                throw;
            }
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
            using var connection = CreateAndOpenConnection();
            using var command = new SqlCommand(query, connection);
            AddParameters(command, parameters);

            var sw = Stopwatch.StartNew();
            try
            {
                using SqlDataReader reader = command.ExecuteReader();
                sw.Stop();
                LogSuccess(nameof(Exists), query, parameters, sw.ElapsedMilliseconds, reader.HasRows ? 1 : 0);
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(Exists), query, parameters, sw.ElapsedMilliseconds, ex);
                throw;
            }
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

            using var connection = CreateAndOpenConnection();
            using var command = new SqlCommand(pagedQuery, connection);
            foreach (var param in paramObj)
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);

            using var adapter = new SqlDataAdapter(command);
            var sw = Stopwatch.StartNew();
            try
            {
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                sw.Stop();
                LogSuccess(nameof(GetPagedDataTable), pagedQuery, parameters, sw.ElapsedMilliseconds, dataTable.Rows.Count);
                return dataTable;
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogFailure(nameof(GetPagedDataTable), pagedQuery, parameters, sw.ElapsedMilliseconds, ex);
                throw;
            }
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
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        internal static string TrimSqlForLog(string sql, int maxLength = 500)
        {
            if (string.IsNullOrWhiteSpace(sql)) return string.Empty;
            if (sql.Length <= maxLength) return sql;
            return sql.Substring(0, maxLength) + "...";
        }

        internal static IReadOnlyDictionary<string, object> ToLoggableParameters(Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0) return new Dictionary<string, object>();
            return parameters.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value ?? "null");
        }

        private SqlConnection CreateAndOpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static void AddParameters(SqlCommand command, Dictionary<string, string> parameters)
        {
            if (parameters == null) return;
            foreach (var item in parameters)
            {
                command.Parameters.AddWithValue(item.Key, item.Value ?? (object)DBNull.Value);
            }
        }

        private static T ConvertResult<T>(object result)
        {
            return (result != null && result != DBNull.Value)
                ? (T)Convert.ChangeType(result, typeof(T))
                : default;
        }

        private void LogSuccess(string operation, string query, Dictionary<string, string> parameters, long elapsedMs, int affectedRows)
        {
            _logger?.LogInformation(
                "{Operation} executed in {ElapsedMs}ms | Rows={Rows} | Sql={Sql} | Params={@Params}",
                operation,
                elapsedMs,
                affectedRows,
                TrimSqlForLog(query),
                ToLoggableParameters(parameters));
        }

        private void LogFailure(string operation, string query, Dictionary<string, string> parameters, long elapsedMs, Exception ex)
        {
            _logger?.LogError(
                ex,
                "{Operation} failed in {ElapsedMs}ms | Sql={Sql} | Params={@Params}",
                operation,
                elapsedMs,
                TrimSqlForLog(query),
                ToLoggableParameters(parameters));
        }
    }

}

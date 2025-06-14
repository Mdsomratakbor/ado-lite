﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Npgsql;

namespace AdoLite.Postgres
{
    public partial class DataQuery : IDataQuery, IDisposable
    {
        private readonly string _databaseConnection;
        private readonly NpgsqlConnection _connection;
        private bool _disposed;

        public DataQuery(string connectionString)
        {
            _databaseConnection = connectionString;
            _connection = new NpgsqlConnection(_databaseConnection);
            _connection.Open(); // Open once and share
        }

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

                using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, _connection)) // PostgreSQL data adapter
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

                    DataRow row = dataTable.Rows[0];
                    return row;

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
              
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, _connection)) // PostgreSQL data adapter
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
            catch (Exception ex)
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
               
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, _connection)) // PostgreSQL data adapter
                    {

                        if (parameter != null && parameter.Count > 0)
                        {
                            foreach (var item in parameter)
                            {
                                dataAdapter.SelectCommand.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }
                        DataTable dt = new DataTable();
                        dataAdapter.Fill(dt);
                        return dt;
                    }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool Exists(string query, Dictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public int GetCount(string query, Dictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string query, Dictionary<string, string> parameters = null)
        {

            throw new NotImplementedException();
        }

        public List<T> GetList<T>(string query, Dictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public List<T> GetMappedList<T>(string query, Func<DataRow, T> mapFunc, Dictionary<string, string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public DataTable GetPagedDataTable(string query, Dictionary<string, string> parameters, int pageNumber, int pageSize)
        {
            try
            {
                int offset = (pageNumber - 1) * pageSize;
                query += $" LIMIT {pageSize} OFFSET {offset}";

                return GetDataTable(query, parameters);
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
                var data = "";
               
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, _connection)) // PostgreSQL command
                    {
                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                data = Convert.ToString(reader[0]);
                            }
                            reader.Close();
                        }
                        return (T)Convert.ChangeType(data, typeof(T));
                    }
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T GetSingleRecord<T>(string query, Dictionary<string, string> parameter = null) where T : new()
        {
            throw new NotImplementedException();
        }

        public List<T> GetRecordList<T>(string query, Dictionary<string, string> parameter = null)
        {
            throw new NotImplementedException();
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

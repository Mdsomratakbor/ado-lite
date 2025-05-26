using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Npgsql;

namespace AdoLite.Postgres
{
    public partial class DataQuery : IDataQuery
    {

        private readonly string _databaseConnection;

        // Constructor to initialize the repository with the connection string
        public DataQuery(string connectionString)
        {
            this._databaseConnection = connectionString;
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
                using (NpgsqlConnection obcon = new NpgsqlConnection(_databaseConnection)) // PostgreSQL connection
                {
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, obcon)) // PostgreSQL data adapter
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
                using (NpgsqlConnection obcon = new NpgsqlConnection(_databaseConnection)) // PostgreSQL connection
                {
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, obcon)) // PostgreSQL data adapter
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
                using (NpgsqlConnection obcon = new NpgsqlConnection(_databaseConnection)) // PostgreSQL connection
                {
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, obcon)) // PostgreSQL data adapter
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
            }
            catch (Exception ex)
            {
                throw ex;
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
                using (NpgsqlConnection connection = new NpgsqlConnection(_databaseConnection)) // PostgreSQL connection
                {
                    connection.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection)) // PostgreSQL command
                    {
                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                data = Convert.ToString(reader[0]);
                            }
                            reader.Close();
                            connection.Close();
                        }
                        return (T)Convert.ChangeType(data, typeof(T));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

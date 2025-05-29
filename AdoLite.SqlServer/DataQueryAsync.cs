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
    }
}

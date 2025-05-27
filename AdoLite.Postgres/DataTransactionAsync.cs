using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Npgsql;

namespace AdoLite.Postgres
{
    public partial class DataQuery : IDataTransactionAsync
    {

        /// <summary>
        /// Executes a series of database queries within a transaction and commits them.
        /// </summary>
        /// <param name="queryPatterns">A list of queries to be executed.</param>
        /// <returns>True if all queries were successfully executed and committed.</returns>
        public async Task<bool> SaveChangesAsync(List<IQueryPattern> queryPatterns, int commandTimeoutSeconds = 30, CancellationToken cancellationToken = default)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_databaseConnection))
                {
                    await connection.OpenAsync();  // Open connection asynchronously
                    var transaction = await connection.BeginTransactionAsync(cancellationToken);  // Begin a transaction

                    try
                    {
                        using (NpgsqlCommand cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = transaction;  // Assign the transaction to the command

                            // Loop through each query pattern and execute it
                            foreach (var data in queryPatterns)
                            {
                                cmd.CommandText = data.Query;  // Set the query text
                                cmd.CommandTimeout = commandTimeoutSeconds;
                                // Add parameters to the command if provided
                                if (data.Parameters.Count > 0 && data.Parameters != null)
                                {
                                    cmd.Parameters.Clear();  // Clear any previous parameters
                                    foreach (var parameter in data.Parameters)
                                    {
                                        foreach (var item in parameter)
                                        {
                                            cmd.Parameters.AddWithValue(item.Key, item.Value);  // Add new parameters
                                        }
                                    }
                                }

                                // Execute the query asynchronously
                                await cmd.ExecuteNonQueryAsync(cancellationToken);
                            }

                            await transaction.CommitAsync(cancellationToken);  // Commit the transaction if all queries were successful
                        }
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync(cancellationToken);  // Rollback the transaction if there was an error
                        throw;
                    }
                }

                return true;  // Return true if all queries were executed successfully
            }
            catch (Exception)
            {
                throw;  // Rethrow the exception for handling at a higher level
            }
        }
    }
}

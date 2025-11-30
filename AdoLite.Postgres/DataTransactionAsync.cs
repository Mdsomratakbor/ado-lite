using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AdoLite.Postgres
{
    public partial class DataQuery : IDataTransactionAsync
    {
        public void BulkInsert<T>(string tableName, List<T> dataList)
        {
            throw new NotImplementedException();
        }

        public Task BulkInsertAsync(string tableName, DataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public Task BulkInsertAsync<T>(string tableName, List<T> dataList)
        {
            throw new NotImplementedException();
        }

        public Task BulkInsertFromCsvAsync(string tableName, string csvFilePath)
        {
            throw new NotImplementedException();
        }

     
        public Task BulkInsertFromJsonAsync<T>(string tableName, string jsonFilePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a series of database queries within a transaction and commits them.
        /// </summary>
        /// <param name="queryPatterns">A list of queries to be executed.</param>
        /// <returns>True if all queries were successfully executed and committed.</returns>
        public async Task<bool> SaveChangesAsync(List<IQueryPattern> queryPatterns, int commandTimeoutSeconds = 30, CancellationToken cancellationToken = default)
        {
            if (queryPatterns == null) throw new ArgumentNullException(nameof(queryPatterns));

            await using var connection = CreateAndOpenConnection();
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);  // Begin a transaction
            var sw = Stopwatch.StartNew();
            try
            {
                await using (NpgsqlCommand cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;  // Assign the transaction to the command

                    // Loop through each query pattern and execute it
                    foreach (var data in queryPatterns)
                    {
                        cmd.CommandText = data.Query;  // Set the query text
                        cmd.CommandTimeout = commandTimeoutSeconds;
                        // Add parameters to the command if provided
                        if (data.Parameters != null && data.Parameters.Count > 0)
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

                sw.Stop();
                _logger?.LogInformation(
                    "{Operation} executed {Count} statements in {ElapsedMs}ms | SqlBatch={@Batch}",
                    nameof(SaveChangesAsync),
                    queryPatterns.Count,
                    sw.ElapsedMilliseconds,
                    queryPatterns.Select(q => TrimSqlForLog(q.Query)).ToList());

                return true;  // Return true if all queries were executed successfully
            }
            catch (Exception ex)
            {
                sw.Stop();
                try
                {
                    await transaction.RollbackAsync(cancellationToken);  // Rollback the transaction if there was an error
                }
                catch (Exception rollbackEx)
                {
                    _logger?.LogError(rollbackEx, "Failed to rollback transaction for {Operation}", nameof(SaveChangesAsync));
                }

                _logger?.LogError(
                    ex,
                    "{Operation} failed in {ElapsedMs}ms | SqlBatch={@Batch}",
                    nameof(SaveChangesAsync),
                    sw.ElapsedMilliseconds,
                    queryPatterns?.Select(q => TrimSqlForLog(q.Query)).ToList());

                throw;  // Rethrow the exception for handling at a higher level
            }
        }
    }
}

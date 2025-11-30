using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AdoLite.SqlServer
{
    public partial class DataQuery : IDataTransactionAsync
    {
        /// <summary>
        /// Executes a series of database queries within a transaction and commits them asynchronously.
        /// </summary>
        /// <param name="queryPatterns">A list of queries to be executed.</param>
        /// <returns>True if all queries were successfully executed and committed.</returns>
        public async Task<bool> SaveChangesAsync(
        List<IQueryPattern> queryPatterns,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default)
        {
            if (queryPatterns == null) throw new ArgumentNullException(nameof(queryPatterns));

            await using var connection = CreateAndOpenConnection();
            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
            var sw = Stopwatch.StartNew();
            try
            {
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandTimeout = commandTimeoutSeconds;

                    foreach (var data in queryPatterns)
                    {
                        cmd.CommandText = data.Query;
                        cmd.Parameters.Clear();

                    if (data.Parameters != null && data.Parameters.Count > 0)
                    {
                        foreach (var parameterDict in data.Parameters)
                        {
                            foreach (var param in parameterDict)
                            {
                                if (param.Value is SqlParameter sqlParam)
                                {
                                    var p = cmd.Parameters.Add(sqlParam.ParameterName ?? param.Key, sqlParam.SqlDbType);
                                    if (!string.IsNullOrWhiteSpace(sqlParam.TypeName))
                                    {
                                        p.TypeName = sqlParam.TypeName;
                                    }
                                    p.Direction = sqlParam.Direction;
                                    p.Size = sqlParam.Size;
                                    p.Value = sqlParam.Value ?? DBNull.Value;
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                                }
                            }
                        }
                    }

                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                sw.Stop();
                _logger?.LogInformation(
                    "{Operation} executed {Count} statements in {ElapsedMs}ms | SqlBatch={@Batch}",
                    nameof(SaveChangesAsync),
                    queryPatterns.Count,
                    sw.ElapsedMilliseconds,
                    queryPatterns.Select(q => DataQuery.TrimSqlForLog(q.Query)).ToList());
                return true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
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
                    queryPatterns?.Select(q => DataQuery.TrimSqlForLog(q.Query)).ToList());
                throw;
            }
        }

        public async Task BulkInsertAsync<T>(string tableName, List<T> dataList)
        {
            var table = ToDataTable(dataList);
            await BulkInsertAsync(tableName, table);
        }

        public async Task BulkInsertAsync(string tableName, DataTable dataTable)
        {
            await using var connection = CreateAndOpenConnection();
            using var bulkCopy = new SqlBulkCopy(connection);
            bulkCopy.DestinationTableName = tableName;
            await bulkCopy.WriteToServerAsync(dataTable);
        }

        public async Task BulkInsertFromCsvAsync(string tableName, string csvFilePath)
        {
            var table = CsvToDataTable(csvFilePath);
            await BulkInsertAsync(tableName, table);
        }

        public async Task BulkInsertFromJsonAsync<T>(string tableName, string jsonFilePath)
        {
            var jsonData = await File.ReadAllTextAsync(jsonFilePath);
            var dataList = JsonConvert.DeserializeObject<List<T>>(jsonData);
            await BulkInsertAsync(tableName, ToDataTable(dataList));
        }

    }
}

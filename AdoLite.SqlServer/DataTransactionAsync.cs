using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using AdoLite.Core.Interfaces;

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
            try
            {

                    await using var transaction = (SqlTransaction)await _connection.BeginTransactionAsync(cancellationToken);
                    
                        try
                        {
                            await using (var cmd = _connection.CreateCommand())
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
                                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                                            }
                                        }
                                    }

                                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                                }
                            }

                            await transaction.CommitAsync(cancellationToken);
                            return true;
                        }
                        catch
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            throw;
                        }
                    
                
            }
            catch
            {
                throw;
            }
        }
    }
}

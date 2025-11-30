using System;
using System.Collections.Generic;
using System.Data;
using AdoLite.Postgres;
using Npgsql;
using Xunit;
using Xunit.Sdk;

namespace AdoLite.Tests.Postgres
{
    public class PostgresBulkInsertTests : IDisposable
    {
        private readonly string _connectionString;
        private readonly DataQuery _dataQuery;

        public PostgresBulkInsertTests()
        {
            _connectionString = Environment.GetEnvironmentVariable("ADOLITE_POSTGRES_CONNECTION")
                                ?? "Host=localhost;Database=postgres;Username=postgres;Password=postgres;";
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS bulkusers (
                        id SERIAL PRIMARY KEY,
                        name TEXT NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new SkipException($"PostgreSQL unavailable: {ex.Message}");
            }

            _dataQuery = new DataQuery(_connectionString);
        }

        [Fact]
        public void BulkInsert_InsertsRows()
        {
            // Arrange
            var users = new List<TestUser>
            {
                new() { Name = "BulkUser1" },
                new() { Name = "BulkUser2" }
            };

            Cleanup();

            // Act
            _dataQuery.BulkInsert("bulkusers", users);

            // Assert
            var count = _dataQuery.GetCount("SELECT COUNT(*) FROM bulkusers WHERE name LIKE 'BulkUser%'");
            Assert.Equal(2, count);
        }

        private void Cleanup()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM bulkusers WHERE name LIKE 'BulkUser%';";
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            try
            {
                Cleanup();
            }
            catch
            {
                // ignore cleanup failures
            }
            _dataQuery.Dispose();
        }

        private class TestUser
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}

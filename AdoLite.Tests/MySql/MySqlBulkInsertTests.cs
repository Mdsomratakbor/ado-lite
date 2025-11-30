using System;
using System.Collections.Generic;
using System.Data;
using AdoLite.MySql;
using MySql.Data.MySqlClient;
using Xunit;
using Xunit.Sdk;

namespace AdoLite.Tests.MySql
{
    public class MySqlBulkInsertTests : IDisposable
    {
        private readonly string _connectionString;
        private readonly DataQuery _dataQuery;

        public MySqlBulkInsertTests()
        {
            _connectionString = Environment.GetEnvironmentVariable("ADOLITE_MYSQL_CONNECTION")
                                ?? "Server=localhost;Database=mysql;Uid=root;Pwd=root;Pooling=true;MaximumPoolSize=50;";
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS BulkUsers (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new SkipException($"MySQL unavailable: {ex.Message}");
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

            // Cleanup any remnants
            Cleanup();

            // Act
            _dataQuery.BulkInsert("BulkUsers", users);

            // Assert
            var count = _dataQuery.GetCount("SELECT COUNT(*) FROM BulkUsers WHERE Name LIKE 'BulkUser%'");
            Assert.Equal(2, count);
        }

        private void Cleanup()
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM BulkUsers WHERE Name LIKE 'BulkUser%';";
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

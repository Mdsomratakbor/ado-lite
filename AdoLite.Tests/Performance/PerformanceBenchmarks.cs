using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdoLite.MySql;
using AdoLite.Postgres;
using AdoLite.SqlServer;
using MySql.Data.MySqlClient;
using Npgsql;
using Xunit;
using Xunit.Sdk;

namespace AdoLite.Tests.Performance
{
    public class SqlServerPerformanceBenchmarks
    {
        private const string DefaultConn = "Server=.;Database=NORTHWND;User Id=sa;Password=007;TrustServerCertificate=True;Pooling=true;Max Pool Size=50;";
        private readonly string _conn;

        public SqlServerPerformanceBenchmarks()
        {
            _conn = Environment.GetEnvironmentVariable("ADOLITE_SQLSERVER_CONNECTION") ?? DefaultConn;
            try
            {
                using var dq = new AdoLite.SqlServer.DataQuery(_conn);
                dq.GetSingleValue<int>("SELECT 1");
            }
            catch (Exception ex)
            {
                throw new SkipException($"SQL Server unavailable for perf benchmark: {ex.Message}");
            }
        }

        [Fact]
        public void GetSingleValue_AvgLatency_IsReasonable()
        {
            var avg = Measure(() =>
            {
                using var dq = new AdoLite.SqlServer.DataQuery(_conn);
                dq.GetSingleValue<int>("SELECT 1");
            });
            Assert.True(avg < 200, $"Average latency too high: {avg} ms");
        }

        [Fact]
        public void GetDataTable_AvgLatency_IsReasonable()
        {
            var avg = Measure(() =>
            {
                using var dq = new AdoLite.SqlServer.DataQuery(_conn);
                dq.GetDataTable("SELECT TOP 5 * FROM Customers", null);
            });
            Assert.True(avg < 300, $"Average latency too high: {avg} ms");
        }

        private static double Measure(Action action, int iterations = 10)
        {
            var times = new List<long>(iterations);
            for (int i = 0; i < iterations; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
            return times.Average();
        }
    }

    public class PostgresPerformanceBenchmarks
    {
        private const string DefaultConn = "Host=localhost;Database=postgres;Username=postgres;Password=postgres;";
        private readonly string _conn;

        public PostgresPerformanceBenchmarks()
        {
            _conn = Environment.GetEnvironmentVariable("ADOLITE_POSTGRES_CONNECTION") ?? DefaultConn;
            try
            {
                using var conn = new NpgsqlConnection(_conn);
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new SkipException($"PostgreSQL unavailable for perf benchmark: {ex.Message}");
            }
        }

        [Fact]
        public void GetSingleValue_AvgLatency_IsReasonable()
        {
            var avg = Measure(() =>
            {
                using var dq = new AdoLite.Postgres.DataQuery(_conn);
                dq.GetSingleValue<int>("SELECT 1");
            });
            Assert.True(avg < 200, $"Average latency too high: {avg} ms");
        }

        [Fact]
        public void GetDataTable_AvgLatency_IsReasonable()
        {
            var avg = Measure(() =>
            {
                using var dq = new AdoLite.Postgres.DataQuery(_conn);
                dq.GetDataTable("SELECT generate_series(1,5) AS n", null);
            });
            Assert.True(avg < 300, $"Average latency too high: {avg} ms");
        }

        private static double Measure(Action action, int iterations = 10)
        {
            var times = new List<long>(iterations);
            for (int i = 0; i < iterations; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
            return times.Average();
        }
    }

    public class MySqlPerformanceBenchmarks
    {
        private const string DefaultConn = "Server=localhost;Database=mysql;Uid=root;Pwd=root;Pooling=true;MaximumPoolSize=50;";
        private readonly string _conn;

        public MySqlPerformanceBenchmarks()
        {
            _conn = Environment.GetEnvironmentVariable("ADOLITE_MYSQL_CONNECTION") ?? DefaultConn;
            try
            {
                using var conn = new MySqlConnection(_conn);
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new SkipException($"MySQL unavailable for perf benchmark: {ex.Message}");
            }
        }

        [Fact]
        public void GetSingleValue_AvgLatency_IsReasonable()
        {
            var avg = Measure(() =>
            {
                using var dq = new AdoLite.MySql.DataQuery(_conn);
                dq.GetSingleValue<int>("SELECT 1", null);
            });
            Assert.True(avg < 200, $"Average latency too high: {avg} ms");
        }

        [Fact]
        public void GetDataTable_AvgLatency_IsReasonable()
        {
            var avg = Measure(() =>
            {
                using var dq = new AdoLite.MySql.DataQuery(_conn);
                dq.GetDataTable("SELECT 1 AS n UNION SELECT 2", null);
            });
            Assert.True(avg < 300, $"Average latency too high: {avg} ms");
        }

        private static double Measure(Action action, int iterations = 10)
        {
            var times = new List<long>(iterations);
            for (int i = 0; i < iterations; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
            return times.Average();
        }
    }
}

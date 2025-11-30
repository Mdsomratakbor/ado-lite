using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdoLite.SqlServer;
using Xunit;
using Xunit.Sdk;

namespace AdoLite.Tests.SqlServer
{
    public class DataQueryConcurrencyTests
    {
        private const string DefaultConn = "Server=.;Database=NORTHWND;User Id=sa;Password=007;TrustServerCertificate=True;Pooling=true;Max Pool Size=50;";

        private static string GetConnectionOrSkip()
        {
            var conn = Environment.GetEnvironmentVariable("ADOLITE_SQLSERVER_CONNECTION") ?? DefaultConn;
            try
            {
                using var dq = new DataQuery(conn);
                dq.GetSingleValue<int>("SELECT 1");
            }
            catch (Exception ex)
            {
                throw new SkipException($"SQL Server unavailable for concurrency test: {ex.Message}");
            }
            return conn;
        }

        [Fact]
        public async Task Concurrent_Reads_WorkWithoutErrors()
        {
            var conn = GetConnectionOrSkip();
            const int requestCount = 500;
            var tasks = new List<Task<int>>(requestCount);

            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    using var dq = new DataQuery(conn);
                    return dq.GetSingleValue<int>("SELECT 1");
                }));
            }

            var results = await Task.WhenAll(tasks);
            Assert.True(results.All(r => r == 1));
        }

        [Fact]
        public async Task Concurrent_Reads_AverageLatency_IsReasonable()
        {
            var conn = GetConnectionOrSkip();
            const int requestCount = 200;
            var sw = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, requestCount).Select(_ => Task.Run(() =>
            {
                using var dq = new DataQuery(conn);
                return dq.GetSingleValue<int>("SELECT 1");
            })).ToArray();

            var results = await Task.WhenAll(tasks);
            sw.Stop();

            Assert.True(results.All(r => r == 1));

            // Rough sanity check: ensure total elapsed is not extremely high (e.g., > 5 seconds for 20 trivial queries).
            Assert.True(sw.Elapsed < TimeSpan.FromSeconds(5), $"Concurrent read latency too high: {sw.Elapsed}");
        }
    }
}

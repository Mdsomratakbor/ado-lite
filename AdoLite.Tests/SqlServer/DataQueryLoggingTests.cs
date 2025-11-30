using System.Collections.Generic;
using Xunit;

namespace AdoLite.Tests.SqlServer
{
    public class DataQueryLoggingTests
    {
        [Fact]
        public void TrimSqlForLog_TruncatesWhenLongerThanLimit()
        {
            var input = new string('x', 600);
            var result = AdoLite.SqlServer.DataQuery.TrimSqlForLog(input, 100);

            Assert.True(result.Length <= 103); // 100 + "..."
            Assert.EndsWith("...", result);
        }

        [Fact]
        public void TrimSqlForLog_ReturnsEmptyForNullOrWhitespace()
        {
            Assert.Equal(string.Empty, AdoLite.SqlServer.DataQuery.TrimSqlForLog(null));
            Assert.Equal(string.Empty, AdoLite.SqlServer.DataQuery.TrimSqlForLog(string.Empty));
            Assert.Equal(string.Empty, AdoLite.SqlServer.DataQuery.TrimSqlForLog("   "));
        }

        [Fact]
        public void TrimSqlForLog_WorksForOtherProviders()
        {
            Assert.Equal(string.Empty, AdoLite.MySql.DataQuery.TrimSqlForLog(null));
            Assert.Equal(string.Empty, AdoLite.Postgres.DataQuery.TrimSqlForLog("   "));
        }

        [Fact]
        public void ToLoggableParameters_HandlesNull()
        {
            var result = AdoLite.SqlServer.DataQuery.ToLoggableParameters(null);
            Assert.Empty(result);
        }

        [Fact]
        public void ToLoggableParameters_ConvertsValues()
        {
            var parameters = new Dictionary<string, string>
            {
                { "@id", "1" },
                { "@name", null }
            };

            var result = AdoLite.SqlServer.DataQuery.ToLoggableParameters(parameters);

            Assert.Equal("1", result["@id"]);
            Assert.Equal("null", result["@name"]);
        }

        [Fact]
        public void ToLoggableParameters_WorksForMySql()
        {
            var parameters = new Dictionary<string, string>
            {
                { "@p", "v" }
            };

            var result = AdoLite.MySql.DataQuery.ToLoggableParameters(parameters);
            Assert.Equal("v", result["@p"]);
        }
    }
}

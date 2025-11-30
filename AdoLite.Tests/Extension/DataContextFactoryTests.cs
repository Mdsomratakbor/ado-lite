using System;
using System.Collections.Generic;
using System.Data;
using AdoLite.Core.Base;
using AdoLite.Core.Enums;
using AdoLite.Core.Interfaces;
using AdoLite.Extension;
using AdoLite.SqlServer;
using Xunit;

namespace AdoLite.Tests.Extension
{
    public class DataContextFactoryTests
    {
        [Fact]
        public void Create_ReturnsSqlServerContext_WhenConfigured()
        {
            var settings = new DatabaseSettings();
            settings.Connections["default"] = new DatabaseConfig
            {
                Provider = DatabaseProvider.SqlServer,
                ConnectionString = "Server=.;Database=Dummy;Integrated Security=True;"
            };

            var factory = new DataContextFactory(settings, new FakeJsonServices());
            var context = factory.Create("default");

            Assert.IsType<MsSqlDataContext>(context);
        }

        [Fact]
        public void Create_Throws_WhenKeyMissing()
        {
            var settings = new DatabaseSettings();
            var factory = new DataContextFactory(settings, new FakeJsonServices());

            Assert.Throws<ArgumentException>(() => factory.Create("unknown"));
        }

        private sealed class FakeJsonServices : IDataJSONServices
        {
            public DataSet JSONToDataSet(string json) => new();
            public DataTable JSONToDataTable(string json) => new();
            public List<T> JSONToList<T>(string json) => new();
            public T JSONToObject<T>(string json) => Activator.CreateInstance<T>();
            public string DataSetToJSON(DataSet dataset) => string.Empty;
            public string DataTableToJSON(DataTable dataTable) => string.Empty;
            public string ListToJSON<T>(List<T> list) => string.Empty;
            public string ObjectToJSON(object obj) => string.Empty;
        }
    }
}

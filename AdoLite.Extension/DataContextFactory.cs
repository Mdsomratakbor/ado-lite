using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Base;
using AdoLite.Core.Enums;
using AdoLite.Core.Interfaces;
using AdoLite.MySql;
using AdoLite.Postgres;
using AdoLite.SqlServer;

namespace AdoLite.Extension
{
    public class DataContextFactory : IDataContextFactory
    {
        private readonly DatabaseSettings _settings;
        private readonly IDataJSONServices _jsonServices;

        public DataContextFactory(DatabaseSettings settings, IDataJSONServices jsonServices)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _jsonServices = jsonServices ?? throw new ArgumentNullException(nameof(jsonServices));
        }

        public IDataContext Create(string connectionKey)
        {
            if (!_settings.Connections.TryGetValue(connectionKey, out var dbConfig))
                throw new ArgumentException($"Connection key '{connectionKey}' not found in configuration.");

            return dbConfig.Provider switch
            {
                DatabaseProvider.PostgreSQL => new PostgresDataContext(dbConfig.ConnectionString, _jsonServices),
                DatabaseProvider.SqlServer => new MsSqlDataContext(dbConfig.ConnectionString, _jsonServices),
                DatabaseProvider.MySQL => new MySqlDataContext(dbConfig.ConnectionString, _jsonServices),
                _ => throw new NotSupportedException($"Database provider '{dbConfig.Provider}' is not supported.")
            };
        }
    }
}

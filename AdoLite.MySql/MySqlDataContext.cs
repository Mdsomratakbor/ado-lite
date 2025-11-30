using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdoLite.MySql
{
    public class MySqlDataContext : DataQuery, IDataContext
    {
        public IDataJSONServices JsonServices { get; }

        public MySqlDataContext(
            string connectionString,
            IDataJSONServices jsonServices,
            ILogger<DataQuery>? logger = null
            ) : base(connectionString, logger)
        {
            JsonServices = jsonServices ?? throw new ArgumentNullException(nameof(jsonServices));
        }


    }
}

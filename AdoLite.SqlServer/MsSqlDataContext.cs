using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdoLite.SqlServer
{
   
        public class MsSqlDataContext : DataQuery, IDataContext
        {
            public IDataJSONServices JsonServices { get; }

            public MsSqlDataContext(
                string connectionString,
                IDataJSONServices jsonServices,
                ILogger<DataQuery>? logger = null
                ) : base(connectionString, logger)
            {
                JsonServices = jsonServices ?? throw new ArgumentNullException(nameof(jsonServices));
            }

 
        }

    }

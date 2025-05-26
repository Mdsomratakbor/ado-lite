using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;

namespace AdoLite.Postgres
{
   
        public class PostgresDataContext : DataQuery, IDataContext
        {
            public IDataJSONServices JsonServices { get; }

            public PostgresDataContext(
                string connectionString,
                IDataJSONServices jsonServices
                ) : base(connectionString)
            {
                JsonServices = jsonServices ?? throw new ArgumentNullException(nameof(jsonServices));
            }

 
        }

    }

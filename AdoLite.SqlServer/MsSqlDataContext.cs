using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;

namespace AdoLite.SqlServer
{
   
        public class MsSqlDataContext : DataQuery, IDataContext
        {
            public IDataJSONServices JsonServices { get; }

            public MsSqlDataContext(
                string connectionString,
                IDataJSONServices jsonServices
                ) : base(connectionString)
            {
                JsonServices = jsonServices ?? throw new ArgumentNullException(nameof(jsonServices));
            }

 
        }

    }

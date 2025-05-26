using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;

namespace AdoLite.Core.Base
{
    public abstract class BaseDatabaseConnection : IDatabaseConnection
    {
        public string ConnectionString { get;  set; }

        public BaseDatabaseConnection(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        

    }
}

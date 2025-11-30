using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Enums;

namespace AdoLite.Core.Base
{

    public class DatabaseSettings
    {
        public Dictionary<string, DatabaseConfig> Connections { get; set; } = new();
    }

    public class DatabaseConfig
    {
        public DatabaseProvider Provider { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Common.Enums;

namespace AdoLite.Core.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        IDatabaseConnection CreateConnection(DatabaseProvider provider, string connectionString);
    }
}

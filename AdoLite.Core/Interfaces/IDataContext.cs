using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataContext : IDataQuery,
    IDataQueryAsync,
    IDataTransaction,
    IDataTransactionAsync
    {
        IDataJSONServices JsonServices { get; }
    }
}

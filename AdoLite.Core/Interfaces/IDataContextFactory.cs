using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IDataContextFactory
    {
        IDataContext Create(string connectionKey);
    }
}

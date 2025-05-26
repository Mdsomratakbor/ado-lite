using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Core.Interfaces
{
    public interface IQueryPattern
    {
        /// <summary>
        /// SQL Query  Statements
        /// </summary>
        string Query { get; set; }

        /// <summary>
        /// Query All Parameter List
        /// </summary>
        List<Dictionary<string, object>> Parameters { get; set; }

    }
}

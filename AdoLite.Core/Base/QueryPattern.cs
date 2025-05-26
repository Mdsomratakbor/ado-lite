using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;

namespace AdoLite.Core.Base;
public class QueryPattern : IQueryPattern
    {
        public string Query { get; set; }
        public List<Dictionary<string, object>> Parameters { get; set; } = new List<Dictionary<string, object>>();
    }


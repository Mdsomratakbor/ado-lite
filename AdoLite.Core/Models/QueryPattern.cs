using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoLite.Core.Interfaces;

namespace AdoLite.Core.Models;

public class QueryPattern : IQueryPattern
{
    public string Query { get; set; }
    public List<Dictionary<string, object>> Parameters { get; set; } = new();

    public QueryPattern(string query)
    {
        Query = query;
    }

    public QueryPattern(string query, Dictionary<string, object> parameters)
    {
        Query = query;
        Parameters.Add(parameters);
    }
}


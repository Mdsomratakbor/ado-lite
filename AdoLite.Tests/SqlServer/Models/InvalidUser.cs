using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite.Tests.SqlServer.Models
{
    public class InvalidUser
    {
        public DateTime Id { get; set; } // Intentionally wrong type
        public string Name { get; set; }
    }
}

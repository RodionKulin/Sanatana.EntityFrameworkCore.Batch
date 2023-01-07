using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.DbContextExtentions
{
    public class IsUtcAttribute : Attribute
    {
        public bool IsUtc { get; }
        public IsUtcAttribute(bool isUtc = true)
        {
            IsUtc = isUtc;
        }
    }
}

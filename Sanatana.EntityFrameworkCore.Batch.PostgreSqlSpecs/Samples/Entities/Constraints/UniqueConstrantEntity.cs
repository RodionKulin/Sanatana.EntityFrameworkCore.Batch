using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities.Constraints
{
    public class UniqueConstrantEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? StringProperty { get; set; }
        public DateTime DateProperty { get; set; }
        public int Counter { get; set; }
    }
}

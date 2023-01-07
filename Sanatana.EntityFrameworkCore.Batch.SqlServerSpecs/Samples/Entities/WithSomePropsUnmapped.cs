using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities
{
    public class WithSomePropsUnmapped
    {
        public int Id { get; set; }
        public string? MappedProp1 { get; set; }
        public DateTime MappedProp2 { get; set; }
        [NotMapped]
        public int NotMappedProp1 { get; set; }
        public int NotMappedProp2 { get; set; }
    }
}

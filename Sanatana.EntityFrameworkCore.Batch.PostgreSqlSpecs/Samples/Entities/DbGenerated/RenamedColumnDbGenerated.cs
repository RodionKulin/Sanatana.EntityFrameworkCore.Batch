using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities
{
    public class RenamedColumnDbGenerated
    {
        public int CustomId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? HelloIAmAProp { get; set; }
    }
}

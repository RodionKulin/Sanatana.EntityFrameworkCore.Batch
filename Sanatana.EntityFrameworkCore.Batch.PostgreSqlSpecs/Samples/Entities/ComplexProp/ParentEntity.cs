using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities
{
    public class ParentEntity
    {
        public int ParentEntityId { get; set; }
        public DateTime CreatedTime { get; set; }
        public EmbeddedEntity Embedded { get; set; }
    }
}

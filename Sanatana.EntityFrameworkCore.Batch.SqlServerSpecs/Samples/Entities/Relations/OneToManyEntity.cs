﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities
{
    public class OneToManyEntity
    {
        public int OneToManyEntityId { get; set; }
        public string? Name { get; set; }


        public virtual ICollection<ManyToOneEntity> ManyToOnes { get; set; }
    }
}

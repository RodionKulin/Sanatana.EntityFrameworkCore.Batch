﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities
{
    public class ConventionKeyDbGenerated
    {
        public long Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? SimpleProp { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities
{
    public class EmbeddedEntity
    {
        public bool IsActive { get; set; }
        public string? Address { get; set; }

    }
}

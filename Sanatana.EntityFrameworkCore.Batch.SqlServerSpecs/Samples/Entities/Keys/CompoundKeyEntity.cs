﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities
{
    public class CompoundKeyEntity
    {
        public int CompoundKeyNumber { get; set; }
        public string? CompoundKeyString { get; set; }
    }
}

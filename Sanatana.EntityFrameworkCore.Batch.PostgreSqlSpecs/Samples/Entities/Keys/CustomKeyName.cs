﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities
{
    public class CustomKeyName
    {
        public string? CustomKey { get; set; }
        public string? IWillBeComputedInDatabase { get; set; }
    }
}

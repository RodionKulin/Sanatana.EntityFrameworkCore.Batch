using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples;
using SpecsFor;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Interfaces
{
    public interface INeedSampleDatabase : ISpecs
    {
        SampleDbContext SampleDatabase { get; set; }
    }
}

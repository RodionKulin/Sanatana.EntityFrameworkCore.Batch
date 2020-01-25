using Sanatana.EntityFrameworkCore.BatchSpecs.Samples;
using SpecsFor;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Interfaces
{
    public interface INeedSampleDatabase : ISpecs
    {
        SampleDbContext SampleDatabase { get; set; }
    }
}

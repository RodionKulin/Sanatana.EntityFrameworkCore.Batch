using Sanatana.EntityFrameworkCore.Commands.Tests.Samples;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCoreSpecs.TestTools.Interfaces
{
    public interface INeedSampleDatabase : ISpecs
    {
        SampleDbContext SampleDatabase { get; set; }
    }
}

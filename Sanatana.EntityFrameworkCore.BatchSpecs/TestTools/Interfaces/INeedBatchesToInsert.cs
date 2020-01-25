using Sanatana.EntityFrameworkCore.BatchSpecs.Samples;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples.Entities;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Interfaces
{
    public interface INeedBatchesToInsert : ISpecs
    {
        string MarkerStringProperty { get; set; }
        List<SampleEntity> InsertItems { get; set; }


    }
}

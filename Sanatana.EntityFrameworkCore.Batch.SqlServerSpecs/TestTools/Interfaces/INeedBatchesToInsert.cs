using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Interfaces
{
    public interface INeedBatchesToInsert : ISpecs
    {
        string MarkerStringProperty { get; set; }
        List<SampleEntity> InsertItems { get; set; }


    }
}

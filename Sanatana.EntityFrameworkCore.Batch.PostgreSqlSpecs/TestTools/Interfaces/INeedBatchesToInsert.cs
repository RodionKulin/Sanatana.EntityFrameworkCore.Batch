using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces
{
    public interface INeedBatchesToInsert : ISpecs
    {
        string MarkerStringProperty { get; set; }
        List<SampleEntity> InsertItems { get; set; }


    }
}

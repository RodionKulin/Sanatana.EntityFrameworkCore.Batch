using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.StructureMap;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.SqlServer;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Providers
{
    public class BatchesToInsertProvider : Behavior<INeedBatchesToInsert>
    {
        public override void SpecInit(INeedBatchesToInsert instance)
        {
            instance.MarkerStringProperty = instance.GetType().FullName;
            instance.InsertItems = new List<SampleEntity>();

            //Parameters count is larger then MaxParametersPerCommand 3 times.
            int paramsPerEntity = typeof(SampleEntity).GetProperties().Length
                - 1; //except Id column that is database generated and not sent
            int maxParameters = new SqlParametersService().MaxParametersPerCommand;
            int entitiesCount = maxParameters / paramsPerEntity - 100;
            entitiesCount = entitiesCount * 3;

            for (int i = 0; i < entitiesCount; i++)
            {
                instance.InsertItems.Add(new SampleEntity
                {
                    IntNotNullProperty = 1,
                    IntProperty = 1,
                    XmlProperty = "<xml></xml>",
                    GuidNullableProperty = null,
                    DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                    GuidProperty = Guid.NewGuid(),
                    StringProperty = instance.MarkerStringProperty
                });
            }
        }

    }
}

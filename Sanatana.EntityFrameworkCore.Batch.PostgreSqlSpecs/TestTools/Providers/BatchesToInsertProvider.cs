using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.StructureMap;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Providers
{
    public class BatchesToInsertProvider : Behavior<INeedBatchesToInsert>
    {
        public override void SpecInit(INeedBatchesToInsert instance)
        {
            instance.MarkerStringProperty = instance.GetType().FullName;
            instance.InsertItems = new List<SampleEntity>();

            //Parameters count should be less then MaxParametersPerCommand
            //Insert does not support batch insert.
            int paramsPerEntity = typeof(SampleEntity).GetProperties().Length 
                - 1; //except Id column that is database generated and not sent
            int maxParameters = new PostgreParametersService().MaxParametersPerCommand;
            int entitiesCount = maxParameters / paramsPerEntity;

            for (int i = 0; i < entitiesCount; i++)
            {
                instance.InsertItems.Add(new SampleEntity
                {
                    IntNotNullProperty = 1,
                    IntProperty = 1,
                    XmlProperty = "<xml></xml>",
                    GuidNullableProperty = null,
                    DateProperty = new DateTime(2000, 2, 2, 2, 2, 0, DateTimeKind.Utc),
                    GuidProperty = Guid.NewGuid(),
                    StringProperty = instance.MarkerStringProperty
                });
            }

        }


    }
}

using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Interfaces;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.ShouldExtensions;

namespace Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Providers
{
    public class BatchesToInsertProvider : Behavior<INeedBatchesToInsert>
    {
        public override void SpecInit(INeedBatchesToInsert instance)
        {
            instance.MarkerStringProperty = instance.GetType().FullName;
            instance.InsertItems = new List<SampleEntity>();

            int paramsPerEntity = typeof(SampleEntity).GetProperties().Length;
            int maxParameters = EntityFrameworkConstants.MAX_NUMBER_OF_SQL_COMMAND_PARAMETERS;
            int entitiesInBatch = maxParameters / paramsPerEntity;
            int entitiesCount = entitiesInBatch * 3;

            for (int i = 0; i < entitiesCount; i++)
            {
                instance.InsertItems.Add(new SampleEntity
                {
                    GuidNullableProperty = null,
                    DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                    GuidProperty = Guid.NewGuid(),
                    StringProperty = instance.MarkerStringProperty
                });
            }

        }


    }
}

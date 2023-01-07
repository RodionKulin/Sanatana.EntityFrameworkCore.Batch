using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Interfaces;
using NUnit.Framework;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.StructureMap;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.SqlServer.Repositories;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Specs
{
    public class RepositoryAsyncSpecs
    {

        [TestFixture]
        public class when_insert_one : SpecsFor<SqlRepositoryAsync>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public async Task then_it_inserts()
            {
                var entity = new ParentEntity
                {
                    CreatedTime = DateTime.Now,
                    Embedded = new EmbeddedEntity
                    {
                        Address = "address1",
                        IsActive = true
                    }
                };

                await SUT.InsertOne(entity);
            }
        }


        [TestFixture]
        public class when_requesting_select_and_count_in_parallel : SpecsFor<SqlRepositoryAsync>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public async Task then_query_completes()
            {
                var entity = new ParentEntity
                {
                    CreatedTime = DateTime.Now,
                    Embedded = new EmbeddedEntity
                    {
                        Address = "address1",
                        IsActive = true
                    }
                };

                TotalResult<ParentEntity> total = await SUT.SelectPage<ParentEntity, DateTime>(
                    1, 10, true, x => true, x => x.CreatedTime, true);

            }
        }

    }
}

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
    public class RepositorySpecs
    {

        [TestFixture]
        public class when_insert_one : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_inserts()
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

                SUT.InsertOne(entity);
            }
        }


        [TestFixture]
        public class when_inserting_one_generic : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_inserts_generic()
            {
                var entity = new GenericEntity<int>
                {
                    EntityId = 0,
                    Name = "Name1"
                };

                SUT.InsertOne(entity);
            }
        }



        [TestFixture]
        public class when_selecting_page : SpecsFor<SqlRepository>
         , INeedSampleDatabase
        {
            private Guid _commonGuidValue = Guid.NewGuid();
            private int _entitiesCount = 15;
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                var entities = new List<SampleEntity>();
                for (int i = 0; i < _entitiesCount; i++)
                {
                    entities.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = DateTime.UtcNow,
                        GuidProperty = _commonGuidValue
                    });
                }

                InsertCommand<SampleEntity> command = SUT.InsertManyCommand(entities);
                command.Insert.ExcludeProperty(x => x.Id);
                int changes = command.Execute();
            }

            [Test]
            public void then_it_selects_page_of_entities()
            {
                int pageSize = 10;

                TotalResult<SampleEntity> select = SUT.SelectPage<SampleEntity, DateTime?>(
                    0, pageSize, true
                    , x => x.GuidProperty == _commonGuidValue
                    , x => x.DateProperty
                    , true);

                select.Data.Count.ShouldEqual(pageSize);
                select.TotalRows.ShouldEqual(_entitiesCount);
            }
        }

    }
}

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

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Specs.Commands
{
    public class InsertCommandSpecs
    {

        [TestFixture]
        public class when_inserting_multiple_entities : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_inserts_multiple_entities()
            {
                int insertCount = 15;
                var entities = new List<SampleEntity>();
                for (int i = 0; i < insertCount; i++)
                {
                    entities.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = DateTime.MinValue,
                        GuidProperty = Guid.NewGuid()
                    });
                }

                InsertCommand<SampleEntity> command = SUT.InsertManyCommand(entities);
                command.Insert.ExcludeProperty(x => x.Id);
                int changes = command.Execute();

                changes.ShouldEqual(insertCount);
            }
        }

        [TestFixture]
        public class when_inserting_multiple_entities_with_output : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_inserts_multiple_entities_and_outputs_ids()
            {
                int insertCount = 15;
                var entities = new List<SampleEntity>();
                for (int i = 0; i < insertCount; i++)
                {
                    entities.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = DateTime.UtcNow,
                        GuidProperty = Guid.NewGuid()
                    });
                }

                InsertCommand<SampleEntity> command = SUT.InsertManyCommand(entities);
                command.Insert.ExcludeProperty(x => x.Id);
                command.Output.IncludeProperty(x => x.Id);
                int changes = command.Execute();

                changes.ShouldEqual(insertCount);
                entities.ForEach(
                    (entity) => entity.Id.ShouldNotEqual(0));
            }
        }



        [TestFixture]
        public class when_inserting_collection_navigation_property : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            private OneToManyEntity _parentEntity;
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                _parentEntity = new OneToManyEntity
                {
                    Name = "parent1"
                };
                SampleDatabase.Set<OneToManyEntity>().Add(_parentEntity);
                SampleDatabase.SaveChanges();
            }

            [Test]
            public void then_it_inserts_collection_navigation_property()
            {
                var entities = new List<ManyToOneEntity>
                {
                    new ManyToOneEntity
                    {
                        Name = "name1",
                        OneToManyEntityId = _parentEntity.OneToManyEntityId
                    },
                    new ManyToOneEntity
                    {
                        Name = "name2",
                        OneToManyEntityId = _parentEntity.OneToManyEntityId
                    }
                };

                InsertCommand<ManyToOneEntity> command = SUT.InsertManyCommand(entities);
                command.Insert.ExcludeProperty(x => x.ManyToOneEntityId);
                int changes = command.Execute();

                changes.ShouldEqual(entities.Count);
            }
        }


        [TestFixture]
        public class when_inserting_complex_property : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_inserts_complex_property()
            {
                var entities = new List<ParentEntity>
                {
                    new ParentEntity
                    {
                        CreatedTime = DateTime.Now,
                        Embedded = new EmbeddedEntity
                        {
                            Address = "address1",
                            IsActive = true
                        }
                    },
                    new ParentEntity
                    {
                        CreatedTime = DateTime.Now.AddDays(1),
                        Embedded = new EmbeddedEntity
                        {
                            Address = "address2",
                            IsActive = true
                        }
                    }
                };

                InsertCommand<ParentEntity> command = SUT.InsertManyCommand(entities);
                command.Insert.ExcludeProperty(x => x.ParentEntityId);
                int changes = command.Execute();

                changes.ShouldEqual(entities.Count);
            }
        }

    }
}

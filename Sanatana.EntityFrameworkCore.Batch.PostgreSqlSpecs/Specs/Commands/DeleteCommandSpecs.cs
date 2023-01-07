using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
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
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Specs.Commands
{
    public class DeleteCommandSpecs
    {

        [TestFixture]
        public class when_deleting_many : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            private Guid _commonGuidValue = Guid.NewGuid();
            private int _entitiesCount = 10;
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

                InsertCommand<SampleEntity> command = SUT.InsertManyCommand(entities); ;
                command.Insert.ExcludeProperty(x => x.Id);
                int changes = command.Execute();
            }

            [Test]
            public void then_it_deletes_multiple_entities()
            {
                int changes = SUT.DeleteMany<SampleEntity>(x => x.GuidProperty == _commonGuidValue);

                changes.ShouldEqual(_entitiesCount);
            }
        }

        [TestFixture]
        public class when_deleting_by_complex_property : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            private string _address = Guid.NewGuid().ToString();
            private int _entitiesCount = 10;
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                var entities = new List<ParentEntity>();
                for (int i = 0; i < _entitiesCount; i++)
                {
                    entities.Add(new ParentEntity
                    {
                        CreatedTime = DateTime.UtcNow.AddMinutes(i),
                        Embedded = new EmbeddedEntity
                        {
                            Address = _address,
                            IsActive = true
                        }
                    });
                }

                InsertCommand<ParentEntity> command = SUT.InsertManyCommand(entities);
                command.Insert.ExcludeProperty(x => x.ParentEntityId);
                int changes = command.Execute();
            }

            [Test]
            public void then_it_deletes_by_complex_property()
            {
                int changes = SUT.DeleteMany<ParentEntity>(x => x.Embedded.Address == _address);

                changes.ShouldEqual(_entitiesCount);
            }
        }

    }
}

using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using NUnit.Framework;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SpecsFor.StructureMap;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Specs.Commands
{
    public class UpdateCommandSpecs
    {

        [TestFixture]
        public class when_updating_many : SpecsFor<PostgreRepository>
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

                InsertCommand<SampleEntity> command = SUT.InsertManyCommand(entities);
                command.Insert.ExcludeProperty(x => x.Id);
                int changes = command.Execute();
            }

            [Test]
            public void then_it_updates_multiple_entities()
            {
                UpdateCommand<SampleEntity> updateOp = SUT.UpdateMany<SampleEntity>();
                updateOp.SetWhere(x => x.GuidProperty == _commonGuidValue);
                updateOp.SetAssign(x => x.DateProperty, x => DateTime.Now);
                int changes = updateOp.Execute();

                changes.Should().Be(_entitiesCount);
            }
        }

        [TestFixture]
        public class when_updating_limited_number_of_rows : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            private Guid _commonGuidValue = Guid.NewGuid();
            private int _entitiesCount = 10;
            private int _limit = 2;
            private int _updateChanges;
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


                //execute limited Update
                UpdateCommand<SampleEntity> updateOp = SUT.UpdateMany<SampleEntity>()
                    .SetWhere(x => x.GuidProperty == _commonGuidValue)
                    .SetAssign(x => x.IntProperty, x => 5)
                    .SetLimit(_limit);
                _updateChanges = updateOp.Execute();
            }

            [Test]
            public void then_it_updates_expected_number_of_entities()
            {
                _updateChanges.Should().Be(_limit);
            }

            [Test]
            public void then_it_returns_updated_entities()
            {
                SampleEntity[] allEntities = SampleDatabase.Set<SampleEntity>()
                    .Where(x => x.GuidProperty == _commonGuidValue)
                    .ToArray();

                allEntities.Count(x => x.IntProperty == 5).Should().Be(_limit);

                int expectedUnchangedCount = _entitiesCount - _limit;
                allEntities.Count(x => x.IntProperty != 5).Should().Be(expectedUnchangedCount);
            }
        }


        [TestFixture]
        public class when_updating_many_with_output : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            private Guid _commonGuidValue = Guid.NewGuid();
            private int _entitiesCount = 10;
            private List<SampleEntity> _updatedEntities;
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

            protected override void When()
            {
                UpdateCommand<SampleEntity> updateOp = SUT.UpdateMany<SampleEntity>();
                updateOp.SetWhere(x => x.GuidProperty == _commonGuidValue);
                updateOp.SetAssign(x => x.DateProperty, x => DateTime.Now);
                updateOp.Output.SetExcludeAllByDefault(ColumnSetEnum.PrimaryKey);
                _updatedEntities = updateOp.ExecuteWithOutput();
            }

            [Test]
            public void then_it_updated_entities_return_in_output_with_expected_count()
            {
                _updatedEntities.Should().HaveCount(_entitiesCount);
            }

            [Test]
            public void then_output_entities_have_unique()
            {
                int actualUniqueCount = _updatedEntities.Select(x => x.Id).Distinct().Count();
                actualUniqueCount.Should().Be(_entitiesCount);
            }
        }

    }
}

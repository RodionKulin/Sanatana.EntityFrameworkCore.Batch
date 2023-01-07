using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using NUnit.Framework;
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
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;
using FluentAssertions;
using System.Data.Common;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities.Constraints;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Specs.Commands
{
    public class MergeCommandSpecs
    {
        [TestFixture]
        public class when_merge_inserting_multiple_entities : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            private List<SampleEntity> _insertedItems;
            private string _testName;
            private int _changes;

            public SampleDbContext SampleDatabase { get; set; }

            protected override void When()
            {
                _testName = GetType().FullName;

                _insertedItems = new List<SampleEntity>();
                for (int i = 0; i < 15; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, i, DateTimeKind.Utc),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

                IMergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, MergeTypeEnum.Insert);
                command.Output.SetExcludeAllByDefault();

                string sql = command.ConstructCommand(_insertedItems);
                _changes = command.Execute();
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_merge_inserted_entities_are_found()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == _testName)
                    .OrderBy(x => x.Id)
                    .ToList();

                for (int i = 0; i < _insertedItems.Count; i++)
                {
                    SampleEntity expectedItem = _insertedItems[i];
                    SampleEntity actualItem = actualList[i];

                    expectedItem.Id = actualItem.Id;

                    actualItem.Should().BeEquivalentTo(expectedItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_upserts_multiple_entities : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            private List<UniqueConstrantEntity> _insertedItems;
            private string _testName;
            private int _changes;

            public SampleDbContext SampleDatabase { get; set; }


            protected override void Given()
            {
                _testName = GetType().FullName;

                _insertedItems = new List<UniqueConstrantEntity>();
                for (int i = 0; i < 15; i++)
                {
                    _insertedItems.Add(new UniqueConstrantEntity
                    {
                        StringProperty = _testName,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, i, DateTimeKind.Utc),
                        Name = $"Name{i}",
                        Counter = 1
                    });
                }

                SUT.DeleteMany<UniqueConstrantEntity>(x => true);
                SampleDatabase.UniqueConstrantEntities.AddRange(_insertedItems);
                SampleDatabase.SaveChanges();
            }

            protected override void When()
            {
                for (int i = 0; i < _insertedItems.Count; i++)
                {
                    _insertedItems[i].DateProperty = new DateTime(2005, 2, 2, 2, 2, i, DateTimeKind.Utc);
                }

                IMergeCommand<UniqueConstrantEntity> command = SUT.Merge(_insertedItems, MergeTypeEnum.Upsert);
                command.Source
                    .IncludeProperty(x => x.Id)
                    .IncludeProperty(x => x.DateProperty)
                    .IncludeProperty(x => x.Name);
                command.On
                    .IncludeProperty(x => x.Name);
                command.Insert
                    .IncludeDefaultValue(x => x.DateProperty)
                    .IncludeDefaultValue(x => x.Name);
                command.SetMatched
                    .IncludeProperty(x => x.DateProperty)
                    .Assign(target => target.Counter, (target, source) => target.Counter + source.Counter + 1);

                _changes = command.Execute();
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_merge_updated_entities_are_found()
            {
                List<UniqueConstrantEntity> actualList = SampleDatabase.UniqueConstrantEntities
                    .Where(x => x.StringProperty == _testName)
                    .OrderBy(x => x.Id)
                    .ToList();

                for (int i = 0; i < _insertedItems.Count; i++)
                {
                    UniqueConstrantEntity actualItem = actualList[i];

                    UniqueConstrantEntity expectedItem = _insertedItems[i];
                    expectedItem.Id = actualItem.Id;
                    expectedItem.Counter = 3;

                    expectedItem.Should().BeEquivalentTo(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_uses_output : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_merge_inserts_multiple_entities_with_output_ids()
            {
                int insertCount = 5;
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

                IMergeCommand<SampleEntity> command = SUT.Merge(entities, MergeTypeEnum.Insert);
                int changes = command.Execute();

                //Assert
                changes.ShouldEqual(insertCount);
                entities.ForEach(
                    (entity) => entity.Id.ShouldNotEqual(0));
            }
        }

        [TestFixture]
        public class when_merge_in_several_batched : SpecsFor<PostgreRepository>
           , INeedSampleDatabase, INeedBatchesToInsert
        {
            private int _changes;

            public string MarkerStringProperty { get; set; }
            public List<SampleEntity> InsertItems { get; set; }
            public SampleDbContext SampleDatabase { get; set; }


            protected override void When()
            {
                IMergeCommand<SampleEntity> command = SUT.Merge(InsertItems, MergeTypeEnum.Insert);
                command.On.IncludeProperty(x => x.Id);
                _changes = command.Execute();
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(InsertItems.Count);
            }

            [Test]
            public void then_merge_inserted_entities_in_several_batches_are_found()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == MarkerStringProperty)
                    .OrderBy(x => x.Id)
                    .ToList();

                for (int i = 0; i < InsertItems.Count; i++)
                {
                    SampleEntity actualItem = actualList[i];

                    SampleEntity expectedItem = InsertItems[i];
                    expectedItem.Id = actualItem.Id;

                    expectedItem.Should().BeEquivalentTo(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_with_output_uses_external_commited_transaction : SpecsFor<PostgreRepository>
         , INeedSampleDatabase
        {
            private List<SampleEntity> _insertedItems;
            private string _testName;
            private int _changes;

            public SampleDbContext SampleDatabase { get; set; }


            protected override void Given()
            {
                _testName = GetType().FullName;

                _insertedItems = new List<SampleEntity>();
                int entitiesCount = 10;
                for (int i = 0; i < entitiesCount; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0, DateTimeKind.Utc),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    DbTransaction transaction = ts.GetDbTransaction();

                    IMergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, MergeTypeEnum.Insert, transaction);
                    command.On.IncludeProperty(x => x.Id);
                    _changes = command.Execute();

                    ts.Commit();

                    //Assert changes count

                }
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_inserted_entities_in_transaction_are_saved()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == _testName)
                    .OrderBy(x => x.Id)
                    .ToList();

                actualList.Count.ShouldEqual(_insertedItems.Count);

                for (int i = 0; i < _insertedItems.Count; i++)
                {
                    SampleEntity actualItem = actualList[i];

                    SampleEntity expectedItem = _insertedItems[i];
                    expectedItem.Id = actualItem.Id;

                    expectedItem.Should().BeEquivalentTo(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_with_output_uses_external_uncommited_transaction : SpecsFor<PostgreRepository>
            , INeedSampleDatabase
        {
            private List<SampleEntity> _insertedItems;
            private string _testName;
            private int _changes;

            public SampleDbContext SampleDatabase { get; set; }


            protected override void Given()
            {
                _testName = GetType().FullName;

                _insertedItems = new List<SampleEntity>();
                int entitiesCount = 10;
                for (int i = 0; i < entitiesCount; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0, DateTimeKind.Utc),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    DbTransaction transaction = ts.GetDbTransaction();

                    IMergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, MergeTypeEnum.Insert, transaction);
                    command.On.IncludeProperty(x => x.Id);
                    _changes = command.Execute();

                }
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_inserted_entities_in_transaction_are_not_saved()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == _testName)
                    .ToList();

                actualList.Count.ShouldEqual(0);
            }
        }

        [TestFixture]
        public class when_merge_without_output_uses_external_commited_transaction : SpecsFor<PostgreRepository>
         , INeedSampleDatabase
        {
            private List<SampleEntity> _insertedItems;
            private string _testName;
            private int _changes;

            public SampleDbContext SampleDatabase { get; set; }


            protected override void Given()
            {
                _testName = GetType().FullName;

                _insertedItems = new List<SampleEntity>();
                int entitiesCount = 10;
                for (int i = 0; i < entitiesCount; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0, DateTimeKind.Utc),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    DbTransaction transaction = ts.GetDbTransaction();

                    IMergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, MergeTypeEnum.Insert, transaction);
                    command.On.IncludeProperty(x => x.Id);
                    command.Output.SetExcludeAllByDefault();
                    _changes = command.Execute();

                    ts.Commit();
                }
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_inserted_entities_in_transaction_are_saved()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == _testName)
                    .OrderBy(x => x.Id)
                    .ToList();

                actualList.Count.ShouldEqual(_insertedItems.Count);

                for (int i = 0; i < _insertedItems.Count; i++)
                {
                    SampleEntity actualItem = actualList[i];

                    SampleEntity expectedItem = _insertedItems[i];
                    expectedItem.Id = actualItem.Id;

                    expectedItem.Should().BeEquivalentTo(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_without_output_uses_external_uncommited_transaction : SpecsFor<PostgreRepository>
            , INeedSampleDatabase
        {
            private List<SampleEntity> _insertedItems;
            private string _testName;
            private int _changes;

            public SampleDbContext SampleDatabase { get; set; }


            protected override void Given()
            {
                _testName = GetType().FullName;

                _insertedItems = new List<SampleEntity>();
                int entitiesCount = 10;
                for (int i = 0; i < entitiesCount; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0, DateTimeKind.Utc),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    DbTransaction transaction = ts.GetDbTransaction();

                    IMergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, MergeTypeEnum.Insert, transaction);
                    command.On.IncludeProperty(x => x.Id);
                    command.Output.SetExcludeAllByDefault();
                    _changes = command.Execute();

                }
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_inserted_entities_in_transaction_are_not_saved()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == _testName)
                    .ToList();

                actualList.Count.ShouldEqual(0);
            }
        }

        [TestFixture]
        public class when_merge_batches_without_output_uses_internal_commited_transaction :
            SpecsFor<PostgreRepository>, INeedSampleDatabase, INeedBatchesToInsert
        {
            private int _changes;

            public string MarkerStringProperty { get; set; }
            public List<SampleEntity> InsertItems { get; set; }
            public SampleDbContext SampleDatabase { get; set; }


            protected override void When()
            {
                IMergeCommand<SampleEntity> command = SUT.Merge(InsertItems, MergeTypeEnum.Insert);
                command.On.IncludeProperty(x => x.Id);
                command.Output.SetExcludeAllByDefault();
                _changes = command.Execute();
            }

            [Test]
            public void then_inserted_entities_in_transaction_are_saved()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == MarkerStringProperty)
                    .OrderBy(x => x.Id)
                    .ToList();

                actualList.Count.ShouldEqual(InsertItems.Count);

                for (int i = 0; i < InsertItems.Count; i++)
                {
                    SampleEntity actualItem = actualList[i];

                    SampleEntity expectedItem = InsertItems[i];
                    expectedItem.Id = actualItem.Id;

                    expectedItem.Should().BeEquivalentTo(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_batches_uses_ambient_transaction : SpecsFor<PostgreRepository>
            , INeedSampleDatabase, INeedBatchesToInsert
        {
            public string MarkerStringProperty { get; set; }
            public List<SampleEntity> InsertItems { get; set; }
            public SampleDbContext SampleDatabase { get; set; }

            protected override void When()
            {
                using (var scope = new TransactionScope())
                {
                    IMergeCommand<SampleEntity> command1 = SUT.Merge(InsertItems, MergeTypeEnum.Insert);
                    command1.UseInnerTransactionForBatches = false;
                    command1.Execute();

                    IMergeCommand<SampleEntity> command2 = SUT.Merge(InsertItems, MergeTypeEnum.Insert);
                    command2.UseInnerTransactionForBatches = false;
                    command2.Execute();

                    scope.Complete();
                }
            }

            [Test]
            public void then_inserted_entities_in_transaction_are_saved()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == MarkerStringProperty)
                    .OrderBy(x => x.Id)
                    .ToList();

                var expectedList = new List<SampleEntity>();
                expectedList.AddRange(InsertItems);
                expectedList.AddRange(InsertItems);

                actualList.Count.ShouldEqual(expectedList.Count);

                for (int i = 0; i < expectedList.Count; i++)
                {
                    SampleEntity actualItem = actualList[i];
                    SampleEntity expectedItem = expectedList[i];
                    expectedItem.Id = actualItem.Id;

                    expectedItem.Should().BeEquivalentTo(actualItem);
                }
            }

        }
    }
}

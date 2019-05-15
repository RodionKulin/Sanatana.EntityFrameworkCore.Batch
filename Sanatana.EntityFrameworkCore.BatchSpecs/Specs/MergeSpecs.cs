using Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Interfaces;
using NUnit.Framework;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.ShouldExtensions;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.ColumnMapping;
using Sanatana.EntityFrameworkCore.Batch;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace Sanatana.EntityFrameworkCore.BatchSpecs.Specs
{
    public class MergeSpecs
    {
        [TestFixture]
        public class when_merge_inserting_multiple_entities : SpecsFor<Repository>
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
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, i),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

                MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems);
                command.Output.ExcludeDbGeneratedByDefault = ExcludeOptions.Exclude;

                string sql = command.ConstructCommand(MergeType.Insert, _insertedItems);
                _changes = command.Execute(MergeType.Insert);
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

                    expectedItem.ShouldLookLike(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_uses_output : SpecsFor<Repository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_merge_inserts_multiple_entities_with_output_ids()
            {
                int insertCount = 15;
                var entities = new List<SampleEntity>();
                for (int i = 0; i < 15; i++)
                {
                    entities.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = DateTime.UtcNow,
                        GuidProperty = Guid.NewGuid()
                    });
                }

                MergeCommand<SampleEntity> command = SUT.Merge<SampleEntity>(entities);
                int changes = command.Execute(MergeType.Insert);

                //Assert
                changes.ShouldEqual(insertCount);
                entities.ForEach(
                    (entity) => entity.Id.ShouldNotEqual(0));
            }
        }
        
        [TestFixture]
        public class when_merge_updates_multiple_entities : SpecsFor<Repository>
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
                for (int i = 0; i < 15; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, i),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

                SampleDatabase.SampleEntities.AddRange(_insertedItems);
                SampleDatabase.SaveChanges();
            }

            protected override void When()
            {
                for (int i = 0; i < 15; i++)
                {
                    _insertedItems[i].DateProperty = new DateTime(2005, 2, 2, 2, 2, i);
                }

                MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems);
                command.Source
                    .IncludeProperty(x => x.Id)
                    .IncludeProperty(x => x.DateProperty);
                command.Compare
                    .IncludeProperty(x => x.Id);
                command.UpdateMatched
                    .IncludeProperty(x => x.DateProperty);
                _changes = command.Execute(MergeType.Update);                
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_merge_updated_entities_are_found()
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

                    expectedItem.ShouldLookLike(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_in_several_batched : SpecsFor<Repository>
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
                int propertiesCount = typeof(SampleEntity).GetProperties().Length;
                int maxParameters = EntityFrameworkConstants.MAX_NUMBER_OF_SQL_COMMAND_PARAMETERS;
                int entitiesCount = maxParameters / propertiesCount * 2;
                for (int i = 0; i < entitiesCount; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems);
                command.Compare.IncludeProperty(x => x.Id);
                _changes = command.Execute(MergeType.Insert);

                //Assert
                
            }

            [Test]
            public void then_number_of_changes_equals_inserted_items_count()
            {
                _changes.ShouldEqual(_insertedItems.Count);
            }

            [Test]
            public void then_merge_inserted_entities_in_several_batches_are_found()
            {
                List<SampleEntity> actualList = SampleDatabase.SampleEntities
                    .Where(x => x.StringProperty == _testName)
                    .OrderBy(x => x.Id)
                    .ToList();

                for (int i = 0; i < _insertedItems.Count; i++)
                {
                    SampleEntity actualItem = actualList[i];

                    SampleEntity expectedItem = _insertedItems[i];
                    expectedItem.Id = actualItem.Id;

                    expectedItem.ShouldLookLike(actualItem);
                }
            }
        }
        
        [TestFixture]
        public class when_merge_with_output_uses_external_commited_transaction : SpecsFor<Repository>
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
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    SqlTransaction transaction = (SqlTransaction)ts.GetDbTransaction();

                    MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, transaction);
                    command.Compare.IncludeProperty(x => x.Id);
                    _changes = command.Execute(MergeType.Insert);

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

                    expectedItem.ShouldLookLike(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_with_output_uses_external_uncommited_transaction : SpecsFor<Repository>
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
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    SqlTransaction transaction = (SqlTransaction)ts.GetDbTransaction();

                    MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, transaction);
                    command.Compare.IncludeProperty(x => x.Id);
                    _changes = command.Execute(MergeType.Insert);

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
        public class when_merge_without_output_uses_external_commited_transaction : SpecsFor<Repository>
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
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    SqlTransaction transaction = (SqlTransaction)ts.GetDbTransaction();

                    MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, transaction);
                    command.Compare.IncludeProperty(x => x.Id);
                    command.Output
                        .SetExcludeAllByDefault(true)
                        .SetExcludeDbGeneratedByDefault(ExcludeOptions.Exclude);
                    _changes = command.Execute(MergeType.Insert);

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

                    expectedItem.ShouldLookLike(actualItem);
                }
            }
        }

        [TestFixture]
        public class when_merge_without_output_uses_external_uncommited_transaction : SpecsFor<Repository>
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
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }

            }

            protected override void When()
            {
                using (IDbContextTransaction ts = SampleDatabase.Database.BeginTransaction())
                {
                    SqlTransaction transaction = (SqlTransaction)ts.GetDbTransaction();

                    MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems, transaction);
                    command.Compare.IncludeProperty(x => x.Id);
                    command.Output
                        .SetExcludeAllByDefault(true)
                        .SetExcludeDbGeneratedByDefault(ExcludeOptions.Exclude);
                    _changes = command.Execute(MergeType.Insert);

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
        public class when_merge_batches_without_output_uses_internal_commited_transaction : SpecsFor<Repository>
            , INeedSampleDatabase
        {
            private List<SampleEntity> _insertedItems;
            private string _testName;
            private int _changes;

            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                _testName = GetType().FullName;

                int paramsPerEntity = typeof(SampleEntity).GetProperties().Length;
                int maxParameters = EntityFrameworkConstants.MAX_NUMBER_OF_SQL_COMMAND_PARAMETERS;
                int entitiesInBatch = maxParameters / paramsPerEntity;
                int entitiesCount = entitiesInBatch * 3;

                _insertedItems = new List<SampleEntity>();

                for (int i = 0; i < entitiesCount; i++)
                {
                    _insertedItems.Add(new SampleEntity
                    {
                        GuidNullableProperty = null,
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        StringProperty = _testName
                    });
                }
            }

            protected override void When()
            {
                MergeCommand<SampleEntity> command = SUT.Merge(_insertedItems);
                command.Compare.IncludeProperty(x => x.Id);
                command.Output
                    .SetExcludeAllByDefault(true)
                    .SetExcludeDbGeneratedByDefault(ExcludeOptions.Exclude);
                _changes = command.Execute(MergeType.Insert);
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

                    expectedItem.ShouldLookLike(actualItem);
                }
            }
        }

    }
}

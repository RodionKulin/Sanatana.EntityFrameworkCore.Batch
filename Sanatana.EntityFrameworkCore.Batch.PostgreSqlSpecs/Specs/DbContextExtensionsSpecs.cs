using NUnit.Framework;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch;
using Should;
using SpecsFor.StructureMap;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Specs
{
    public class DbContextExtensionsSpecs
    {

        [TestFixture]
        public class when_getting_table_name : SpecsFor<SampleDbContext>
            , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_returns_renamed_table_name()
            {
                string tableName = SUT.GetTableName<SampleEntity>();
                
                string expectedName = $"[{SampleDbContext.SAMPLE_TABLE_SCHEMA}].[{SampleDbContext.SAMPLE_TABLE_NAME}]";
                Assert.AreEqual(expectedName, tableName);
            }

            [Test]
            public void then_it_returns_plural_default_name()
            {
                string tableName = SUT.GetTableName<ParentEntity>();

                string expectedName = $"[ParentEntities]";
                Assert.AreEqual(expectedName, tableName);
            }

            [Test]
            public void then_it_returns_default_table_name()
            {
                string tableName = SUT.GetTableName<CustomKeyName>();

                string expectedName = $"[{nameof(CustomKeyName)}]";
                Assert.AreEqual(expectedName, tableName);
            }
        }
        
        [TestFixture]
        public class when_getting_ef_mapped_names : SpecsFor<SampleDbContext>
            , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }


            [Test]
            public void then_it_returns_table_name()
            {
                string tableName = SUT.GetTableName<SampleEntity>();

                string expectedSchemaAndName = $"[{SampleDbContext.SAMPLE_TABLE_SCHEMA}].[{SampleDbContext.SAMPLE_TABLE_NAME}]";
                tableName.ShouldEqual(expectedSchemaAndName);
            }


            [Test]
            public void then_it_returns_column_name()
            {
                string columnName = SUT.GetColumnName<SampleEntity>(x => x.IntProperty);

                columnName.ShouldEqual(SampleDbContext.SAMPLE_ID_COLUMN_NAME);
            }

            [Test]
            public void then_it_returns_complex_object_column_name()
            {
                string columnName = SUT.GetColumnName<ParentEntity>(x => x.Embedded.IsActive);
                columnName.ShouldEqual("Embedded_IsActive");
            }

            [Test]
            public void then_it_returns_generic_class_column_name_int()
            {
                string columnName = SUT.GetColumnName<GenericEntity<int>>(x => x.Name);
                columnName.ShouldEqual("Name");
            }

            [Test]
            public void then_it_returns_generic_class_column_name_guid()
            {
                string columnName = SUT.GetColumnName<GenericEntity<Guid>>(x => x.Name);
                columnName.ShouldEqual("Name");
            }

            [Test]
            public void then_it_returns_complex_object_annotation()
            {
                var parentEntity = SUT.Model.FindEntityType(typeof(ParentEntity).FullName);
                var annotations = parentEntity.GetAnnotations();
                IAnnotation embeddedAnnotation = annotations.ToList()[1];    
                
                Assert.IsNotNull(embeddedAnnotation);
            }

            [Test]
            public void then_it_returns_complex_object_renamed_column_name()
            {
                string columnName = SUT.GetColumnName<ParentEntity>(x => x.Embedded.Address);
                columnName.ShouldEqual(SampleDbContext.COMPLEX_TYPE_COLUMN_NAME);
            }
        }
        
        [TestFixture]
        public class when_getting_ef_database_generated_keys : SpecsFor<SampleDbContext>
            , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_returns_default_identity_names()
            {
                //Act
                string[] actualKeyNames = SUT.GetDatabaseGeneratedColumns<ParentEntity>();

                //Assert
                actualKeyNames.ShouldNotBeNull();
                List<string> expectedKeyNames = new List<string> {
                    nameof(ParentEntity.ParentEntityId)
                };
                actualKeyNames.SequenceEqual(expectedKeyNames)
                    .ShouldBeTrue("Expected keys list do not match");
            }

            [Test]
            public void then_it_returns_attributed_identity_names()
            {
                //Act
                string[] actualKeyNames = SUT.GetDatabaseGeneratedColumns<AttributedIdDbGenerated>();

                //Assert
                actualKeyNames.ShouldNotBeNull();
                List<string> expectedKeyNames = new List<string> {
                    nameof(AttributedIdDbGenerated.AttributedIdDbGeneratedId)
                };
                actualKeyNames.SequenceEqual(expectedKeyNames)
                    .ShouldBeTrue("Expected keys list do not match");
            }

            [Test]
            public void then_it_returns_attributed_and_convention_names()
            {
                //Act
                string[] actualKeyNames = SUT.GetDatabaseGeneratedColumns<ConventionKeyDbGenerated>();

                //Assert
                actualKeyNames.ShouldNotBeNull();
                List<string> expectedKeyNames = new List<string> {
                    nameof(ConventionKeyDbGenerated.Id),
                    nameof(ConventionKeyDbGenerated.SimpleProp),
                };
                actualKeyNames.SequenceEqual(expectedKeyNames)
                    .ShouldBeTrue("Expected keys list do not match");
            }

            [Test]
            public void then_it_returns_renamed_computed_names()
            {
                //Act
                string[] actualKeyNames = SUT.GetDatabaseGeneratedColumns<RenamedColumnDbGenerated>();

                //Assert
                actualKeyNames.ShouldNotBeNull();
                List<string> expectedKeyNames = new List<string> {
                    nameof(RenamedColumnDbGenerated.CustomId),
                    SampleDbContext.RENAMED_DB_GENERATED_COLUMN_NAME,
                };
                actualKeyNames.SequenceEqual(expectedKeyNames)
                    .ShouldBeTrue("Expected keys list do not match");
            }
        }

        [TestFixture]
        public class when_getting_ef_mapped_properties : SpecsFor<SampleDbContext>
            , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_excludes_all_unmapped_properties()
            {
                //Invoke
                List<string> actualMappedNames = SUT.GetAllMappedProperties<WithSomePropsUnmapped>();

                //Assert
                actualMappedNames.ShouldNotBeNull();
                List<string> expectedKeyNames = new List<string> {
                    nameof(WithSomePropsUnmapped.Id),
                    nameof(WithSomePropsUnmapped.MappedProp1),
                    nameof(WithSomePropsUnmapped.MappedProp2)
                };
                actualMappedNames.SequenceEqual(expectedKeyNames)
                    .ShouldBeTrue("Expected keys list do not match");
            }
        }
    }

   
}


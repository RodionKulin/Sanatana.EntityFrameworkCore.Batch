using NUnit.Framework;
using Sanatana.EntityFrameworkCore.Commands.Tests.Samples;
using Sanatana.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Commands.Tests.Samples.Entities;
using Should;
using SpecsFor.ShouldExtensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace Sanatana.EntityFrameworkCoreSpecs
{
    public class DbContextExtensionsSpecs
    {
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
                var ementity = SUT.Model.FindEntityType(typeof(EmbeddedEntity).FullName);

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
    }
}

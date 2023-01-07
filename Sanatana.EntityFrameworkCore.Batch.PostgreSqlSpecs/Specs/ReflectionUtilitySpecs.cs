using NUnit.Framework;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Internals.Reflection;
using Should;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;
using SpecsFor.StructureMap;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Specs
{
    public class ReflectionUtilitySpecs
    {
        [TestFixture]
        public class when_getting_entity_member_names : SpecsFor<SampleDbContext>
            , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_it_returns_entity_member_name()
            {
                string propertyName = ReflectionService.GetDefaultEfMemberName<SampleEntity>(x => x.IntProperty);

                propertyName.ShouldEqual(nameof(SampleEntity.IntProperty));
            }

            [Test]
            public void then_it_returns_entity_complex_member_name()
            {
                string propertyName = ReflectionService.GetDefaultEfMemberName<ParentEntity>(x => x.Embedded.Address);

                propertyName.ShouldEqual("Embedded_Address");
            }
        }
    }
}

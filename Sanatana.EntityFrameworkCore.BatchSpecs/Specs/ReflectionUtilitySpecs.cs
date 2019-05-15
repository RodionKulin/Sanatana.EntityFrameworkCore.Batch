using NUnit.Framework;
using Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Reflection;
using Should;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples.Entities;

namespace Sanatana.EntityFrameworkCore.BatchSpecs.Specs
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
                string propertyName = ReflectionUtility.GetDefaultEfMemberName<SampleEntity>(x => x.IntProperty);

                propertyName.ShouldEqual(nameof(SampleEntity.IntProperty));
            }

            [Test]
            public void then_it_returns_entity_complex_member_name()
            {
                string propertyName = ReflectionUtility.GetDefaultEfMemberName<ParentEntity>(x => x.Embedded.Address);

                propertyName.ShouldEqual("Embedded_Address");
            }
        }
    }
}

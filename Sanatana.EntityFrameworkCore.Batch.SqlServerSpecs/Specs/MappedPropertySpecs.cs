using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using System.Text.RegularExpressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities;
using SpecsFor.StructureMap;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using FluentAssertions;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Specs
{
    [TestFixture]
    public class MappedPropertySpecs
    {
        [Test]
        public void Distinct_WhenCalledOnMappedProperty_ExcludesItemsWithSamePropertyInfo()
        {
            //arrange
            Type entityType = typeof(SampleEntity);
            var mappedProperties = new MappedProperty[]
            {
                new MappedProperty() { PropertyInfo = entityType.GetProperty(nameof(SampleEntity.IntProperty)) },
                new MappedProperty() { PropertyInfo = entityType.GetProperty(nameof(SampleEntity.IntProperty)) },
            };

            //act
            MappedProperty[] uniqueProps = mappedProperties.Distinct().ToArray();

            //assert
            uniqueProps.Should().HaveCount(1);
        }

        [Test]
        public void Distinct_WhenCalledOnMappedProperty_KeepsItemsWithDifferentPropertyInfo()
        {
            //arrange
            Type entityType = typeof(SampleEntity);
            var mappedProperties = new MappedProperty[]
            {
                new MappedProperty() { PropertyInfo = entityType.GetProperty(nameof(SampleEntity.IntProperty)) },
                new MappedProperty() { PropertyInfo = entityType.GetProperty(nameof(SampleEntity.GuidProperty)) },
            };

            //act
            MappedProperty[] uniqueProps = mappedProperties.Distinct().ToArray();

            //assert
            uniqueProps.Should().HaveCount(2);
        }
    }
}

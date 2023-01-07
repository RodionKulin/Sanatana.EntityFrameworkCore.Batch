using NUnit.Framework;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Should;
using SpecsFor.StructureMap;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Specs
{
    public class EFCoreSpecs
    {
        [TestFixture]
        public class when_using_ambient_transaction : SpecsFor<PostgreRepository>
           , INeedSampleDatabase
        {
            public SampleDbContext SampleDatabase { get; set; }

            protected override void When()
            {
                using (var scope = new TransactionScope())
                {
                    using (var context = new SampleDbContext())
                    {
                        var sample = new SampleEntity();
                        sample.DateProperty = new DateTime(2020, 5, 6, 0, 0, 0, DateTimeKind.Utc);
                        context.Add(sample);
                        context.SaveChanges();
                    }

                    using (var context = new SampleDbContext())
                    {
                        var sample = new SampleEntity();
                        sample.DateProperty = new DateTime(2020, 5, 6, 0, 0, 0, DateTimeKind.Utc);
                        context.Add(sample);
                        context.SaveChanges();
                    }

                    scope.Complete();
                }
            }

            [Test]
            public void then_inserted_entities_are_found()
            {
                List<SampleEntity> sampleEntities = SampleDatabase.SampleEntities
                    .Where(x => x.DateProperty == new DateTime(2020, 5, 6))
                    .ToList();

                sampleEntities.ShouldNotBeNull();
                sampleEntities.Count.ShouldEqual(2);
            }
        }

    }
}

using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Interfaces;
using NUnit.Framework;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.StructureMap;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples.Entities;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.SqlServer.Repositories;
using Sanatana.EntityFrameworkCore.Batch.Commands;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Specs.Commands
{
    public class MaxParametersSpecs
    {
        [TestFixture]
        public class when_sending_parameters_number_less_then_limit : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            public List<SampleEntity> InsertItems { get; set; }
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                int paramsPerEntity = typeof(SampleEntity).GetProperties().Length
                    - 1; //except Id column that is generated;

                int maxParameters = 2100;
                int entitiesCount = maxParameters / paramsPerEntity;
                entitiesCount--; //remove one to not exceed max number of params to send

                InsertItems = Enumerable.Range(0, entitiesCount)
                    .Select(x => new SampleEntity
                    {
                        //make sure not nulls, and each property will produce Parameter
                        GuidNullableProperty = Guid.NewGuid(),
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        IntNotNullProperty = 1,
                        IntProperty = 1,
                        StringProperty = "1",
                        XmlProperty = "<xml></xml>"
                    })
                    .ToList();
            }

            [Test]
            public void then_no_exception_is_thrown()
            {
                InsertCommand<SampleEntity> insertCommand = SUT.InsertManyCommand(InsertItems);
                insertCommand.Execute();
            }

        }

        [TestFixture]
        public class when_sending_too_many_parameters : SpecsFor<SqlRepository>
           , INeedSampleDatabase
        {
            public List<SampleEntity> InsertItems { get; set; }
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                int paramsPerEntity = typeof(SampleEntity).GetProperties().Length
                    - 1; //except Id column that is generated;

                //exceed  max number of params to send
                int maxParameters = 2100;
                int entitiesCount = maxParameters / paramsPerEntity;

                InsertItems = Enumerable.Range(0, entitiesCount)
                    .Select(x => new SampleEntity
                    {
                        //make sure not nulls, and each property will produce Parameter
                        GuidNullableProperty = Guid.NewGuid(),
                        DateProperty = new DateTime(2000, 2, 2, 2, 2, 0),
                        GuidProperty = Guid.NewGuid(),
                        IntNotNullProperty = 1,
                        IntProperty = 1,
                        StringProperty = "1",
                        XmlProperty = "<xml></xml>"
                    })
                    .ToList();
            }

            [Test]
            public void then_throws_exception_with_too_many_parameters_message()
            {
                InsertCommand<SampleEntity> insertCommand = SUT.InsertManyCommand(InsertItems);

                string expectedMessage = "The incoming request has too many parameters. The server supports a maximum of 2100 parameters. Reduce the number of parameters and resend the request.";
                Assert.That(() =>
                    insertCommand.Execute(),
                    Throws.TypeOf<SqlException>().With.Property("Message").EqualTo(expectedMessage)
                );

            }

        }
    }
}

using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Providers;
using NUnit.Framework;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[SetUpFixture]
public class ServicesTestConfig : SpecsForConfiguration
{
    public ServicesTestConfig()
    {
        WhenTesting<INeedSampleDatabase>().EnrichWith<SampleDbContextProvider>();
        WhenTesting<INeedSampleDatabase>().EnrichWith<SampleDbCreator>();
        WhenTesting<INeedSampleDatabase>().EnrichWith<DataPurger>();
        WhenTesting<INeedBatchesToInsert>().EnrichWith<BatchesToInsertProvider>();
    }
}

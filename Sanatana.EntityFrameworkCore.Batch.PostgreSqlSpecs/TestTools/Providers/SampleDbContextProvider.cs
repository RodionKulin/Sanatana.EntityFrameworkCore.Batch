using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools;
using StructureMap.AutoMocking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql;
using Sanatana.EntityFrameworkCore.Batch.Internals;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Providers
{
    public class SampleDbContextProvider : Behavior<INeedSampleDatabase>
    {
        //methods
        public override void SpecInit(INeedSampleDatabase instance)
        {
            instance.SampleDatabase = new SampleDbContext();

            AutoMockedContainer container = instance.Mocker.GetContainer();
            container.Configure(cfg =>
            {
                cfg.For<SampleDbContext>().Use(instance.SampleDatabase);
                cfg.For<DbContext>().Use(instance.SampleDatabase);
                cfg.For<Func<DbContext>>().Use(() => new SampleDbContext());
                cfg.For<IDbParametersService>().Use(new PostgreParametersService());
            });
        }

        public override void AfterSpec(INeedSampleDatabase instance)
        {
            instance.SampleDatabase.Dispose();
        }
    }
}

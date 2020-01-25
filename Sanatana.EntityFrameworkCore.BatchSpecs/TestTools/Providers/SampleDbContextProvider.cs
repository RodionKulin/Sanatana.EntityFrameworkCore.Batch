using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.BatchSpecs.Samples;
using Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.BatchSpecs.TestTools;
using StructureMap.AutoMocking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Sanatana.EntityFrameworkCore.BatchSpecs.TestTools.Providers
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
            });
        }

        public override void AfterSpec(INeedSampleDatabase instance)
        {
            instance.SampleDatabase.Dispose();
        }
    }
}

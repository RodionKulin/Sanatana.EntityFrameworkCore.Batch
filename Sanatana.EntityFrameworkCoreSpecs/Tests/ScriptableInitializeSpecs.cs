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
using Sanatana.EntityFrameworkCore.Reflection;
using Sanatana.EntityFrameworkCoreSpecs.Samples;
using System.IO;
using Sanatana.EntityFrameworkCore.Scripts;
using Sanatana.EntityFrameworkSpecs.Resources.Scripts;

namespace Sanatana.EntityFrameworkCoreSpecs
{
    public class ScriptableInitializeSpecs
    {
        [TestFixture]
        public class when_initializeing_dbcontext : SpecsFor<object>
        {
            [Test]
            public void then_it_executes_scripts_from_files()
            {
                var scriptsDirectory = new DirectoryInfo("Resources/Scripts");
                var strategy = new ScriptInitializer(scriptsDirectory);
               
                using (var context = new TestInitializerDbContext())
                {
                    strategy.InitializeDatabase(context);
                }
            }
            
            [Test]
            public void then_it_executes_scripts_from_resources()
            {
                var strategy = new ScriptInitializer(typeof(ScriptsRes));
               
                using (var context = new TestInitializerDbContext())
                {
                    strategy.InitializeDatabase(context);
                }
            }
        }
    }
}

using Sanatana.EntityFrameworkCore.Commands.Tests.Samples;
using Sanatana.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Sanatana.EntityFrameworkCoreSpecs.TestTools.Providers
{ 
    public class DataPurger : Behavior<INeedSampleDatabase>
    {
        public override void SpecInit(INeedSampleDatabase instance)
        {
            using (var context = new SampleDbContext())
            {
                //Disable all foreign keys.
                context.Database
                    .ExecuteSqlCommand("EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"");

                //Remove all data from tables EXCEPT for the EF Migration History table!
                context.Database
                    .ExecuteSqlCommand("EXEC sp_msforeachtable \"IF '?' != '[dbo].[__MigrationHistory]' DELETE FROM ?\"");

                //Turn FKs back on
                context.Database
                    .ExecuteSqlCommand("EXEC sp_msforeachtable \"ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all\"");
            }
        }
    }
}

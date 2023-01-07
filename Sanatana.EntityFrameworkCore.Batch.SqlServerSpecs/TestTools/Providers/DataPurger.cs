using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.Samples;
using SpecsFor.Core.Configuration;

namespace Sanatana.EntityFrameworkCore.Batch.SqlServerSpecs.TestTools.Providers
{ 
    public class DataPurger : Behavior<INeedSampleDatabase>
    {
        public override void SpecInit(INeedSampleDatabase instance)
        {
            //Disable all foreign keys.
            instance.SampleDatabase.Database.ExecuteSqlRaw("EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"");

            //Remove all data from tables EXCEPT for the EF Migration History table!
            instance.SampleDatabase.Database.ExecuteSqlRaw("EXEC sp_msforeachtable \"IF '?' != '[dbo].[__MigrationHistory]' DELETE FROM ?\"");

            //Turn FKs back on
            instance.SampleDatabase.Database.ExecuteSqlRaw("EXEC sp_msforeachtable \"ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all\"");
        }
    }
}

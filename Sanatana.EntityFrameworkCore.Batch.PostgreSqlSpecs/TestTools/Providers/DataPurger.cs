using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples;
using SpecsFor.Core.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using Dapper;
using System.Data.Common;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.TestTools.Providers
{ 
    public class DataPurger : Behavior<INeedSampleDatabase>
    {
        public override void SpecInit(INeedSampleDatabase instance)
        {
            DatabaseFacade database = instance.SampleDatabase.Database;
            DbConnection conn = database.GetDbConnection();
            string[] tableNames = GetTablesList(conn);

            foreach (string tableName in tableNames)
            {
                if (tableName == "\"public\".\"__MigrationHistory\""){
                    continue;
                }

                //Disable all foreign keys.
                conn.Query($"ALTER TABLE {tableName} DISABLE TRIGGER ALL");

                //Remove all data from tables EXCEPT for the EF Migration History table!
                conn.Query($"DELETE FROM {tableName}");

                //Turn FKs back on
                conn.Query($"ALTER TABLE {tableName} ENABLE TRIGGER ALL");
            }

        }

        private string[] GetTablesList(DbConnection conn)
        {
            string commandText = @"
select table_schema, table_name from information_schema.tables
where table_schema not in ('information_schema', 'pg_catalog') 
and table_schema = 'public'
and table_type = 'BASE TABLE'";

            dynamic[] res = conn.Query(commandText).ToArray();

            return res.Select(x => $"\"{x.table_schema}\".\"{x.table_name}\"").ToArray();
        }

    }
}

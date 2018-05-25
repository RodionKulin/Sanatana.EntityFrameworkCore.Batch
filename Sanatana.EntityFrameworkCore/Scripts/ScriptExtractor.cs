using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Scripts
{
    public class ScriptExtractor
    {
        public static void ExtractFromDbContext(DbContext context)
        {
            string script = GenerateCreateScript(context.Database);
            
            bool connectionOpened = false;
            SqlConnection connection = (SqlConnection)context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                connectionOpened = true;
            }
                        
            context.Database.ExecuteSqlCommand(script);
            
            if (connectionOpened)
            {
                connection.Close();
            }
        }

        public static string GenerateCreateScript(DatabaseFacade database)
        {
            var model = database.GetService<IModel>();
            var differ = database.GetService<IMigrationsModelDiffer>();
            var generator = database.GetService<IMigrationsSqlGenerator>();
            var sql = database.GetService<ISqlGenerationHelper>();

            var operations = differ.GetDifferences(null, model);
            var commands = generator.Generate(operations, model);

            var builder = new StringBuilder();
            foreach (MigrationCommand command in commands)
            {
                builder
                    .Append(command.CommandText)
                    .AppendLine(sql.BatchTerminator);
            }

            return builder.ToString();
        }

        public static void CreateTablesInExistingDatabase(DatabaseFacade database)
        {
            database.EnsureCreated();
            var databaseCreator = (RelationalDatabaseCreator)database.GetService<IDatabaseCreator>();
            databaseCreator.CreateTables();
        }
    }
}

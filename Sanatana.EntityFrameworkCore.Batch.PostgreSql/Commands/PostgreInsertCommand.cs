using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.Commands
{
    public class PostgreInsertCommand<TEntity> : InsertCommand<TEntity>
        where TEntity : class
    {

        //init
        public PostgreInsertCommand(IEnumerable<TEntity> entities, DbContext dbContext, IDbParametersService dbParametersService, DbTransaction? transaction = null)
            : base(entities, dbContext, dbParametersService, transaction)
        {
        }


        //methods
        protected override string CombineCommandText(string tableName, string insertColumns, string values, string outputColumns)
        {
            outputColumns = string.IsNullOrEmpty(outputColumns)
               ? ""
               : $"RETURNING {outputColumns}";

            return @$"
INSERT INTO {tableName} 
({insertColumns})
VALUES
{values}
{outputColumns}
";
        }

    }
}

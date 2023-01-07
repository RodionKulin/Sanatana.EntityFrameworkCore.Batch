using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using System.Data.Common;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.Commands
{
    public class PostgreUpdateCommand<TEntity> : UpdateCommand<TEntity>
        where TEntity : class
    {

        //init
        public PostgreUpdateCommand(DbContext dbContext, IDbParametersService dbParametersService, DbTransaction? transaction = null)
            : base(dbContext, dbParametersService, transaction)
        {
        }


        //methods
        protected override string CombineCommandText(string tableName, string setPart, string matchPath, string outputPart)
        {
            string targetAlias = ExpressionsToSql.DEFAULT_ALIASES[0];
            string sourceAlias = "cte";

            string limit = Limit == null
                ? string.Empty
                : $"LIMIT {Limit.Value}";

            string[] primaryKeyColumns = _dbContext.GetPrimaryKeyColumns<TEntity>()
               .Select(dbColumnName => _dbParametersService.FormatColumnName(dbColumnName))
               .ToArray();
            string[] matchColumns = primaryKeyColumns
                .Select(efMappedName => string.Format("{0}.{1}={2}.{1}", targetAlias, efMappedName, sourceAlias))
                .ToArray();
            string matchPart = string.Join(", ", matchColumns);
            matchPart = _dbParametersService.FormatExpression(matchPart);

            string outputColumns = _propertyMappingService.CombineOutput(Output, targetAlias);
            outputPart = string.IsNullOrEmpty(outputPart)
               ? ""
               : $"RETURNING {outputColumns}";

            //improve notes https://dba.stackexchange.com/questions/69471/postgres-update-limit-1
            return @$"
WITH {sourceAlias} AS (
    SELECT *
        FROM {tableName} 
    WHERE {matchPath}
    {limit}
)
UPDATE {tableName} {targetAlias}
SET {setPart}
FROM {sourceAlias}
WHERE {matchPart}
{outputPart}
;";
        }

    }
}

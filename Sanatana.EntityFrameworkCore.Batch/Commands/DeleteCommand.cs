using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;

namespace Sanatana.EntityFrameworkCore.Batch.Commands
{
    public class DeleteCommand<TEntity> : IExecutableCommand
        where TEntity : class
    {
        //fields
        protected DbContext _dbContext;
        protected IDbParametersService _dbParametersService;
        protected Expression<Func<TEntity, bool>>? _whereExpression;
        protected PropertyMappingService _propertyMappingService;


        //init
        public DeleteCommand(DbContext dbContext, IDbParametersService dbParametersService)
        {
            _dbContext = dbContext;
            _dbParametersService = dbParametersService;

            Type entityType = typeof(TEntity);
            _propertyMappingService = new PropertyMappingService(_dbContext, entityType, dbParametersService);
        }


        //Configure methods
        /// <summary>
        /// Expression to select rows to be effected by DELETE command.
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public virtual DeleteCommand<TEntity> SetWhere(Expression<Func<TEntity, bool>> whereExpression)
        {
            _whereExpression = whereExpression;
            return this;
        }


        //Execute methods
        public virtual int Execute()
        {
            string commandText = ConstructDeleteCommand();
            return _dbContext.Database.ExecuteSqlRaw(commandText);
        }

        public virtual Task<int> ExecuteAsync()
        {
            string commandText = ConstructDeleteCommand();
            return _dbContext.Database.ExecuteSqlRawAsync(commandText);
        }


        //Combine command text methods
        protected virtual string ConstructDeleteCommand()
        {
            if (_whereExpression == null)
            {
                throw new ArgumentNullException($"Provided {nameof(_whereExpression)} is null. Use {nameof(SetWhere)} method to set.");
            }

            string tableName = GetTableName();

            string matchPart = _propertyMappingService.CombineWhere(_whereExpression, useLambdaAlias: false);
          
            return CombineCommandText(tableName, matchPart);
        }

        protected virtual string GetTableName()
        {
            string tableName = _dbContext.GetTableName<TEntity>();
            return _dbParametersService.FormatTableName(tableName);
        }

        protected virtual string CombineCommandText(string tableName, string matchPart)
        {
            return @$"
DELETE FROM {tableName}
WHERE {matchPart}"
;
        }

    }
}

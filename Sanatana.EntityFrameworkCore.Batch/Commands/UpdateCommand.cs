using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace Sanatana.EntityFrameworkCore.Batch.Commands
{
    public class UpdateCommand<TEntity>
        where TEntity : class
    {
        //fields
        protected DbContext _dbContext;
        protected DbTransaction? _transaction;
        protected IDbParametersService _dbParametersService;
        protected PropertyMappingService _propertyMappingService;
        protected List<Expression> _updateExpressions;
        protected Expression<Func<TEntity, bool>>? _whereExpression;


        //properties
        public long? Limit { get; protected set; }
        /// <summary>
        /// List of properties to return for updated rows. 
        /// No columns are included by default.
        /// Include primary key properties to identify changes rows.
        /// Returned values will be returned on new TEntity instance with other values default.
        /// </summary>
        public CommandArgs<TEntity> Output { get; protected set; }


        //init
        public UpdateCommand(DbContext dbContext, IDbParametersService dbParametersService, DbTransaction? transaction = null)
        {
            _dbContext = dbContext;
            _transaction = transaction;
            _dbParametersService = dbParametersService;
            _updateExpressions = new List<Expression>();

            Type entityType = typeof(TEntity);
            _propertyMappingService = new PropertyMappingService(_dbContext, entityType, dbParametersService);
            List<MappedProperty> properties = _propertyMappingService.GetAllEntityProperties();

            Output = new CommandArgs<TEntity>(properties, _propertyMappingService)
                .SetExcludeAllByDefault();
        }


        //Configure methods

        /// <summary>
        /// Expression to update columns of Target table. Example: (t) => t.IntProperty, (t) => t.OtherIntProperty * 2.
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="targetProperty"></param>
        /// <param name="assignedValue"></param>
        /// <returns></returns>
        public virtual UpdateCommand<TEntity> SetAssign<TProp>(
            Expression<Func<TEntity, TProp>> targetProperty,
            Expression<Func<TEntity, TProp>> assignedValue)
        {
            _updateExpressions.Add(new AssignLambdaExpression()
            {
                Left = targetProperty,
                Right = assignedValue
            });
            return this;
        }

        /// <summary>
        /// Expression to select rows to be effected by UPDATE command.
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public virtual UpdateCommand<TEntity> SetWhere(Expression<Func<TEntity, bool>> whereExpression)
        {
            _whereExpression = whereExpression;
            return this;
        }

        /// <summary>
        /// Limit number or rows to be updated
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public virtual UpdateCommand<TEntity> SetLimit(long? limit)
        {
            Limit = limit;
            return this;
        }


        //Execute methods
        /// <summary>
        /// With execute UPDATE command and return number of rows effected.
        /// </summary>
        /// <returns></returns>
        public virtual int Execute()
        {
            string commandText = GetCommandText();
            return _dbContext.Database.ExecuteSqlRaw(commandText);
        }

        /// <summary>
        /// With execute UPDATE command and return number of rows effected.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> ExecuteAsync()
        {
            string commandText = GetCommandText();
            return await _dbContext.Database.ExecuteSqlRawAsync(commandText);
        }

        /// <summary>
        /// Execute UPDATE command and return new instances of TEntity with populated properties, specified in Output. 
        /// Other properties will have default values.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual List<TEntity> ExecuteWithOutput()
        {
            string commandText = GetCommandText();

            List<MappedProperty> outputProperties = Output.GetSelectedFlat();
            if (outputProperties.Count == 0)
            {
                throw new ArgumentException("No output properties selected for UPDATE command");
            }

            var readCommandExecutor = new ReadCommandExecutor<TEntity>(_dbContext, _transaction, outputProperties);
            return readCommandExecutor.ReadOutputToNewEntities(commandText, new DbParameter[0]);
        }

        /// <summary>
        /// Execute UPDATE command and return new instances of TEntity with populated properties, specified in Output. 
        /// Other properties will have default values.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual Task<List<TEntity>> ExecuteWithOutputAsync()
        {
            string commandText = GetCommandText();

            List<MappedProperty> outputProperties = Output.GetSelectedFlat();
            if(outputProperties.Count == 0)
            {
                throw new ArgumentException("No output properties selected for UPDATE command");
            }

            var readCommandExecutor = new ReadCommandExecutor<TEntity>(_dbContext, _transaction, outputProperties);
            return readCommandExecutor.ReadOutputToNewEntitiesAsync(commandText, new DbParameter[0]);
        }


        //Combine command text methods
        protected virtual string GetCommandText()
        {
            string tableName = GetTableName();

            string setPart = _propertyMappingService.CombineSet<TEntity>(_updateExpressions, useLambdaAlias: false);

            string wherePart = _propertyMappingService.CombineWhere(_whereExpression, useLambdaAlias: false);
                
            string outputPart = _propertyMappingService.CombineOutput(Output);

            return CombineCommandText(tableName, setPart, wherePart, outputPart);
        }

        protected virtual string GetTableName()
        {
            string tableName = _dbContext.GetTableName<TEntity>();
            return _dbParametersService.FormatTableName(tableName);
        }

        protected virtual string CombineCommandText(string tableName, string setPart, string wherePart, string outputPart)
        {
            string targetAlias = ExpressionsToSql.DEFAULT_ALIASES[0];

            string limit = Limit == null
                ? string.Empty
                : $"TOP({Limit.Value}) ";

            outputPart = string.IsNullOrEmpty(outputPart)
                ? ""
                : $"OUTPUT {outputPart}";

            return $@"
UPDATE {limit} {targetAlias}
SET {setPart} 
{outputPart}
FROM {tableName} {targetAlias}
WHERE {wherePart}"
;
        }

    }
}

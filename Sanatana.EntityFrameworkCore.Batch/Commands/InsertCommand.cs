using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Commands
{
    public class InsertCommand<TEntity> : IExecutableCommand
        where TEntity : class
    {
        //fields
        protected DbContext _dbContext;
        protected DbTransaction? _transaction;
        protected IDbParametersService _dbParametersService;
        protected PropertyMappingService _propertyMappingService;
        protected List<TEntity> _entitiesList;


        //properties
        /// <summary>
        /// List of columns to insert.
        /// Database generated properties are excluded by default.
        /// All other properties are included by default.
        /// </summary>
        public CommandArgs<TEntity> Insert { get; protected set; }
        /// <summary>
        /// List of properties to return for inserted rows. 
        /// Include properties that are generated on database side, like auto increment field.
        /// Returned values will be set to provided entities properties.
        /// Database generated or computed properties are included by default.
        /// </summary>
        public CommandArgs<TEntity> Output { get; protected set; }


        //init
        public InsertCommand(IEnumerable<TEntity> entities, DbContext dbContext, IDbParametersService dbParametersService, DbTransaction? transaction = null)
        {
            _entitiesList = entities == null
                ? throw new ArgumentNullException(nameof(entities))
                : entities.ToList();
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbParametersService = dbParametersService ?? throw new ArgumentNullException(nameof(dbContext));
            _transaction = transaction;

            Type entityType = typeof(TEntity);
            _propertyMappingService = new PropertyMappingService(_dbContext, entityType, dbParametersService);
            List<MappedProperty> properties = _propertyMappingService.GetAllEntityProperties();

            Insert = new CommandArgs<TEntity>(properties, _propertyMappingService)
                .SetIncludeAllByDefault(ColumnSetEnum.DbGenerated);
            Output = new CommandArgs<TEntity>(properties, _propertyMappingService)
                .SetExcludeAllByDefault(ColumnSetEnum.DbGenerated);
        }


        //Execute methods
        public virtual int Execute()
        {
            if (_entitiesList.Count == 0)
            {
                return 0;
            }

            string commandText = GetCommandText(_entitiesList, out DbParameter[] parameters);
            var readCommandExecutor = new ReadCommandExecutor<TEntity>(_dbContext, _transaction, Output.GetSelectedFlat());
            return readCommandExecutor.Execute(commandText, parameters, _entitiesList);
        }

        public virtual async Task<int> ExecuteAsync()
        {
            if (_entitiesList.Count == 0)
            {
                return 0;
            }

            string commandText = GetCommandText(_entitiesList, out DbParameter[] parameters);
            var readCommandExecutor = new ReadCommandExecutor<TEntity>(_dbContext, _transaction, Output.GetSelectedFlat());
            return await readCommandExecutor.ExecuteAsync(commandText, parameters, _entitiesList);
        }


        //Combine command text methods
        protected virtual string GetCommandText(List<TEntity> entities, out DbParameter[] parameters)
        {
            string tableName = GetTableName();
            string columnNames = _propertyMappingService.CombineColumns(Insert, "insert");
            string values = _propertyMappingService.CombineInsertValues(Insert, entities, out parameters);
            string outputPart = _propertyMappingService.CombineOutput(Output);
            return CombineCommandText(tableName, columnNames, values, outputPart);
        }

        protected virtual string GetTableName()
        {
            string tableName = _dbContext.GetTableName<TEntity>();
            return _dbParametersService.FormatTableName(tableName);
        }

        protected virtual string CombineCommandText(string tableName, string columns, string values, string outputPart)
        {
            outputPart = string.IsNullOrEmpty(outputPart)
                ? ""
                : $"OUTPUT {outputPart}";

            return @$"
INSERT INTO {tableName} 
({columns})
{outputPart}
VALUES
{values}
";
            
        }

    }
}

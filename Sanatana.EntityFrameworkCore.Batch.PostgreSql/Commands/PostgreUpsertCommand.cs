using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Commands.Arguments;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch.Commands;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.Commands
{
    public class PostgreUpsertCommand<TEntity> : IExecutableCommand        
        where TEntity : class
    {
        //fields
        protected string _targetAlias;
        protected string _sourceAlias;
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
        /// List of columns for CONFLICT part to check before insert.
        /// It is required to have a UNIQUE constraint.
        /// All columns are excluded by default. Required to select at least single column.
        /// </summary>
        public CommandArgs<TEntity> Conflict { get; protected set; }
        /// <summary>
        /// List of columns to update on Target table for rows that already existed.
        /// All properties are included by default.
        /// </summary>
        public MergeSetArgs<TEntity> Set { get; protected set; }
        /// <summary>
        /// List of properties to return for updated rows.
        /// No columns are included by default.
        /// Include primary key properties to identify changes rows.
        /// Returned values will be returned on new TEntity instance with other values default.
        /// </summary>
        public CommandArgs<TEntity> Output { get; protected set; }


        //init
        public PostgreUpsertCommand(IEnumerable<TEntity> entities, DbContext dbContext, IDbParametersService dbParametersService, DbTransaction? transaction = null)
        {
            _targetAlias = ExpressionsToSql.DEFAULT_ALIASES[0];
            _sourceAlias = "EXCLUDED";

            _entitiesList = entities == null
                ? throw new ArgumentNullException(nameof(entities))
                : entities.ToList();
            _dbContext = dbContext;
            _transaction = transaction;
            _dbParametersService = dbParametersService;

            Type entityType = typeof(TEntity);
            _propertyMappingService = new PropertyMappingService(_dbContext, entityType, dbParametersService);
            List<MappedProperty> properties = _propertyMappingService.GetAllEntityProperties();

            Insert = new CommandArgs<TEntity>(properties, _propertyMappingService)
                .SetIncludeAllByDefault(ColumnSetEnum.DbGenerated);
            Conflict = new CommandArgs<TEntity>(properties, _propertyMappingService)
                .SetExcludeAllByDefault();
            Set = new MergeSetArgs<TEntity>(properties, _propertyMappingService)
                .SetIncludeAllByDefault();
            Output = new CommandArgs<TEntity>(properties, _propertyMappingService)
                .SetExcludeAllByDefault();
        }


        //Execute methods
        /// <summary>
        /// With execute UPDATE command and return number of rows effected.
        /// </summary>
        /// <returns></returns>
        public virtual int Execute()
        {
            if (_entitiesList.Count == 0)
            {
                return 0;
            }

            string commandText = GetCommandText(out DbParameter[] parameters);
            return _dbContext.Database.ExecuteSqlRaw(commandText, parameters);
        }

        /// <summary>
        /// With execute UPDATE command and return number of rows effected.
        /// </summary>
        /// <returns></returns>
        public virtual Task<int> ExecuteAsync()
        {
            if (_entitiesList.Count == 0)
            {
                return Task.FromResult(0);
            }

            string commandText = GetCommandText(out DbParameter[] parameters);
            return _dbContext.Database.ExecuteSqlRawAsync(commandText, parameters);
        }


        //Combine command text methods
        protected virtual string GetCommandText(out DbParameter[] parameters)
        {
            string tableName = _dbContext.GetTableName<TEntity>();
            tableName = _dbParametersService.FormatTableName(tableName);

            string insertColumns = _propertyMappingService.CombineColumns(Insert, "insert");

            string values = _propertyMappingService.CombineInsertValues(Insert, _entitiesList, out parameters);

            string conflictColumns = _propertyMappingService.CombineColumns(Conflict, "conflict");

            //should result similar to: email = EXCLUDED.email || ';' || EXT.email
            //this will not work:   EXT.email = EXCLUDED.email || ';' || EXT.email
            string setPart = _propertyMappingService.CombineSetFromValues(Set, _targetAlias, _sourceAlias, false);
           
            string outputColumns = _propertyMappingService.CombineOutput(Output);

            return CombineCommandText(tableName, insertColumns, values, conflictColumns, setPart, outputColumns);
        }
        
        protected virtual string CombineCommandText(string tableName, string insertColumns, string values, string conflictColumns, string setPart, string outputColumns)
        {
            string sourceAlias = ExpressionsToSql.DEFAULT_ALIASES[0];

            outputColumns = string.IsNullOrEmpty(outputColumns)
               ? ""
               : $"RETURNING {outputColumns}";

            return @$"
INSERT INTO {tableName} as {sourceAlias} ({insertColumns}) 
VALUES {values}
ON CONFLICT ({conflictColumns})
DO 
    UPDATE SET {setPart} 
{outputColumns}
;";
        }

    }
}

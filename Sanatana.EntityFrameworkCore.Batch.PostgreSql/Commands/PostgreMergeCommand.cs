using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Sanatana.EntityFrameworkCore.Batch.Commands.Arguments;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using Sanatana.EntityFrameworkCore.Batch.Commands;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.Commands
{
    public class PostgreMergeCommand<TEntity> : IMergeCommand<TEntity>
        where TEntity : class
    {
        //fields
        protected MergeTypeEnum _mergeType;
        protected List<TEntity> _entityList;
        protected DbContext _dbContext;
        protected IDbParametersService _dbParametersService;
        protected DbTransaction? _transaction;
        protected List<MappedProperty> _entityProperties;
        protected PropertyMappingService _propertyMappingService;


        //properties
        /// <summary>
        /// Target table name taken from EntityFramework settings by default. Can be changed manually.
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Enable transaction usage when there is more then a single merge request.
        /// Merge requests get splitted into batches when max number of SqlParameters per query is reached. 
        /// Each property of an entity is passed as DbParameter if not using TVP.
        /// You might want to prevent Merge from creating transaction if using ambient transaction from your code.
        /// Enabled by default.
        /// </summary>
        public bool UseInnerTransactionForBatches { get; set; } = true;
        /// <summary>
        /// List of columns to include as parameters to the query from provided Source entities.        /// 
        /// All properties are included by default.
        /// </summary>
        public CommandArgs<TEntity> Source { get; protected set; }
        /// <summary>
        /// List of columns used to match Target table rows to Source rows.
        /// All properties are excluded by default.
        /// Parameter is required for all merge types except Insert. If not specified will insert all rows into Target table. 
        /// </summary>
        public MergeCompareArgs<TEntity> On { get; protected set; }
        /// <summary>
        /// Used if Update or Upsert type of merge is executed.
        /// List of columns to update on Target table for rows that did match Source rows.
        /// All properties are included by default.
        /// </summary>
        public MergeSetArgs<TEntity> SetMatched { get; protected set; }
        /// <summary>
        /// Used if Update type of merge is executed.
        /// List of columns to update on Target table for rows that did not match Source rows.
        /// All properties are excluded by default.
        /// </summary>
        public MergeSetArgs<TEntity> SetNotMatched { get; protected set; }
        /// <summary>
        /// Used if Insert or Upsert type of merge is executed.
        /// List of columns to insert.
        /// Database generated properties are excluded by default.
        /// All other properties are included by default.
        /// </summary>
        public MergeInsertArgs<TEntity> Insert { get; protected set; }
        /// <summary>
        /// List of properties to return for inserted rows. 
        /// Include properties that are generated on database side, like auto increment field.
        /// Returned values will be set to provided entities properties.
        /// Database generated or computed properties are included by default.
        /// Not implemented for TVP merge.
        /// </summary>
        public CommandArgs<TEntity> Output { get; protected set; }


        //not impemented IMergeCommand<TEntity> properties
        public string SqlTVPTypeName { get; set; }
        public string SqlTVPParameterName { get; set; }


        //init
        private PostgreMergeCommand(DbContext dbContext, IDbParametersService dbParametersService, MergeTypeEnum mergeType, 
            DbTransaction? transaction = null)
        {
            _mergeType = mergeType;
            _dbContext = dbContext;
            _dbParametersService = dbParametersService;
            _transaction = transaction;
            TableName = _dbContext.GetTableName<TEntity>();

            Type entityType = typeof(TEntity);
            _propertyMappingService = new PropertyMappingService(_dbContext, entityType, dbParametersService);
            _entityProperties = _propertyMappingService.GetAllEntityProperties();

            Source = new CommandArgs<TEntity>(_entityProperties, _propertyMappingService)
              .SetIncludeAllByDefault();
            On = new MergeCompareArgs<TEntity>(_entityProperties, _propertyMappingService)
                .SetExcludeAllByDefault();
            SetMatched = new MergeSetArgs<TEntity>(_entityProperties, _propertyMappingService)
                .SetIncludeAllByDefault();
            SetNotMatched = new MergeSetArgs<TEntity>(_entityProperties, _propertyMappingService)
                .SetExcludeAllByDefault();
            Insert = new MergeInsertArgs<TEntity>(_entityProperties, _propertyMappingService, _dbParametersService)
                .SetIncludeAllByDefault(ColumnSetEnum.DbGenerated);
            Output = new CommandArgs<TEntity>(_entityProperties, _propertyMappingService)
                .SetExcludeAllByDefault(ColumnSetEnum.DbGenerated);
        }

        /// <summary>
        /// Merge entity into the table and pass values as DbParameter.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbParametersService"></param>
        /// <param name="mergeType"></param>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        public PostgreMergeCommand(DbContext dbContext, IDbParametersService dbParametersService, MergeTypeEnum mergeType, 
            TEntity entity, DbTransaction? transaction = null)
            : this(dbContext, dbParametersService, mergeType, transaction)
        {
            _entityList = new List<TEntity>() { entity };
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as DbParameter.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbParametersService"></param>
        /// <param name="mergeType"></param>
        /// <param name="entityList"></param>
        /// <param name="transaction"></param>
        public PostgreMergeCommand(DbContext dbContext, IDbParametersService dbParametersService, MergeTypeEnum mergeType, 
            List<TEntity> entityList, DbTransaction? transaction = null)
            : this(dbContext, dbParametersService, mergeType, transaction)
        {
            _entityList = entityList;
        }


        //public methods
        public virtual int Execute()
        {
            if (_entityList.Count == 0)
            {
                return 0;
            }

            IExecutableCommand command = GetCommand();
            return command.Execute();
        }

        public virtual Task<int> ExecuteAsync()
        {
            if (_entityList.Count == 0)
            {
                return Task.FromResult(0);
            }

            IExecutableCommand command = GetCommand();
            return command.ExecuteAsync();
        }

        protected virtual IExecutableCommand GetCommand()
        {
            if (_mergeType == MergeTypeEnum.Insert)
            {
                var command = new PostgreInsertCommand<TEntity>(_entityList, _dbContext, _dbParametersService, _transaction);
                command.Insert.Clone(Insert);
                command.Output.Clone(Output);
                return command;
            }
            else if (_mergeType == MergeTypeEnum.Upsert)
            {
                var command = new PostgreUpsertCommand<TEntity>(_entityList, _dbContext, _dbParametersService, _transaction);
                command.Conflict.Clone(On);
                command.Insert.Clone(Insert);
                command.Set.Clone(SetMatched);
                command.Output.Clone(Output);
                return command;
            }

            throw new NotImplementedException($"MergeType {_mergeType} is not yet supported for PostgreSql in {GetType().Assembly.GetName()}");
        }

        public virtual string ConstructCommand(List<TEntity> entitiesBatch)
        {
            //not implemented
            return null;
        }
    }
}

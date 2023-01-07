using System;
using System.Linq.Expressions;
using System.Text;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using System.Data;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;
using System.Transactions;
using Sanatana.EntityFrameworkCore.Batch.Commands.Arguments;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Internals;

namespace Sanatana.EntityFrameworkCore.Batch.Commands
{
    public class MergeCommand<TEntity> : IMergeCommand<TEntity>
        where TEntity : class
    {
        //fields
        protected const string SOURCE_ID_COLUMN_NAME = "_SourceRowId";
        protected string _targetAlias;
        protected string _sourceAlias;
        protected MergeTypeEnum _mergeType;
        protected bool _useTVP;
        protected List<TEntity> _entityList;
        protected DbContext _dbContext;
        protected IDbParametersService _dbParametersService;
        protected DbTransaction? _transaction;
        protected List<MappedProperty> _entityProperties;
        protected PropertyMappingService _propertyMappingService;


        //properties
        /// <summary>
        /// Type of the Table Valued Parameter that is expected to be already created on SQL server before executing merge, describing order or Source columns. 
        /// Required when using merge constructor with TVP, not required if using SqlParameters constructor.
        /// </summary>
        public string SqlTVPTypeName { get; set; }
        /// <summary>
        /// Name of the Table Valued Parameter that defaults to @Table. This can be any string.
        /// Required when using merge constructor with TVP, not required if using SqlParameters constructor.
        /// </summary>
        public string SqlTVPParameterName { get; set; }
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
        /// List of columns to include as parameters to the query from provided Source entities.
        /// All properties are included by default.
        /// </summary>
        public CommandArgs<TEntity> Source { get; protected set; }
        /// <summary>
        /// List of columns used to compare Target table rows to Source rows.
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
        protected bool UseOutputColumns
        {
            get
            {
                return _useTVP == false && Output.GetSelectedFlat().Count > 0;
            }
        }


        //init
        private MergeCommand(DbContext dbContext, IDbParametersService dbParametersService, MergeTypeEnum mergeType, 
            DbTransaction? transaction = null)
        {
            _targetAlias = ExpressionsToSql.DEFAULT_ALIASES[0];
            _sourceAlias = ExpressionsToSql.DEFAULT_ALIASES[1];

            _dbContext = dbContext;
            _dbParametersService = dbParametersService;
            _mergeType = mergeType;
            _transaction = transaction;

            TableName = _dbContext.GetTableName<TEntity>();
            TableName = _dbParametersService.FormatTableName(TableName);

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
        public MergeCommand(DbContext dbContext, IDbParametersService dbParametersService, MergeTypeEnum mergeType, 
            TEntity entity, DbTransaction? transaction = null)
            : this(dbContext, dbParametersService, mergeType, transaction)
        {
            _entityList = new List<TEntity>() { entity };
            _useTVP = false;
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as DbParameter.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbParametersService"></param>
        /// <param name="mergeType"></param>
        /// <param name="entityList"></param>
        /// <param name="transaction"></param>
        public MergeCommand(DbContext dbContext, IDbParametersService dbParametersService, MergeTypeEnum mergeType,
            IEnumerable<TEntity> entityList, DbTransaction? transaction = null)
            : this(dbContext, dbParametersService, mergeType, transaction)
        {
            _entityList = entityList.ToList();
            _useTVP = false;
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as TVP. Order of selected Source fields must match the order of columns in TVP declaration.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbParametersService"></param>
        /// <param name="entityList"></param>
        /// <param name="mergeType"></param>
        /// <param name="sqlTVPTypeName"></param>
        /// <param name="sqlTVPParameterName"></param>
        /// <param name="transaction"></param>
        public MergeCommand(DbContext dbContext, IDbParametersService dbParametersService, MergeTypeEnum mergeType, 
            IEnumerable<TEntity> entityList, string sqlTVPTypeName, string sqlTVPParameterName = "@Table", DbTransaction? transaction = null)
            : this(dbContext, dbParametersService, mergeType, transaction)
        {
            _entityList = entityList.ToList();
            _useTVP = true;
            SqlTVPTypeName = sqlTVPTypeName;
            SqlTVPParameterName = sqlTVPParameterName;
        }
        

        //public methods
        public virtual int Execute()
        {
            if (_entityList.Count == 0)
            {
                return 0;
            }

            List<TEntity>[] entityBatches = GetEntityBatches(_entityList);
            if(_transaction != null
                || UseInnerTransactionForBatches == false
                || Transaction.Current != null) //use ambient transaction if provided
            {
                return ReadOutput(entityBatches, _transaction);
            }

            int changes = 0;
            using (IDbContextTransaction batchTransaction = _dbContext.Database.BeginTransaction())
            {
                DbTransaction dbTransaction = batchTransaction.GetDbTransaction();
                changes += ReadOutput(entityBatches, dbTransaction);
                batchTransaction.Commit();
            }

            return changes;
        }
        
        public virtual async Task<int> ExecuteAsync()
        {
            if (_entityList.Count == 0)
            {
                return 0;
            }

            List<TEntity>[] entityBatches = GetEntityBatches(_entityList);
            if (_transaction != null
                || UseInnerTransactionForBatches == false
                || Transaction.Current != null) //use ambient transaction if provided
            {
                return await ReadOutputAsync(entityBatches, _transaction)
                    .ConfigureAwait(false);
            }

            int changes = 0;
            using (IDbContextTransaction batchTransaction =
                await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                DbTransaction dbTransaction = batchTransaction.GetDbTransaction();
                changes = await ReadOutputAsync(entityBatches, dbTransaction).ConfigureAwait(false);
                batchTransaction.Commit();
            }

            return changes;
        }
        

        //limit number of entities in command
        protected virtual List<TEntity>[] GetEntityBatches(List<TEntity> entities)
        {
            int paramsPerEntity = Source.GetSelectedFlat().Count;
            if(paramsPerEntity == 0 || _useTVP)
            {
                return new List<TEntity>[] { entities };
            }
            
            int maxParams = _dbParametersService.MaxParametersPerCommand;
            if (paramsPerEntity > maxParams)
            {
                throw new NotSupportedException($"Single command can not have more than {maxParams} sql parameters. Consider using TVP version of Merge.");
            }

            int maxEntitiesInBatch = maxParams / paramsPerEntity;
            int batchesCount = (int)Math.Ceiling((decimal)entities.Count / maxEntitiesInBatch);

            var batches = new List<TEntity>[batchesCount];
            for (int request = 0; request < batchesCount; request++)
            {
                int skip = maxEntitiesInBatch * request;
                List<TEntity> requestEntities = entities
                    .Skip(skip)
                    .Take(maxEntitiesInBatch)
                    .ToList();
                batches[request] = requestEntities;
            }

            return batches;
        }


        //parameters
        protected virtual DbParameter[] ConstructParameterValues(List<TEntity> entities)
        {
            var dbParams = new List<DbParameter>();

            for (int i = 0; i < entities.Count; i++)
            {
                TEntity entity = entities[i];
                List<MappedProperty> entityProperties = Source.GetSelectedFlatWithValues(entity);

                foreach (MappedProperty property in entityProperties)
                {
                    if (property.Value != null)
                    {
                        string paramName = $"@{property.DbColumnName}{i}";
                        DbParameter sqlParameter = _dbParametersService.GetDbParameter(paramName, property.Value);
                        dbParams.Add(sqlParameter);
                    }
                }
            }

            return dbParams.ToArray();
        }

        protected virtual DbParameter[] ConstructParameterTVP(List<TEntity> entities)
        {
            List<string> sourcePropertyNames = Source.GetSelectedPropertyNames();
            DataTable entitiesDataTable = entities.ToDataTable(sourcePropertyNames);

            DbParameter tableParam = _dbParametersService.GetDbParameter(SqlTVPParameterName, entitiesDataTable, SqlTVPTypeName);
          
            return new DbParameter[] { tableParam };
        }



        //construct sql command text
        public virtual string ConstructCommand(List<TEntity> entitiesBatch)
        {
            StringBuilder sql = _useTVP
                ? ConstructHeadTVP()
                : ConstructHeadValues(entitiesBatch);
               
            //on
            if (_mergeType == MergeTypeEnum.Insert)
            {
                sql.AppendFormat(" ON 1 = 0 ");
            }
            else
            {
                ConstructOn(sql);
            }

            //merge update
            bool doUpdate = _mergeType == MergeTypeEnum.Update
                || _mergeType == MergeTypeEnum.Upsert;
            if (doUpdate)
            {
                ConstructUpdateMatched(sql);
            }
            if (_mergeType == MergeTypeEnum.Update)
            {
                ConstructUpdateNotMatched(sql);
            }

            //merge insert
            bool doInsert = _mergeType == MergeTypeEnum.Insert
                || _mergeType == MergeTypeEnum.Upsert;
            if (doInsert)
            {
                ConstructInsert(sql);
            }

            //merge delete
            bool doDelete = _mergeType == MergeTypeEnum.DeleteMatched
                || _mergeType == MergeTypeEnum.DeleteNotMatched;
            if (doDelete)
            {
                ConstructDelete(sql);
            }

            //output
            if (!_useTVP)
            {
                ConstructOutput(sql);
            }

            sql.Append(";");
            return sql.ToString();
        }

        protected virtual StringBuilder ConstructHeadValues(List<TEntity> entities)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("MERGE INTO {0} AS {1} ", TableName, _targetAlias);
            sql.AppendFormat("USING (VALUES ");

            //values
            for (int i = 0; i < entities.Count; i++)
            {
                sql.Append("(");
                sql.Append(i + ",");    //row id to match command outputs to input entities

                TEntity entity = entities[i];
                List<MappedProperty> entityProperties = Source.GetSelectedFlatWithValues(entity);

                for (int p = 0; p < entityProperties.Count; p++)
                {
                    if (entityProperties[p].Value == null)
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        string paramName = $"@{entityProperties[p].DbColumnName}{i}";
                        sql.Append(paramName);
                    }

                    bool isLastProperty = p == entityProperties.Count - 1;
                    if (isLastProperty == false)
                    {
                        sql.Append(",");
                    }
                }

                sql.Append(")");
                bool isLastEntity = i == entities.Count - 1;
                if (isLastEntity == false)
                {
                    sql.Append(",");
                }
            }

            //column names
            List<string> columnNameList = Source.GetSelectedFlat()
                .Select(x => x.DbColumnName)
                .ToList();
            columnNameList.Insert(0, SOURCE_ID_COLUMN_NAME);    //row id to match command outputs to input entities
            string columnNameString = string.Join(",", columnNameList);
            sql.AppendFormat(") AS {0} ({1})", _sourceAlias, columnNameString);

            return sql;
        }

        protected virtual StringBuilder ConstructHeadTVP()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("MERGE INTO {0} AS {1} ", TableName, _targetAlias);
            sql.AppendFormat("USING {0} AS {1}", SqlTVPParameterName, _sourceAlias);

            return sql;
        }

        protected virtual void ConstructOn(StringBuilder sql)
        {
            List<string> stringParts = On.GetSelectedFlat()
                .Select(p => _dbParametersService.FormatColumnName(p.DbColumnName))
                .Select(efMappedName => string.Format("{0}.{1}={2}.{1}", _targetAlias, efMappedName, _sourceAlias))
                .ToList();

            if(stringParts.Count == 0)
            {
                throw new ArgumentException($"{nameof(On)} is required and should contain properties to match sources with target table.");
            }

            foreach (Expression item in On.Expressions)
            {
                string expressionSql = item.ToSqlString(_dbParametersService, _dbContext);
                stringParts.Add(expressionSql);
            }

            if (stringParts.Count == 0 && _mergeType == MergeTypeEnum.Insert)
            {
                stringParts.Add("0=1");
            }
            if (stringParts.Count == 0 && _mergeType != MergeTypeEnum.Insert)
            {
                throw new ArgumentNullException($"{nameof(On)} expression must be specified if it is not an Insert command.");
            }

            string compare = string.Join(" AND ", stringParts);
            sql.AppendFormat(" ON ({0}) ", compare);
        }

        protected virtual void ConstructUpdateMatched(StringBuilder sql)
        {
            string setPart = _propertyMappingService.CombineSetFromValues(SetMatched, _targetAlias, _sourceAlias);
            if (!string.IsNullOrEmpty(setPart))
            {
                sql.AppendFormat("WHEN MATCHED THEN UPDATE SET {0}", setPart);
            }
        }

        protected virtual void ConstructUpdateNotMatched(StringBuilder sql)
        {
            string setPart = _propertyMappingService.CombineSetFromValues(SetNotMatched, _targetAlias, _sourceAlias);
            if(!string.IsNullOrEmpty(setPart))
            {
                sql.AppendFormat("WHEN NOT MATCHED THEN UPDATE SET {0}", setPart);
            }
        }

        protected virtual void ConstructInsert(StringBuilder sql)
        {
            string targetString = _propertyMappingService.CombineColumns(Insert);
            string sourceString = _propertyMappingService.CombineMergeInsertValues(Insert, _sourceAlias);

            sql.AppendFormat(" WHEN NOT MATCHED THEN INSERT ({0})", targetString);
            sql.AppendFormat(" VALUES ({0})", sourceString);
        }

        protected virtual void ConstructDelete(StringBuilder sql)
        {
            string not = _mergeType == MergeTypeEnum.DeleteMatched
                ? ""
                : " NOT";
            sql.AppendFormat($" WHEN{not} MATCHED THEN DELETE");
        }

        protected virtual void ConstructOutput(StringBuilder sql)
        {
            List<MappedProperty> outputProperties = Output.GetSelectedFlat();
            if (!UseOutputColumns)
            {
                return;
            }

            sql.Append(" OUTPUT ");
            sql.Append(_sourceAlias + "." + SOURCE_ID_COLUMN_NAME + ",");

            for (int i = 0; i < outputProperties.Count; i++)
            {
                MappedProperty prop = outputProperties[i];
                string name = _dbParametersService.FormatOutputParameter(prop.DbColumnName);
                sql.Append(name);

                bool isLast = i == outputProperties.Count - 1;
                if (isLast == false)
                {
                    sql.Append(",");
                }
            }
        }


        //read output
        protected virtual int ReadOutput(List<TEntity>[] entityBatches, DbTransaction? transaction)
        {
            int res = 0;
            foreach (List<TEntity> entitiesBatch in entityBatches)
            {
                string sql = ConstructCommand(entitiesBatch);
                DbParameter[] parameters = _useTVP
                    ? ConstructParameterTVP(entitiesBatch)
                    : ConstructParameterValues(entitiesBatch);

                if (!UseOutputColumns)
                {
                    res += _dbContext.Database.ExecuteSqlRaw(sql, parameters);
                    continue;
                }

                using (DbCommand cmd = InitCommandWithParameters(sql, parameters, transaction))
                using (DbDataReader dr = cmd.ExecuteReader())
                {
                    res += ReadFromDataReader(entitiesBatch, dr);
                }
            }
            return res;
        }

        protected virtual async Task<int> ReadOutputAsync(List<TEntity>[] entityBatches, DbTransaction? transaction)
        {
            int res = 0;
            foreach (List<TEntity> entitiesBatch in entityBatches)
            {
                string commandText = ConstructCommand(entitiesBatch);
                DbParameter[] parameters = _useTVP
                    ? ConstructParameterTVP(entitiesBatch)
                    : ConstructParameterValues(entitiesBatch);

                if (!UseOutputColumns)
                {
                    res += await _dbContext.Database.ExecuteSqlRawAsync(commandText, parameters)
                        .ConfigureAwait(false);
                    continue;
                }

                using (DbCommand cmd = InitCommandWithParameters(commandText, parameters, transaction))
                using (DbDataReader dr = await cmd.ExecuteReaderAsync()
                    .ConfigureAwait(false))
                {
                    res += ReadFromDataReader(entitiesBatch, dr);
                }
            }

            return res;
        }

        protected virtual DbCommand InitCommandWithParameters(string sql, 
            DbParameter[] parameters, DbTransaction transaction)
        {
            //DbContext will dispose the connection
            DbConnection con = _dbContext.Database.GetDbConnection();
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            DbCommand cmd = transaction == null
                ? con.CreateCommand(sql)
                : con.CreateCommand(sql, transaction);
            cmd.CommandType = CommandType.Text;

            foreach (DbParameter param in parameters)
            {
                cmd.Parameters.Add(param);
            }

            return cmd;
        }

        protected virtual int ReadFromDataReader(List<TEntity> entities, DbDataReader datareader)
        {
            List<MappedProperty> outputProperties = Output.GetSelectedFlat();
            int changes = 0;

            //will read all if any rows returned 
            //will return false is no rows returned
            //will throw exception message if exception produced by SQL
            while (datareader.Read())
            {
                changes++;

                int sourceRowId = (int)datareader[SOURCE_ID_COLUMN_NAME];
                TEntity entity = entities[sourceRowId];

                foreach (MappedProperty prop in outputProperties)
                {
                    object? value = datareader[prop.DbColumnName];
                    Type propType = Nullable.GetUnderlyingType(prop.PropertyInfo.PropertyType) ?? prop.PropertyInfo.PropertyType;
                    value = value == null
                        ? null
                        : Convert.ChangeType(value, propType);
                    prop.PropertyInfo.SetValue(entity, value);
                }
            }

            return changes;
        }
    }
}

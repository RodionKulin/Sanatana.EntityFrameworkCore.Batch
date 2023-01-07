using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Batch.Internals.Services
{
    public class ReadCommandExecutor<TEntity>
        where TEntity : class
    {
        //field
        protected DbContext _dbContext;
        protected DbTransaction? _transaction;
        protected List<MappedProperty> _outputProperties;


        //init
        public ReadCommandExecutor(DbContext context, DbTransaction? transaction, List<MappedProperty> outputProperties)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _transaction = transaction;
            _outputProperties = outputProperties ?? throw new ArgumentNullException(nameof(outputProperties));
        }


        //public methods and validation
        public virtual int Execute(string sql, DbParameter[] parameters, List<TEntity>? entities = null)
        {
            bool hasOutput = _outputProperties.Count > 0;

            if (hasOutput)
            {
                return ReadOutput(sql, parameters, entities);
            }
            else
            {
                return _dbContext.Database.ExecuteSqlRaw(sql, parameters);
            }
        }

        public virtual async Task<int> ExecuteAsync(string sql, DbParameter[] parameters, List<TEntity>? entities = null)
        {
            bool hasOutput = _outputProperties.Count > 0;

            if (hasOutput)
            {
                return await ReadOutputAsync(sql, parameters, entities)
                    .ConfigureAwait(false);
            }
            else
            {
                return await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters)
                    .ConfigureAwait(false);
            }
        }

        public virtual List<TEntity> ReadOutputToNewEntities(string sql, DbParameter[] parameters)
        {
            (int _, List<TEntity> populatedEntities) = ReadOutputImp(sql, parameters, null);
            return populatedEntities;
        }

        public virtual async Task<List<TEntity>> ReadOutputToNewEntitiesAsync(string sql, DbParameter[] parameters)
        {
            (int _, List<TEntity> populatedEntities) = await ReadOutputImpAsync(sql, parameters, null)
                .ConfigureAwait(false);
            return populatedEntities;
        }

        public virtual int ReadOutput(string sql, DbParameter[] parameters, List<TEntity> entities)
        {
            entities = entities ?? throw new ArgumentNullException(nameof(entities));
            (int changes, List<TEntity> _) = ReadOutputImp(sql, parameters, entities);
            return changes;
        }

        public virtual async Task<int> ReadOutputAsync(string sql, DbParameter[] parameters, List<TEntity>? entities)
        {
            entities = entities ?? throw new ArgumentNullException(nameof(entities));
            (int changes, List<TEntity> _) = await ReadOutputImpAsync(sql, parameters, entities)
                .ConfigureAwait(false);
            return changes;
        }


        //protected methods
        protected virtual (int, List<TEntity>) ReadOutputImp(string sql, DbParameter[] parameters, List<TEntity>? entities)
        {
            DbConnection con = _dbContext.Database.GetDbConnection();
            using (DbCommand cmd = InitCommandWithParameters(sql, con, parameters))
            {
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                using (DbDataReader dr = cmd.ExecuteReader())
                {
                    int changes = ReadFromDataReader(dr, ref entities);
                    return (changes, entities);
                }
            }
        }

        protected virtual async Task<(int, List<TEntity>)> ReadOutputImpAsync(string sql, DbParameter[] parameters, List<TEntity>? entities)
        {
            DbConnection con = _dbContext.Database.GetDbConnection();
            using (DbCommand cmd = InitCommandWithParameters(sql, con, parameters))
            {
                if (con.State != ConnectionState.Open)
                {
                    await con.OpenAsync().ConfigureAwait(false);
                }
                using (DbDataReader dr = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    int changes = ReadFromDataReader(dr, ref entities);
                    return (changes, entities);
                }
            }
        }

        protected virtual DbCommand InitCommandWithParameters(string sql, DbConnection con, DbParameter[] parameters)
        {
            DbCommand cmd = _transaction == null
                ? con.CreateCommand(sql)
                : con.CreateCommand(sql, _transaction);
            cmd.CommandType = CommandType.Text;

            foreach (DbParameter param in parameters)
            {
                cmd.Parameters.Add(param);
            }

            return cmd;
        }

        protected virtual int ReadFromDataReader(DbDataReader dataReader, ref List<TEntity>? populatedEntities)
        {
            int entityIndex = 0;
            bool createNewItems = populatedEntities == null;
            populatedEntities = populatedEntities ?? new List<TEntity>();

            //will read all if any rows returned 
            //will return false is no rows returned
            //will throw exception message if exception produced by SQL
            while (dataReader.Read())
            {
                TEntity entity;
                if (createNewItems)
                {
                    entity = Activator.CreateInstance<TEntity>();
                    populatedEntities.Add(entity);
                }
                else
                {
                    entity = populatedEntities[entityIndex];
                }
                entityIndex++;

                foreach (MappedProperty prop in _outputProperties)
                {
                    object? value = dataReader[prop.DbColumnName];
                    Type propType = Nullable.GetUnderlyingType(prop.PropertyInfo.PropertyType) ?? prop.PropertyInfo.PropertyType;
                    value = value == null
                        ? null
                        : Convert.ChangeType(value, propType);
                    prop.PropertyInfo.SetValue(entity, value);
                }
            }

            return entityIndex;
        }
    }
}

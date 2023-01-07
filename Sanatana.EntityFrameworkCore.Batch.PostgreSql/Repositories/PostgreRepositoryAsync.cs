using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Commands;
using System.Linq.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.Commands;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories
{
    public class PostgreRepositoryAsync : RepositoryAsync
    {
        //init
        public PostgreRepositoryAsync(DbContext dbContext)
            : base(dbContext, new PostgreParametersService())
        {
        }


        //insert methods
        public override Task<int> InsertMany<TEntity>(IEnumerable<TEntity> entities, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new PostgreInsertCommand<TEntity>(entities, DbContext, DbParametersService, transaction)
                .ExecuteAsync();
        }

        public override InsertCommand<TEntity> InsertManyCommand<TEntity>(IEnumerable<TEntity> entities, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new PostgreInsertCommand<TEntity>(entities, DbContext, DbParametersService, transaction);
        }


        //update methods
        public override UpdateCommand<TEntity> UpdateMany<TEntity>(DbTransaction? transaction = null)
            where TEntity : class
        {
            return new PostgreUpdateCommand<TEntity>(DbContext, DbParametersService, transaction);
        }


        //merge methods
        /// <summary>
        /// Merge entity into the table and pass values as DbParameters.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="mergeType"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public override IMergeCommand<TEntity> Merge<TEntity>(TEntity entity, MergeTypeEnum mergeType, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new PostgreMergeCommand<TEntity>(DbContext, DbParametersService, mergeType, entity, transaction);
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as DbParameters.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityList"></param>
        /// <param name="mergeType"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public override IMergeCommand<TEntity> Merge<TEntity>(List<TEntity> entityList, MergeTypeEnum mergeType, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new PostgreMergeCommand<TEntity>(DbContext, DbParametersService, mergeType, entityList, transaction);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch.Internals.Expressions;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data.Common;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using System.Collections;
using Sanatana.EntityFrameworkCore.Batch.Internals;

namespace Sanatana.EntityFrameworkCore.Batch.Repositories
{
    /// <summary>
    /// Base Repository implementation. In produnction use derived classes like PostgreRepository or SqlRepository.
    /// </summary>
    public class Repository : IRepository
    {
        //properties
        public IDbParametersService DbParametersService { get; protected set; }
        public DbContext DbContext { get; protected set; }


        //init
        public Repository(DbContext dbContext, IDbParametersService dbParametersService)
        {
            DbParametersService = dbParametersService ?? throw new ArgumentNullException(nameof(dbParametersService));
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }


        //insert methods
        public virtual void InsertOne<TEntity>(TEntity item)
            where TEntity : class
        {
            DbContext.Set<TEntity>().Add(item);
            DbContext.SaveChanges();
        }

        public virtual int InsertMany<TEntity>(IEnumerable<TEntity> entities, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new InsertCommand<TEntity>(entities, DbContext, DbParametersService, transaction)
                .Execute();
        }

        public virtual InsertCommand<TEntity> InsertManyCommand<TEntity>(IEnumerable<TEntity> entities, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new InsertCommand<TEntity>(entities, DbContext, DbParametersService, transaction);
        }


        //select methods
        /// <summary>
        /// Select first row matching whereExpression predicate.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public virtual TEntity? SelectFirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> whereExpression)
            where TEntity : class
        {
            return DbContext.Set<TEntity>()
                .FirstOrDefault(whereExpression);
        }

        /// <summary>
        /// Select the only element of a sequence that satisfies a specified condition, 
        /// and throws an exception if more than one such element exists.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public virtual TEntity SelectSingle<TEntity>(Expression<Func<TEntity, bool>> whereExpression)
            where TEntity : class
        {
            try
            {
                return DbContext.Set<TEntity>()
                    .Single(whereExpression);
            }
            catch (InvalidOperationException inner)
            {
                throw new InvalidOperationException($"{nameof(SelectSingle)} query on type {typeof(TEntity).FullName} resulted in exception: {inner.Message}", inner);
            }
        }

        /// <summary>
        /// Select all rows matching whereExpression predicate.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public virtual List<TEntity> SelectAll<TEntity>(Expression<Func<TEntity, bool>> whereExpression)
            where TEntity : class
        {
            return DbContext.Set<TEntity>()
                .Where(whereExpression)
                .ToList();
        }

        /// <summary>
        /// Select a page of row, optionally getting total count of rows matching Where criteria.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TOrder"></typeparam>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderExpression"></param>
        /// <param name="countTotal"></param>
        /// <returns>If page number starts from 0, otherwise from 1.</returns>
        public virtual TotalResult<TEntity> SelectPage<TEntity, TOrder>(int pageIndex, int pageSize, bool descending
            , Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TOrder>> orderExpression
            , bool countTotal)
            where TEntity : class
        {
            var result = new TotalResult<TEntity>();

            result.Data = SelectPageQuery(pageIndex, pageSize
                , descending, whereExpression, orderExpression)
                .ToList();

            if (countTotal)
            {
                result.TotalRows = DbContext.Set<TEntity>()
                    .Where(whereExpression)
                    .LongCount();
            }

            return result;
        }

        /// <summary>
        /// Construct a query to select page of rows
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TOrder"></typeparam>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderExpression"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> SelectPageQuery<TEntity, TOrder>(int pageIndex, int pageSize, bool descending
            , Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TOrder>> orderExpression)
            where TEntity : class
        {
            int skip = SqlDataFomatting.ToSkipNumberZeroBased(pageIndex, pageSize);

            IQueryable<TEntity> query = DbContext.Set<TEntity>().AsQueryable();
            if (whereExpression != null)
                query = query.Where(whereExpression);

            if (descending)
                query = query.OrderByDescending(orderExpression);
            else
                query = query.OrderBy(orderExpression);

            if (skip > 0)
            {
                query = query.Skip(skip);
            }
            query = query.Take(pageSize);

            return query;
        }


        //update methods
        public virtual int UpdateOne<TEntity>(TEntity entity)
            where TEntity : class
        {
            DbContext.Set<TEntity>().Attach(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
            return DbContext.SaveChanges();
        }

        public virtual int UpdateOne<TEntity>(TEntity entity
            , params Expression<Func<TEntity, object>>[] properties)
            where TEntity : class
        {
            DbContext.Set<TEntity>().Attach(entity);
            EntityEntry<TEntity> entry = DbContext.Entry(entity);

            foreach (Expression<Func<TEntity, object>> prop in properties)
            {
                entry.Property(prop).IsModified = true;
            }

            return DbContext.SaveChanges();
        }

        public virtual UpdateCommand<TEntity> UpdateMany<TEntity>(DbTransaction? transaction = null)
            where TEntity : class
        {
            return new UpdateCommand<TEntity>(DbContext, DbParametersService, transaction);
        }


        //delete methods
        public virtual int DeleteOne<TEntity>(TEntity entity)
            where TEntity : class
        {
            DbContext.Set<TEntity>().Attach(entity);
            DbContext.Entry(entity).State = EntityState.Deleted;

            return DbContext.SaveChanges();
        }

        public virtual int DeleteOne(object entity)
        {
            DbContext.Attach(entity);
            DbContext.Entry(entity).State = EntityState.Deleted;

            return DbContext.SaveChanges();
        }

        public virtual int DeleteMany<TEntity>(Expression<Func<TEntity, bool>> whereExpression)
            where TEntity : class
        {
            var command = new DeleteCommand<TEntity>(DbContext, DbParametersService)
                .SetWhere(whereExpression);
            int changes = command.Execute();
            return changes;
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
        public virtual IMergeCommand<TEntity> Merge<TEntity>(TEntity entity, MergeTypeEnum mergeType, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new MergeCommand<TEntity>(DbContext, DbParametersService, mergeType, entity, transaction);
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as DbParameters.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityList"></param>
        /// <param name="mergeType"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual IMergeCommand<TEntity> Merge<TEntity>(List<TEntity> entityList, MergeTypeEnum mergeType, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new MergeCommand<TEntity>(DbContext, DbParametersService, mergeType, entityList, transaction);
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as TVP. Order of selected Source fields must match the order of columns in TVP declaration.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityList"></param>
        /// <param name="mergeType"></param>
        /// <param name="sqlTVPName"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual IMergeCommand<TEntity> MergeTVP<TEntity>(List<TEntity> entityList, MergeTypeEnum mergeType, string sqlTVPName, DbTransaction? transaction = null)
            where TEntity : class
        {
            return new MergeCommand<TEntity>(DbContext, DbParametersService, mergeType, entityList, sqlTVPTypeName: sqlTVPName, transaction: transaction);
        }


        //Disposable
        public virtual void Dispose()
        {
            if(DbContext != null)
            {
                DbContext.Dispose();
            }
        }
    }
}

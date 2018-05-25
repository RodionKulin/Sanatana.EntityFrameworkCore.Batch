﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Expressions;
using Sanatana.EntityFrameworkCore.Commands.Merge;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Sanatana.EntityFrameworkCore.Commands
{
    public class Repository : IDisposable
    {
        //properties
        public DbContext Context { get; set; }


        //init
        public Repository(DbContext context)
        {
            Context = context;
        }


        //methods
        public virtual InsertCommand<TEntity> Insert<TEntity>(SqlTransaction transaction = null)
            where TEntity : class
        {
            return new InsertCommand<TEntity>(Context, transaction);
        }

        public virtual RepositoryResult<TEntity> SelectPage<TEntity, TOrder>(int page, int pageSize, bool descending
            , Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TOrder>> orderExpression
            , bool countTotal)
            where TEntity : class
        {
            var result = new RepositoryResult<TEntity>();

            result.Data = SelectPageQuery(page, pageSize, descending, whereExpression, orderExpression)
                    .ToList();

            if (countTotal)
            {
                result.TotalRows = Context.Set<TEntity>()
                    .Where(whereExpression)
                    .LongCount();
            }
            
            return result;
        }

        public virtual async Task<RepositoryResult<TEntity>> SelectPageAsync<TEntity, TOrder>(int page, int pageSize, bool descending
            , Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TOrder>> orderExpression
            , bool countTotal)
            where TEntity : class
        {
            var result = new RepositoryResult<TEntity>();

            Task<List<TEntity>> listQuery = SelectPageQuery(page, pageSize, descending, whereExpression, orderExpression)
                   .ToListAsync();

            Task<long> countQuery = null;
            if (countTotal)
            {
                countQuery = Context.Set<TEntity>()
                    .Where(whereExpression)
                    .LongCountAsync();
            }

            result.Data = await listQuery.ConfigureAwait(false);
            if (countTotal)
            {
                result.TotalRows = await countQuery.ConfigureAwait(false);
            }
            
            return result;
        }

        public virtual IQueryable<TEntity> SelectPageQuery<TEntity, TOrder>(int page, int pageSize, bool descending
            , Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TOrder>> orderExpression)
            where TEntity : class
        {
            int skip = SqlUtility.ToSkipNumber(page, pageSize);

            IQueryable<TEntity> query = Context.Set<TEntity>().AsQueryable();
            if (whereExpression != null)
                query = query.Where(whereExpression);

            if (descending)
                query = query.OrderByDescending(orderExpression);
            else
                query = query.OrderBy(orderExpression);

            if (skip > 0)
                query = query.Skip(skip);
            query = query.Take(pageSize);

            return query;
        }

        public virtual int UpdateOne<TEntity>(TEntity entity)
            where TEntity : class
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry<TEntity>(entity).State = EntityState.Modified;

            int changes = Context.SaveChanges();
       
            return changes;
        }

        public virtual UpdateCommand<TEntity> UpdateMany<TEntity>(Expression<Func<TEntity, bool>> matchExpression)
            where TEntity : class
        {
            return new UpdateCommand<TEntity>(Context, matchExpression);
        }

        public virtual async Task<int> UpdateOneAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry<TEntity>(entity).State = EntityState.Modified;

            int changes = await Context.SaveChangesAsync().ConfigureAwait(false);

            return changes;
        }

        public virtual int UpdateOne<TEntity>(TEntity entity
            , params Expression<Func<TEntity, object>>[] properties)
            where TEntity : class
        {
            Context.Set<TEntity>().Attach(entity);
            EntityEntry<TEntity> entry = Context.Entry<TEntity>(entity);

            foreach (Expression<Func<TEntity, object>> prop in properties)
            {
                entry.Property(prop).IsModified = true;
            }
            
            int changes = Context.SaveChanges();
            return changes;
        }

        public virtual async Task<int> UpdateOneAsync<TEntity>(TEntity entity
            , params Expression<Func<TEntity, object>>[] properties)
            where TEntity : class
        {
            Context.Set<TEntity>().Attach(entity);
            EntityEntry<TEntity> entry = Context.Entry<TEntity>(entity);

            foreach (Expression<Func<TEntity, object>> prop in properties)
            {
                entry.Property(prop).IsModified = true;
            }
            
            int changes = await Context.SaveChangesAsync().ConfigureAwait(false);
            return changes;
        }

        public virtual int DeleteOne<TEntity>(TEntity entity)
            where TEntity : class
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry<TEntity>(entity).State = EntityState.Deleted;

            int changes = Context.SaveChanges();

            return changes;
        }

        public virtual int DeleteOne(object entity)
        {
            Context.Attach(entity);
            Context.Entry(entity).State = EntityState.Deleted;

            int changes = Context.SaveChanges();

            return changes;
        }

        public virtual int DeleteMany<TEntity>(Expression<Func<TEntity, bool>> matchExpression)
            where TEntity : class
        {
            var command = new DeleteCommand<TEntity>(Context);
            int changes = command.Execute(matchExpression);
            return changes;
        }

        public virtual async Task<int> DeleteOneAsync(object entity)
        {
            Context.Attach(entity);
            Context.Entry(entity).State = EntityState.Deleted;

            int changes = await Context.SaveChangesAsync().ConfigureAwait(false);
            return changes;
        }

        public virtual async Task<int> DeleteOneAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry<TEntity>(entity).State = EntityState.Deleted;

            int changes = await Context.SaveChangesAsync().ConfigureAwait(false);

            return changes;
        }

        public virtual async Task<int> DeleteManyAsync<TEntity>(Expression<Func<TEntity, bool>> matchExpression)
            where TEntity : class
        {
            var command = new DeleteCommand<TEntity>(Context);
            int changes = await command.ExecuteAsync(matchExpression);
            return changes;
        }

        /// <summary>
        /// Merge entity into the table and pass values as SqlParameters.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual MergeCommand<TEntity> MergeParameters<TEntity>(TEntity entity, SqlTransaction transaction = null)
            where TEntity : class
        {
            return new MergeCommand<TEntity>(Context, entity, transaction);
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as SqlParameters.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual MergeCommand<TEntity> MergeParameters<TEntity>(List<TEntity> entityList, SqlTransaction transaction = null)
            where TEntity : class
        {
            return new MergeCommand<TEntity>(Context, entityList, transaction);
        }

        /// <summary>
        /// Merge list of entities into the table and pass values as TVP. Order of selected Source fields must match the order of columns in TVP declaration.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityList"></param>
        /// <param name="sqlTVPName"></param>
        /// <returns></returns>
        public virtual MergeCommand<TEntity> MergeTVP<TEntity>(List<TEntity> entityList, string sqlTVPName)
            where TEntity : class
        {
            return new MergeCommand<TEntity>(Context, entityList, sqlTVPName);
        }



        public virtual void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
            }
        }
    }
}

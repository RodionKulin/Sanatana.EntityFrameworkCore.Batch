using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Internals;
using System.Data.Common;
using System.Linq.Expressions;

namespace Sanatana.EntityFrameworkCore.Batch.Repositories
{
    public interface IRepository : IDisposable
    {
        IDbParametersService DbParametersService { get; }
        DbContext DbContext { get; }
        
        int InsertMany<TEntity>(IEnumerable<TEntity> entities, DbTransaction? transaction = null) where TEntity : class;
        InsertCommand<TEntity> InsertManyCommand<TEntity>(IEnumerable<TEntity> entities, DbTransaction? transaction = null) where TEntity : class;
        void InsertOne<TEntity>(TEntity item) where TEntity : class;
        IMergeCommand<TEntity> Merge<TEntity>(List<TEntity> entityList, MergeTypeEnum mergeType, DbTransaction? transaction = null) where TEntity : class;
        IMergeCommand<TEntity> Merge<TEntity>(TEntity entity, MergeTypeEnum mergeType, DbTransaction? transaction = null) where TEntity : class;
        TEntity? SelectFirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> whereExpression) where TEntity : class;
        TEntity SelectSingle<TEntity>(Expression<Func<TEntity, bool>> whereExpression) where TEntity : class;
        List<TEntity> SelectAll<TEntity>(Expression<Func<TEntity, bool>> whereExpression) where TEntity : class;
        TotalResult<TEntity> SelectPage<TEntity, TOrder>(int pageIndex, int pageSize, bool descending, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TOrder>> orderExpression, bool countTotal) where TEntity : class;
        IQueryable<TEntity> SelectPageQuery<TEntity, TOrder>(int pageIndex, int pageSize, bool descending, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TOrder>> orderExpression) where TEntity : class;
        UpdateCommand<TEntity> UpdateMany<TEntity>(DbTransaction? transaction = null) where TEntity : class;
        int UpdateOne<TEntity>(TEntity entity) where TEntity : class;
        int UpdateOne<TEntity>(TEntity entity, params Expression<Func<TEntity, object>>[] properties) where TEntity : class;


        int DeleteMany<TEntity>(Expression<Func<TEntity, bool>> whereExpression) where TEntity : class;
        int DeleteOne(object entity);
        int DeleteOne<TEntity>(TEntity entity) where TEntity : class;

    }
}
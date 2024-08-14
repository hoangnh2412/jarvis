using System.Linq.Expressions;
using EFCore.BulkExtensions;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;

namespace Jarvis.EntityFramework.Repositories;

public class BaseBulkCommandRepository<TEntity> : BaseCommandRepository<TEntity>, IBulkCommandRepository<TEntity>
    where TEntity : class, IEntity
{
    public async Task InsertBatchAsync(IEnumerable<TEntity> entities)
    {
        await DbContextBulkExtensions.BulkInsertAsync(StorageContext, entities);
    }

    public async Task UpdateBatchAsync(IEnumerable<TEntity> entities, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        foreach (var item in entities)
        {
            var memberInitExpression = (MemberInitExpression)updateFactory.Body;
            foreach (MemberAssignment memberAssignment in memberInitExpression.Bindings)
            {
                var property = typeof(TEntity).GetProperty(memberAssignment.Member.Name);

                object? newValue = ((ConstantExpression)memberAssignment.Expression).Value;
                if (memberAssignment.Expression.NodeType != ExpressionType.Constant)
                {
                    var lambda = Expression.Lambda(memberAssignment.Expression);
                    var compiledLambda = lambda.Compile();
                    newValue = compiledLambda.DynamicInvoke();
                }

                property?.SetValue(item, newValue);
            }
        }

        await DbContextBulkExtensions.BulkUpdateAsync(StorageContext, entities);
    }

    public async Task<int> UpdateBatchAsync(IQueryable<TEntity> queryable, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return await queryable.UpdateFromQueryAsync(updateFactory);
    }

    public async Task DeleteBatchAsync(IEnumerable<TEntity> entities)
    {
        await DbContextBulkExtensions.BulkDeleteAsync(StorageContext, entities);
    }

    public async Task<int> DeleteBatchAsync(IQueryable<TEntity> queryable)
    {
        return await BatchDeleteExtensions.DeleteFromQueryAsync(queryable);
    }
}
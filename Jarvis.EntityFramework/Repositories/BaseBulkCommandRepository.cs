using System.Linq.Expressions;
using EFCore.BulkExtensions;
using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;

namespace Jarvis.EntityFramework.Repositories;

public class BaseBulkCommandRepository<TEntity> : BaseCommandRepository<TEntity>, IBulkCommandRepository<TEntity>
    where TEntity : class, IEntity
{
    public async Task InsertBatchAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbContextBulkExtensions.BulkInsertAsync(StorageContext, entities, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateBatchAsync(
        IEnumerable<TEntity> entities,
        Expression<Func<TEntity, TEntity>> updateFactory,
        CancellationToken cancellationToken = default)
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

        await DbContextBulkExtensions.BulkUpdateAsync(StorageContext, entities, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> UpdateBatchAsync(
        IQueryable<TEntity> queryable,
        Expression<Func<TEntity, TEntity>> updateFactory,
        CancellationToken cancellationToken = default)
    {
        return await queryable.UpdateFromQueryAsync(updateFactory, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteBatchAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbContextBulkExtensions.BulkDeleteAsync(StorageContext, entities, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> DeleteBatchAsync(IQueryable<TEntity> queryable, CancellationToken cancellationToken = default)
    {
        return await BatchDeleteExtensions.DeleteFromQueryAsync(queryable, cancellationToken).ConfigureAwait(false);
    }
}

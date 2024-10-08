﻿using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.Interfaces.Repositories;

/// <summary>
/// The interface abstract repositories
/// </summary>
public interface IRepository
{
    void SetStorageContext(IStorageContext storageContext);
}

/// <summary>
/// The interface abstract repositories with read and write operations
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IEFRepository<TEntity> : IQueryRepository<TEntity>, ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
}
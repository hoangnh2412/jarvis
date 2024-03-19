using Microsoft.Extensions.Localization;
using Jarvis.Application.CrudGeneric;
using Jarvis.Application.DTOs;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.WebApi.CrudGeneric;

public abstract class CrudAuditableController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>
    : CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>
    where TCrudService : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity, ILogUpdatedEntity
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    private readonly TCrudService _crudService;
    private readonly IStringLocalizer<CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>> _localizer;

    public CrudAuditableController(
        TCrudService crudService,
        IStringLocalizer<CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>> localizer) : base(crudService, localizer)
    {
        _crudService = crudService;
        _localizer = localizer;
    }
}
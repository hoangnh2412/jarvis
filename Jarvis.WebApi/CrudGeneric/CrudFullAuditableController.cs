using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Jarvis.Application.CrudGeneric;
using Jarvis.Application.DTOs;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.WebApi.CrudGeneric;

public abstract class CrudFullAuditableController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>
    : CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>
    where TCrudService : ICrudFullAuditableService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity<TKey>
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    private readonly TCrudService _crudService;
    private readonly IStringLocalizer<CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>> _localizer;

    public CrudFullAuditableController(
        TCrudService crudService,
        IStringLocalizer<CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>> localizer) : base(crudService, localizer)
    {
        _crudService = crudService;
        _localizer = localizer;
    }

    /// <summary>
    /// Soft delete by Id
    /// </summary>
    /// <param name="crudService"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("trash/{id}")]
    public virtual async Task<IActionResult> TrashAsync(
        [FromServices] TCrudService crudService,
        [FromRoute] TKey id)
    {
        var rows = await crudService.TrashByIdAsync(id);
        if (rows == 0)
            return NotFound("Dữ liệu không tồn tại");

        return Ok();
    }
}
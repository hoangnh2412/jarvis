using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Jarvis.Application.CrudGeneric;
using Jarvis.Application.DTOs;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.WebApi.CrudGeneric;

/// <summary>
/// Generic controller with Create, Update, Delete, Pagination of a Entity
/// </summary>
/// <typeparam name="TUnitOfWork">Type of UnitOfWork contains Entity</typeparam>
/// <typeparam name="TKey">Data type field Id of Entity</typeparam>
/// <typeparam name="TEntity">Type of Entity</typeparam>
/// <typeparam name="TModel">Type of model return for method Get</typeparam>
/// <typeparam name="TPagingInput">Type of input model for method Pagination</typeparam>
/// <typeparam name="TPagingOutput">Type of output model for method Pagination</typeparam>
/// <typeparam name="TCreateInput">Type of input model for method Create</typeparam>
/// <typeparam name="TUpdateInput">Type of input model for method Update</typeparam>
/// <typeparam name="TCrudService">Type of CrudService</typeparam>
public abstract class CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>
    : ControllerBase
    where TCrudService : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    private readonly TCrudService _crudService;
    private readonly IStringLocalizer<CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>> _localizer;

    public CrudBasicController(
        TCrudService crudService,
        IStringLocalizer<CrudBasicController<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput, TCrudService>> localizer)
    {
        _crudService = crudService;
        _localizer = localizer;
    }

    /// <summary>
    /// Return list of items after pagination, not get all items
    /// </summary>
    /// <param name="paging"></param>
    /// <param name="crudService"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<IActionResult> PaginationAsync(
        [FromQuery] TPagingInput paging)
    {
        var data = await _crudService.PaginationAsync(paging, true);
        return Ok(data);
    }

    /// <summary>
    /// Return a item
    /// </summary>
    /// <param name="data"></param>
    /// <param name="crudService"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetAsync(
        [FromRoute] TKey id)
    {
        TModel item = await _crudService.GetByIdAsync(id);

        // if (type == null)
        // {
        //     var id = data.ToGenericType<TKey>();
        //     if (id.Equals(default(TKey)))
        //         return BadRequest($"{data} is not valid");

        //     item = await _crudService.GetByIdAsync(id);
        // }
        // else
        // {
        //     if (!Guid.TryParse(data, out Guid key))
        //         return BadRequest($"{data} is not valid");

        //     // TODO: Refactor to other Controller
        //     var id = data.ToGenericType<TKey>();
        //     item = await _crudService.GetByIdAsync(id);
        // }

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Create new item
    /// </summary>
    /// <param name="input"></param>
    /// <param name="crudService"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<IActionResult> CreateAsync(
        [FromBody] TCreateInput input)
    {
        var model = await _crudService.CreateAsync(input);
        if (model.Equals(default(TModel)))
            return Conflict();

        return Ok();
    }

    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatchAsync(
        [FromBody] IEnumerable<TCreateInput> input
    )
    {
        var model = await _crudService.CreateManyAsync(input);
        if (model.Equals(default(TModel)))
            return Conflict();

        return Ok();
    }

    /// <summary>
    /// Update a item by Key
    /// </summary>
    /// <param name="data"></param>
    /// <param name="input"></param>
    /// <param name="crudService"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public virtual async Task<IActionResult> UpdateAsync(
        [FromRoute] TKey id,
        [FromBody] TUpdateInput input)
    {
        TModel model = await _crudService.UpdateAsync(id, input);
        if (model == null || model.Equals(default(TModel)))
            return NotFound();

        return Ok();
    }

    [HttpPut("batch")]
    public virtual async Task<IActionResult> UpdateAsync(
        [FromBody] IEnumerable<TUpdateInput> input)
    {
        // IEnumerable<TModel> model = await _crudService.UpdateBatchAsync(x => x.Id != null);
        // if (model == null || model.Equals(default(TModel)))
        //     return NotFound();
        await Task.Yield();
        return Ok();
    }

    /// <summary>
    /// Delete item by Key
    /// </summary>
    /// <param name="crudService"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> DeleteAsync(
        [FromRoute] TKey id)
    {
        TModel model = await _crudService.DeleteByIdAsync(id, true);
        // if (type == null)
        // {
        //     var id = data.ToGenericType<TKey>();
        //     if (id.Equals(default(TKey)))
        //         return BadRequest($"{data} is not valid");

        //     model = await _crudService.DeleteByIdAsync(id, true);
        // }
        // else
        // {
        //     if (!Guid.TryParse(data, out Guid key))
        //         return BadRequest($"{data} is not valid");

        //     // TODO: Refactor to other Controller
        //     var id = data.ToGenericType<TKey>();
        //     model = await _crudService.DeleteByIdAsync(id, true);
        // }

        if (model == null || model.Equals(default(TModel)))
            return NotFound();

        return Ok();
    }
}
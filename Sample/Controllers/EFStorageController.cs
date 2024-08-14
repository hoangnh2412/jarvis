using Jarvis.Application.Extensions;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;
using Sample.DataStorage.EntityFramework;
using Sample.RequestResponseModels;

namespace Sample.Controllers;

[ApiController]
[Route("ef")]
public class EFStorageController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public EFStorageController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("read")]
    public async Task<IActionResult> ReadAsync(
        [FromQuery] CustomPaging paging,
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IEFRepository<User>>();
        CustomPaged<User> users = (Paged<User>)await repo.GetQuery().ToPagedAsync(paging);
        return Ok(users);
    }

    [HttpGet("create")]
    public async Task<IActionResult> CreateAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IEFRepository<User>>();
        var users = await repo.InsertAsync(new DataStorage.User
        {
            Name = "sample",
            Age = 11
        });
        await uow.CommitAsync();
        return Ok(users);
    }

    [HttpGet("update")]
    public async Task<IActionResult> UpdateAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IEFRepository<User>>();
        var user = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id == 1);

        user.Name = "updated";
        user.Age = 12;

        await repo.UpdateAsync(user);
        await uow.CommitAsync();
        return Ok(user);
    }

    [HttpGet("delete")]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IEFRepository<User>>();
        var user = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id == 2);

        await repo.DeleteAsync(user);
        await uow.CommitAsync();
        return Ok(user);
    }
}
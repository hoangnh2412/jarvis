using Jarvis.Domain.Repositories;
using Jarvis.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Controllers;

[Route("api/v1/storage")]
[ApiController]
public class StorageController(
    ISampleUnitOfWork uow
) : ControllerBase
{
    private readonly ISampleUnitOfWork _uow = uow;
    private readonly IRepository<Student> _repo = uow.GetRepository<IRepository<Student>>();

    [HttpGet]
    public async Task<IActionResult> GetAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var items = await _repo.GetQuery().ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] Student student
    )
    {
        await _repo.InsertAsync(new Student
        {
            Id = Guid.NewGuid(),
            Name = StringExtension.GenerateRandom(10, true, true, true, true),
            Age = Random.Shared.Next(1, 100),
        });
        await _uow.CommitAsync();
        return Ok();
    }
}
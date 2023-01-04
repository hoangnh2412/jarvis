using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Models;
using Jarvis.Core.Permissions;
using System;
using System.Linq;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Services;
using System.Threading.Tasks;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Labels;
using Jarvis.Core.Events.Labels;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("labels")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        [HttpGet]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Read))]
        public async Task<IActionResult> GetAsync(
            [FromQuery] Paging paging,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workcontext)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.LabelPolicy.Label_Read));

            var repo = uow.GetRepository<ILabelRepository>();
            var paged = await repo.PagingAsync(context, paging);
            var result = new Paged<LabelModel>
            {
                Data = paged.Data.Select(x => (LabelModel)x),
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
            return Ok(result);
        }

        [HttpGet("{code}")]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Read))]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid code,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.LabelPolicy.Label_Read));

            var repo = uow.GetRepository<ILabelRepository>();
            var label = await repo.GetByCodeAsync(context, code);
            return Ok((LabelModel)label);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Create))]
        public async Task<IActionResult> PostAsync(
            [FromBody] LabelModel model,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();
            var code = Guid.NewGuid();
            var repo = uow.GetRepository<ILabelRepository>();
            await repo.InsertAsync(new Label
            {
                Color = model.Color,
                Description = model.Description,
                Icon = model.Icon,
                Code = code,
                TenantCode = tenantCode,
                Name = model.Name,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = workcontext.GetUserCode()
            });
            await uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<LabelCreatedEventModel>, ILabelCreatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new LabelCreatedEventModel
                {
                    TenantCode = tenantCode,
                    Code = code,
                    Name = model.Name
                });
            });
            return Ok();
        }

        [HttpPut("{code}")]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Update))]
        public async Task<IActionResult> PutAsync(
            [FromRoute] Guid code,
            [FromBody] LabelModel model,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();

            var repo = uow.GetRepository<ILabelRepository>();
            var label = await repo.GetByCodeAsync(tenantCode, code);
            if (label == null)
                return NotFound();

            label.Color = model.Color;
            label.Description = model.Description;
            label.Icon = model.Icon;
            label.Name = model.Name;
            label.UpdatedAt = DateTime.Now;
            label.UpdatedAtUtc = DateTime.UtcNow;
            label.UpdatedBy = Guid.Empty;
            repo.Update(label);
            await uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<LabelUpdatedEventModel>, ILabelUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new LabelUpdatedEventModel
                {
                    TenantCode = tenantCode,
                    Code = code,
                    Name = model.Name
                });
            });
            return Ok();
        }

        [HttpDelete("{code}")]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Delete))]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid code,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();

            var repo = uow.GetRepository<ILabelRepository>();
            var label = await repo.GetByCodeAsync(tenantCode, code);
            if (label == null)
                return NotFound();

            repo.Delete(label);
            await uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<LabelDeletedEventModel>, ILabelDeletedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new LabelDeletedEventModel
                {
                    TenantCode = tenantCode,
                    Code = code
                });
            });
            return Ok();
        }
    }
}

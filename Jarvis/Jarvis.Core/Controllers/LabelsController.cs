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

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("labels")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        private readonly ICoreUnitOfWork _uow;
        private readonly IWorkContext _workcontext;

        public LabelsController(
            ICoreUnitOfWork uow,
            IWorkContext workcontext)
        {
            _uow = uow;
            _workcontext = workcontext;
        }

        [HttpGet]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Read))]
        public async Task<IActionResult> GetAsync([FromQuery]Paging paging)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.LabelPolicy.Label_Read));

            var repo = _uow.GetRepository<ILabelRepository>();
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
        public async Task<IActionResult> GetAsync(Guid code)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.LabelPolicy.Label_Read));

            var repo = _uow.GetRepository<ILabelRepository>();
            var label = await repo.GetByCodeAsync(context, code);
            return Ok((LabelModel)label);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Create))]
        public async Task<IActionResult> PostAsync([FromBody] LabelModel model)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.LabelPolicy.Label_Create));

            var repo = _uow.GetRepository<ILabelRepository>();
            await repo.InsertAsync(new Label
            {
                Color = model.Color,
                Description = model.Description,
                Icon = model.Icon,
                Code = Guid.NewGuid(),
                TenantCode = await _workcontext.GetTenantCodeAsync(),
                Name = model.Name,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = _workcontext.GetUserCode()
            });
            await _uow.CommitAsync();
            return Ok();
        }

        [HttpPut("{code}")]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Update))]
        public async Task<IActionResult> PutAsync([FromRoute]Guid code, [FromBody]LabelModel model)
        {
            var permission = await _workcontext.GetContextAsync(nameof(CorePolicy.LabelPolicy.Label_Update));
            var repo = _uow.GetRepository<ILabelRepository>();
            var label = await repo.GetByCodeAsync(permission, code);
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
            await _uow.CommitAsync();

            return Ok();
        }

        [HttpDelete("{code}")]
        [Authorize(nameof(CorePolicy.LabelPolicy.Label_Delete))]
        public async Task<IActionResult> DeleteAsync([FromRoute]Guid code)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.LabelPolicy.Label_Delete));

            var repo = _uow.GetRepository<ILabelRepository>();
            var label = await repo.GetByCodeAsync(context, code);
            if (label == null)
                return NotFound();

            repo.Delete(label);
            await _uow.CommitAsync();

            return Ok();
        }
    }
}

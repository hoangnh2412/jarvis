using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Models.Emails;
using Jarvis.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("emails/templates")]
    [ApiController]
    public class EmailTemplateCrudController : ApiCrudController<ICoreUnitOfWork, int, EmailTemplate, EmailTemplateModel, IEmailTemplateCrudService, EmailTemplatePagingOutput, EmailTemplateCreateInput, EmailTemplateUpdateInput>
    {
    }
}
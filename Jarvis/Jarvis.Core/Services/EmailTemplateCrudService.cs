using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Models.Emails;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Services
{
    public interface IEmailTemplateCrudService : ICrudService<ICoreUnitOfWork, int, EmailTemplate, EmailTemplateModel, EmailTemplatePagingOutput, EmailTemplateCreateInput, EmailTemplateUpdateInput>
    {
        Task<EmailTemplateModel> GetByCodeAsync(string code);
    }

    public class EmailTemplateCrudService : CrudService<ICoreUnitOfWork, int, EmailTemplate, EmailTemplateModel, EmailTemplatePagingOutput, EmailTemplateCreateInput, EmailTemplateUpdateInput>, IEmailTemplateCrudService
    {
        private readonly ICoreUnitOfWork _uow;
        private readonly IDomainWorkContext _workContext;

        public EmailTemplateCrudService(ICoreUnitOfWork uow, IDomainWorkContext workContext) : base(uow, workContext)
        {
            _uow = uow;
            _workContext = workContext;
        }

        public async Task<EmailTemplateModel> GetByCodeAsync(string code)
        {
            var repo = _uow.GetRepository<IRepository<EmailTemplate>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Code == code);
            return EmailTemplateModel.MapToModel(entity);
        }

        protected override EmailTemplate MapToEntity(EmailTemplateCreateInput input)
        {
            return EmailTemplateCreateInput.MapToEntity(input);
        }

        protected override EmailTemplate MapToEntity(EmailTemplateUpdateInput input, EmailTemplate entity)
        {
            return EmailTemplateUpdateInput.MapToEntity(input, entity);
        }

        protected override EmailTemplateModel MapToModel(EmailTemplate entity)
        {
            return EmailTemplateModel.MapToModel(entity);
        }

        protected override EmailTemplatePagingOutput MapToPagingOutput(EmailTemplate entity)
        {
            return EmailTemplatePagingOutput.MapToModel(entity);
        }
    }
}
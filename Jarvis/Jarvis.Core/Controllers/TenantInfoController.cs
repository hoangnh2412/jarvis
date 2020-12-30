using Jarvis.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Database;
using System.Linq;
using Jarvis.Core.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Constants;
using System.Threading.Tasks;
using Jarvis.Core.Models.Tenant;
using System;
using Jarvis.Core.Permissions;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Tenants;
using Jarvis.Core.Events.Tenants;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("tenant-info")]
    [ApiController]
    public class TenantInfoController : ControllerBase
    {
        private readonly ICoreUnitOfWork _uow;
        private readonly IWorkContext _workContext;
        private readonly IEventFactory _eventFactory;

        public TenantInfoController(
            ICoreUnitOfWork uow,
            IWorkContext workContext,
            IEventFactory eventFactory)
        {
            _uow = uow;
            _workContext = workContext;
            _eventFactory = eventFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var info = await repoTenant.GetInfoByCodeAsync(tenantCode);
            if (info == null)
                return NotFound();

            var model = new TenantInfoModel
            {
                Id = info.Id,
                Code = info.Code,
                FullNameVi = info.FullNameVi,
                TaxCode = info.TaxCode,
                Address = info.Address,
                City = info.City,
                District = info.District,
                Country = info.Country,
                Emails = info.Emails,
                Fax = info.Fax,
                FullNameEn = info.FullNameEn,
                LegalName = info.LegalName,
                BusinessType = info.BusinessType,
                SecretKey = info.SecretKey
            };

            //Nếu setting ko có metadata thì trả về kết quả luôn
            var repoSetting = _uow.GetRepository<ISettingRepository>();
            var setting = await repoSetting.GetByKeyAsync(Guid.Empty, SettingKey.ThongTinDoanhNghiep_Khac.ToString());

            if (setting == null)
                return Ok(model);

            //Metadata lấy được từ DB
            var metadatas = string.IsNullOrWhiteSpace(info.Metadata) ? new List<MetadataModel>() : JsonConvert.DeserializeObject<List<MetadataModel>>(info.Metadata);

            model.Metadata = new List<MetadataModel>();
            var splited1 = setting.Value.Split("|");
            foreach (var item in splited1)
            {
                var splited2 = item.Split(":");
                var key = splited2[0];
                var name = splited2[1];
                var metadata = metadatas.FirstOrDefault(x => x.Key == key);

                //Nếu DB ko lưu value thì trả về null
                model.Metadata.Add(new MetadataModel
                {
                    Key = key,
                    Name = name,
                    Value = metadata == null ? null : metadata.Value
                });
            }

            return Ok(model);
        }

        [HttpPut]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Update))]
        public async Task<IActionResult> PutAsync([FromBody] TenantInfoModel command)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();

            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var info = await repoTenant.GetInfoByCodeAsync(tenantCode);
            info.City = command.City;
            info.District = command.District;
            info.LegalName = command.LegalName;
            info.Fax = command.Fax;
            info.Emails = command.Emails;
            info.BusinessType = command.BusinessType;
            info.SecretKey = command.SecretKey;

            if (command.Metadata != null && command.Metadata.Count != 0)
                info.Metadata = JsonConvert.SerializeObject(command.Metadata);

            repoTenant.UpdateInfo(info);
            await _uow.CommitAsync();

            _eventFactory.GetOrAddEvent<IEvent<TenantInfoUpdatedEventModel>, ITenantInfoUpdatedEvent>().ForEach((e) =>
            {
                e.PublishAsync(new TenantInfoUpdatedEventModel
                {
                    TenantCode = tenantCode
                });
            });
            return Ok();
        }

        [HttpGet("taxCode/{includeHierarchy}")]
        [Authorize]
        public async Task<IActionResult> GetTaxCodeAsync([FromRoute] bool includeHierarchy)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var listTaxCodes = new List<KeyValuePair<string, Guid>>();

            if (!includeHierarchy)
            {
                var tenant = await repoTenant.GetInfoByCodeAsync(tenantCode);
                if (tenant == null)
                    return NotFound();

                listTaxCodes.Add(new KeyValuePair<string, Guid>(tenant.TaxCode, tenant.Code));
            }
            else
            {
                // nếu lấy dữ liệu của chi nhánh
                var hierarchies = await repoTenant.GetHierarchyByCodeAsync(tenantCode);
                listTaxCodes = hierarchies.Select(x => new KeyValuePair<string, Guid>(x.Name, x.Code)).ToList();
            }

            return Ok(listTaxCodes.Select(x => new { TaxCode = x.Key, Code = x.Value}));
        }
    }
}

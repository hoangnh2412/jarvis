using Jarvis.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Permissions;
using Infrastructure.Database.Models;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using System.Linq;
using System.Collections.Generic;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Constants;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Database.Entities;
using Jarvis.Core.Models.Tenant;
using Jarvis.Core.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Infrastructure.Abstractions;
using Infrastructure;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Tenants;
using Jarvis.Core.Events.Tenants;

namespace Jarvis.Core.Controllers
{
    //TODO: Cần tách job update metadata nếu update setting
    [Authorize]
    [Route("tenants")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ICoreUnitOfWork _uow;
        private readonly IWorkContext _workContext;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordValidator<User> _passwordValidator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IDistributedCache _cache;
        private readonly IModuleManager _moduleManager;
        private readonly IEventFactory _eventFactory;

        public TenantsController(
            ICoreUnitOfWork uow,
            UserManager<User> userManager,
            IPasswordValidator<User> passwordValidator,
            IWorkContext workContext,
            IPasswordHasher<User> passwordHasher,
            IDistributedCache cache,
            IModuleManager moduleManager,
            IEventFactory eventFactory)
        {
            _uow = uow;
            _userManager = userManager;
            _passwordValidator = passwordValidator;
            _workContext = workContext;
            _passwordHasher = passwordHasher;
            _cache = cache;
            _moduleManager = moduleManager;
            _eventFactory = eventFactory;
        }

        [HttpGet]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Read))]
        public async Task<IActionResult> GetAsync([FromQuery] Paging paging)
        {
            var idTenant = await _workContext.GetTenantCodeAsync();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var paged = await repoTenant.PagingAsync(idTenant, paging);
            var tenantCodes = paged.Data.Select(x => x.Code).ToList();
            var infos = (await repoTenant.GetInfoByCodesAsync(tenantCodes)).ToDictionary(x => x.Code, x => x);
            var hosts = (await repoTenant.GetHostByCodesAsync(tenantCodes)).GroupBy(x => x.Code).ToDictionary(x => x.Key, x => x.ToList());

            var result = new Paged<TenantModel>
            {
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages,
            };

            var data = new List<TenantModel>();
            foreach (var item in paged.Data)
            {
                var model = new TenantModel();
                model.Code = item.Code;
                model.HostName = string.Join(';', hosts[item.Code].Select(x => x.HostName));
                model.IsEnable = item.IsEnable;
                model.Theme = item.Theme;

                if (!infos.ContainsKey(item.Code))
                {
                    data.Add(model);
                    continue;
                }

                var info = infos[item.Code];
                model.Info = new TenantInfoModel
                {
                    Id = info.Id,
                    Code = info.Code,
                    Address = info.Address,
                    City = info.City,
                    Country = info.Country,
                    District = info.District,
                    Emails = info.Emails,
                    Fax = info.Fax,
                    FullNameVi = info.FullNameVi,
                    FullNameEn = info.FullNameEn,
                    Phones = info.Phones,
                    TaxCode = info.TaxCode
                };
                data.Add(model);
            }
            result.Data = data;
            return Ok(result);
        }

        // [HttpGet("metadata/{code?}")]
        // [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Read))]
        // public async Task<IActionResult> GetMetadataAsync([FromRoute]Guid code)
        // {
        //     var model = new List<MetadataModel>();

        //     //Nếu setting ko có metadata thì trả về kết quả luôn
        //     var repoSetting = _uow.GetRepository<ISettingRepository>();
        //     var setting = await repoSetting.GetByKeyAsync(code, SettingKey.ThongTinDoanhNghiep_Khac);
        //     if (setting == null)
        //         return Ok(model);

        //     var splited1 = setting.Value.Split("|");
        //     foreach (var item in splited1)
        //     {
        //         var splited2 = item.Split(":");
        //         var key = splited2[0];
        //         var name = splited2[1];

        //         //Nếu DB ko lưu value thì trả về null
        //         model.Add(new MetadataModel
        //         {
        //             Key = key,
        //             Name = name,
        //         });
        //     }

        //     return Ok(model);
        // }

        [HttpGet("{code}")]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Read))]
        public async Task<IActionResult> GetAsync([FromRoute] Guid code)
        {
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetByCodeAsync(code);
            if (tenant == null)
                return NotFound();

            var info = await repoTenant.GetInfoByCodeAsync(code);
            if (info == null)
                return NotFound();

            var hosts = await repoTenant.GetHostByCodeAsync(code);
            if (hosts.Count == 0)
                return NotFound();

            var model = new TenantModel
            {
                Code = tenant.Code,
                HostName = string.Join(';', hosts.Select(x => x.HostName)),
                Info = new TenantInfoModel
                {
                    TaxCode = info.TaxCode,
                    FullNameVi = info.FullNameVi,
                    Address = info.Address
                }
            };

            return Ok(model);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Create))]
        public async Task<IActionResult> PostAsync([FromBody] CreateTenantCommand model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUser = _workContext.GetUserCode();
            var currentTenant = await _workContext.GetCurrentTenantAsync();

            var repoTenant = _uow.GetRepository<ITenantRepository>();
            if (await repoTenant.AnyByNameAsync(model.Info.TaxCode))
                throw new Exception("Mã số thuế đã bị trùng");

            if (await repoTenant.AnyByHostNameAsync(model.HostName))
                throw new Exception("Tên miền đã bị trùng");

            //check tên tk có bị trùng với tk root của chi nhánh không 
            //(tk này đặc biệt không được gán nhóm quyền nào mà chỉ có quyền là adminTenant)
            var userRootTenant = "root";
            if (model.User.UserName.ToUpper() == userRootTenant.ToUpper())
                throw new Exception("Tên đăng nhập không hợp lệ. Vui lòng nhập tên đăng nhập không phải là admin!");

            //Nếu ko nhập pasword sẽ tự động random
            var isRandomPassword = false;
            if (string.IsNullOrEmpty(model.User.Password))
            {
                model.User.Password = RandomExtension.Random(10);
                isRandomPassword = true;
            }

            //insert tenant, tenantInfo
            var tenantInfo = await InsertTenant(model, currentUser, currentTenant);

            //Insert user root của tenant
            var passwordRoot = RandomExtension.Random(10);
            var rootUser = await InsertRootTenantUserAsync(model, userRootTenant, passwordRoot, currentUser, tenantInfo.Code);

            //insert quyền  mặc định
            var adminRole = await AddDefaultRolesAsync(tenantInfo, currentUser);

            //Insert user của chi nhánh mà nsd nhập và gán quyền admin mặc định
            var adminUser = await InsertAdminTenantUserAsync(model, currentUser, tenantInfo.Code, adminRole.Id);

            await _uow.CommitAsync();

            _eventFactory.GetOrAddEvent<IEvent<TenantCreatedEventModel>, ITenantCreatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new TenantCreatedEventModel
                {
                    TenantCode = currentTenant.Code,
                    IdUserRoot = rootUser.Id,
                    PasswordUserRoot = passwordRoot,
                    IdUserAdmin = adminUser.Id,
                    PasswordUserAdmin = model.User.Password,
                    IsRandomPasswordAdmin = isRandomPassword
                });
            });

            return Ok(new
            {
                TenantCode = tenantInfo.Code,
            });
        }

        [HttpPut("{code}")]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Update))]
        public async Task<IActionResult> PutAsync([FromRoute] Guid code, [FromBody] UpdateTenantCommand model)
        {
            var userCode = _workContext.GetUserCode();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetByCodeAsync(code);
            if (tenant == null)
                return NotFound();

            if (tenant.IsEnable)
                throw new Exception("Chi nhánh đã tạo đăng ký phát hành không thể sửa!");

            var info = await repoTenant.GetInfoByCodeAsync(code);
            if (info == null)
                return NotFound();

            if ((await repoTenant.QueryHostByHostNameAsync(model.HostName)).Any(x => x.Code != code))
                throw new Exception("Tên miền đã bị trùng");

            var hosts = await repoTenant.GetHostByCodeAsync(code);
            if (hosts.Count == 0)
                return NotFound();

            var news = model.HostName.Split(';');

            var deletes = new List<TenantHost>();
            foreach (var item in hosts)
            {
                var delete = news.FirstOrDefault(x => x == item.HostName);
                if (delete == null)
                    deletes.Add(item);
            }

            var inserts = new List<TenantHost>();
            foreach (var item in news)
            {
                var insert = hosts.FirstOrDefault(x => x.HostName == item);
                if (insert == null)
                    inserts.Add(new TenantHost
                    {
                        Code = code,
                        HostName = item
                    });
            }

            if (deletes.Count != 0)
            {
                repoTenant.DeleteHosts(deletes);

                //xóa cache
                foreach (var item in deletes)
                {
                    _cache.Remove($":TenantHost:{item.HostName}");
                }

                //xóa token của các tk trong chi nhánh này
                await DeleteTokenAsync(code);
            }

            if (inserts.Count != 0)
                await repoTenant.InsertHostsAsync(inserts);

            repoTenant.UpdateTenantFields(tenant,
                tenant.Set(x => x.Name, model.Info.TaxCode),
                tenant.Set(x => x.UpdatedAt, DateTime.Now),
                tenant.Set(x => x.UpdatedAtUtc, DateTime.UtcNow),
                tenant.Set(x => x.UpdatedBy, userCode));

            repoTenant.UpdateInfoFields(info,
                info.Set(x => x.TaxCode, model.Info.TaxCode),
                info.Set(x => x.FullNameVi, model.Info.FullNameVi),
                info.Set(x => x.Address, model.Info.Address));

            await _uow.CommitAsync();

            _eventFactory.GetOrAddEvent<IEvent<TenantUpdatedEventModel>, ITenantUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new TenantUpdatedEventModel
                {
                    TenantCode = tenant.Code
                });
            });
            return Ok();
        }

        [HttpDelete("{code}")]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Delete))]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid code)
        {
            var userCode = _workContext.GetUserCode();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetByCodeAsync(code);
            if (tenant == null)
                return NotFound();

            if (tenant.IsEnable)
                throw new Exception("Chi nhánh đã tạo đăng ký phát hành không thể xóa!");

            var tenantHosts = await repoTenant.GetHostByCodeAsync(code);
            if (!tenantHosts.Any())
                return NotFound();

            repoTenant.UpdateTenantFields(tenant,
                tenant.Set(x => x.DeletedVersion, tenant.Id),
                tenant.Set(x => x.DeletedAt, DateTime.Now),
                tenant.Set(x => x.DeletedAtUtc, DateTime.UtcNow),
                tenant.Set(x => x.DeletedBy, userCode));

            foreach (var tenantHost in tenantHosts)
            {
                repoTenant.UpdateTenantHostFields(tenantHost,
                    tenantHost.Set(x => x.DeletedVersion, tenantHost.Id));

                //xóa cache
                _cache.Remove($":TenantHost:{tenantHost.HostName}");
            }

            //xóa token của các tk trong chi nhánh này
            await DeleteTokenAsync(code);

            await _uow.CommitAsync();

            _eventFactory.GetOrAddEvent<IEvent<TenantDeletedEventModel>, ITenantDeletedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new TenantDeletedEventModel
                {
                    TenantCode = tenant.Code
                });
            });
            return Ok();
        }

        /// <summary>
        /// xóa token của các tài khoản trong 1 công ty/chi nhánh
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private async Task DeleteTokenAsync(Guid code)
        {
            var repoTokenInfo = _uow.GetRepository<ITokenInfoRepository>();
            var tokens = await repoTokenInfo.QueryByTenantAsync(code);

            foreach (var token in tokens)
            {
                repoTokenInfo.Delete(token);

                //xóa token trong cache
                await _cache.RemoveAsync($":TokenInfos:{token.Code}");
            }
        }

        /// <summary>
        /// thêm quyền mặc định
        /// </summary>
        /// <param name="tenantInfo"></param>
        /// <param name="idUser"></param>
        /// <returns>quyền admin</returns>
        private async Task<Role> AddDefaultRolesAsync(TenantInfo tenantInfo, Guid idUser)
        {
            //lấy tất các các quyền
            var allClaims = GetAllClaims();

            //quyền admin
            //quyền chỉ tạo/sửa và thao tác với hóa đơn 
            var adminPermission = new Dictionary<string, string>();
            var accountantPermission = new Dictionary<string, string>();

            foreach (var item in allClaims)
            {
                adminPermission.Add(item, $"{ClaimOfResource.Tenant}|{ClaimOfChildResource.Tenant}");

                if (item.Contains("Invoice") || item.Contains("BC26") || item.Contains("Dashboard"))
                    accountantPermission.Add(item, $"{default(ClaimOfResource)}|{ClaimOfChildResource.Owner.ToString()}");
                //else
                //    accountantPermission.Add(item, $"{default(ClaimOfResource)}|{default(ClaimOfChildResource)}");
            }

            //thêm quyền
            var adminRole = new Role
            {
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = idUser,
                Id = Guid.NewGuid(),
                TenantCode = tenantInfo.Code,
                Name = $"ADMIN_{tenantInfo.TaxCode}",
                NormalizedName = $"ADMIN_{tenantInfo.TaxCode}"
            };
            var repoRole = _uow.GetRepository<IRoleRepository>();
            await repoRole.InsertAsync(adminRole);

            var accountantRole = new Role
            {
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = idUser,
                Id = Guid.NewGuid(),
                TenantCode = tenantInfo.Code,
                Name = $"Kế toán",
                NormalizedName = $"KẾ TOÁN"
            };
            await repoRole.InsertAsync(accountantRole);

            //thêm roleclaim
            var repoRoleClaim = _uow.GetRepository<IPermissionRepository>();

            await repoRoleClaim.InsertRoleClaimsAsync(adminPermission.Select(x => new IdentityRoleClaim<Guid>
            {
                ClaimType = x.Key,
                ClaimValue = x.Value,
                RoleId = adminRole.Id
            }).ToList());

            await repoRoleClaim.InsertRoleClaimsAsync(accountantPermission.Select(x => new IdentityRoleClaim<Guid>
            {
                ClaimType = x.Key,
                ClaimValue = x.Value,
                RoleId = accountantRole.Id
            }).ToList());

            return adminRole;
        }

        /// <summary>
        /// lấy tất cả các quyền
        /// </summary>
        /// <returns></returns>
        private List<string> GetAllClaims()
        {
            var allClaims = new List<string>();

            var crudPolicies = _moduleManager.GetInstances<IAuthorizationCrudPolicy>();
            foreach (var policy in crudPolicies)
            {
                //Các quyền CRUD ko sử dụng abstract
                allClaims.Add($"{policy.Name}_Read");
                allClaims.Add($"{policy.Name}_Create");
                allClaims.Add($"{policy.Name}_Update");
                allClaims.Add($"{policy.Name}_Delete");
            }

            var policies = _moduleManager.GetInstances<IAuthorizationPolicy>();
            foreach (var policy in policies)
            {
                allClaims.Add(policy.Name);
            }

            return allClaims.Distinct().ToList();
        }


        /// <summary>
        /// thêm tk admin của chi nhánh và gán với 1 nhóm quyền mặc định
        /// </summary>
        /// <param name="model"></param>
        /// <param name="idCurrentUser"></param>
        /// <param name="tenantCode"></param>
        /// <param name="idRole"></param>
        /// <returns></returns>
        private async Task<User> InsertAdminTenantUserAsync(CreateTenantCommand model, Guid idCurrentUser, Guid tenantCode, Guid idRole)
        {
            var repoUser = _uow.GetRepository<IUserRepository>();

            //tk admin 
            var user = new User
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = idCurrentUser,
                UserName = model.User.UserName,
                NormalizedUserName = model.User.UserName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString().ToUpper().Replace("-", ""),
                LockoutEnabled = true,
                TenantCode = tenantCode,
                Email = model.User.Email,
                NormalizedEmail = string.IsNullOrEmpty(model.User.Email) ? null : model.User.Email.ToUpper()
            };
            var validateResult = await _passwordValidator.ValidateAsync(_userManager, user, model.User.Password);
            if (!validateResult.Succeeded)
                throw new Exception(validateResult.ToMessage());

            //Insert user admin
            var passwordHashed = _passwordHasher.HashPassword(user, model.User.Password);
            user.PasswordHash = passwordHashed;

            await repoUser.InsertUserAsync(user);
            await repoUser.InsertUserInfoAsync(new UserInfo
            {
                Id = user.Id,
                FullName = model.User.FullName
            });

            //gán tk admin đc tạo có quyền admin
            var repoPermission = _uow.GetRepository<IPermissionRepository>();
            await repoPermission.InsertUserRolesAsync(
              new List<IdentityUserRole<Guid>> {
                    new IdentityUserRole<Guid>
                    {
                        UserId = user.Id,
                        RoleId = idRole
                    }
              });

            return user;
        }

        /// <summary>
        /// insert tk root của chi nhánh
        /// gán quyền tenant_Admin
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="idCurrentUser"></param>
        /// <param name="tenantCode"></param>
        private async Task<User> InsertRootTenantUserAsync(CreateTenantCommand model, string userName, string password, Guid idCurrentUser, Guid tenantCode)
        {
            //tk root của chi nhánh
            var rootUser = new User
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = idCurrentUser,
                UserName = userName,
                NormalizedUserName = userName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString().ToUpper().Replace("-", ""),
                LockoutEnabled = true,
                TenantCode = tenantCode,
                Email = model.User.Email,
                NormalizedEmail = string.IsNullOrEmpty(model.User.Email) ? null : model.User.Email.ToUpper()
            };
            var validateResultAdmin = await _passwordValidator.ValidateAsync(_userManager, rootUser, password);
            if (!validateResultAdmin.Succeeded)
                throw new Exception(validateResultAdmin.ToMessage());

            var passwordRootHashed = _passwordHasher.HashPassword(rootUser, password);
            rootUser.PasswordHash = passwordRootHashed;

            var repoUser = _uow.GetRepository<IUserRepository>();
            await repoUser.InsertUserAsync(rootUser);
            await repoUser.InsertUserInfoAsync(new UserInfo
            {
                Id = rootUser.Id,
                FullName = model.User.FullName
            });

            //Authorization
            var repoPermission = _uow.GetRepository<IPermissionRepository>();
            await repoPermission.InsertUserClaimsAsync(
                new List<IdentityUserClaim<Guid>> {
                    new IdentityUserClaim<Guid>
                    {
                        UserId = rootUser.Id,
                        ClaimType = nameof(SpecialPolicy.Special_TenantAdmin),
                        ClaimValue = $"{ClaimOfResource.Tenant}|{ClaimOfChildResource.Tenant}"
                    }
                }
            );

            return rootUser;
        }

        /// <summary>
        /// thêm mới tenant, tenantInfo
        /// </summary>
        /// <param name="model"></param>
        /// <param name="idCurrentUser"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private async Task<TenantInfo> InsertTenant(CreateTenantCommand model, Guid idCurrentUser, Tenant parent)
        {
            //Insert tenant
            var code = Guid.NewGuid();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = new Tenant
            {
                Code = code,
                Name = model.Info.TaxCode,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = idCurrentUser,
                IdParent = parent.Code,
                Path = $"{parent.Path}|{code}",
                IsEnable = false
            };
            await repoTenant.InsertAsync(tenant);

            var tenantInfo = new TenantInfo
            {
                Code = code,
                IsCurrent = true,
                TaxCode = model.Info.TaxCode,
                Address = model.Info.Address,
                FullNameVi = model.Info.FullNameVi,
                City = "#",
                Country = "#",
                District = "#",
                // FullNameEn = model.FullNameEn,
                // LegalName = model.LegalName,
                // Fax = model.Fax,
                // BusinessType = model.BusinessType,
                // Emails = model.Emails,
                // Phones = model.Phones,
                // Metadata = model.Metadata.Count == 0 ? null : JsonConvert.SerializeObject(model.Metadata)
            };
            await repoTenant.InsertInfoAsync(tenantInfo);

            var splited = model.HostName.Split(';');
            foreach (var item in splited)
            {
                await repoTenant.InsertHostAsync(new TenantHost
                {
                    Code = code,
                    HostName = item
                });
            }

            return tenantInfo;
        }

    }
}

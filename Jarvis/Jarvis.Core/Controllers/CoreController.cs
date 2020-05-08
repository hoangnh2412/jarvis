using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Models;
using Jarvis.Core.Permissions;
using Jarvis.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Repositories;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("core")]
    [ApiController]
    public class CoreController : ControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly INavigationService _navigationService;
        private readonly ICoreUnitOfWork _uow;

        public CoreController(
            IWorkContext workContext,
            INavigationService navigationService,
            ICoreUnitOfWork uow)
        {
            _workContext = workContext;
            _navigationService = navigationService;
            _uow = uow;
        }

        /// <summary>
        /// Lấy navigation
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("navigation")]
        public async Task<IActionResult> GetNavigationAsync()
        {
            var session = await _workContext.GetSessionAsync();

            var navigation = _navigationService.GetNavigation(session);
            return Ok(navigation);
        }

        [Authorize]
        [HttpGet("tenants")]
        public async Task<IActionResult> GetTenantAsync()
        {
            //Cây phân cấp
            //         A
            //       /   \
            //     B       C
            //   /   \   /   \
            // E      F G      H
            // -----------------
            //Dữ liệu lưu trên DB
            //A = A
            //B = A|B
            //C = A|C
            //E = A|B|E
            //F = A|B|F
            //G = A|C|G
            //H = A|C|H
            // ------------------
            //Hiển thị ra UI
            //A
            //_.B
            //_._.E
            //_._.F
            //_.C
            //_._.G
            //_._.H

            var tenants = new List<HierarchyTenantModel>();
            var session = await _workContext.GetSessionAsync();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var hierarchy = (await repoTenant.GetHierarchyByCodeAsync(session.TenantInfo.Code)).Select(x => new HierarchyTenantModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Path = x.Path,
                Codes = x.Path.Split('|').ToList(),
                Level = x.Path.Split('|').Length,
                IdParent = x.IdParent,
                CreatedAt = x.CreatedAt
            }).ToList();

            if (hierarchy.Count == 0)
                return Ok(tenants);

            //Nếu TenantCode là chi nhánh => Min Level != 1 => Cần giảm tất cả level cho xuất hiện Level 1 để hiển thị TenantCode hiện tại là root
            var distance = 0;
            var min = hierarchy.Min(x => x.Level);
            if (min != 1)
                distance = min - 1;

            //Sắp xếp lại
            var max = hierarchy.Max(x => x.Level);

            for (int i = min; i <= max; i++)
            {
                var items = hierarchy.Where(x => x.Level == i).OrderByDescending(x => x.CreatedAt).ToList();
                foreach (var item in items)
                {
                    var index = tenants.FindIndex(x => x.Code == item.IdParent);
                    tenants.Insert(index + 1, new HierarchyTenantModel
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.Name,
                        Path = item.Path,
                        Codes = item.Codes,
                        Level = item.Level - distance,
                        IdParent = item.IdParent,
                        CreatedAt = item.CreatedAt
                    });
                }
            }

            ////Đổi tên thành _.
            //var infos = (await repoTenant.GetInfoByCodesAsync(hierarchy.Select(x => x.Code).ToList())).ToDictionary(x => x.Code, x => x);

            //foreach (var item in tenants)
            //{
            //    if (!infos.ContainsKey(item.Code))
            //        continue;

            //    var info = infos[item.Code];

            //    var level = new string[item.Level];
            //    for (int j = 0; j < item.Level - 1; j++)
            //    {
            //        level[j] = "_";
            //    }
            //    level[item.Level - 1] = $"{item.Name}: {info.FullNameVi}";
            //    item.FullName = info.FullNameVi;
            //    item.HierarchyName = string.Join('.', level);
            //}



            //Đổi tên thành _.
            var infos = (await repoTenant.GetInfoByCodesAsync(hierarchy.Select(x => x.Code).ToList())).ToDictionary(x => x.Code, x => x);

            //lưu lại item cuối cùng có level = 2
            var lastSecond = 0;

            for (int i = 0; i < tenants.Count; i++)
            {
                var item = tenants[i];
                if (!infos.ContainsKey(item.Code))
                    continue;

                var info = infos[item.Code];

                var level = new string[item.Level];

                for (int j = 0; j < item.Level - 1; j++)
                {
                    level[j] = "#####";
                }

                if (item.Level >= 2)
                    level[item.Level - 2] = "|___";

                if (item.Level == 2)
                    lastSecond = i;

                //tìm item có level cùng cấp mình gần nhất phía trước
                //nếu chưa phải  item có level cùng cấp gần nhất => cộng vào đầu item có level bé hơn 1 ký | tự để phân biệt
                for (int j = i - 1; j > 0; j--)
                {
                    if (tenants[j].Level <= item.Level)
                        break;

                    if (tenants[j].Level >= 2)
                        tenants[j].HierarchyNames[item.Level - 2] = "|&nbsp; &nbsp; &nbsp; &nbsp;";
                }

                level[item.Level - 1] = $"{item.Name}: {info.FullNameVi}";
                item.HierarchyNames = level;
                item.FullName = info.FullNameVi;
                item.HierarchyName = string.Join("#", level);
            }

            //ghi lại 
            foreach (var item in tenants)
            {
                item.HierarchyName = string.Join("", item.HierarchyNames).Replace("#", "&nbsp;");
            }

            var result = tenants.Select(x => new
            {
                Code = x.Code,
                Name = x.Name,
                FullName = x.FullName,
                HierarchyName = x.HierarchyName
            }).ToList();
            return Ok(result);
        }
    }
}

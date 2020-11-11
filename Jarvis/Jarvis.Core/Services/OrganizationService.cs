using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Database.Models;
using Infrastructure.Extensions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Models.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Jarvis.Core.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ICoreUnitOfWork _uowCore;

        public OrganizationService(
            ICoreUnitOfWork uowCore)
        {
            _uowCore = uowCore;
        }

        public async Task<bool> CreateUserAsync(CreateOrganizationUserRequestModel request)
        {
            var repoOrganizationUser = _uowCore.GetRepository<IOrganizationUserRepository>();
            if (await repoOrganizationUser.AnyAsync(x => x.OrganizationCode == request.OrganizationUnitCode && x.IdUser == request.OrganizationUserCode))
                return false;

            await repoOrganizationUser.InsertAsync(new OrganizationUser
            {
                IdUser = request.OrganizationUserCode,
                Level = request.Level,
                OrganizationCode = request.OrganizationUnitCode
            });
            await _uowCore.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(DeleteOrganizationUserRequestModel request)
        {
            var repoOrganizationUser = _uowCore.GetRepository<IOrganizationUserRepository>();
            var organizationUser = await repoOrganizationUser.GetQuery().FirstOrDefaultAsync(x => x.OrganizationCode == request.OrganizationUnitCode && x.IdUser == request.OrganizationUserCode);
            if (organizationUser == null)
                return false;

            repoOrganizationUser.Delete(organizationUser);
            await _uowCore.CommitAsync();
            return true;
        }

        public async Task<List<GetOrganizationUnitResponseModel>> GetAllAsync(Guid tenantCode)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var items = await repoOrganizationUnit.GetAllAsync(tenantCode);
            return items.Select(x => new GetOrganizationUnitResponseModel
            {
                Code = x.Code,
                Description = x.Description,
                FullName = x.FullName,
                IdParent = x.IdParent,
                Name = x.Name,
                TenantCode = x.TenantCode
            }).ToList();
        }

        public async Task<GetOrganizationUnitResponseModel> GetUnitByCodeAsync(Guid code)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var organizationUnit = await repoOrganizationUnit.GetQuery().FirstOrDefaultAsync(x => x.Code == code);
            return new GetOrganizationUnitResponseModel
            {
                Code = organizationUnit.Code,
                Description = organizationUnit.Description,
                FullName = organizationUnit.FullName,
                IdParent = organizationUnit.IdParent,
                Name = organizationUnit.Name,
                TenantCode = organizationUnit.TenantCode
            };
        }

        public async Task<Paged<OrganizationUserInfoModel>> GetUsersInUnitAsync(Guid tenantCode, Guid code, Paging paging)
        {
            var repoOrganizationUser = _uowCore.GetRepository<IOrganizationUserRepository>();
            var paged = await repoOrganizationUser.PagingAsync(code, paging);

            var userCodes = paged.Data.Select(x => x.IdUser).ToList();
            var repoUser = _uowCore.GetRepository<IUserRepository>();
            var infos = (await repoUser.FindUserInfoByIdsAsync(userCodes)).ToDictionary(x => x.Id, x => x);
            var usernames = (await repoUser.FindUserByIdsAsync(userCodes)).ToDictionary(x => x.Id, x => x.UserName);

            var users = new List<OrganizationUserInfoModel>();
            foreach (var item in paged.Data)
            {
                UserInfo info = null;
                if (infos.ContainsKey(item.IdUser))
                    info = infos[item.IdUser];

                users.Add(new OrganizationUserInfoModel
                {
                    Avatar = info == null ? null : info.AvatarPath,
                    Code = item.IdUser,
                    FullName = info == null ? null : info.FullName,
                    UserName = usernames.ContainsKey(item.IdUser) ? usernames[item.IdUser] : null
                });
            }

            return new Paged<OrganizationUserInfoModel>
            {
                Data = users,
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
        }

        public async Task<Paged<OrganizationUserInfoModel>> GetUsersNotInUnitAsync(Guid tenantcode, Guid code, Paging paging)
        {
            //Lấy danh sách user trong unit
            var repoOrganizationUser = _uowCore.GetRepository<IOrganizationUserRepository>();
            var userCodes = await repoOrganizationUser.GetQuery().Where(x => x.OrganizationCode == code).Select(x => x.IdUser).AsQueryable().ToListAsync();

            //Lấy thông in user nhưng loại bỏ các user đã có trong unit và phân trang
            var repoUser = _uowCore.GetRepository<IUserRepository>();
            var paged = await repoUser.PagingWithoutSomeUsersAsync(tenantcode, paging, userCodes);
            userCodes = paged.Data.Select(x => x.Id).ToList();

            var repoUserInfo = _uowCore.GetRepository<IUserRepository>();
            var infos = (await repoUserInfo.FindUserInfoByIdsAsync(userCodes)).ToDictionary(x => x.Id, x => x);

            var users = new List<OrganizationUserInfoModel>();
            foreach (var data in paged.Data)
            {
                UserInfo info = null;
                if (infos.ContainsKey(data.Id))
                    info = infos[data.Id];

                users.Add(new OrganizationUserInfoModel
                {
                    Code = data.Id,
                    UserName = data.UserName,
                    FullName = info == null ? info.FullName : null,
                    Avatar = info == null ? info.AvatarPath : null
                });
            }

            return new Paged<OrganizationUserInfoModel>
            {
                Data = users,
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
        }

        public async Task<Paged<PagingOrganizationResponseModel>> PaginationAsync(Guid tenantCode, Guid userCode, PagingOrganzationRequestModel paging)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var paged = await repoOrganizationUnit.PagingAsync(tenantCode, userCode, paging);

            return new Paged<PagingOrganizationResponseModel>
            {
                Data = paged.Data.Select(x => new PagingOrganizationResponseModel
                {
                    Code = x.Code,
                    Description = x.Description,
                    FullName = x.FullName,
                    IdParent = x.IdParent,
                    Name = x.Name,
                    TenantCode = x.TenantCode
                }),
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
        }

        public async Task<bool> UpdateUnitAsync(Guid tenantCode, Guid userCode, Guid code, UpdateOrganizationUnitRequestModel request)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var organizationUnit = await repoOrganizationUnit.GetQuery().FirstOrDefaultAsync(x => x.Code == code);
            if (organizationUnit == null)
                return false;

            repoOrganizationUnit.UpdateFields(organizationUnit,
                organizationUnit.Set(x => x.FullName, request.FullName),
                organizationUnit.Set(x => x.Name, request.Name),
                organizationUnit.Set(x => x.UpdatedAt, DateTime.Now),
                organizationUnit.Set(x => x.UpdatedAtUtc, DateTime.UtcNow),
                organizationUnit.Set(x => x.UpdatedBy, userCode),
                organizationUnit.Set(x => x.IdParent, request.IdParent),
                organizationUnit.Set(x => x.Description, request.Description)
            );

            await _uowCore.CommitAsync();
            return true;
        }

        public async Task<List<GetTreeNodeResponseModel>> GetTreeAsync(Guid tenantCode)
        {
            var tree = new List<GetTreeNodeResponseModel>();
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var entities = await repoOrganizationUnit.GetAllAsync(tenantCode);
            if (entities.Count == 0)
                return tree;

            var units = entities.Select(x => new OrganizationUnit
            {
                Code = x.Code,
                Description = x.Description,
                FullName = x.FullName,
                IdParent = x.IdParent.HasValue ? x.IdParent.Value : Guid.Empty,
                Name = x.Name,
                TenantCode = x.TenantCode,
                CreatedAt = x.CreatedAt,
                CreatedAtUtc = x.CreatedAtUtc,
                CreatedBy = x.CreatedBy,
                DeletedAt = x.DeletedAt,
                DeletedAtUtc = x.DeletedAtUtc,
                DeletedBy = x.DeletedBy,
                DeletedVersion = x.DeletedVersion,
                Left = x.Left,
                Right = x.Right,
                UpdatedAt = x.UpdatedAt,
                UpdatedAtUtc = x.UpdatedAtUtc,
                UpdatedBy = x.UpdatedBy
            }).OrderBy(x => x.Left).ToList();

            var group = units.GroupBy(x => x.IdParent);
            var index = group.ToDictionary(x => x.Key, x => x.ToList());
            var roots = index[Guid.Empty];
            foreach (var item in roots)
            {
                GetTreeNodeResponseModel node = BuildNode(index, item);
                tree.Add(node);
            }
            return tree;
        }

        public async Task<bool> InsertRootAsync(Guid tenantCode, Guid userCode, Guid code, CreateOrganizationUnitRequestModel request)
        {
            using (var transaction = await _uowCore.BeginTransactionAsync<IDbContextTransaction>())
            {
                var repoUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
                if (await repoUnit.AnyAsync(x => x.Name == request.Name && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue))
                    return false;

                var node = await repoUnit.GetQuery().Where(x => x.TenantCode == tenantCode && !x.DeletedVersion.HasValue).OrderByDescending(x => x.Right).AsQueryable().FirstOrDefaultAsync();

                var left = 1;
                var right = 2;
                if (node != null)
                {
                    left = node.Right + 1;
                    right = node.Right + 2;
                }

                await repoUnit.InsertAsync(new OrganizationUnit
                {
                    Code = code,
                    CreatedAt = DateTime.Now,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = userCode,
                    DeletedAt = null,
                    DeletedAtUtc = null,
                    DeletedBy = null,
                    DeletedVersion = null,
                    Description = request.Description,
                    FullName = request.FullName,
                    IdParent = null,
                    Left = left,
                    Name = request.Name,
                    Right = right,
                    TenantCode = tenantCode,
                    UpdatedAt = null,
                    UpdatedAtUtc = null,
                    UpdatedBy = null
                });
                await _uowCore.CommitAsync();
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
            }

            return true;
        }

        public async Task<bool> InsertNodeAsync(Guid tenantCode, Guid userCode, Guid code, CreateOrganizationUnitRequestModel request)
        {
            using (var transaction = await _uowCore.BeginTransactionAsync<IDbContextTransaction>())
            {
                var repoUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
                if (await repoUnit.AnyAsync(x => x.Name == request.Name && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue))
                    return false;

                //Lấy node cha
                var node = await repoUnit.GetByCodeAsync(tenantCode, request.IdParent.Value);
                if (node == null)
                    return false;

                //Tính số space cần dịch chuyển. 1 node cần 2 space để lưu trữ
                var space = 2;

                //Tạo 2 biến này để tránh rights và lefts phải update node cha
                var right = node.Right;
                var left = node.Left;

                // Tăng khoảng trống RIGHT của các node bên phải
                var rights = await repoUnit.GetQuery().Where(x => x.Right >= right && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue).ToListAsync();
                foreach (var item in rights)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Right, item.Right + space)
                    );
                }

                // Tăng khoảng trống LEFT của các node bên phải
                var lefts = await repoUnit.GetQuery().Where(x => x.Left >= right && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue).ToListAsync();
                foreach (var item in lefts)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Left, item.Left + space)
                    );
                }
                await _uowCore.CommitAsync();

                //Chèn node với LEFT = parent RIGHT - 2 và RIGHT = parent RIGHT - 1
                await repoUnit.InsertAsync(new OrganizationUnit
                {
                    Code = code,
                    CreatedAt = DateTime.Now,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = userCode,
                    DeletedAt = null,
                    DeletedAtUtc = null,
                    DeletedBy = null,
                    DeletedVersion = null,
                    IdParent = request.IdParent,
                    Description = request.Description,
                    FullName = request.FullName,
                    Name = request.Name,
                    Left = right,
                    Right = right + 1,
                    TenantCode = tenantCode,
                    UpdatedAt = null,
                    UpdatedAtUtc = null,
                    UpdatedBy = null
                });

                await _uowCore.CommitAsync();
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
            }


            return true;
        }

        public async Task<bool> MoveNodeAsync(Guid tenantCode, Guid userCode, Guid sourceCode, MoveNodeRequestModel request)
        {
            using (var transaction = await _uowCore.BeginTransactionAsync<IDbContextTransaction>())
            {
                var repoUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();

                //Tìm node cần di chuyển
                var node = await repoUnit.GetByCodeAsync(tenantCode, sourceCode);
                if (node == null)
                    return false;

                //Tìm các node con của node cần di chuyển
                var children = await repoUnit.GetChildrenAsync(tenantCode, node);

                //Tính space cần di chuyển, mỗi node ứng với 2 space
                var nodes = new List<OrganizationUnit>();
                nodes.Add(node);
                nodes.AddRange(children);
                var codes = nodes.Select(x => x.Code).ToList();

                var space = nodes.Count * 2;

                //Tìm các RIGHT của node bên phải và dồn về bên trái
                var rightForRemoves = await repoUnit.GetQuery()
                    .Where(x => x.Right > node.Right && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue)
                    .ToListAsync();
                foreach (var item in rightForRemoves)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Right, item.Right - space)
                    );
                }

                //Tìm các LEFT của node bên phải và dồn về bên trái
                var leftForRemoves = await repoUnit.GetQuery()
                    .Where(x => x.Left > node.Right && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue)
                    .ToListAsync();
                foreach (var item in leftForRemoves)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Left, item.Left - space)
                    );
                }
                await _uowCore.CommitAsync();

                //Tìm vị trí cần chèn
                int position = 0;

                if (request.LeftNode.HasValue)
                {
                    //Tìm LEFT node
                    var leftNode = await repoUnit.GetByCodeAsync(tenantCode, request.LeftNode.Value);
                    position = leftNode.Right;
                }
                else
                {
                    if (request.ParentNode.HasValue)
                    {
                        //Tìm PARENT node
                        var parentNode = await repoUnit.GetByCodeAsync(tenantCode, request.ParentNode.Value);
                        position = parentNode.Left;
                    }
                }

                //Tìm các RIGHT của node bên phải và dồn về bên phải
                var rightForInserts = await repoUnit.GetQuery()
                    .Where(x => x.Right > position && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue)
                    .ToListAsync();

                //Bỏ qua không dồn các node cần dịch chuyển
                rightForInserts = rightForInserts.Where(x => !codes.Contains(x.Code)).ToList();
                foreach (var item in rightForInserts)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Right, item.Right + space)
                    );
                }

                //Tìm các LEFT của node bên phải và dồn về bên phải
                var leftForInserts = await repoUnit.GetQuery()
                    .Where(x => x.Left > position && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue)
                    .ToListAsync();

                //Bỏ qua không dồn các node cần dịch chuyển
                leftForInserts = leftForInserts.Where(x => !codes.Contains(x.Code)).ToList();
                foreach (var item in leftForInserts)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Left, item.Left + space)
                    );
                }
                await _uowCore.CommitAsync();

                //Chèn nodes
                var move = node.Left - position - 1;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var item = nodes[i];
                    if (i == 0) //Nếu là node gốc cần chuyển thì update lại IdParent
                    {
                        repoUnit.UpdateFields(item,
                            item.Set(x => x.Right, item.Right - move),
                            item.Set(x => x.Left, item.Left - move),
                            item.Set(x => x.IdParent, request.ParentNode),
                            item.Set(x => x.DeletedAt, null),
                            item.Set(x => x.DeletedAtUtc, null),
                            item.Set(x => x.DeletedBy, null),
                            item.Set(x => x.DeletedVersion, null),
                            item.Set(x => x.UpdatedAt, DateTime.Now),
                            item.Set(x => x.UpdatedAtUtc, DateTime.UtcNow),
                            item.Set(x => x.UpdatedBy, userCode)
                        );
                    }
                    else
                    {
                        repoUnit.UpdateFields(item,
                            item.Set(x => x.Right, item.Right - move),
                            item.Set(x => x.Left, item.Left - move),
                            item.Set(x => x.DeletedAt, null),
                            item.Set(x => x.DeletedAtUtc, null),
                            item.Set(x => x.DeletedBy, null),
                            item.Set(x => x.DeletedVersion, null),
                            item.Set(x => x.UpdatedAt, DateTime.Now),
                            item.Set(x => x.UpdatedAtUtc, DateTime.UtcNow),
                            item.Set(x => x.UpdatedBy, userCode)
                        );
                    }
                }

                await _uowCore.CommitAsync();
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
            }
            return true;
        }

        public async Task<bool> RemoveNodeAsync(Guid tenantCode, Guid userCode, Guid code)
        {
            using (var transaction = await _uowCore.BeginTransactionAsync<IDbContextTransaction>())
            {
                var repoUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();

                //Tìm node cần xoá
                var node = await repoUnit.GetQuery().FirstOrDefaultAsync(x => x.Code == code && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue);
                if (node == null)
                    return false;

                //Tìm các node con của node cần xoá
                var children = await repoUnit.GetQuery()
                    .Where(x => x.Left > node.Left && x.Right < node.Right && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue)
                    .ToListAsync();

                var nodes = new List<OrganizationUnit>();
                nodes.Add(node);
                nodes.AddRange(children);

                //Xoá node
                foreach (var item in nodes)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.DeletedAt, DateTime.Now),
                        item.Set(x => x.DeletedAtUtc, DateTime.UtcNow),
                        item.Set(x => x.DeletedBy, userCode),
                        item.Set(x => x.DeletedVersion, item.Id)
                    );
                }
                await _uowCore.CommitAsync();

                //Tính số space cần dịch chuyển. 1 node cần 2 space để lưu trữ
                var space = nodes.Count * 2;

                //Tìm các node RIGHT
                var rights = await repoUnit.GetQuery()
                    .Where(x => x.Right > node.Right && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue)
                    .ToListAsync();
                foreach (var item in rights)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Right, item.Right - space)
                    );
                }

                //Tìm các node LEFT
                var lefts = await repoUnit.GetQuery()
                    .Where(x => x.Left > node.Right && x.TenantCode == tenantCode && !x.DeletedVersion.HasValue)
                    .ToListAsync();
                foreach (var item in lefts)
                {
                    repoUnit.UpdateFields(item,
                        item.Set(x => x.Left, item.Left - space)
                    );
                }
                await _uowCore.CommitAsync();
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
            }
            return true;
        }

        private static GetTreeNodeResponseModel BuildNode(Dictionary<Guid?, List<OrganizationUnit>> items, GetTreeNodeResponseModel node)
        {
            if (items.ContainsKey(node.Code))
            {
                node.Nodes = items[node.Code].OrderBy(x => x.Left).Select(x => (GetTreeNodeResponseModel)x).ToList();
                foreach (var item in node.Nodes)
                {
                    BuildNode(items, item);
                }
            }
            else
            {
                node.Nodes = new List<GetTreeNodeResponseModel>();
            }
            return node;
        }

    }
}
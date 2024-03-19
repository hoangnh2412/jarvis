using Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Jarvis.Core.Constants;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Extensions;
using Jarvis.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Core.Database.Repositories;
using Jarvis.Models.Identity.Models.Identity;
using Jarvis.Core.Models.Identity;
using Infrastructure.Extensions;
using Jarvis.Core.Abstractions;
using Infrastructure.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Services
{
    public interface IIdentityService
    {
        Task<TokenModel> LoginAsync(Guid tenantKey, LoginModel model);

        Task LogoutAsync();

        Task<TokenModel> RefreshTokenAsync(string refreshToken);

        Task<Guid> RegisterAsync(Guid tenantKey, RegisterModel model, UserType type);

        Task<Guid> CreateAsync(Guid tenantKey, CreateUserModel model);

        Task DeleteAsync(Guid userKey);

        Task ChangePasswordAsync(Guid tenantKey, Guid userKey, ChangePasswordModel model);

        Task LockAsync(Guid tenantKey, Guid userKey, string time);

        Task UnlockAsync(Guid tenantKey, Guid userKey, string time);

        /// <summary>
        /// đổi mật khẩu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Guid> ForgotPasswordAsync(Guid tenantKey, ForgotPasswordModel model);

        /// <summary>
        /// đổi mật khâu khi chọn quên mật khẩu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Guid> ResetForgotPasswordAsync(Guid tenantKey, ResetForgotPasswordModel model);

        /// <summary>
        /// Đặt lại mật khẩu tự động
        /// </summary>
        /// <param name="tenantKey"></param>
        /// <param name="userKey"></param>
        /// <param name="emails"></param>
        /// <returns></returns>
        Task ResetPasswordAsync(Guid tenantKey, Guid userKey, string password, string emails);

        /// <summary>
        /// Lấy token theo user
        /// </summary>
        /// <param name="userKey"></param>
        /// <returns></returns>
        Task<TokenInfo> GetTokenAsync(Guid userKey);

        /// <summary>
        /// Tạo token theo user
        /// </summary>
        /// <param name="tenantKey"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<TokenInfo> GenerateTokenAsync(Guid tenantKey, User user);

        /// <summary>
        /// Tạo token
        /// </summary>
        /// <param name="issuedAt"></param>
        /// <param name="expiredAt"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        string GenerateAccessToken(DateTime issuedAt, DateTime expiredAt, Dictionary<string, object> claims);
    }

    public class IdentityService : IIdentityService
    {
        private IdentityOption _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ICoreUnitOfWork _uow;
        private readonly IEnumerable<IUserInfoService> _userInfoServices;
        private readonly IDistributedCache _cache;
        private readonly IWorkContext _workContext;

        public IdentityService(
            IOptions<IdentityOption> options,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IPasswordHasher<User> passwordHasher,
            ICoreUnitOfWork uow,
            IEnumerable<IUserInfoService> userInfoServices,
            IDistributedCache cache,
            IWorkContext workContext)
        {
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordHasher = passwordHasher;
            _uow = uow;
            _userInfoServices = userInfoServices;
            _cache = cache;
            _workContext = workContext;
        }

        public async Task<TokenModel> LoginAsync(Guid tenantKey, LoginModel model)
        {
            model.UserName = model.UserName.ToUpper();

            var repoUser = _uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByUsernameAsync(tenantKey, model.UserName);
            if (user == null)
                throw new Exception("Tài khoản hoặc mật khẩu không đúng");

            var account = await _userManager.FindByIdAsync(user.Id.ToString());
            var result = await _signInManager.CheckPasswordSignInAsync(account, model.Password, true);
            if (result.IsLockedOut)
                throw new Exception("Tài khoản bị khóa");

            if (!result.Succeeded)
                throw new Exception("Tài khoản hoặc mật khẩu không đúng");

            var token = await GenerateTokenAsync(tenantKey, user);

            return new TokenModel
            {
                AccessToken = token.AccessToken,
                // ExpireIn = (token.ExpireAtUtc - DateTime.UtcNow).TotalMinutes,
                ExpireAt = token.ExpireAtUtc,
                Timezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalHours,
                RefreshToken = token.RefreshToken
            };
        }

        public async Task<TokenInfo> GenerateTokenAsync(Guid tenantKey, User user)
        {
            //Xóa các Token đã hết hạn
            await RemoveTokenExpiredAsync(user.Key);

            var token = await GetTokenAsync(user.Key);
            if (token != null)
                return token;

            var userInfo = await GetInfoAsync(user.Key);

            var tokenKey = Guid.NewGuid();
            var expireIn = TimeSpan.FromMinutes(_options.ExpireTime);
            var createAt = DateTime.Now;
            var createAtUtc = DateTime.UtcNow;
            var expireAt = createAt.Add(expireIn);
            var expireAtUtc = createAtUtc.Add(expireIn);
            var claims = new Dictionary<string, object>();
            claims.Add(JwtRegisteredClaimNames.Jti, tokenKey.ToString());
            claims.Add(ClaimTypes.Sid, user.Key.ToString());
            claims.Add(ClaimTypes.Role, user.Type.ToString());
            claims.Add(ClaimTypes.GroupSid, tenantKey.ToString());
            claims.Add(ClaimTypes.Name, userInfo.FullName);
            claims.Add(ClaimTypes.NameIdentifier, user.UserName);

            var accessToken = GenerateAccessToken(createAtUtc, expireAtUtc, claims);
            var refreshToken = GenerateRefreshToken();

            var session = new SessionModel
            {
                IdUser = user.Key,
                Type = EnumExtension.ToEnum<UserType>(user.Type),
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                CreatedAt = createAt,
                UserInfo = await GetInfoAsync(user.Key),
                TenantInfo = await GetTenantInfoAsync(user.TenantCode),
                Claims = await GetClaimsAsync(user.Id),
                OrganizationInfos = await GetOrganizationInfoAsync(user.Key)
            };

            token = new TokenInfo
            {
                AccessToken = accessToken,
                CreatedAt = createAt,
                CreatedAtUtc = createAtUtc,
                ExpireAt = expireAt,
                ExpireAtUtc = expireAtUtc,
                Key = tokenKey,
                IdUser = user.Key,
                LocalIpAddress = NetworkExtension.GetLocalIpAddress(_httpContextAccessor.HttpContext.Request).ToString(),
                PublicIpAddress = NetworkExtension.GetRemoteIpAddress(_httpContextAccessor.HttpContext.Request).ToString(),
                Metadata = JsonConvert.SerializeObject(session),
                RefreshToken = refreshToken,
                Source = "Application",
                TimeToLife = expireIn.TotalMinutes,
                UserAgent = NetworkExtension.GetUserAgent(_httpContextAccessor.HttpContext.Request),
                TenantCode = tenantKey,
            };

            var repoToken = _uow.GetRepository<ITokenRepository>();
            await repoToken.InsertAsync(token);
            await _uow.CommitAsync();

            return token;
        }

        private async Task RemoveTokenExpiredAsync(Guid userKey)
        {
            var repoToken = _uow.GetRepository<ITokenRepository>();
            var tokens = await repoToken.GetQuery().Where(x => x.IdUser == userKey && x.ExpireAtUtc <= DateTime.UtcNow).ToListAsync();
            if (tokens.Count > 0)
            {
                repoToken.Deletes(tokens);
                await _uow.CommitAsync();
            }
        }

        public async Task LogoutAsync()
        {
            var userKey = _workContext.GetUserKey();
            if (userKey == Guid.Empty)
                return;

            var userAgent = NetworkExtension.GetUserAgent(_httpContextAccessor.HttpContext.Request);
            var localIpAddress = NetworkExtension.GetLocalIpAddress(_httpContextAccessor.HttpContext.Request).ToString();
            var remoteIpAddress = NetworkExtension.GetRemoteIpAddress(_httpContextAccessor.HttpContext.Request).ToString();

            var repoToken = _uow.GetRepository<ITokenRepository>();
            var token = await repoToken.GetByUserAsync(userKey, userAgent, localIpAddress, remoteIpAddress);
            if (token == null)
                return;

            await _cache.RemoveAsync($":TokenInfos:{token.Id}");

            repoToken.Delete(token);
            await _uow.CommitAsync();
        }

        public async Task<TokenModel> RefreshTokenAsync(string refreshToken)
        {
            var repoToken = _uow.GetRepository<ITokenRepository>();
            var token = await repoToken.GetQuery().FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (token == null)
                return null;

            var expireIn = TimeSpan.FromMinutes(_options.ExpireTime);
            var createAt = DateTime.Now;
            var createAtUtc = DateTime.UtcNow;
            var expireAt = createAt.Add(expireIn);
            var expireAtUtc = createAtUtc.Add(expireIn);
            var metadata = JsonConvert.DeserializeObject<SessionModel>(token.Metadata);

            var claims = new Dictionary<string, object>();
            claims.Add(JwtRegisteredClaimNames.Jti, token.Key);
            claims.Add(ClaimTypes.Sid, token.IdUser.ToString());
            claims.Add(ClaimTypes.GroupSid, token.TenantCode.ToString());
            claims.Add(ClaimTypes.Name, metadata.UserInfo.FullName);
            claims.Add(ClaimTypes.NameIdentifier, metadata.UserName);

            repoToken.UpdateFields(token,
                token.Set(x => x.CreatedAt, createAt),
                token.Set(x => x.CreatedAtUtc, createAtUtc),
                token.Set(x => x.ExpireAt, expireAt),
                token.Set(x => x.ExpireAtUtc, expireAtUtc),
                token.Set(x => x.AccessToken, GenerateAccessToken(createAtUtc, expireAtUtc, claims)),
                token.Set(x => x.RefreshToken, GenerateRefreshToken())
            );
            await _uow.CommitAsync();

            return new TokenModel
            {
                AccessToken = token.AccessToken,
                // ExpireIn = (token.ExpireAtUtc - DateTime.UtcNow).TotalMinutes,
                ExpireAt = token.ExpireAtUtc,
                Timezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalHours,
                RefreshToken = token.RefreshToken
            };
        }

        public async Task<Guid> ForgotPasswordAsync(Guid tenantKey, ForgotPasswordModel model)
        {
            var repoUser = _uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByUsernameAsync(tenantKey, model.UserName);
            if (user == null)
                throw new Exception("Tài khoản không tồn tại");

            // kiểm tra có đúng là mail của tài khoản hay không
            if (string.IsNullOrEmpty(user.Email))
                throw new Exception("Tài khoản không có email. Vui lòng liên hệ công ty/chi nhánh để cấp lại mật khẩu");

            // kiểm tra có đúng là mail của tài khoản hay không
            var userEmails = user.Email.Split(";");
            if (!userEmails.Contains(model.Email))
                throw new Exception("Email không đúng. Vui lòng nhập đúng email của tài khoản");

            return user.Key;
        }

        public async Task<Guid> ResetForgotPasswordAsync(Guid tenantKey, ResetForgotPasswordModel model)
        {
            // tìm tài khoản
            var repoUser = _uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByKeyAsync(tenantKey, model.Id);
            if (user == null)
                throw new Exception("Không tìm thấy thông tin tài khoản");

            if (model.SecurityStamp != user.SecurityStamp)
                throw new Exception("Tài khoản đã được đổi mật khẩu. Thời gian đổi mật khẩu đã hết hạn");

            var passwordHasher = _passwordHasher.HashPassword(user, model.NewPassword);

            user.PasswordHash = passwordHasher;
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.Now;
            user.UpdatedAtUtc = DateTime.UtcNow;
            user.UpdatedBy = user.Key;

            repoUser.Update(user);
            await _uow.CommitAsync();

            return user.Key;
        }


        public async Task<TokenInfo> GetTokenAsync(Guid userKey)
        {
            var userAgent = NetworkExtension.GetUserAgent(_httpContextAccessor.HttpContext.Request);
            var localIpAddress = NetworkExtension.GetLocalIpAddress(_httpContextAccessor.HttpContext.Request).ToString();
            var remoteIpAddress = NetworkExtension.GetRemoteIpAddress(_httpContextAccessor.HttpContext.Request).ToString();

            TokenInfo token = null;
            //Lấy token từ cache theo IdUser
            var bytes = await _cache.GetAsync($":Sessions:{userKey}");
            if (bytes != null)
            {
                var tokenCodes = JsonConvert.DeserializeObject<List<Guid>>(Encoding.UTF8.GetString(bytes));
                foreach (var tokenCode in tokenCodes)
                {
                    bytes = await _cache.GetAsync($":TokenInfos:{tokenCode}");
                    if (bytes != null)
                    {
                        var tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(Encoding.UTF8.GetString(bytes));
                        if (tokenInfo.UserAgent == userAgent && tokenInfo.LocalIpAddress == localIpAddress && tokenInfo.PublicIpAddress == remoteIpAddress)
                        {
                            return tokenInfo;
                        }
                    }
                }
            }

            //Lấy token từ DB còn sử dụng dc theo IdUser
            if (token == null)
            {
                var repoToken = _uow.GetRepository<ITokenRepository>();
                var tokens = await repoToken.GetUnexpiredTokenByUserAsync(userKey);
                token = tokens.FirstOrDefault(x =>
                    x.UserAgent == userAgent
                    && x.LocalIpAddress == localIpAddress
                    && x.PublicIpAddress == remoteIpAddress);

                if (token == null)
                    return null;

                var idTokens = tokens.Select(x => x.Key).ToList();
                await _cache.SetAsync($":Sessions:{userKey}", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(idTokens)));

                if (token.ExpireAtUtc > DateTime.UtcNow)
                {
                    var cacheOption = new DistributedCacheEntryOptions();
                    cacheOption.AbsoluteExpirationRelativeToNow = token.ExpireAtUtc - DateTime.UtcNow;
                    await _cache.SetAsync($":TokenInfos:{token.Key}", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(token)), cacheOption);
                }

            }

            return token;
        }

        private async Task<SessionInfoModel> GetInfoAsync(Guid userKey)
        {
            var repoInfo = _uow.GetRepository<IUserRepository>();
            var info = await repoInfo.FindUserInfoByKeyAsync(userKey);
            var model = new SessionInfoModel
            {
                AvatarPath = info.AvatarPath,
                FullName = info.FullName,
            };
            return model;
        }

        private async Task<SessionTenantModel> GetTenantInfoAsync(Guid tenantKey)
        {

            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetByCodeAsync(tenantKey);
            if (tenant == null)
                throw new Exception("Đơn vị không tồn tại");


            var repoInfo = _uow.GetRepository<ITenantRepository>();
            var info = await repoInfo.GetInfoByCodeAsync(tenantKey);
            return new SessionTenantModel
            {
                Code = tenant.Key,
                Theme = tenant.Theme,
                TaxCode = info.TaxCode,
                FullNameVi = info.FullNameVi,
                FullNameEn = info.FullNameEn,
                ShortName = info.ShortName,
                BranchName = info.BranchName
            };
        }

        private async Task<Dictionary<string, List<string>>> GetClaimsAsync(Guid userKey)
        {
            var permissions = new Dictionary<string, List<string>>();
            var repoPermission = _uow.GetRepository<IPermissionRepository>();

            var userClaims = await repoPermission.FindUserClaimByUserAsync(userKey);
            permissions.AddRange(userClaims.ToDictionary(x => x.ClaimType, x => new List<string> { x.ClaimValue }));

            var roleKeys = (await repoPermission.FindRolesByUserAsync(userKey)).Select(x => x.RoleId).ToList();
            var roleClaims = await repoPermission.FindRoleClaimByRolesAsync(roleKeys);

            // Merge role claim
            var rolePermission = new Dictionary<string, List<string>>();
            var grouped = roleClaims.GroupBy(x => x.ClaimType);
            foreach (var group in grouped)
            {
                var claimValues = new List<string>();
                foreach (var roleClaim in group)
                {
                    if (string.IsNullOrEmpty(roleClaim.ClaimValue))
                        continue;

                    claimValues.AddRange(roleClaim.ClaimValue.Split('|'));
                }
                claimValues = claimValues.Distinct().ToList();
                rolePermission.Add(group.Key, claimValues);
            }

            permissions.AddRange(rolePermission);
            return permissions;
        }

        private async Task<List<SessionOrganizationModel>> GetOrganizationInfoAsync(Guid userKey)
        {
            var repoOrganizationUser = _uow.GetRepository<IRepository<OrganizationUser>>();
            var organizationUsers = await repoOrganizationUser.GetQuery()
                .Where(x => x.IdUser == userKey)
                .ToListAsync();

            var codes = organizationUsers.Select(x => x.OrganizationCode).ToList();
            var repoOrganizationUnit = _uow.GetRepository<IRepository<OrganizationUnit>>();
            var organizations = await repoOrganizationUnit.GetQuery()
                .Where(x => codes.Contains(x.Key))
                .Select(x => new SessionOrganizationModel
                {
                    Code = x.Key,
                    FullName = x.FullName,
                    Name = x.Name
                })
                .ToListAsync();

            foreach (var item in organizations)
            {
                var organizationUser = organizationUsers.FirstOrDefault(x => x.OrganizationCode == item.Code);
                if (organizationUser != null)
                {
                    item.Level = organizationUser.Level;
                }
            }

            return organizations;
        }

        public async Task<Guid> RegisterAsync(Guid tenantKey, RegisterModel model, UserType type)
        {
            var repoUser = _uow.GetRepository<IUserRepository>();
            //kiểm tra xem đã bị trùng username chưa
            if (await repoUser.AnyAsync(x => x.UserName == model.UserName && x.TenantCode == tenantKey))
                throw new Exception("Tài khoản đã bị trùng");

            var userKey = Guid.NewGuid();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Key = userKey,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                TenantCode = tenantKey,
                CreatedBy = Guid.Empty,
                LockoutEnabled = true,
                UserName = model.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                NormalizedUserName = model.UserName.ToUpper(),
                Type = type.GetHashCode()
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            await repoUser.InsertAsync(user);

            await repoUser.InsertUserInfoAsync(new UserInfo
            {
                Key = userKey,
                AvatarPath = null,
                FullName = model.FullName
            });
            await _uow.CommitAsync();
            return userKey;
        }

        public async Task<Guid> CreateAsync(Guid tenantKey, CreateUserModel model)
        {
            var repoUser = _uow.GetRepository<IUserRepository>();

            //kiểm tra xem đã bị trùng username chưa
            if (await repoUser.AnyAsync(x => x.UserName == model.UserName && x.TenantCode == tenantKey))
                throw new Exception("Tài khoản đã bị trùng");

            var userId = Guid.NewGuid();
            var userKey = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Key = userKey,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                TenantCode = tenantKey,
                CreatedBy = _workContext.GetUserKey(),
                LockoutEnabled = true,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                NormalizedUserName = model.UserName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
                Type = model.Type.GetHashCode()
            };

            if (!string.IsNullOrEmpty(model.Email))
                user.NormalizedEmail = model.Email.ToUpper();

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            await repoUser.InsertAsync(user);

            await repoUser.InsertUserInfoAsync(new UserInfo
            {
                Key = userKey,
                AvatarPath = null,
                FullName = model.FullName
            });
            await _uow.CommitAsync();

            //insert metadata
            foreach (var item in _userInfoServices)
            {
                await item.CreateAsync(userKey, model.Metadata);
            }

            return userId;
        }

        public async Task DeleteAsync(Guid userKey)
        {
            //Xóa tài khoản
            var user = await _userManager.FindByIdAsync(userKey.ToString());
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            //Xóa thông tin
            var repoUser = _uow.GetRepository<IUserRepository>();
            var info = await repoUser.FindUserInfoByKeyAsync(user.Key);
            if (info != null)
            {
                repoUser.DeleteUserInfo(info);
                await _uow.CommitAsync();
            }

            //Xóa toàn bộ token
            await DeleteTokenAsync(user);
        }

        public async Task ChangePasswordAsync(Guid tenantKey, Guid userKey, ChangePasswordModel model)
        {
            var repo = _uow.GetRepository<IUserRepository>();
            var user = await repo.FindUserByKeyAsync(tenantKey, userKey);
            if (user == null)
                throw new Exception("Tài khoản không tồn tại");

            var account = await _userManager.FindByIdAsync(user.Id.ToString());
            var result = await _userManager.ChangePasswordAsync(account, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            // Xóa toàn bộ token
            await DeleteTokenAsync(account);
        }

        public async Task LockAsync(Guid tenantKey, Guid userKey, string time)
        {
            var repo = _uow.GetRepository<IUserRepository>();
            var user = await repo.FindUserByKeyAsync(tenantKey, userKey);
            if (user == null)
                throw new Exception("Tài khoản không tồn tại");

            var account = await _userManager.FindByIdAsync(user.Id.ToString());

            IdentityResult result;
            if (string.IsNullOrEmpty(time))
            {
                result = await _userManager.SetLockoutEndDateAsync(account, new DateTimeOffset(DateTime.MaxValue));
            }
            else
            {
                TimeSpan offset = ParseTimeSpan(time);
                result = await _userManager.SetLockoutEndDateAsync(account, new DateTimeOffset(DateTime.UtcNow, offset));
            }

            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            //Xóa toàn bộ token
            await DeleteTokenAsync(account);
        }

        public async Task UnlockAsync(Guid tenantKey, Guid userKey, string time)
        {
            var repo = _uow.GetRepository<IUserRepository>();
            var user = await repo.FindUserByKeyAsync(tenantKey, userKey);
            if (user == null)
                throw new Exception("Tài khoản không tồn tại");

            var account = await _userManager.FindByIdAsync(user.Id.ToString());

            IdentityResult result;
            if (string.IsNullOrEmpty(time))
            {
                result = await _userManager.SetLockoutEndDateAsync(account, null);
            }
            else
            {
                TimeSpan offset = ParseTimeSpan(time);
                result = await _userManager.SetLockoutEndDateAsync(account, new DateTimeOffset(DateTime.UtcNow, offset));
            }

            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            //Xóa toàn bộ token
            await DeleteTokenAsync(account);
        }

        public async Task ResetPasswordAsync(Guid tenantKey, Guid userKey, string password, string emails)
        {
            var user = await _userManager.FindByIdAsync(userKey.ToString());

            if (user == null)
                throw new Exception("Tài khoản không tồn tại");

            if (string.IsNullOrEmpty(password))
                password = RandomExtension.Random(10);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(",", result.Errors.Select(x => x.Description)));
        }



        public string GenerateAccessToken(DateTime issuedAt, DateTime expiredAt, Dictionary<string, object> claims)
        {
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims.Select(x => new Claim(x.Key, x.Value.ToString())),
                notBefore: issuedAt,
                expires: expiredAt,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)), SecurityAlgorithms.HmacSha256)
            );

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(token);
            return accessToken;
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        }

        private static TimeSpan ParseTimeSpan(string time)
        {
            TimeSpan offset;
            var value = int.Parse(time.Substring(0, time.Length - 2));
            var unit = time[time.Length - 1].ToString().ToLower();
            switch (unit)
            {
                case "m":
                    offset = TimeSpan.FromMinutes(value);
                    break;
                case "h":
                    offset = TimeSpan.FromHours(value);
                    break;
                case "d":
                    offset = TimeSpan.FromDays(value);
                    break;
                default:
                    offset = TimeSpan.FromMinutes(value);
                    break;
            }

            return offset;
        }

        private async Task DeleteTokenAsync(User user)
        {
            var repoToken = _uow.GetRepository<ITokenRepository>();
            var tokens = await repoToken.GetByUserAsync(user.Key);
            if (tokens.Count > 0)
            {
                repoToken.Deletes(tokens);
                await _uow.CommitAsync();
            }
        }
    }
}

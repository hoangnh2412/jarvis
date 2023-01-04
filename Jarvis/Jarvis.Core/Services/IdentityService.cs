﻿using Infrastructure.Database.Entities;
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
        Task<TokenModel> LoginAsync(Guid tenantCode, LoginModel model);

        Task LogoutAsync();

        Task<TokenModel> RefreshTokenAsync(string refreshToken);

        Task RegisterAsync(Guid tenantCode, RegisterModel model);

        Task<Guid> CreateAsync(Guid tenantCode, CreateUserModel model);

        Task DeleteAsync(Guid idUser);

        Task ChangePasswordAsync(Guid idUser, ChangePasswordModel model);

        Task LockAsync(Guid idUser, string time);

        Task UnlockAsync(Guid idUser, string time);

        /// <summary>
        /// đổi mật khẩu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task ForgotPasswordAsync(ForgotPasswordModel model);

        /// <summary>
        /// đổi mật khâu khi chọn quên mật khẩu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task ResetForgotPasswordAsync(ResetForgotPasswordModel model);

        /// <summary>
        /// Đặt lại mật khẩu tự động
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="userCode"></param>
        /// <param name="emails"></param>
        /// <returns></returns>
        Task ResetPasswordAsync(Guid tenantCode, Guid userCode, string password, string emails);

        /// <summary>
        /// Lấy token theo user
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        Task<TokenInfo> GetTokenAsync(Guid userCode);

        /// <summary>
        /// Tạo token theo user
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<TokenInfo> GenerateTokenAsync(Guid tenantCode, User user);

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

        public async Task<TokenModel> LoginAsync(Guid tenantCode, LoginModel model)
        {
            model.UserName = model.UserName.ToUpper();

            var repoUser = _uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByUsernameAsync(tenantCode, model.UserName);
            if (user == null)
                throw new Exception("Tài khoản hoặc mật khẩu không đúng");

            var account = await _userManager.FindByIdAsync(user.Id.ToString());
            var result = await _signInManager.CheckPasswordSignInAsync(account, model.Password, true);
            if (result.IsLockedOut)
                throw new Exception("Tài khoản bị khóa");

            if (!result.Succeeded)
                throw new Exception("Tài khoản hoặc mật khẩu không đúng");

            var token = await GenerateTokenAsync(tenantCode, user);

            return new TokenModel
            {
                AccessToken = token.AccessToken,
                // ExpireIn = (token.ExpireAtUtc - DateTime.UtcNow).TotalMinutes,
                ExpireAt = token.ExpireAtUtc,
                Timezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalHours,
                RefreshToken = token.RefreshToken
            };
        }

        public async Task<TokenInfo> GenerateTokenAsync(Guid tenantCode, User user)
        {
            //Xóa các Token đã hết hạn
            await RemoveTokenExpiredAsync(user.Id);

            var token = await GetTokenAsync(user.Id);
            if (token != null)
                return token;

            var userInfo = await GetInfoAsync(user.Id);

            var tokenCode = Guid.NewGuid();
            var expireIn = TimeSpan.FromMinutes(_options.ExpireTime);
            var createAt = DateTime.Now;
            var createAtUtc = DateTime.UtcNow;
            var expireAt = createAt.Add(expireIn);
            var expireAtUtc = createAtUtc.Add(expireIn);
            var claims = new Dictionary<string, object>();
            claims.Add(JwtRegisteredClaimNames.Jti, tokenCode.ToString());
            claims.Add(ClaimTypes.Sid, user.Id.ToString());
            claims.Add(ClaimTypes.GroupSid, tenantCode.ToString());
            claims.Add(ClaimTypes.Name, userInfo.FullName);
            claims.Add(ClaimTypes.NameIdentifier, user.UserName);

            var accessToken = GenerateAccessToken(createAtUtc, expireAtUtc, claims);
            var refreshToken = GenerateRefreshToken();

            var session = new SessionModel
            {
                IdUser = user.Id,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                CreatedAt = createAt,
                UserInfo = await GetInfoAsync(user.Id),
                TenantInfo = await GetTenantInfoAsync(user.TenantCode),
                Claims = await GetClaimsAsync(user.Id),
                OrganizationInfos = await GetOrganizationInfoAsync(user.Id)
            };

            token = new TokenInfo
            {
                AccessToken = accessToken,
                CreatedAt = createAt,
                CreatedAtUtc = createAtUtc,
                ExpireAt = expireAt,
                ExpireAtUtc = expireAtUtc,
                Code = tokenCode,
                IdUser = user.Id,
                LocalIpAddress = NetworkExtension.GetLocalIpAddress(_httpContextAccessor.HttpContext.Request).ToString(),
                PublicIpAddress = NetworkExtension.GetRemoteIpAddress(_httpContextAccessor.HttpContext.Request).ToString(),
                Metadata = JsonConvert.SerializeObject(session),
                RefreshToken = refreshToken,
                Source = "Application",
                TimeToLife = expireIn.TotalMinutes,
                UserAgent = NetworkExtension.GetUserAgent(_httpContextAccessor.HttpContext.Request),
                TenantCode = tenantCode,
            };

            var repoToken = _uow.GetRepository<ITokenRepository>();
            await repoToken.InsertAsync(token);
            await _uow.CommitAsync();

            return token;
        }

        private async Task RemoveTokenExpiredAsync(Guid userCode)
        {
            var repoToken = _uow.GetRepository<ITokenRepository>();
            var tokens = await repoToken.GetQuery().Where(x => x.IdUser == userCode && x.ExpireAtUtc <= DateTime.UtcNow).ToListAsync();
            if (tokens.Count > 0)
            {
                repoToken.Deletes(tokens);
                await _uow.CommitAsync();
            }
        }

        public async Task LogoutAsync()
        {
            var userCode = _workContext.GetUserCode();
            if (userCode == Guid.Empty)
                return;

            var userAgent = NetworkExtension.GetUserAgent(_httpContextAccessor.HttpContext.Request);
            var localIpAddress = NetworkExtension.GetLocalIpAddress(_httpContextAccessor.HttpContext.Request).ToString();
            var remoteIpAddress = NetworkExtension.GetRemoteIpAddress(_httpContextAccessor.HttpContext.Request).ToString();

            var repoToken = _uow.GetRepository<ITokenRepository>();
            var token = await repoToken.GetByUserAsync(userCode, userAgent, localIpAddress, remoteIpAddress);
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
            claims.Add(JwtRegisteredClaimNames.Jti, token.Code);
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

        public async Task ForgotPasswordAsync(ForgotPasswordModel model)
        {
            //lấy ra link hiện tại 
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenantHost = await repoTenant.GetHostByHostNameAsync(model.HostName);

            if (tenantHost == null)
                throw new Exception("Không tìm thấy công ty");

            // tìm tài khoản
            var repoUser = _uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByUsernameAsync(tenantHost.Code, model.UserName);
            if (user == null)
                throw new Exception("Không tìm thấy thông tin tài khoản");

            //kiểm tra có đúng là mail của tài khoản hay không
            if (string.IsNullOrEmpty(user.Email))
                throw new Exception("Tài khoản không có email. Vui lòng liên hệ công ty/chi nhánh để cấp lại mật khẩu");

            //kiểm tra có đúng là mail của tài khoản hay không
            var userEmails = user.Email.Split(";");
            if (!userEmails.Contains(model.Email))
                throw new Exception("Email không trùng với email của tài khoản. Vui lòng nhập đúng email của tài khoản");

            //var db = _redis.GetDatabase();
            //await db.ListLeftPushAsync(KeyQueueBackground.SendMail, JsonConvert.SerializeObject(new
            //{
            //    Action = "SendForgotPassword",
            //    Datas = JsonConvert.SerializeObject(new
            //    {
            //        HostName = model.HostName,
            //        IdUser = user.Id,
            //        Email = model.Email,
            //        TenantCode = tenantHost.Code
            //    })
            //}));
        }

        public async Task ResetForgotPasswordAsync(ResetForgotPasswordModel model)
        {
            // tìm tài khoản
            var repoUser = _uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindByIdAsync(model.Id);
            if (user == null)
                throw new Exception("Không tìm thấy thông tin tài khoản");

            if (model.SecurityStamp != user.SecurityStamp)
                throw new Exception("Tài khoản đã được đổi mật khẩu. Thời gian đổi mật khẩu đã hết hạn");

            var passwordHasher = _passwordHasher.HashPassword(user, model.NewPassword);

            user.PasswordHash = passwordHasher;
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.Now;
            user.UpdatedAtUtc = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            repoUser.Update(user);

            await _uow.CommitAsync();
        }


        public async Task<TokenInfo> GetTokenAsync(Guid userCode)
        {
            var userAgent = NetworkExtension.GetUserAgent(_httpContextAccessor.HttpContext.Request);
            var localIpAddress = NetworkExtension.GetLocalIpAddress(_httpContextAccessor.HttpContext.Request).ToString();
            var remoteIpAddress = NetworkExtension.GetRemoteIpAddress(_httpContextAccessor.HttpContext.Request).ToString();

            TokenInfo token = null;
            //Lấy token từ cache theo IdUser
            var bytes = await _cache.GetAsync($":Sessions:{userCode}");
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
                var tokens = await repoToken.GetUnexpiredTokenByUserAsync(userCode);
                token = tokens.FirstOrDefault(x =>
                    x.UserAgent == userAgent
                    && x.LocalIpAddress == localIpAddress
                    && x.PublicIpAddress == remoteIpAddress);

                if (token == null)
                    return null;

                var idTokens = tokens.Select(x => x.Code).ToList();
                await _cache.SetAsync($":Sessions:{userCode}", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(idTokens)));

                if (token.ExpireAtUtc > DateTime.UtcNow)
                {
                    var cacheOption = new DistributedCacheEntryOptions();
                    cacheOption.AbsoluteExpirationRelativeToNow = token.ExpireAtUtc - DateTime.UtcNow;
                    await _cache.SetAsync($":TokenInfos:{token.Code}", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(token)), cacheOption);
                }

            }

            return token;
        }

        private async Task<SessionInfoModel> GetInfoAsync(Guid idUser)
        {
            var repoInfo = _uow.GetRepository<IUserRepository>();
            var info = await repoInfo.FindUserInfoByIdAsync(idUser);
            var model = new SessionInfoModel
            {
                AvatarPath = info.AvatarPath,
                FullName = info.FullName,
            };
            return model;
        }

        private async Task<SessionTenantModel> GetTenantInfoAsync(Guid tenantCode)
        {
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetInfoByCodeAsync(tenantCode);
            return new SessionTenantModel
            {
                Code = tenant.Code,
                // Theme = tenant.
                TaxCode = tenant.TaxCode,
                FullNameVi = tenant.FullNameVi,
                FullNameEn = tenant.FullNameEn,
                ShortName = tenant.ShortName,
                BranchName = tenant.BranchName
            };
        }

        private async Task<Dictionary<string, KeyValuePair<ClaimOfResource, ClaimOfChildResource>>> GetClaimsAsync(Guid idUser)
        {
            var permissions = new Dictionary<string, KeyValuePair<ClaimOfResource, ClaimOfChildResource>>();
            var repoUserRole = _uow.GetRepository<IPermissionRepository>();

            //Lấy permission theo User
            var userClaims = (await repoUserRole.FindUserClaimByUserAsync(idUser)).Select(x => new KeyValuePair<string, string>(x.ClaimType, x.ClaimValue)).ToList();
            ParsePermission(permissions, userClaims);

            //Lấy permission theo Role
            var idRoles = (await repoUserRole.FindRolesByUserAsync(idUser)).Select(x => x.RoleId).ToList();
            var roleClaims = (await repoUserRole.FindRoleClaimByRolesAsync(idRoles)).Select(x => new KeyValuePair<string, string>(x.ClaimType, x.ClaimValue)).ToList();
            ParsePermission(permissions, roleClaims);

            return permissions;
        }

        private async Task<List<SessionOrganizationModel>> GetOrganizationInfoAsync(Guid idUser)
        {
            var repoOrganizationUser = _uow.GetRepository<IRepository<OrganizationUser>>();
            var organizationUsers = await repoOrganizationUser.GetQuery()
                .Where(x => x.IdUser == idUser)
                .ToListAsync();

            var codes = organizationUsers.Select(x => x.OrganizationCode).ToList();
            var repoOrganizationUnit = _uow.GetRepository<IRepository<OrganizationUnit>>();
            var organizations = await repoOrganizationUnit.GetQuery()
                .Where(x => codes.Contains(x.Code))
                .Select(x => new SessionOrganizationModel
                {
                    Code = x.Code,
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

        public async Task RegisterAsync(Guid idTenant, RegisterModel model)
        {
            var repoUser = _uow.GetRepository<IUserRepository>();
            //kiểm tra xem đã bị trùng username chưa
            if (await repoUser.AnyAsync(x => x.UserName == model.Username && x.TenantCode == idTenant))
                throw new Exception("Tài khoản đã bị trùng");

            var idUser = Guid.NewGuid();
            var user = new User
            {
                Id = idUser,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                TenantCode = idTenant,
                CreatedBy = Guid.Empty,
                LockoutEnabled = true,
                UserName = model.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
                NormalizedUserName = model.Username.ToUpper()
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            await repoUser.InsertAsync(user);

            await repoUser.InsertUserInfoAsync(new UserInfo
            {
                Id = idUser,
                AvatarPath = null,
                FullName = model.FullName
            });
            await _uow.CommitAsync();
        }

        public async Task<Guid> CreateAsync(Guid idTenant, CreateUserModel model)
        {
            var repoUser = _uow.GetRepository<IUserRepository>();

            //kiểm tra xem đã bị trùng username chưa
            if (await repoUser.AnyAsync(x => x.UserName == model.UserName && x.TenantCode == idTenant))
                throw new Exception("Tài khoản đã bị trùng");

            var idUser = Guid.NewGuid();
            var user = new User
            {
                Id = idUser,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                TenantCode = idTenant,
                CreatedBy = _workContext.GetUserCode(),
                LockoutEnabled = true,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                NormalizedUserName = model.UserName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            if (!string.IsNullOrEmpty(model.Email))
                user.NormalizedEmail = model.Email.ToUpper();

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            await repoUser.InsertAsync(user);

            await repoUser.InsertUserInfoAsync(new UserInfo
            {
                Id = idUser,
                AvatarPath = null,
                FullName = model.FullName
            });
            await _uow.CommitAsync();

            //insert metadata
            foreach (var item in _userInfoServices)
            {
                await item.CreateAsync(idUser, model.Metadata);
            }

            return idUser;
        }

        public async Task DeleteAsync(Guid idUser)
        {
            //Xóa tài khoản
            var user = await _userManager.FindByIdAsync(idUser.ToString());
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            //Xóa thông tin
            var repoUser = _uow.GetRepository<IUserRepository>();
            var info = await repoUser.FindUserInfoByIdAsync(user.Id);
            if (info != null)
            {
                repoUser.DeleteUserInfo(info);
                await _uow.CommitAsync();
            }

            //Xóa toàn bộ token
            await DeleteTokenAsync(user);
        }

        public async Task ChangePasswordAsync(Guid idUser, ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(idUser.ToString());
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            //Xóa toàn bộ token
            await DeleteTokenAsync(user);
        }

        public async Task LockAsync(Guid idUser, string time)
        {
            var user = await _userManager.FindByIdAsync(idUser.ToString());

            IdentityResult result;
            if (string.IsNullOrEmpty(time))
            {
                result = await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.MaxValue));
            }
            else
            {
                TimeSpan offset = ParseTimeSpan(time);
                result = await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.UtcNow, offset));
            }

            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            //Xóa toàn bộ token
            await DeleteTokenAsync(user);
        }

        public async Task UnlockAsync(Guid idUser, string time)
        {
            var user = await _userManager.FindByIdAsync(idUser.ToString());

            IdentityResult result;
            if (string.IsNullOrEmpty(time))
            {
                result = await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                TimeSpan offset = ParseTimeSpan(time);
                result = await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.UtcNow, offset));
            }

            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(x => x.Description).ToList()));

            //Xóa toàn bộ token
            await DeleteTokenAsync(user);
        }

        public async Task ResetPasswordAsync(Guid tenantCode, Guid idUser, string password, string emails)
        {
            var user = await _userManager.FindByIdAsync(idUser.ToString());

            if (user == null)
                throw new Exception("Tài khoản không tồn tại");

            if (string.IsNullOrEmpty(password))
                password = RandomExtension.Random(10);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(",", result.Errors.Select(x => x.Description)));

            ////gửi mail
            //var db = _redis.GetDatabase();
            //await db.ListLeftPushAsync(KeyQueueBackground.SendMail, JsonConvert.SerializeObject(new
            //{
            //    Action = "ResetPassword",
            //    Datas = JsonConvert.SerializeObject(new
            //    {
            //        Emails = emails,
            //        Password = password,
            //        TenantCode = tenantCode,
            //        IdUser = user.Id
            //    })
            //}));
        }



        public string GenerateAccessToken(DateTime issuedAt, DateTime expiredAt, Dictionary<string, object> claims)
        {
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims.Select(x => new Claim(x.Key, x.Value.ToString())),
                notBefore: issuedAt,
                expires: expiredAt,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.SecretKey)), SecurityAlgorithms.HmacSha256)
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
            var tokens = await repoToken.GetByUserAsync(user.Id);
            if (tokens.Count > 0)
            {
                repoToken.Deletes(tokens);
                await _uow.CommitAsync();
            }
        }

        private static void ParsePermission(Dictionary<string, KeyValuePair<ClaimOfResource, ClaimOfChildResource>> permissions, List<KeyValuePair<string, string>> userClaims)
        {
            foreach (var item in userClaims)
            {
                var splited = item.Value.Split('|');
                var claimOfResource = EnumExtension.ToEnum<ClaimOfResource>(splited[0]);
                var claimOfChildResource = EnumExtension.ToEnum<ClaimOfChildResource>(splited[1]);

                if (!permissions.ContainsKey(item.Key))
                    permissions.Add(item.Key, new KeyValuePair<ClaimOfResource, ClaimOfChildResource>(claimOfResource, claimOfChildResource));

                var claimValue = permissions[item.Key];
                //Lấy quyền cao nhất, vẫn giữ Child Resource
                if (claimValue.Key < claimOfResource)
                    permissions[item.Key] = new KeyValuePair<ClaimOfResource, ClaimOfChildResource>(claimOfResource, claimValue.Value);

                //Lấy quyền cao nhất, vẫn giữ Resource
                if (claimValue.Value < claimOfChildResource)
                    permissions[item.Key] = new KeyValuePair<ClaimOfResource, ClaimOfChildResource>(permissions[item.Key].Key, claimOfChildResource);
            }
        }
    }
}

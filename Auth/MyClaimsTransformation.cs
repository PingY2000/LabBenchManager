// Auth/MyClaimsTransformation.cs
namespace LabBenchManager.Auth
{
    using LabBenchManager.Models;
    using LabBenchManager.Services;
    using Microsoft.AspNetCore.Authentication;
    using System.Security.Claims;

    public class MyClaimsTransformation : IClaimsTransformation
    {
        private readonly UserService _userService;
        private readonly ILogger<MyClaimsTransformation> _logger;

        public MyClaimsTransformation(UserService userService, ILogger<MyClaimsTransformation> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated || string.IsNullOrEmpty(identity.Name))
            {
                return principal;
            }

            // 🔥 避免重复处理
            if (identity.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                return principal;
            }

            var userName = identity.Name;
            var userInDb = await _userService.GetUserWithRoleAsync(userName);

            // 🆕 用户不存在时自动注册为 Requester
            if (userInDb == null)
            {
                _logger.LogInformation("New user '{UserName}' detected. Auto-registering with role '{Role}'.",
                    userName, AppRoles.Requester);

                userInDb = await _userService.CreateUserAsync(new ApplicationUser
                {
                    NtAccount = userName,
                    Role = AppRoles.Requester,
                    DisplayName = GetDisplayNameFromIdentity(identity) ?? UserService.GetUserName(userName),
                    Email = GetEmailFromIdentity(identity)
                });

                if (userInDb == null)
                {
                    _logger.LogError("Failed to auto-register user '{UserName}'.", userName);
                    return principal;
                }

                _logger.LogInformation("Successfully auto-registered user '{UserName}' with role '{Role}'.",
                    userName, AppRoles.Requester);
            }
            // 🆕 用户存在但无角色时，分配默认角色
            else if (string.IsNullOrWhiteSpace(userInDb.Role))
            {
                _logger.LogWarning("User '{UserName}' exists but has no role. Assigning default role '{Role}'.",
                    userName, AppRoles.Requester);

                await _userService.UpdateUserRoleAsync(userName, AppRoles.Requester);
                userInDb.Role = AppRoles.Requester;
            }

            // 添加角色声明
            var claims = new List<Claim>(identity.Claims)
            {
                new Claim(ClaimTypes.Role, userInDb.Role)
            };

            // 可选：添加其他声明
            if (!string.IsNullOrEmpty(userInDb.DisplayName))
            {
                claims.Add(new Claim("DisplayName", userInDb.DisplayName));
            }
            if (!string.IsNullOrEmpty(userInDb.Department))
            {
                claims.Add(new Claim("Department", userInDb.Department));
            }
            if (!string.IsNullOrEmpty(userInDb.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, userInDb.Email));
            }

            var newIdentity = new ClaimsIdentity(
                claims,
                identity.AuthenticationType,
                ClaimTypes.Name,
                ClaimTypes.Role
            );

            _logger.LogInformation("Added role '{Role}' for user '{UserName}'.", userInDb.Role, userName);

            return new ClaimsPrincipal(newIdentity);
        }

        /// <summary>
        /// 从 Identity 中提取显示名称
        /// </summary>
        private string? GetDisplayNameFromIdentity(ClaimsIdentity identity)
        {
            return identity.FindFirst(ClaimTypes.GivenName)?.Value
                ?? identity.FindFirst("name")?.Value
                ?? identity.FindFirst("displayname")?.Value
                ?? identity.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// 从 Identity 中提取邮箱
        /// </summary>
        private string? GetEmailFromIdentity(ClaimsIdentity identity)
        {
            return identity.FindFirst(ClaimTypes.Email)?.Value
                ?? identity.FindFirst("email")?.Value;
        }
    }
}
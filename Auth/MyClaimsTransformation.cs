// Auth/MyClaimsTransformation.cs
namespace LabBenchManager.Auth
{
    using LabBenchManager.Services;
    using Microsoft.AspNetCore.Authentication;
    using System.Security.Claims;
    using System.Threading.Tasks;

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

            var userInDb = await _userService.GetUserWithRoleAsync(identity.Name);

            if (userInDb == null || string.IsNullOrWhiteSpace(userInDb.Role))
            {
                _logger.LogInformation("User '{UserName}' not found or has no role assigned.", identity.Name);
                return principal;
            }

            if (identity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == userInDb.Role))
            {
                return principal;
            }

            var claims = new List<Claim>(identity.Claims)
            {
                new Claim(ClaimTypes.Role, userInDb.Role)
            };

            var newIdentity = new ClaimsIdentity(
                claims,
                identity.AuthenticationType,
                ClaimTypes.Name,
                ClaimTypes.Role  // 明确指定角色类型
            );

            _logger.LogInformation("Added role '{Role}' for user '{UserName}'.", userInDb.Role, identity.Name);

            return new ClaimsPrincipal(newIdentity);
        }
    }
}
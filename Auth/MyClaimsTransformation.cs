// 命名空间应与你的项目和文件夹结构匹配
namespace LabBenchManager.Auth
{
    // 引入必要的命名空间
    using LabBenchManager.Services;
    using Microsoft.AspNetCore.Authentication;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// 自定义 Claims 转换器。
    /// 这个类的核心作用是：在用户通过 Windows 身份验证成功登录后，
    /// 从我们自己的数据库中查询该用户的应用程序角色，
    /// 并将这个角色作为一种 "Claim" (声明) 添加到用户的身份凭证中。
    /// 这样，ASP.NET Core 的授权系统（如 [Authorize(Roles = "...")]）才能识别这些角色。
    /// </summary>
    public class MyClaimsTransformation : IClaimsTransformation
    {
        private readonly UserService _userService;
        private readonly ILogger<MyClaimsTransformation> _logger;

        /// <summary>
        /// 构造函数，通过依赖注入获取所需的服务。
        /// </summary>
        /// <param name="userService">用于与用户数据交互的服务。</param>
        /// <param name="logger">用于记录日志。</param>
        public MyClaimsTransformation(UserService userService, ILogger<MyClaimsTransformation> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 转换身份凭证的核心方法。此方法在每次身份验证成功后由 ASP.NET Core 自动调用。
        /// </summary>
        /// <param name="principal">原始的、只包含基本 Windows 身份信息的 ClaimsPrincipal 对象。</param>
        /// <returns>一个可能包含额外角色声明的、新的 ClaimsPrincipal 对象。</returns>
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // 克隆一份原始身份，我们将在副本上操作，这是一种良好的实践。
            var newIdentity = principal.Identity as ClaimsIdentity;
            if (newIdentity == null)
            {
                return principal; // 如果没有身份信息，则无法转换。
            }

            // 检查用户是否已通过身份验证，并且有用户名（NT账号）。
            if (!newIdentity.IsAuthenticated || string.IsNullOrEmpty(newIdentity.Name))
            {
                return principal; // 未登录或没有用户名，直接返回原始凭证。
            }

            // 尝试从我们自己的数据库中查找用户记录。
            // newIdentity.Name 在 Windows 认证下通常是 "DOMAIN\username" 格式。
            var userInDb = await _userService.GetUserWithRoleAsync(newIdentity.Name);

            // 如果数据库中不存在该用户，或者用户没有被分配任何角色
            if (userInDb == null || string.IsNullOrWhiteSpace(userInDb.Role))
            {
                // 记录一条信息，这对于调试新用户为何没有权限很有用。
                _logger.LogInformation("User '{UserName}' not found in the application database or has no role assigned. No custom claims added.", newIdentity.Name);
                return principal; // 返回原始凭证，不添加任何额外角色。
            }

            // 检查当前身份凭证是否已经包含了我们要添加的角色声明。
            // 这可以防止在某些场景下（例如页面刷新）重复添加相同的声明。
            if (!newIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == userInDb.Role))
            {
                // 创建一个新的角色声明 (Claim)。
                // ClaimTypes.Role 是 ASP.NET Core 授权系统识别的“角色”标准类型。
                var roleClaim = new Claim(ClaimTypes.Role, userInDb.Role);

                // 将新的角色声明添加到身份副本中。
                newIdentity.AddClaim(roleClaim);

                _logger.LogInformation("Added role claim '{Role}' for user '{UserName}'.", userInDb.Role, newIdentity.Name);
            }

            // 返回包含新角色声明的、经过“升级”的身份凭证。
            // 从现在起，应用程序的其余部分（如 AuthorizeView）都将使用这个新的 principal。
            return new ClaimsPrincipal(newIdentity);
        }
    }
}
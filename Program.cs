// ====================================================================
// 1. 引用 (Usings)
// 确保所有需要的命名空间都被引入。
// ====================================================================
using LabBenchManager.Auth; // <--- 引入我们自定义的认证相关类
using LabBenchManager.Data;
using LabBenchManager.Models;
using LabBenchManager.Services;
using Microsoft.AspNetCore.Authentication; // <--- 引入 Claims Transformation 所需的接口
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;


// ====================================================================
// 2. WebApplication Builder 设置
// 配置所有服务容器。
// ====================================================================
var builder = WebApplication.CreateBuilder(args);

// --- 核心服务 ---
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// --- 数据库上下文 ---
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=LabBenchDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<LabDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 业务逻辑服务 ---
builder.Services.AddScoped<BenchService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<UserService>(); // <--- 注册 UserService

// --- 身份认证与授权 ---
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();
builder.Services.AddAuthorization();

// 注册 Claims Transformation 服务，这是权限系统的“心脏”
builder.Services.AddScoped<IClaimsTransformation, MyClaimsTransformation>();


// ====================================================================
// 3. WebApplication 构建
// 创建应用程序实例。
// ====================================================================
var app = builder.Build();


// ====================================================================
// 4. HTTP 请求管道配置
// 定义中间件的顺序。
// ====================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 认证和授权中间件必须放在 UseRouting 和 MapBlazorHub 之间
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


// ====================================================================
// 5. 应用程序启动时初始化 (数据库迁移与种子数据)
// 确保数据库是最新的，并包含初始数据。
// ====================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<LabDbContext>();

        // 1. 自动应用所有挂起的数据库迁移
        dbContext.Database.Migrate();

        // 2. 调用种子数据方法，创建初始角色和管理员
        await SeedData(dbContext);
    }
    catch (Exception ex)
    {
        // 如果在数据库初始化过程中发生错误，记录下来
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}


// ====================================================================
// 6. 运行应用程序
// 启动 Web 服务器并开始监听请求。
// ====================================================================
app.Run();


// ====================================================================
// 7. 种子数据辅助方法
// 这是一个局部函数，用于初始化数据库的初始内容。
// ====================================================================
async Task SeedData(LabDbContext context)
{
    // 检查数据库中是否已经有用户了，如果有，则说明种子数据已添加过，直接返回。
    if (await context.ApplicationUsers.AnyAsync())
    {
        return;
    }

    var adminUser = new ApplicationUser
    {
        NtAccount = @"apac\yup2cha", 
        Role = AppRoles.Admin,
        DisplayName = "初始管理员"
    };

    // 将新用户添加到上下文中
    context.ApplicationUsers.Add(adminUser);

    // 将更改保存到数据库
    await context.SaveChangesAsync();
}

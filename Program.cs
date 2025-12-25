using LabBenchManager.Auth;
using LabBenchManager.Data;
using LabBenchManager.Models;
using LabBenchManager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Claims;  

var builder = WebApplication.CreateBuilder(args);

// --- 核心服务 ---
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// --- 数据库上下文 ---
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found in configuration."); 
    //?? "Server=(localdb)\\MSSQLLocalDB;Database=LabBenchDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
   
builder.Services.AddDbContextFactory<LabDbContext>(options =>
{
    options.UseSqlServer(connectionString);
}, ServiceLifetime.Scoped);

builder.Services.AddDbContext<LabDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 业务逻辑服务 ---
builder.Services.AddScoped<BenchService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TestPlanService>();
builder.Services.AddScoped<ReportApprovalService>();
builder.Services.AddScoped<TestPlanHistoryService>();

// --- 身份认证与授权 ---
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate()
        .AddCookie("DevAuth", options =>
        {
            options.LoginPath = "/dev/login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });
}
else
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();
}

builder.Services.AddAuthorization();
builder.Services.AddScoped<IClaimsTransformation, MyClaimsTransformation>();

// 添加配置项
builder.Configuration["IsDevelopment"] = builder.Environment.IsDevelopment().ToString();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 🔥 开发环境：添加角色切换端点
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/switch-role", async (HttpContext context, string role, LabDbContext db) =>
    {
        var userName = context.User.Identity?.Name ?? "apac\\devuser";

        var user = await db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.NtAccount.ToLower() == userName.ToLower());

        if (user != null)
        {
            user.Role = role == "None" ? AppRoles.Requester : role;
            await db.SaveChangesAsync();
        }
        else if (role != "None")
        {
            db.ApplicationUsers.Add(new ApplicationUser
            {
                NtAccount = userName,
                DisplayName = "开发测试用户",
                Role = role
            });
            await db.SaveChangesAsync();
        }

        return Results.Redirect("/");
    });

    app.MapGet("/dev/login", async (HttpContext context, LabDbContext db) =>
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "apac\\devuser"),
            new Claim(ClaimTypes.Role, AppRoles.Admin)
        };

        var identity = new ClaimsIdentity(claims, "DevAuth", ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync("DevAuth", principal);

        var user = await db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.NtAccount == "apac\\devuser");

        if (user == null)
        {
            db.ApplicationUsers.Add(new ApplicationUser
            {
                NtAccount = "apac\\devuser",
                DisplayName = "开发测试用户",
                Role = AppRoles.Admin
            });
            await db.SaveChangesAsync();
        }

        return Results.Redirect("/");
    });
}

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// 数据库迁移和种子数据
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<LabDbContext>();
        dbContext.Database.Migrate();
        await SeedData(dbContext, builder.Environment);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

app.Run();

// 🔥 修改种子数据方法签名
async Task SeedData(LabDbContext context, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        if (!await context.ApplicationUsers.AnyAsync())
        {
            context.ApplicationUsers.AddRange(
                new ApplicationUser
                {
                    NtAccount = @"apac\yup2cha",
                    Role = AppRoles.Admin,
                    DisplayName = "管理员"
                },
                new ApplicationUser
                {
                    NtAccount = @"apac\devuser",
                    Role = AppRoles.Admin,
                    DisplayName = "开发测试用户"
                },
                new ApplicationUser
                {
                    NtAccount = @"apac\engineer",
                    Role = AppRoles.TestEngineer,
                    DisplayName = "测试工程师"
                },
                new ApplicationUser
                {
                    NtAccount = @"apac\viewer",
                    Role = AppRoles.Requester,
                    DisplayName = "测试查看者"
                }
            );
            await context.SaveChangesAsync();
        }
    }
    else
    {
        if (!await context.ApplicationUsers.AnyAsync())
        {
            var adminUser = new ApplicationUser
            {
                NtAccount = @"apac\yup2cha",
                Role = AppRoles.Admin,
                DisplayName = "初始管理员"
            };
            context.ApplicationUsers.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
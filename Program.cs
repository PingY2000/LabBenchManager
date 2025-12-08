using LabBenchManager.Data;
using LabBenchManager.Models;
using LabBenchManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);


// 1. 服务容器配置 (Service Container Configuration)
// =================================================

// 添加 Razor Pages 和 Blazor Server 服务
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<WeatherForecastService>();

// SQL Server 连接串（从 appsettings.json 读取，若为空则使用 LocalDB 默认值）
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=LabBenchDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

// 注册 DbContext（使用 SQL Server）
builder.Services.AddDbContext<LabDbContext>(options =>
    options.UseSqlServer(connectionString));

// 注册业务服务（依赖 DbContext，生命周期设为 Scoped）
builder.Services.AddScoped<BenchService>();


// =================================================
var app = builder.Build();
// =================================================


// 2. HTTP 请求管道配置 (HTTP Request Pipeline Configuration)
// =========================================================

// 开发环境外的异常处理
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 映射 Blazor Hub 和回退页面
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


// 3. 应用程序启动时初始化 (Initialization on Application Startup)
// ===============================================================

// 启动时初始化数据库
// （无迁移时可自动建库建表；若已添加迁移，建议改用 db.Database.Migrate()）
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // 注意：GetRequiredService 需要指定要获取的服务类型
    var dbContext = services.GetRequiredService<LabDbContext>();

    // 确保数据库已创建。如果使用 EF Core Migrations，请注释掉此行，并取消下一行的注释。
    dbContext.Database.EnsureCreated();

    // 如果已添加迁移，改为使用 Migrate() 来应用迁移
    // dbContext.Database.Migrate(); 
}


// 4. 运行应用程序 (Run the Application)
// ======================================
app.Run();
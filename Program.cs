using LabBenchManager.Data;
using LabBenchManager.Models;
using LabBenchManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddDbContext<LabDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<BenchService>();

//builder.Services.AddRazorPages(); 
//builder.Services.AddServerSideBlazor();

var app = builder.Build();




// ========== 数据库初始化（直接内联代码）==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<LabDbContext>();

        // 创建数据库和表
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("数据库已创建或已存在");

        // 检查并添加种子数据
        if (!await context.Benches.AnyAsync())
        {
            logger.LogInformation("正在添加种子数据...");

            var benches = new Bench[]
            {
                new Bench { Name = "工作台1", Location = "实验室A", Description = "化学实验专用" },
                new Bench { Name = "工作台2", Location = "实验室B", Description = "物理实验专用" },
                new Bench { Name = "工作台3", Location = "实验室C", Description = "生物实验专用" }
            };

            context.Benches.AddRange(benches);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ 成功添加 {Count} 条种子数据", benches.Length);
        }
        else
        {
            logger.LogInformation("数据库已有数据，跳过种子数据添加");
        }

        logger.LogInformation("✅ 数据库初始化完成");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ 数据库初始化时发生错误");
        throw;
    }
}
// ========== 数据库初始化结束 ==========





// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();



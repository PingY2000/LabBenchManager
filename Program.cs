using LabBenchManager.Auth;
using LabBenchManager.Data;
using LabBenchManager.Models;
using LabBenchManager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TestPlanService>();

// --- 身份认证与授权 ---
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();
builder.Services.AddAuthorization();

builder.Services.AddScoped<IClaimsTransformation, MyClaimsTransformation>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<LabDbContext>();
        dbContext.Database.Migrate();
        await SeedData(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

app.Run();

async Task SeedData(LabDbContext context)
{
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

    context.ApplicationUsers.Add(adminUser);
    await context.SaveChangesAsync();
}
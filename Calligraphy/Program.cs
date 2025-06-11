using Calligraphy.Data;
using Calligraphy.Models;
using Calligraphy.Services;
using Calligraphy.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// 註冊 CalligraphyContext，使用另一組 SQL Server 連線字串
builder.Services.AddDbContext<CalligraphyContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CalligraphyDB"));
});

// 註冊資料庫開發人員例外頁面過濾器（開發時顯示詳細錯誤）
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 註冊 MVC 控制器與視圖服務（Razor Pages 專案通常也會用到）
builder.Services.AddControllersWithViews();

// 註冊 HttpContextAccessor 服務，方便在服務中取得 HttpContext
builder.Services.AddHttpContextAccessor();

// 註冊自訂的 AuthHelper 服務（用於驗證相關輔助功能）
builder.Services.AddScoped<AuthHelper>();

// 註冊 Email 服務，使用 SmtpEmailService 實作 IEmailService
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

//註冊IP服務
builder.Services.AddScoped<IClientIpService, GetClientIPService>();

//註冊Log服務
builder.Services.AddScoped<ILogService, LogService>();

//註冊SignUp商業邏輯
builder.Services.AddScoped<ISignUpService, SignUpService>();

//驗證登入cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/SignUp/Login";

        // 啟用滑動過期（每次存取都會延長登入時效）
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/SignUp/Login";
    });

var app = builder.Build();

//隱藏X-Powered-By標頭，防止洩漏使用的技術棧
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
       // 移除 X-Powered-By 標頭
        context.Response.Headers.Remove("X-Powered-By");
        return Task.CompletedTask;
    });
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // 非開發環境下使用全域例外處理頁面
    app.UseExceptionHandler("/SignUp/Error");
    // 啟用 HSTS（強制 HTTPS，預設 30 天）
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

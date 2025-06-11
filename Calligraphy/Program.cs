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
// ���U CalligraphyContext�A�ϥΥt�@�� SQL Server �s�u�r��
builder.Services.AddDbContext<CalligraphyContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CalligraphyDB"));
});

// ���U��Ʈw�}�o�H���ҥ~�����L�o���]�}�o����ܸԲӿ��~�^
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ���U MVC ����P���ϪA�ȡ]Razor Pages �M�׳q�`�]�|�Ψ�^
builder.Services.AddControllersWithViews();

// ���U HttpContextAccessor �A�ȡA��K�b�A�Ȥ����o HttpContext
builder.Services.AddHttpContextAccessor();

// ���U�ۭq�� AuthHelper �A�ȡ]�Ω����Ҭ������U�\��^
builder.Services.AddScoped<AuthHelper>();

// ���U Email �A�ȡA�ϥ� SmtpEmailService ��@ IEmailService
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

//���UIP�A��
builder.Services.AddScoped<IClientIpService, GetClientIPService>();

//���ULog�A��
builder.Services.AddScoped<ILogService, LogService>();

//���USignUp�ӷ~�޿�
builder.Services.AddScoped<ISignUpService, SignUpService>();

//���ҵn�Jcookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/SignUp/Login";

        // �ҥηưʹL���]�C���s�����|�����n�J�ɮġ^
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/SignUp/Login";
    });

var app = builder.Build();

//����X-Powered-By���Y�A����|�ϥΪ��޳N��
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
       // ���� X-Powered-By ���Y
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
    // �D�}�o���ҤU�ϥΥ���ҥ~�B�z����
    app.UseExceptionHandler("/SignUp/Error");
    // �ҥ� HSTS�]�j�� HTTPS�A�w�] 30 �ѡ^
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

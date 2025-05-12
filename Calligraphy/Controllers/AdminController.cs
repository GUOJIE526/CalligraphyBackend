using Azure.Identity;
using BCrypt.Net;
using Calligraphy.Models;
using Calligraphy.Services;
using Calligraphy.Services.Interfaces;
using Calligraphy.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Calligraphy.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin,Artist")]
    public class AdminController : Controller
    {
        private readonly CalligraphyContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthHelper _authHelper;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminController> _logger;
        public AdminController(CalligraphyContext context, IHttpContextAccessor contextAccessor, AuthHelper authHelper, IEmailService emailService, ILogger<AdminController> logger)
        {
            _context = context;
            _httpContextAccessor = contextAccessor;
            _authHelper = authHelper;
            _emailService = emailService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "帳號或密碼錯誤");
                return View(model);
            }
            await _authHelper.SignInUserAsync(user, model.RememberMe);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (await _context.TbExhUser.AnyAsync(u => u.Username == model.UserName))
            {
                ModelState.AddModelError("", "帳號已存在");
                return View(model);
            }
            var emaiToken = Guid.NewGuid().ToString();
            var user = new TbExhUser
            {
                Username = model.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Artist",//之後再想怎麼改成不寫死
                Creator = model.UserName,
                CreateFrom = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                MailConfirmcode = emaiToken,
                MailConfirmdate = DateTime.UtcNow.AddDays(1),
            };
            _context.TbExhUser.Add(user);
            //建立驗證連結
            var confirmLink = Url.Action("ConfirmEmail", "Admin", new { token = emaiToken, email = model.UserName }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(model.UserName, "RuoliCalligraphy驗證信", $@"
                                                                                        <p>親愛的使用者您好，</p>
                                                                                        <p>請點擊以下連結以完成帳號驗證：</p>
                                                                                        <p><a href=""{confirmLink}"">{confirmLink}</a></p>
                                                                                        <p>此連結將於 24 小時內失效。</p>");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Email sending timed out.");
            }
            await _context.SaveChangesAsync();
            await _authHelper.SignInUserAsync(user, true);
            return RedirectToAction("Index", "Home");
        }
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.MailConfirmcode == token && u.Username == email);
            if (user == null || user.MailConfirmdate < DateTime.UtcNow)
            {
                return View("Error");
            }
            user.MailConfirm = true;
            await _context.SaveChangesAsync();
            await _authHelper.SignInUserAsync(user, true);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookie");
            return RedirectToAction("Login");
        }
    }
}

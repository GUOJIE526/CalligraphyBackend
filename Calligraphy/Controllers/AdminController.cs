using Azure.Identity;
using BCrypt.Net;
using Calligraphy.Models;
using Calligraphy.Services;
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
        public AdminController(CalligraphyContext context, IHttpContextAccessor contextAccessor, AuthHelper authHelper)
        {
            _context = context;
            _httpContextAccessor = contextAccessor;
            _authHelper = authHelper;
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
            var user = new TbExhUser
            {
                Username = model.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Artist",
                IsActive = true,
                Creator = model.UserName,
                CreateFrom = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            };
            _context.TbExhUser.Add(user);
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

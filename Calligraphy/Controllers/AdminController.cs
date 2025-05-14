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

        /// <summary>
        /// 登入頁面
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 登入頁面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
                ModelState.AddModelError("Password", "帳號或密碼錯誤");
                return View(model);
            }
            if (user.MailConfirm == false)
            {
                ModelState.AddModelError("Username", "帳號尚未驗證");
                return View(model);
            }
            await _authHelper.SignInUserAsync(user, model.RememberMe);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 註冊頁面
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View();

        /// <summary>
        /// 註冊頁面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (await _context.TbExhUser.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "帳號已存在");
                return View(model);
            }
            var emaiToken = Guid.NewGuid().ToString();
            var user = new TbExhUser
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Artist",//之後再想怎麼改成不寫死
                Creator = model.Username,
                CreateFrom = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                MailConfirmcode = emaiToken,
                MailConfirmdate = DateTime.UtcNow.AddDays(1),
            };
            _context.TbExhUser.Add(user);
            //建立驗證連結
            var confirmLink = Url.Action("ConfirmEmail", "Admin", new { token = emaiToken, email = model.Username }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(model.Username, "RuoliCalligraphy驗證信", $@"
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
            //await _authHelper.SignInUserAsync(user, true);
            return RedirectToAction("RegisterRemind", "Admin", new {email = user.Username});
        }

        /// <summary>
        /// 註冊後的提醒頁面
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult RegisterRemind(string email, bool check)
        {
            ViewBag.Email = email;
            ViewBag.Check = check;
            return View();
        }

        /// <summary>
        /// 重新寄送驗證信
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResendConfirmEmail(string email)
        {
            bool check = false;
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.Username == email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "使用者錯誤";
                ViewBag.Email = email;
                return View("RegisterRemind");
            }
            if(user.MailConfirm == true)
            {
                ViewBag.ErrorMessage = "使用者已驗證";
                return View("RegisterRemind");
            }
            user.MailConfirmcode = Guid.NewGuid().ToString();
            user.MailConfirmdate = DateTime.UtcNow.AddDays(1);
            //建立驗證連結
            var confirmLink = Url.Action("ConfirmEmail", "Admin", new { token = user.MailConfirmcode, email = user.Username }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(user.Username, "RuoliCalligraphy驗證信", $@"
                                                                                        <p>親愛的使用者您好，</p>
                                                                                        <p>請點擊以下連結以完成帳號驗證：</p>
                                                                                        <p><a href=""{confirmLink}"">{confirmLink}</a></p>
                                                                                        <p>此連結將於 24 小時內失效。</p>");
                check = true;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Email sending timed out.");
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("RegisterRemind", "Admin", new { email = user.Username, check });
        }

        /// <summary>
        /// 驗證信連結
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>

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
            return RedirectToAction("MailConfirmWelcome", "Admin");
        }

        /// <summary>
        /// 註冊成功後的歡迎頁面
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult MailConfirmWelcome() => View();

        /// <summary>
        /// 忘記密碼頁面
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult ForgotPassword(bool check)
        {
            if (check)
            {
                ViewBag.Check = check;
                ViewBag.Message = "已寄送驗證信，請至信箱收信";
            }
            return View();
        }

        /// <summary>
        /// 忘記密碼頁面
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            bool check = false;
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.Username == email);
            if (user == null)
            {
                ModelState.AddModelError("", "使用者不存在");
                return View();
            }
            var resetToken = Guid.NewGuid().ToString();
            user.MailConfirmcode = resetToken;
            user.MailConfirmdate = DateTime.UtcNow.AddDays(1);
            //建立驗證連結
            var resetLink = Url.Action("ResetPassword", "Admin", new { token = resetToken, email = user.Username }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(user.Username, "RuoliCalligraphy密碼重設信", $@"
                                                                                        <p>親愛的使用者您好，</p>
                                                                                        <p>請點擊以下連結以重設密碼：</p>
                                                                                        <p><a href=""{resetLink}"">{resetLink}</a></p>
                                                                                        <p>此連結將於 24 小時內失效。</p>");
                check = true;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Email sending timed out.");
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("ForgotPassword", "Admin", new { check });
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookie");
            return View("Login");
        }
    }
}

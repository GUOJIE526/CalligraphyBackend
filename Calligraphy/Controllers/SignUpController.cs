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
    public class SignUpController : Controller
    {
        private readonly CalligraphyContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthHelper _authHelper;
        private readonly IEmailService _emailService;
        private readonly ILogger<SignUpController> _logger;
        public SignUpController(CalligraphyContext context, IHttpContextAccessor contextAccessor, AuthHelper authHelper, IEmailService emailService, ILogger<SignUpController> logger)
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
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.DisplayName == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("Password", "帳號或密碼錯誤");
                return View(model);
            }
            if (user.MailConfirm == false)
            {
                ModelState.AddModelError("Username", "帳號尚未啟用");
                return View(model);
            }
            await _authHelper.SignInUserAsync(user, model.RememberMe);
            return RedirectToAction("Dashboard", "Home");
        }

        /// <summary>
        /// 註冊頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register() => View();

        /// <summary>
        /// 註冊頁面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (await _context.TbExhUser.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Username", "帳號已存在");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("Name", "請輸入大名");
                return View(model);
            }
            if (model.Name == "Admin" || model.Name == "Administrator" || model.Name == "admin" || model.Name == "administrator")
            {
                ModelState.AddModelError("Name", "姓名含有系統保留字");
                return View(model);
            }
            string emaiToken = Guid.NewGuid().ToString();
            var user = new TbExhUser
            {
                Email = model.Email,
                DisplayName = model.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Artist",//之後再想怎麼改成不寫死
                Creator = model.Email,
                CreateFrom = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                MailConfirmcode = emaiToken,
                MailConfirmdate = DateTime.UtcNow.AddDays(1),
            };
            _context.TbExhUser.Add(user);
            //建立驗證連結
            var confirmLink = Url.Action("ConfirmEmail", "SignUp", new { token = emaiToken, email = model.Email }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(model.Email, "RuoliCalligraphy驗證信", $@"
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
            return RedirectToAction("RegisterRemind", "SignUp", new {email = user.Email});
        }

        /// <summary>
        /// 註冊後的提醒頁面
        /// </summary>
        /// <param name="email"></param>
        /// <param name="check"></param>
        /// <returns></returns>
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
        [HttpPost]
        public async Task<IActionResult> ResendConfirmEmail(string email)
        {
            bool check = false;
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.Email == email);
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
            var confirmLink = Url.Action("ConfirmEmail", "SignUp", new { token = user.MailConfirmcode, email = user.Email }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(user.Email, "RuoliCalligraphy驗證信", $@"
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
            return RedirectToAction("RegisterRemind", "SignUp", new { email = user.Email, check });
        }

        /// <summary>
        /// 驗證信連結
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.MailConfirmcode == token && u.Email == email);
            if (user == null || user.MailConfirmdate < DateTime.UtcNow)
            {
                return View("Error");
            }
            user.MailConfirm = true;
            await _context.SaveChangesAsync();
            await _authHelper.SignInUserAsync(user, true);
            return RedirectToAction("MailConfirmWelcome", "SignUp");
        }

        /// <summary>
        /// 註冊成功後的歡迎頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult MailConfirmWelcome() => View();

        /// <summary>
        /// 忘記密碼頁面
        /// </summary>
        /// <param name="check"></param>
        /// <returns>check=true前端才會跳alert提醒</returns>
        public IActionResult ForgotPassword(bool check)
        {
            if (check)
            {
                ViewBag.Check = check;
            }
            return View();
        }

        /// <summary>
        /// 忘記密碼頁面
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            bool check = false;
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "使用者不存在");
                return View();
            }
            var resetToken = Guid.NewGuid().ToString();
            user.RestpwdToken = resetToken;
            user.RestpwdLimitdate = DateTime.UtcNow.AddDays(1);
            user.RestpwdConfirm = false;
            //建立驗證連結
            var resetLink = Url.Action("ResetPassword", "SignUp", new { token = resetToken, email = user.Email }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(user.Email, "RuoliCalligraphy密碼重設信", $@"
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
            return RedirectToAction("ForgotPassword", "SignUp", new { check });
        }

        /// <summary>
        /// 重設密碼頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetPassword(string token, string email)
        {
            bool check = false;
            var user = _context.TbExhUser
                .FirstOrDefault(u => u.RestpwdToken == token && u.Email == email);
            if (user != null)
            {
                ViewBag.Token = user.RestpwdToken;
                ViewBag.Name = user.DisplayName;
                ViewBag.Email = user.Email;
                if (user.RestpwdLimitdate < DateTime.UtcNow)
                {
                    check = true;
                    ViewBag.ErrorMessage = "連結已失效";
                    ViewBag.Check = check;
                }
                if (user.RestpwdConfirm == true)
                {
                    ViewBag.ResetConfirm = user.RestpwdConfirm;
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPwdViewModel model)
        {
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.RestpwdToken == model.Token && u.Email == model.Email);
            if (user == null || user.MailConfirmdate < DateTime.UtcNow)
            {
                ModelState.AddModelError("email", "連結已失效或使用者不存在");
                return View();
            }
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "密碼不一致");
                return View();
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.RestpwdConfirm = true;
            string token = model.Token;
            string email = model.Email;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error updating user password");
                ModelState.AddModelError("ConfirmPassword", "更新密碼失敗，請稍後再試");
                return View();
            }
            return RedirectToAction("ResetPassword", "SignUp", new { token, email });
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await _authHelper.SignOutUserAsync();
            return View("Login");
        }
    }
}

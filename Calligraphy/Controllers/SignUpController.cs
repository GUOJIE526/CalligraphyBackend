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
using System.Security.Claims;

namespace Calligraphy.Controllers
{
    public class SignUpController : Controller
    {
        private readonly CalligraphyContext _context;
        private readonly AuthHelper _authHelper;
        private readonly IEmailService _emailService;
        private readonly ILogger<SignUpController> _logger;
        private readonly IClientIpService _clientIp;
        private readonly ILogService _log;

        public SignUpController(CalligraphyContext context, AuthHelper authHelper, IEmailService emailService, ILogger<SignUpController> logger, IClientIpService clientIp, ILogService log)
        {
            _context = context;
            _authHelper = authHelper;
            _emailService = emailService;
            _logger = logger;
            _clientIp = clientIp;
            _log = log;
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
        [ValidateAntiForgeryToken]
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
            return RedirectToAction("Index", "Home");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (await _context.TbExhUser.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "帳號已存在");
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
            string emailToken = Guid.NewGuid().ToString();
            var user = new TbExhUser
            {
                Email = model.Email,
                DisplayName = model.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Artist",//之後再想怎麼改成不寫死
                MailConfirmcode = emailToken,
                MailConfirmdate = DateTime.Now.AddMinutes(10),
            };
            _context.TbExhUser.Add(user);
            //建立驗證連結
            var confirmLink = Url.Action("ConfirmEmail", "SignUp", new { token = emailToken, email = model.Email }, Request.Scheme);
            try
            {
                await _context.SaveChangesAsync();
                await _emailService.SendAsync(model.Email, "RuoliCalligraphy驗證信", $@"
                                                                                        <p>親愛的使用者您好，</p>
                                                                                        <p>請點擊以下連結以完成帳號驗證：</p>
                                                                                        <p><a href=""{confirmLink}"">{confirmLink}</a></p>
                                                                                        <p>此連結將於 10 分鐘內失效。</p>");
                await _log.LogAsync(user.UserId, "Register", $"使用者 {user.DisplayName} ({user.Email}) 註冊成功", _clientIp.GetClientIP());
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Email sending timed out.");
                await _log.LogAsync(user.UserId, "RegisterError", $"使用者 {user.DisplayName} ({user.Email}) 註冊時發生錯誤: 郵件發送超時", _clientIp.GetClientIP());
            }
            return RedirectToAction("RegisterRemind", "SignUp", new {email = user.Email});
        }

        /// <summary>
        /// 註冊後的提醒頁面
        /// </summary>
        /// <param name="email"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public IActionResult RegisterRemind(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        /// <summary>
        /// 重新寄送驗證信
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmEmail(string email)
        {
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return View("RegisterRemind");
            }
            if(user.MailConfirm == true)
            {
                return View("RegisterRemind");
            }
            user.MailConfirmcode = Guid.NewGuid().ToString();
            user.MailConfirmdate = DateTime.Now.AddMinutes(10);
            //建立驗證連結
            var confirmLink = Url.Action("ConfirmEmail", "SignUp", new { token = user.MailConfirmcode, email = user.Email }, Request.Scheme);
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                await _emailService.SendAsync(user.Email, "RuoliCalligraphy驗證信", $@"
                                                                                        <p>親愛的使用者您好，</p>
                                                                                        <p>請點擊以下連結以完成帳號驗證：</p>
                                                                                        <p><a href=""{confirmLink}"">{confirmLink}</a></p>
                                                                                        <p>此連結將於 10 分鐘內失效。</p>");
                await _log.LogAsync(user.UserId, "ResendConfirmEmail", $"使用者 {user.DisplayName} ({user.Email}) 重新寄送驗證信", _clientIp.GetClientIP());
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Email sending timed out.");
                await _log.LogAsync(user.UserId, "ResendConfirmEmailError", $"使用者 {user.DisplayName} ({user.Email}) 重新寄送驗證信時發生錯誤: 郵件發送超時", _clientIp.GetClientIP());
            }
            TempData["ResendMessage"] = true;
            return RedirectToAction("RegisterRemind", "SignUp", new { email = user.Email });
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
            if (user == null || user.MailConfirmdate < DateTime.Now)
            {
                TempData["ConfirmError"] = true;
                return RedirectToAction("Error");
            }
            user.MailConfirm = true;
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                await _authHelper.SignInUserAsync(user, true);
                return RedirectToAction("MailConfirmWelcome", "SignUp");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error confirming email");
                ModelState.AddModelError("", "驗證失敗，請稍後再試");
                await _log.LogAsync(user.UserId, "ConfirmEmailError", $"使用者 {user.DisplayName} ({user.Email}) 驗證信連結失敗: {ex.Message}", _clientIp.GetClientIP());
                return View("Error");
            }
        }

        /// <summary>
        /// 註冊成功後的歡迎頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult MailConfirmWelcome() => View();

        /// <summary>
        /// 忘記密碼頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// 忘記密碼頁面
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "使用者不存在");
                return View();
            }
            var resetToken = Guid.NewGuid().ToString();
            user.RestpwdToken = resetToken;
            user.RestpwdLimitdate = DateTime.Now.AddMinutes(10);
            user.RestpwdConfirm = false;
            //建立驗證連結
            var resetLink = Url.Action("ResetPassword", "SignUp", new { token = resetToken, email = user.Email }, Request.Scheme);
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                await _emailService.SendAsync(user.Email, "RuoliCalligraphy密碼重設信", $@"
                                                                                        <p>親愛的使用者您好，</p>
                                                                                        <p>請點擊以下連結以重設密碼：</p>
                                                                                        <p><a href=""{resetLink}"">{resetLink}</a></p>
                                                                                        <p>此連結將於 10 分鐘內失效。</p>");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Email sending timed out.");
                await _log.LogAsync(user.UserId, "ForgotPasswordError", $"使用者 {user.DisplayName} ({user.Email}) 忘記密碼時發生錯誤: 郵件發送超時", _clientIp.GetClientIP());
            }
            TempData["ForgotPassword"] = true;
            return RedirectToAction("ForgotPassword", "SignUp");
        }

        /// <summary>
        /// 重設密碼頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetPassword(string token, string email)
        {
            var user = _context.TbExhUser
                .FirstOrDefault(u => u.RestpwdToken == token && u.Email == email);
            if (user != null)
            {
                ViewBag.Token = user.RestpwdToken;
                ViewBag.Name = user.DisplayName;
                ViewBag.Email = user.Email;
                if (user.RestpwdLimitdate < DateTime.Now)
                {
                    TempData["LimitError"] = true;
                    return RedirectToAction("Error");
                }
                if (user.RestpwdConfirm == true)
                {
                    ViewBag.ResetConfirm = user.RestpwdConfirm;
                }
            }
            return View();
        }

        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPwdViewModel model)
        {
            var user = await _context.TbExhUser
                .FirstOrDefaultAsync(u => u.RestpwdToken == model.Token && u.Email == model.Email);
            if (user != null)
            {
                ViewBag.Name = user.DisplayName;
                ViewBag.Email = user.Email;
                ViewBag.Token = user.RestpwdToken;
            }
            //檢查用戶是否存在, 驗證時間是否過期
            if (user == null || user.RestpwdLimitdate < DateTime.Now)
            {
                ModelState.AddModelError("", "連結已失效或使用者不存在");
                return View("ResetPassword", model);
            }
            //檢查密碼是否一致
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "密碼不一致");
                return View("ResetPassword", model);
            }
            //檢查密碼是否為空
            if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "密碼不可為空");
                return View("ResetPassword", model);
            }
            //檢查密碼是否與之前的密碼相同
            if (BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "不能與前次密碼相同");
                return View("ResetPassword", model);
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.RestpwdConfirm = true;
            //存log紀錄
            string token = model.Token;
            string email = model.Email;
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                await _log.LogAsync(user.UserId, "ResetPassword", $"使用者 {user.DisplayName} ({user.Email}) 重設密碼", _clientIp.GetClientIP());
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error updating user password");
                ModelState.AddModelError("ConfirmPassword", "更新密碼失敗，請稍後再試");
                await _log.LogAsync(user.UserId, "ResetPasswordError", $"使用者 {user.DisplayName} ({user.Email}) 重設密碼失敗: {ex.Message}", _clientIp.GetClientIP());
                return View();
            }
            return RedirectToAction("ResetPassword", "SignUp", new { token, email });
        }

        /// <summary>
        /// 修改個人資料頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangeProfile()
        {
            var nowLoginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.TbExhUser.Find(Guid.Parse(nowLoginUserId));
            if (user == null)
            {
                TempData["ChangeProfileError"] = true;
                return RedirectToAction("Error", "SignUp");
            }
            var model = new ChangeProfileViewModel
            {
                Name = user.DisplayName
            };
            return View(model);
        }

        /// <summary>
        /// 修改個人資料頁面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeProfile(ChangeProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var nowLoginUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.TbExhUser.FindAsync(Guid.Parse(nowLoginUserId));
            if (user == null)
            {
                TempData["ChangeProfileError"] = true;
                return RedirectToAction("Error", "SignUp");
            }
            //檢查姓名是否含有系統保留字
            if (User.IsInRole("Artist"))
            {
                if (model.Name == "Admin" || model.Name == "Administrator" || model.Name == "admin" || model.Name == "administrator")
                {
                    ModelState.AddModelError("", "姓名含有系統保留字");
                    return View(model);
                }
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("", "請輸入密碼");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                ModelState.AddModelError("", "請輸入大名");
                return View(model);
            }
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "密碼不一致");
                return View(model);
            }
            //檢查新密碼是否與舊密碼相同
            if (!string.IsNullOrEmpty(model.Password) && !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            else if (!string.IsNullOrEmpty(model.Password) && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "新密碼不能與舊密碼相同");
                return View(model);
            }
            user.DisplayName = model.Name;
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                await _log.LogAsync(user.UserId, "ChangeProfile", $"使用者 {user.DisplayName} 修改個人資料", _clientIp.GetClientIP());
                TempData["ChangeProfileSuccess"] = true;
                return RedirectToAction("ChangeProfile", "SignUp");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                ModelState.AddModelError("", "更新個人資料失敗，請稍後再試");
                await _log.LogAsync(user.UserId, "ChangeProfileError", $"使用者 {user.DisplayName} 修改個人資料失敗: {ex.Message}", _clientIp.GetClientIP());
                return View(model);
            }
        }

        /// <summary>
        /// 404頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await _authHelper.SignOutUserAsync();
            //一定要返回新的Login 不然預設AspNetCore.Antiforgery的cookie不會刷新會報400錯
            return RedirectToAction("Login");
        }
    }
}

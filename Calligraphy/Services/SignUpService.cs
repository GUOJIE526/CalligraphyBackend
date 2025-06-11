using Calligraphy.Models;
using Calligraphy.Services.Interfaces;
using Calligraphy.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Calligraphy.Services
{
    public class SignUpService : ISignUpService
    {
        private readonly CalligraphyContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogService _log;
        private readonly IClientIpService _clientIp;

        public SignUpService(CalligraphyContext context, IEmailService emailService, ILogService log, IClientIpService clientIp)
        {
            _context = context;
            _emailService = emailService;
            _log = log;
            _clientIp = clientIp;
        }
        public async Task<RegisterResult> SignUpAsync(RegisterViewModel model, Func<string, string, string> confirmLink)
        {
            if (await _context.TbExhUser.AnyAsync(u => u.Email == model.Email))
            {
                return RegisterResult.Failure("此Email已被註冊過，請使用其他Email。");
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                return RegisterResult.Failure("請輸入姓名。");
            }
            if (new[] {"admin", "administrator", "Admin", "ADMIN", "Administrator"}.Contains(model.Name))
            {
                return RegisterResult.Failure("姓名不能是系統保留字。");
            }
            if (await _context.TbExhUser.AnyAsync(u => u.DisplayName == model.Name))
            {
                return RegisterResult.Failure("此姓名已被使用，請使用其他姓名。");
            }
            string emailToken = Guid.NewGuid().ToString();
            var user = new TbExhUser
            {
                Email = model.Email,
                DisplayName = model.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Artist",
                MailConfirmcode = emailToken,
                MailConfirmdate = TimeHelper.GetTaipeiTimeNow().AddMinutes(10),
            };
            _context.TbExhUser.Add(user);
            var confirmUrl = confirmLink(emailToken, model.Email);
            try
            {
                await _context.SaveChangesAsync();
                await _emailService.SendAsync(model.Email, "RuoliCalligraphy驗證信", $@"<p>親愛的使用者您好，</p>
                                                                                        <p>請點擊以下連結以完成帳號驗證：</p>
                                                                                        <p><a href=""{confirmUrl}"">{confirmUrl}</a></p>
                                                                                        <p>此連結將於 10 分鐘內失效。</p>");
                //通知我有人註冊帳號
                await _emailService.SendAsync("hungkaojay@gmail.com", "有人註冊RuoliBackend", $"<p>{model.Email} 註冊新帳號</p>");
                await _log.LogAsync(user.UserId, "Register", $"使用者 {user.DisplayName} ({user.Email}) 註冊成功", _clientIp.GetClientIP());
            }
            catch (TaskCanceledException)
            {
                await _log.LogAsync(user.UserId, "RegisterError", $"使用者 {user.DisplayName} ({user.Email}) 註冊時發生錯誤: 郵件發送超時", _clientIp.GetClientIP());
            }
            return RegisterResult.Success(user.Email);
        }
    }
}

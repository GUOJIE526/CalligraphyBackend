using Calligraphy.Models;
using Calligraphy.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Calligraphy.Services
{
    public class AuthHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientIpService _clientIp;
        private readonly ILogService _logService;
        public AuthHelper(IHttpContextAccessor httpContextAccessor, IClientIpService clientIp, ILogService logService)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientIp = clientIp;
            _logService = logService;
        }
        //封裝登入ClaimsIdentity的邏輯
        public async Task SignInUserAsync(TbExhUser exhUser, bool rememberMe)
        {
            //先檢查IP是信任
            var remoteIp = _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress?.ToString();
            if (!_clientIp.IsTrustedIP(remoteIp!))
            {
                //記錄未授權的登入嘗試
                await _logService.LogAsync(
                        exhUser.UserId,
                        "未授權登入嘗試",
                        $"IP {remoteIp} 不在信任列表中",
                        remoteIp!
                    );

                throw new UnauthorizedAccessException("IP不在信任列表中，無法登入。");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, exhUser.UserId.ToString()),
                new Claim(ClaimTypes.Name, exhUser.DisplayName),
                new Claim(ClaimTypes.Role, exhUser.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await _httpContextAccessor.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });
        }
        //登出邏輯
        public async Task SignOutUserAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

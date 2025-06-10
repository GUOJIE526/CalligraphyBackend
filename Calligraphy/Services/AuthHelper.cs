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
                ExpiresUtc = TimeHelper.GetTaipeiTimeNowOffset(DateTimeOffset.UtcNow).AddMinutes(30)
            });
        }
        //登出邏輯
        public async Task SignOutUserAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

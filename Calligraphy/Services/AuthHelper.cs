using Calligraphy.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Calligraphy.Services
{
    public class AuthHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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

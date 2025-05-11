using Calligraphy.Models;
using Microsoft.AspNetCore.Authentication;
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
                new Claim(ClaimTypes.Name, exhUser.Username),
                new Claim(ClaimTypes.Role, exhUser.Role),
            };
            var identity = new ClaimsIdentity(claims, "AdminCookie");
            var principal = new ClaimsPrincipal(identity);
            await _httpContextAccessor.HttpContext!.SignInAsync("AdminCookie", principal, new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });
        }
    }
}

using Calligraphy.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Calligraphy.Services
{
    public class AuthHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string administrator = "AdminCookie";
        private const string artist = "ArtistCookie";
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
            //如果role是artist，則使用artist cookie
            string scheme = exhUser.Role == "artist" ? artist : administrator;
            var identity = new ClaimsIdentity(claims, scheme);
            var principal = new ClaimsPrincipal(identity);
            await _httpContextAccessor.HttpContext!.SignInAsync(scheme, principal, new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });
        }
        //登出邏輯
        public async Task SignOutUserAsync()
        {
            var schemes = new List<string> { administrator, artist };
            foreach (var scheme in schemes)
            {
                await _httpContextAccessor.HttpContext!.SignOutAsync(scheme);
            }
        }
    }
}

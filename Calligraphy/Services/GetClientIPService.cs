using Calligraphy.Services.Interfaces;
using System.Net;

namespace Calligraphy.Services
{
    public class GetClientIPService : IClientIpService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        public GetClientIPService(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }
        //封裝抓取用戶IP
        public string GetClientIP()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            //讀取AppSettings.json的IPSource設定
            var configSection = _config.GetSection("IPSource");
            var enable = configSection.GetValue<bool>("Enabled");
            var trustIPs = configSection.GetValue<string>("TrustIPs")?.Split(',') ?? Array.Empty<string>();
            var ipDepth = configSection.GetValue<int>("IPDepth");

            string ipResult = remoteIp;
            if (enable && !string.IsNullOrEmpty(forwardedFor))
            {
                if(trustIPs.Contains(remoteIp))
                {
                    //分割TrustIPs裡的所有IP, 判斷TrustIPs數量是否大於0, 是的話取第{ipDeth}個IP, 否則取最後一個IP
                    var ipList = forwardedFor.Split(',', ';').Select(ip => ip.Trim()).ToList();
                    ipResult = ipList.Count > ipDepth ? ipList[ipDepth] : ipList.LastOrDefault() ?? remoteIp;

                }
            }
            return NormalizeIp(ipResult);
        }
        //檢查IP是否信任
        public bool IsTrustedIP(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var remoteIp))
            {
                return false;
            }
            var configSection = _config.GetSection("IPSource");
            var trustIPs = configSection.GetValue<string>("TrustIPs")?.Split(',') ?? Array.Empty<string>();

            foreach (var trustIp in trustIPs)
            {
                if (IPAddress.TryParse(trustIp.Trim(), out var parsedTrustIp))
                {
                    if (remoteIp.Equals(parsedTrustIp))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //IP 格式正規化
        private string NormalizeIp(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip == "Unknown")
            {
                return "";
            }
            if(IPAddress.TryParse(ip, out var parsedIp))
            {
                if(parsedIp.IsIPv4MappedToIPv6)
                    return parsedIp.MapToIPv4().ToString();
                return parsedIp.ToString();
            }
            return ip;
        }
    }
}

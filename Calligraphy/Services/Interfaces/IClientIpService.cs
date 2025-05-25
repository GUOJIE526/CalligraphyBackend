namespace Calligraphy.Services.Interfaces
{
    public interface IClientIpService
    {
        //封裝抓取用戶IP
        string GetClientIP();
        //檢查IP是否信任
        bool IsTrustedIP(string ipAddress);
    }
}

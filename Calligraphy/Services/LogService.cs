using Calligraphy.Models;
using Calligraphy.Services.Interfaces;

namespace Calligraphy.Services
{
    public class LogService : ILogService
    {
        private readonly CalligraphyContext _context;
        private readonly ILogger<LogService> _logger;
        public LogService(CalligraphyContext context, ILogger<LogService> logger)
        {
            _context = context;
            _logger = logger;
        }
        //實作Log紀錄儲存
        public async Task LogAsync(Guid userId, string action, string message, string ip)
        {
            var log = new TbExhLog
            {
                UserId = userId,
                Action = action,
                Message = message,
                IpAddress = ip,
            };
            try
            {
                _context.TbExhLog.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //處理異常，可能是資料庫連線問題或其他錯誤
                _logger.LogError(ex, "LogAsync failed for user {UserId} with action {Action}", userId, action);
                throw;
            }
        }
    }
}

using Calligraphy.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Calligraphy.Models
{
    public partial class CalligraphyContext : DbContext
    {
        private readonly IClientIpService _ip;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CalligraphyContext(DbContextOptions<CalligraphyContext> options, IClientIpService ip, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _ip = ip;
            _httpContextAccessor = httpContextAccessor;
        }

        //更新DB下Scafoold指令才不會覆蓋掉連線字串
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("CalligraphyDB"));
            }
        }
        //讓EF Core每次SaveChange幫你生成DateTimeOffset.Now
        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreateDate").CurrentValue = DateTimeOffset.Now;
                    entry.Property("Creator").CurrentValue = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                    entry.Property("CreateFrom").CurrentValue = _ip.GetClientIP() ?? "";
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("ModifyDate").CurrentValue = DateTimeOffset.Now;
                    entry.Property("Modifier").CurrentValue = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "";
                    entry.Property("ModifyFrom").CurrentValue = _ip.GetClientIP() ?? "";
                }
            }
        }
    }
}

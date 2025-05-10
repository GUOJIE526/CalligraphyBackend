using Microsoft.EntityFrameworkCore;

namespace Calligraphy.Models
{
    public partial class CalligraphyContext : DbContext
    {
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
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CREATE_DATE").CurrentValue = DateTimeOffset.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("MODIFY_DATE").CurrentValue = DateTimeOffset.Now;
                }
            }
            return base.SaveChanges();
        }
    }
}

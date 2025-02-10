using IT2163_01_231928H_JoonJunHan.Models;
using Microsoft.EntityFrameworkCore;

namespace IT2163_01_231928H_JoonJunHan.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        // Update constructor to accept IConfiguration
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if options are not already set.
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = _configuration.GetConnectionString("AppConnectionString");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
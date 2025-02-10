using IT2163_01_231928H_JoonJunHan.Data;
using IT2163_01_231928H_JoonJunHan.Models;

namespace IT2163_01_231928H_JoonJunHan.Services
{
    public class Logger
    {
        private readonly AppDbContext _context;

        public Logger(AppDbContext context)
        {
            _context = context;
        }

        public void Log(string email, string action)
        {
            if (string.IsNullOrEmpty(email))
            {
                email = "Unknown";
            }

            var logEntry = new AuditLogs
            {
                Email = email,
                Action = action,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(logEntry);
            _context.SaveChanges();
        }
    }
}

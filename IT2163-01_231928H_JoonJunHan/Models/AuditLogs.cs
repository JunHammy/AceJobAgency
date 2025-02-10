namespace IT2163_01_231928H_JoonJunHan.Models
{
    public class AuditLogs
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

namespace IT2163_01_231928H_JoonJunHan.Models
{
    public class Users
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Gender { get; set; }
        public required string NRIC { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required string ResumeFilePath { get; set; }
        public required string WhoAmI { get; set; }
        public required string SessionToken { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
    }
}

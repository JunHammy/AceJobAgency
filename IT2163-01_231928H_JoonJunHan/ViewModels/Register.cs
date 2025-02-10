using System.ComponentModel.DataAnnotations;

namespace IT2163_01_231928H_JoonJunHan.ViewModels
{
    public class Register
    {
        [Required]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; } // Male, Female, Other

        [Required]
        [RegularExpression(@"^[STFG]\d{7}[A-Z]$", ErrorMessage = "Invalid NRIC format.")]
        public string NRIC { get; set; } // Encrypt this before saving

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        //[FileExtensions(Extensions = "docx,pdf", ErrorMessage = "Only .docx or .pdf files are allowed.")]
        [AllowedExtensions(new string[] { ".docx", ".pdf" })]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = "Maximum file size is 2MB.")]
        public IFormFile Resume { get; set; } // Resume file upload

        [Required]
        [MaxLength(500, ErrorMessage = "Who Am I section cannot exceed 500 characters.")]
        public string WhoAmI { get; set; } // Allow all special characters

        // Encrypt NRIC before saving
        public static string EncryptNRIC(string nric)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(nric);
            return Convert.ToBase64String(bytes);
        }

        // Hash password before saving
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Validate password on login
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }

    // Custom attribute to enforce max file size
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;
        public MaxFileSizeAttribute(int maxSize) { _maxSize = maxSize; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null && file.Length > _maxSize)
            {
                return new ValidationResult($"Maximum allowed file size is {_maxSize / 1024 / 1024}MB.");
            }
            return ValidationResult.Success;
        }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult($"Only {string.Join(", ", _extensions)} files are allowed.");
                }
            }

            return ValidationResult.Success;
        }
    }
}

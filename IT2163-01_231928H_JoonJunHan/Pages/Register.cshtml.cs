using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using IT2163_01_231928H_JoonJunHan.Data;
using IT2163_01_231928H_JoonJunHan.Models;
using IT2163_01_231928H_JoonJunHan.ViewModels;
using IT2163_01_231928H_JoonJunHan.Services;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;

namespace IT2163_01_231928H_JoonJunHan.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly Logger _logger;
        private readonly Captcha _captcha;

        public RegisterModel(AppDbContext context, Captcha captcha)
        {
            _context = context;
            _logger = new Logger(context);
            _captcha = captcha;
        }

        [BindProperty]
        public Register Register { get; set; }

        [BindProperty]
        public string RecaptchaResponse { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate reCAPTCHA and get the result as a boolean
            var recaptchaResult = await _captcha.ValidateCaptchaAsync(RecaptchaResponse);

            // If CAPTCHA validation fails, log the error and show an appropriate message
            if (!recaptchaResult)
            {
                _logger.Log(Register.Email, "Account Registratioon CAPTCHA verification failed.");
                ErrorMessage = "Captcha validation failed. Please try again.";
                return Page();
            }

            // Check for duplicate email
            if (_context.Users.Any(u => u.Email == Register.Email))
            {
                ModelState.AddModelError("Register.Email", "Email is already registered.");
                return Page();
            }

            // Validate NRIC format (example: S1234567D)
            if (!IsValidNRIC(Register.NRIC))
            {
                ModelState.AddModelError("Register.NRIC", "Invalid NRIC format.");
                return Page();
            }

            // Validate email format
            if (!IsValidEmail(Register.Email))
            {
                ModelState.AddModelError("Register.Email", "Invalid email format.");
                return Page();
            }

            // Validate password strength
            if (!IsPasswordStrong(Register.Password))
            {
                ModelState.AddModelError("Register.Password", "Password must be at least 12 characters long, contain uppercase, lowercase, digits, and special characters.");
                return Page();
            }

            // Sanitize WhoAmI field by removing harmful special characters
            Register.WhoAmI = EncodeInput(Register.WhoAmI);

            // Handle Resume File Upload with validation
            string filePath = null;
            if (Register.Resume != null)
            {
                string fileExtension = Path.GetExtension(Register.Resume.FileName).ToLower();
                if (fileExtension != ".pdf" && fileExtension != ".docx")
                {
                    ModelState.AddModelError("Register.Resume", "Invalid file type. Only PDF and DOCX are allowed.");
                    return Page();
                }

                // Check for file size (limit to 100 MB)
                if (Register.Resume.Length > 100 * 1024 * 1024)
                {
                    ModelState.AddModelError("Register.Resume", "File size exceeds the 100 MB limit.");
                    return Page();
                }

                // Process the resume file upload
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);
                filePath = Path.Combine(uploadsFolder, Register.Resume.FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Register.Resume.CopyToAsync(fileStream);
                }
            }

            // Encrypt NRIC and hash the password
            var encryptionService = new Encryption();
            string encryptedNRIC = encryptionService.EncryptData(Register.NRIC);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Register.Password);


            // Save user data to the database
            var user = new Users
            {
                FirstName = Register.FirstName,
                LastName = Register.LastName,
                Gender = Register.Gender,
                NRIC = encryptedNRIC,
                Email = Register.Email,
                PasswordHash = hashedPassword,
                DateOfBirth = Register.DateOfBirth,
                ResumeFilePath = filePath,
                WhoAmI = Register.WhoAmI,
                SessionToken = ""
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.Log(user.Email, "Account Created");

            return RedirectToPage("/Index");
        }

        private bool IsPasswordStrong(string password)
        {
            return password.Length >= 12 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => "!@#$%^&*()_+[]{}|;:'\",.<>?/`~".Contains(ch));
        }

        private bool IsValidNRIC(string nric)
        {
            // Example NRIC validation (S1234567D)
            var regex = new Regex(@"^[STFG]\d{7}[A-Z]$");
            return regex.IsMatch(nric);
        }

        private bool IsValidEmail(string email)
        {
            // Validate email format
            var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }

        private string EncodeInput(string input)
        {
            // HTML encode the special characters in the input (WhoAmI field)
            return WebUtility.HtmlEncode(input);
        }
    }
}

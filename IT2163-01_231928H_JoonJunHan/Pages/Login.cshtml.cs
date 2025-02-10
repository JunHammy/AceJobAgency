using IT2163_01_231928H_JoonJunHan.Data;
using IT2163_01_231928H_JoonJunHan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using IT2163_01_231928H_JoonJunHan.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace IT2163_01_231928H_JoonJunHan.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly Logger _logger;
        private readonly HttpClient _httpClient;
        private readonly Captcha _captcha;

        public LoginModel(AppDbContext context, IHttpClientFactory httpClientFactory, Captcha captcha)
        {
            _context = context;
            _logger = new Logger(context);
            _httpClient = httpClientFactory.CreateClient();
            _captcha = captcha;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [BindProperty]
        public string RecaptchaResponse { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate reCAPTCHA and get the result as a boolean
            var recaptchaResult = await _captcha.ValidateCaptchaAsync(RecaptchaResponse);

            // If CAPTCHA validation fails, log the error and show an appropriate message
            if (!recaptchaResult)
            {
                _logger.Log(Email, "CAPTCHA verification failed.");
                ErrorMessage = "Captcha validation failed. Please try again.";
                return Page();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
            {
                _logger.Log(Email, "Login Failed: User not found");
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.Now)
            {
                _logger.Log(user.Email, "Login Failed: Account locked");
                ErrorMessage = "Account is locked. Try again later.";
                return Page();
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEnd = DateTime.Now.AddMinutes(1);
                    _logger.Log(user.Email, "Account Locked due to multiple failed login attempts");
                }
                else
                {
                    _logger.Log(user.Email, "Login Failed: Incorrect password");
                }

                await _context.SaveChangesAsync();
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Reset failed attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            // Generate new session token
            var sessionToken = Guid.NewGuid().ToString();
            user.SessionToken = sessionToken;
            await _context.SaveChangesAsync();

            // Store session data
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("SessionToken", sessionToken);
            HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());

            _logger.Log(user.Email, "Login Successful");

            return RedirectToPage("/Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IT2163_01_231928H_JoonJunHan.Data;
using IT2163_01_231928H_JoonJunHan.Services;

namespace IT2163_01_231928H_JoonJunHan.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly Logger _logger;

        public LogoutModel(AppDbContext context)
        {
            _context = context;
            _logger = new Logger(context);
        }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(userId))
            {
                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
                if (user != null)
                {
                    _logger.Log(user.Email, "Logout Successful");
                }
            }

            // Clear session
            HttpContext.Session.Clear();

            // Expire session cookie
            if (HttpContext.Request.Cookies.ContainsKey(".AspNetCore.Session"))
            {
                HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
            }

            return RedirectToPage("/Login"); // Redirect to login
        }
    }
}

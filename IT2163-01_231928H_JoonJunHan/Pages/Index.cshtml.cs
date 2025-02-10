using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using IT2163_01_231928H_JoonJunHan.Data;
using IT2163_01_231928H_JoonJunHan.Models;
using IT2163_01_231928H_JoonJunHan.Services;
using System.Linq;

namespace IT2163_01_231928H_JoonJunHan.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public Users CurrentUser { get; set; }

        public IActionResult OnGet()
        {
            var sessionToken = HttpContext.Session.GetString("SessionToken");
            var lastActivity = HttpContext.Session.GetString("LastActivity");
            var userId = HttpContext.Session.GetString("UserId");

            // Only check session validity if the user is logged in (session exists)
            if (!string.IsNullOrEmpty(sessionToken) && !string.IsNullOrEmpty(userId))
            {
                // Check for session timeout
                if (DateTime.TryParse(lastActivity, out DateTime lastActivityTime))
                {
                    if ((DateTime.Now - lastActivityTime).TotalMinutes > 15)
                    {
                        return RedirectToPage("/Logout");
                    }
                }

                // Fetch the user from the database
                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

                // Validate session token
                if (user == null || user.SessionToken != sessionToken)
                {
                    return RedirectToPage("/Logout");
                }

                // Update last activity timestamp
                HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());

                // Decrypt NRIC for display
                CurrentUser = user;
                var encryptionService = new Encryption();
                CurrentUser.NRIC = encryptionService.DecryptData(CurrentUser.NRIC);
            }

            // Always return the page (show homepage even if not logged in)
            return Page();
        }
    }
}

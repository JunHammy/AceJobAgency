using IT2163_01_231928H_JoonJunHan.Data;
using IT2163_01_231928H_JoonJunHan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace IT2163_01_231928H_JoonJunHan.Pages
{
    public class AuditLogModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuditLogModel(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public List<AuditLogs> Logs { get; set; } = new();

        [BindProperty]
        public string SearchQuery { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var sessionToken = HttpContext.Session.GetString("SessionToken");
            var lastActivity = HttpContext.Session.GetString("LastActivity");
            var userId = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(sessionToken) && !string.IsNullOrEmpty(userId))
            {
                if (DateTime.TryParse(lastActivity, out DateTime lastActivityTime))
                {
                    if ((DateTime.Now - lastActivityTime).TotalMinutes > 15)
                    {
                        return RedirectToPage("/Logout");
                    }
                }

                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

                if (user == null || user.SessionToken != sessionToken)
                {
                    return RedirectToPage("/Logout");
                }

                HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());

                var adminEmail = _configuration["AdminAccount:Email"];
                var loggedInUserEmail = user.Email;

                if (loggedInUserEmail != adminEmail)
                {
                    return RedirectToPage("/errors/403");
                }

                Logs = await _context.AuditLogs
                                     .OrderByDescending(log => log.Timestamp)
                                     .ToListAsync();
                return Page();
            }

            return RedirectToPage("/errors/403");
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                Logs = await _context.AuditLogs
                                     .Where(log => log.Email.Contains(SearchQuery) ||
                                                   log.Action.Contains(SearchQuery) ||
                                                   log.Timestamp.ToString().Contains(SearchQuery))
                                     .OrderByDescending(log => log.Timestamp)
                                     .ToListAsync();
            }
            else
            {
                Logs = await _context.AuditLogs
                                     .OrderByDescending(log => log.Timestamp)
                                     .ToListAsync();
            }

            return Page();
        }
    }
}
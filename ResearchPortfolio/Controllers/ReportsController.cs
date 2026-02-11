using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchPortfolio.Data;
using ResearchPortfolio.Models;
using System.Text;

namespace ResearchPortfolio.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> PublicationsByUser(string userId, int? fromYear, int? toYear)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = _userManager.GetUserId(User);
            }

            var query = _context.Publications
                .Include(p => p.AuthorPublications)
                .ThenInclude(ap => ap.User)
                .Where(p => p.AuthorPublications.Any(ap => ap.UserId == userId))
                .AsQueryable();

            if (fromYear.HasValue)
            {
                query = query.Where(p => p.Year >= fromYear.Value);
            }

            if (toYear.HasValue)
            {
                query = query.Where(p => p.Year <= toYear.Value);
            }

            var publications = await query
                .OrderByDescending(p => p.Year)
                .ToListAsync();

            ViewBag.Users = _userManager.Users.ToList();
            ViewBag.SelectedUserId = userId;
            ViewBag.FromYear = fromYear;
            ViewBag.ToYear = toYear;

            return View(publications);
        }

        [Authorize]
        public async Task<IActionResult> ExportPublicationsCsv(string userId, int? fromYear, int? toYear)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = _userManager.GetUserId(User);
            }

            var query = _context.Publications
                .Include(p => p.AuthorPublications)
                .ThenInclude(ap => ap.User)
                .Where(p => p.AuthorPublications.Any(ap => ap.UserId == userId))
                .AsQueryable();

            if (fromYear.HasValue)
                query = query.Where(p => p.Year >= fromYear.Value);

            if (toYear.HasValue)
                query = query.Where(p => p.Year <= toYear.Value);

            var publications = await query
                .OrderByDescending(p => p.Year)
                .ToListAsync();


            var sb = new StringBuilder();
            sb.AppendLine("Title,Year,Authors");

            foreach (var p in publications)
            {
                var authors = string.Join(" | ", p.AuthorPublications.Select(a => a.User.Email));

                sb.AppendLine(
                    $"\"{p.Title}\",{p.Year},\"{authors}\""
                );
            }
                
            var csv = sb.ToString();

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv))
                .ToArray();

            return File(bytes, "text/csv", "publications_report.csv");
        }
    }
}

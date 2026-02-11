using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchPortfolio.Data;
using ResearchPortfolio.Models;

namespace ResearchPortfolio.Controllers
{
    public class PublicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PublicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Publications
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var publications = await _context.Publications
               .Where(p => p.AuthorPublications
               .Any(ap => ap.UserId == userId))
               .Include(p => p.AuthorPublications)
                   .ThenInclude(ap => ap.User)
               .ToListAsync();

            return View(publications);

        }

        // GET: Publications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publication = await _context.Publications
                .Include(p => p.AuthorPublications)
                    .ThenInclude(ap => ap.User)
                .Include(p => p.References)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (publication == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            ViewBag.IsAuthor = publication.AuthorPublications
                .Any(ap => ap.UserId == currentUserId);

            return View(publication);
        }

        // GET: Publications/Create
        public IActionResult Create()
        {
            var currentUserId = _userManager.GetUserId(User);

            ViewBag.Authors = _userManager.Users
                .Where(u => u.Id != currentUserId)
                .ToList();

            return View();
        }

        // POST: Publications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publication publication, List<string> authorIds)
        {
            ModelState.Remove(nameof(Publication.AuthorPublications));
            ModelState.Remove(nameof(Publication.References));
            ModelState.Remove(nameof(Publication.CreatedByUserId));

            var currentUserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                ViewBag.Authors = _userManager.Users
                    .Where(u => u.Id != currentUserId)
                    .ToList();

                return View(publication);
            }

            publication.CreatedByUserId = currentUserId;

            _context.Publications.Add(publication);
            await _context.SaveChangesAsync();

            _context.AuthorPublications.Add(new AuthorPublication
            {
                UserId = currentUserId,
                PublicationId = publication.Id
            });

            if (authorIds != null)
            {
                foreach (var authorId in authorIds)
                {
                    _context.AuthorPublications.Add(new AuthorPublication
                    {
                        UserId = authorId,
                        PublicationId = publication.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Publications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publication = await _context.Publications.FindAsync(id);
            if (publication == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            bool isAuthor = _context.AuthorPublications
                .Any(ap => ap.PublicationId == id && ap.UserId == currentUserId);

            if (!isAuthor)
            {
                return Forbid();
            }

            ViewBag.IsCreator = publication.CreatedByUserId == currentUserId;

            ViewBag.AllAuthors = _userManager.Users
                .Where(u => u.Id != publication.CreatedByUserId)
                .ToList();

            ViewBag.SelectedAuthorIds = _context.AuthorPublications
                .Where(ap => ap.PublicationId == id && ap.UserId != publication.CreatedByUserId)
                .Select(ap => ap.UserId)
                .ToList();

            return View(publication);
        }

        // POST: Publications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publication publication, List<string> authorIds)
        {
            var dbPublication = await _context.Publications
                .Include(p => p.AuthorPublications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (dbPublication == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            bool isAuthor = dbPublication.AuthorPublications
                .Any(ap => ap.UserId == currentUserId);

            if (!isAuthor)
                return Forbid();

            ModelState.Remove(nameof(Publication.AuthorPublications));
            ModelState.Remove(nameof(Publication.References));
            ModelState.Remove(nameof(Publication.CreatedByUserId));

            if (!ModelState.IsValid)
            {
                ViewBag.IsCreator = dbPublication.CreatedByUserId == currentUserId;
                ViewBag.AllAuthors = _userManager.Users
                    .Where(u => u.Id != dbPublication.CreatedByUserId)
                    .ToList();

                ViewBag.SelectedAuthorIds = dbPublication.AuthorPublications
                    .Where(ap => ap.UserId != dbPublication.CreatedByUserId)
                    .Select(ap => ap.UserId)
                    .ToList();

                return View(dbPublication);
            }

            dbPublication.Title = publication.Title;
            dbPublication.Abstract = publication.Abstract;
            dbPublication.Year = publication.Year;

            if (dbPublication.CreatedByUserId == currentUserId)
            {
                var selectedIds = authorIds ?? new List<string>();

                var toRemove = dbPublication.AuthorPublications
                    .Where(ap => 
                        ap.UserId != dbPublication.CreatedByUserId &&
                        !selectedIds.Contains(ap.UserId))
                    .ToList();

                foreach (var ap in toRemove)
                    _context.AuthorPublications.Remove(ap);

                foreach (var userId in selectedIds)
                {
                    if (!dbPublication.AuthorPublications.Any(ap => ap.UserId == userId))
                    {
                        dbPublication.AuthorPublications.Add(new AuthorPublication
                        {
                            UserId = userId,
                            PublicationId = id
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Publications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userId = _userManager.GetUserId(User);

            bool isAuthor = _context.AuthorPublications
                .Any(ap => ap.PublicationId == id && ap.UserId == userId);

            if (!isAuthor)
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var publication = await _context.Publications
                .FirstOrDefaultAsync(m => m.Id == id);

            if (publication == null)
            {
                return NotFound();
            }

            return View(publication);
        }

        // POST: Publications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publication = await _context.Publications.FindAsync(id);
            if (publication != null)
            {
                _context.Publications.Remove(publication);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PublicationExists(int id)
        {
            return _context.Publications.Any(e => e.Id == id);
        }
    }
}

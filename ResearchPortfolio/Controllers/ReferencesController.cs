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
    public class ReferencesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReferencesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: References/Create?publicationId=5
        public async Task<IActionResult> Create(int publicationId)
        {
            var publication = await _context.Publications
                .Include(p => p.AuthorPublications)
                .FirstOrDefaultAsync(p => p.Id == publicationId);

            if (publication == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            bool isAuthor = publication.AuthorPublications
                .Any(ap => ap.UserId == currentUserId);

            if (!isAuthor)
                return Forbid();

            return View(new Reference{PublicationId = publicationId});
        }

        // POST: References/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reference reference)
        {
            var publication = await _context.Publications
                .Include(p => p.AuthorPublications)
                .FirstOrDefaultAsync(p => p.Id == reference.PublicationId);

            if (publication == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            bool isAuthor = publication.AuthorPublications
                .Any(ap => ap.UserId == currentUserId);

            if (!isAuthor)
                return Forbid();

            ModelState.Remove(nameof(Reference.Publication));

            if (!ModelState.IsValid)
                return View(reference);

            _context.References.Add(reference);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Publications",
                new { id = reference.PublicationId }
            );
        }

        // GET: References/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var reference = await _context.References
                .Include(r => r.Publication)
                    .ThenInclude(p => p.AuthorPublications)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reference == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            bool isAuthor = reference.Publication.AuthorPublications
                .Any(ap => ap.UserId == currentUserId);

            if (!isAuthor)
                return Forbid();

            return View(reference);
        }

        // POST: References/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reference reference)
        {
            var dbReference = await _context.References
                .Include(r => r.Publication)
                    .ThenInclude(p => p.AuthorPublications)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (dbReference == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            bool isAuthor = dbReference.Publication.AuthorPublications
                .Any(ap => ap.UserId == currentUserId);

            if (!isAuthor)
                return Forbid();

            ModelState.Remove("Publication");

            if (!ModelState.IsValid)
                return View(reference);

            dbReference.Title = reference.Title;
            dbReference.Source = reference.Source;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Publications",
                new { id = dbReference.PublicationId }
            );
        }

        // POST: References/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reference = await _context.References
                .Include(r => r.Publication)
                    .ThenInclude(p => p.AuthorPublications)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reference == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            bool isAuthor = reference.Publication.AuthorPublications
                .Any(ap => ap.UserId == currentUserId);

            if (!isAuthor)
                return Forbid();

            int publicationId = reference.PublicationId;

            _context.References.Remove(reference);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Publications",
                new { id = publicationId }
            );
        }
    }
}

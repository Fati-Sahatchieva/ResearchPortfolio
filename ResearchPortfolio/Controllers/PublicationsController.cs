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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publication publication, List<string> authorIds)
        {
            // 1. Взимаме ID-то на потребителя
            var currentUserId = _userManager.GetUserId(User);

            // ДЕБЪГ ПРОВЕРКА: Ако не си логнат, currentUserId ще е null. 
            // Това ще ни каже дали проблемът е в Identity системата.
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge(); // Препраща те към Login страницата
            }

            // 2. ПРИНУДИТЕЛНО ПРИСВОЯВАНЕ
            publication.CreatedByUserId = currentUserId;

            // 3. ПЪЛНО ИЗЧИСТВАНЕ НА ГРЕШКИТЕ ЗА ТОЗИ МОДЕЛ
            // Понякога ModelState.Remove не е достатъчен, ако има скрити грешки
            ModelState.Clear();

            // 4. РЪЧНА ВАЛИДАЦИЯ (за всеки случай)
            // Тъй като изчистихме всичко, проверяваме само най-важното
            if (string.IsNullOrEmpty(publication.Title))
            {
                ModelState.AddModelError("Title", "Заглавието е задължително!");
                ViewBag.Authors = _userManager.Users.Where(u => u.Id != currentUserId).ToList();
                return View(publication);
            }

            try
            {
                // 5. ЗАПИС
                _context.Add(publication);
                await _context.SaveChangesAsync();

                // 6. АВТОРСТВО
                var mainAuthor = new AuthorPublication
                {
                    UserId = currentUserId,
                    PublicationId = publication.Id
                };
                _context.AuthorPublications.Add(mainAuthor);

                if (authorIds != null)
                {
                    foreach (var authorId in authorIds)
                    {
                        if (!string.IsNullOrEmpty(authorId))
                        {
                            _context.AuthorPublications.Add(new AuthorPublication
                            {
                                UserId = authorId,
                                PublicationId = publication.Id
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ако пак гръмне, това ще ни покаже грешката в браузъра вместо стандартния екран
                ModelState.AddModelError("", "Грешка при запис: " + ex.Message);
                ViewBag.Authors = _userManager.Users.Where(u => u.Id != currentUserId).ToList();
                return View(publication);
            }
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

        // 1. Справка: Публикации на конкретен потребител
        public IActionResult MyPublications(int userId)
        {
          var myPubs = _context.Publications
          .Include(p => p.AuthorPublications) // Зареждаме и авторите
          .Where(p => p.AuthorPublications.Any(a => a.PublicationId == userId))
          .ToList();
        
          return View(myPubs);
        }

        // 2. Справка: Публикации по години
        public IActionResult ByYear(int year)
        {
          var pubsByYear = _context.Publications
          .Include(p => p.AuthorPublications)
          .Where(p => p.Year == year)
          .ToList();
        
          return View(pubsByYear);
        }
        // GET: Publications/ByYear?year=2024
       public async Task<IActionResult> ByYear(int? year)
       {
         if (year == null)
         {
           return View(new List<Publication>());
         }

         var publications = await _context.Publications
         .Include(p => p.AuthorPublications)
         .Include(p => p.References)
         .Where(p => p.Year == year)
         .ToListAsync();

         ViewBag.SelectedYear = year;
         return View(publications);
        }

        public async Task<IActionResult> MyPortfolio()
        {
           // Взимаме ID-то на текущия потребител (Identity)
           var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
  
           var myPublications = await _context.Publications
           .Include(p => p.AuthorPublications)
           .Where(p => p.CreatedByUserId == userId || p.AuthorPublications.Any(ap => ap.UserId == userId))
         .ToListAsync();

          return View(myPublications);
        }
    }
}

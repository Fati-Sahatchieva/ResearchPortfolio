using Microsoft.AspNetCore.Mvc;

namespace ResearchPortfolio.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Щом Publications работи, нека това бъде началната ни страница
            return RedirectToAction("Index", "Publications");
        }
    }
}
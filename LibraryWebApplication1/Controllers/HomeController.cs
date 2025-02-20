using LibraryWebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
namespace LibraryWebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DblibraryContext _context;
        public HomeController(ILogger<HomeController> logger, DblibraryContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            var user = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (user != null)
            {
                ViewBag.Name = user.Name;
                ViewBag.Surname = user.Surname;
            }
            else
            {
                ViewBag.Name = null;
                ViewBag.Surname = null;
            }
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

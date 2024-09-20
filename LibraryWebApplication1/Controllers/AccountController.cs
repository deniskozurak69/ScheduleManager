using Microsoft.AspNetCore.Mvc;
using LibraryWebApplication1.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace LibraryWebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly DblibraryContext _context;
        public AccountController(DblibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.ApplicationUsers.SingleOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        Username = model.Username,
                        Email = model.Email
                    };
                    int maxId = _context.ApplicationUsers.Max(c => (int?)c.Id) ?? 0;
                    newUser.Id = maxId + 1;
                    _context.ApplicationUsers.Add(newUser);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    ModelState.AddModelError("", "User with this email already exists.");
                }
            }
            return View(model);
        }
    }
}
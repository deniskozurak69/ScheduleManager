using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
namespace LibraryWebApplication1.Controllers
{
    public class UsersController : Controller
    {
        private readonly DblibraryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;        
        public UsersController(DblibraryContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            var users = await _context.Users
                .OrderBy(u => u.Priority)
                    .ToListAsync();
            return View(users);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var users = await _context.Users
                    .ToListAsync();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Name,Surname")] User user)
        {
            int maxUserId = _context.Users.Max(c => (int?)c.UserId) ?? 0;
            user.UserId = maxUserId + 1;
            user.Password = "123";
            user.IsLogged = 0;
            user.Priority = 1;
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Surname,Priority")] User user)
        {            
            if (id != user.UserId)
            {
                return NotFound();
            }
            try
            {
                _context.Update(user);                
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.UserId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
            return View(user);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            await _context.ScheduleParts.ExecuteDeleteAsync();
            var tsToDelete = _context.TeacherSubjects.Where(a => a.TeacherId == id);
            _context.TeacherSubjects.RemoveRange(tsToDelete);
            var reqsToDelete = _context.Requests.Where(a => a.TeacherId == id);
            _context.Requests.RemoveRange(reqsToDelete);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAutocompleteData(string term)
        {
            var users = await _context.Users
                .Where(u => u.Name.Contains(term))
                .Select(u => new { u.UserId, u.Name })
                .ToListAsync();
            foreach (var user in users)
            {
                Console.WriteLine($"Name: {user.Name}");
            }
            return Json(users);
        }
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
        public IActionResult Register()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Name,Surname,Password")] User user)
        {
            
            int maxUserId = _context.Users.Max(c => (int?)c.UserId) ?? 0;
            user.UserId = maxUserId + 1;
            user.IsLogged = 0;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
            return View(user);
        }

        public IActionResult Login()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string name, string surname, string password)
        {
            
            var user = _context.Users.FirstOrDefault(u => u.Name == name && u.Surname == surname && u.Password == password);
            if (user != null)
            {
                foreach (var u in _context.Users)
                {
                    u.IsLogged = 0;
                    _context.Update(u);
                }
                user.IsLogged = 1;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "Невірне ім'я, прізвище або пароль");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var user = _context.Users.FirstOrDefault(u => u.IsLogged == 1);
            if (user != null)
            {
                user.IsLogged = 0;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePriority(int UserId, int Priority)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }
            user.Priority = Priority;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}


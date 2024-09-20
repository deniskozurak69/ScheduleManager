using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
namespace LibraryWebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly DblibraryContext _context;
        public LoginController(DblibraryContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var user = _context.ApplicationUsers.SingleOrDefault(u => u.IsLogged == 1);
            if (user != null)
            {
                ViewBag.UserEmail = user.Email;
                ViewBag.Username = user.Username;
            }
            else
            {
                ViewBag.UserEmail = null;
                ViewBag.Username = null;
            }
            return View();
        }
        public IActionResult Login()
        {
            return new ChallengeResult(
                GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse", "Login")
                });
        }
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("External");
            if (!authenticateResult.Succeeded)
                return BadRequest();
            if (authenticateResult.Principal != null)
            {
                var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _context.ApplicationUsers.SingleOrDefaultAsync(u => u.Email == email);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "This email isn't attached to any account";
                        return RedirectToAction("Index");
                    }
                    var allUsers = _context.ApplicationUsers.ToList();
                    foreach (var u in allUsers)
                    {
                        u.IsLogged = 0;
                    }
                    user.IsLogged = 1;
                    _context.SaveChanges();
                    var claimsIdentity = new ClaimsIdentity("Application");
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                    await HttpContext.SignInAsync("Application", new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index", "Login");
                }
            }
            return RedirectToAction("Index", "Login");
        }
        public async Task<IActionResult> Logout()
        {
            var user = await _context.ApplicationUsers.SingleOrDefaultAsync(u => u.IsLogged == 1);
            if (user != null)
            {
                user.IsLogged = 0;
                await _context.SaveChangesAsync();
            }
            await HttpContext.SignOutAsync("Application");
            return RedirectToAction("Index", "Login");
        }
    }
}
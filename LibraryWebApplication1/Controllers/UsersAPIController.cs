using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace LibraryWebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersAPIController : ControllerBase
    {
        private readonly DblibraryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly LuceneService _luceneService;
        private readonly IMemoryCache _memoryCache;
        public UsersAPIController(DblibraryContext context, IWebHostEnvironment webHostEnvironment, LuceneService luceneService, IMemoryCache memoryCache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _luceneService = luceneService;
            _memoryCache = memoryCache;
        }

        // GET: api/UsersAPI
        [HttpGet]
        public async Task<ActionResult> GetUsers(int? skip=0, int? limit=5)
        {
            int currentSkip = skip ?? 0;
            int currentLimit = limit ?? 5;
            var totalUsers = await _context.Users.CountAsync();
            var query = _context.Users
                .Include(c => c.Articles)
                .OrderBy(c => c.UserId)
                .Select(c => new
                {
                    c.UserId,
                    c.Username,
                    c.Password,
                    Articles = c.Articles.Select(a => new
                    {
                        a.ArticleId,
                        a.ArticleName,
                        a.PublishDate,
                        a.CategoryId,
                    }).ToList()
                });
            var users = await query
                .Skip(currentSkip)
                .Take(currentLimit)
                .ToListAsync();
            string nextLink = null;
            if (currentSkip + currentLimit < totalUsers)
            {
                nextLink = Url.Action("GetUsers", new { skip = currentSkip + currentLimit, limit = currentLimit });
            }
            /*if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            var categories = await query.ToListAsync();

            return Ok(categories);*/
            var response = new
            {
                TotalCount = totalUsers,
                Users = users,
                NextLink = nextLink
            };
            return Ok(response);
        }
        /*public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Password,
                    u.ProfilePhoto,
                    Articles = u.Articles.Select(a => new
                    {
                        a.ArticleId,
                        a.ArticleName,
                        a.PublishDate,
                        a.CategoryId,
                        a.Text
                    }).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }*/

        // GET: api/UsersAPI/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Password,
                    u.ProfilePhoto,
                    Articles = u.Articles.Select(a => new
                    {
                        a.ArticleId,
                        a.ArticleName,
                        a.PublishDate,
                        a.CategoryId,
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/UsersAPI/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromForm] string username, [FromForm] string password, [FromForm] IFormFile profilePhoto)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }
            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                return BadRequest("Profile photo is required.");
            }
            if (Path.GetExtension(profilePhoto.FileName) != ".png" && Path.GetExtension(profilePhoto.FileName) != ".jpg" && Path.GetExtension(profilePhoto.FileName) != ".jpeg")
            {
                return BadRequest("File format is inappropirate");
            }
            existingUser.Username = username;
            existingUser.Password = password;
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePhoto.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);
            var localPath = Path.Combine("/uploads", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePhoto.CopyToAsync(stream);
            }
            existingUser.ProfilePhoto = localPath;
            _context.Entry(existingUser).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return NoContent();
        }

        // POST: api/UsersAPI
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromForm] string username, [FromForm] string password, [FromForm] IFormFile profilePhoto)
        {
            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                return BadRequest("Profile photo is required.");
            }
            if (Path.GetExtension(profilePhoto.FileName) != ".png" && Path.GetExtension(profilePhoto.FileName) != ".jpg" && Path.GetExtension(profilePhoto.FileName) != ".jpeg")
            {
                return BadRequest("File format is inappropirate");
            }
            var newUser = new User
            {
                Username = username,
                Password = password
            };

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profilePhoto.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var localPath = Path.Combine("/uploads", uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePhoto.CopyToAsync(stream);
            }
            newUser.ProfilePhoto = localPath;
            int maxUserId = _context.Users.Max(c => (int?)c.UserId) ?? 0;
            newUser.UserId = maxUserId + 1;
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return CreatedAtAction("GetUser", new { id = newUser.UserId }, newUser);
        }

        // DELETE: api/UsersAPI/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var articlesToDelete = _context.Articles.Where(a => a.AuthorId == user.UserId);
            _context.Articles.RemoveRange(articlesToDelete);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchUsers([FromBody] JsonElement request)
        {
            if (!request.TryGetProperty("queryText", out var queryTextElement) || queryTextElement.ValueKind != JsonValueKind.String)
            {
                return BadRequest("Invalid or missing 'queryText'.");
            }

            string queryText = queryTextElement.GetString();
            var results = await _luceneService.SearchUserAsync(queryText);
            return Ok(results);
        }


    }
}

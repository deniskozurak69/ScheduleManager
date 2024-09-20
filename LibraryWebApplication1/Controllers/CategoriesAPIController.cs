using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using LibraryWebApplication1.Controllers;
namespace LibraryWebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesAPIController : ControllerBase
    {
        private readonly DblibraryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly LuceneService _luceneService;
        private readonly IMemoryCache _memoryCache;
        public CategoriesAPIController(DblibraryContext context, IWebHostEnvironment webHostEnvironment, LuceneService luceneService, IMemoryCache memoryCache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _luceneService = luceneService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult> GetCategories(int? skip = 0, int? limit = 5)
        {
            int currentSkip = skip ?? 0;
            int currentLimit = limit ?? 5;
            var totalCategories = await _context.Categories.CountAsync();
            var categoriesQuery = _context.Categories
                .Include(c => c.Articles)
                .OrderBy(c => c.CategoryId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    Articles = c.Articles.Select(a => new
                    {
                        a.ArticleId,
                        a.ArticleName,
                        a.PublishDate,
                        a.AuthorId,
                    }).ToList()
                });
            var categories = await categoriesQuery
                .Skip(currentSkip)
                .Take(currentLimit)
                .ToListAsync();
            string nextLink = null;
            if (currentSkip + currentLimit < totalCategories) nextLink = Url.Action("GetCategories", new { skip = currentSkip + currentLimit, limit = currentLimit });
            var response = new
            {
                TotalCount = totalCategories,
                Categories = categories,
                NextLink = nextLink
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Articles)
                .Where(c => c.CategoryId == id)
                .Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    Articles = c.Articles.Select(a => new
                    {
                        a.ArticleId,
                        a.ArticleName,
                        a.PublishDate,
                        a.AuthorId,
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromForm] string name, [FromForm] IFormFile description)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            if (description == null || description.Length == 0) return BadRequest("File is required.");
            if (Path.GetExtension(description.FileName) != ".docx") return BadRequest("Only .docx files are allowed.");
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(description.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var localPath = Path.Combine("/uploads", uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await description.CopyToAsync(stream);
            }
            category.Description = localPath;
            category.Name = name;
            _context.Entry(category).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id)) return NotFound();
                else throw;
            }
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory([FromForm] string name, [FromForm] IFormFile description)
        {
            if (description == null || description.Length == 0) return BadRequest("File is required.");
            if (Path.GetExtension(description.FileName) != ".docx") return BadRequest("Only .docx files are allowed.");
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(description.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var localPath = Path.Combine("/uploads", uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await description.CopyToAsync(stream);
            }
            var category = new Category
            {
                Name = name,
                Description = localPath
            };
            int maxCategoryId = _context.Categories.Max(c => (int?)c.CategoryId) ?? 0;
            category.CategoryId = maxCategoryId + 1;
            _context.Categories.Add(category);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CategoryExists(category.CategoryId)) return Conflict();
                else throw;
            }
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            var articlesToDelete = _context.Articles.Where(a => a.CategoryId == category.CategoryId);
            _context.Articles.RemoveRange(articlesToDelete);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return NoContent();
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchCategories([FromBody] JsonElement request)
        {
            if (!request.TryGetProperty("queryText", out var queryTextElement) || queryTextElement.ValueKind != JsonValueKind.String) return BadRequest("Invalid or missing 'queryText'.");
            string queryText = queryTextElement.GetString();
            var results = await _luceneService.SearchCategoryAsync(queryText);
            return Ok(results);
        }
    }
}

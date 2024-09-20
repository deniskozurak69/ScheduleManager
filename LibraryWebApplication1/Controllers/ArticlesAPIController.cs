using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LibraryWebApplication1.Models;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryWebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesAPIController : ControllerBase
    {
        private readonly DblibraryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly LuceneService _luceneService;
        private readonly IMemoryCache _memoryCache;
        public ArticlesAPIController(DblibraryContext context, IWebHostEnvironment webHostEnvironment, LuceneService luceneService, IMemoryCache memoryCache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _luceneService = luceneService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetArticles(int? skip=0, int? limit=5)
        {
            int currentSkip = skip ?? 0;
            int currentLimit = limit ?? 5;
            var totalArticles = await _context.Articles.CountAsync();
            var query = _context.Articles
                .OrderBy(c => c.ArticleId)
                .Select(a => new
                {
                    a.ArticleId,
                    a.ArticleName,
                    a.PublishDate,
                    a.CategoryId,
                    a.AuthorId,
                });
            var articles = await query
                .Skip(currentSkip)
                .Take(currentLimit)
                .ToListAsync();
            string nextLink = null;
            if (currentSkip + currentLimit < totalArticles) nextLink = Url.Action("GetArticles", new { skip = currentSkip + currentLimit, limit = currentLimit });
            var response = new
            {
                TotalCount = totalArticles,
                Articles = articles,
                NextLink = nextLink
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetArticle(int id)
        {
            var article = await _context.Articles
                .Where(a => a.ArticleId == id)
                .Select(a => new
                {
                    a.ArticleId,
                    a.ArticleName,
                    a.PublishDate,
                    a.CategoryId,
                    a.AuthorId,
                })
                .FirstOrDefaultAsync();
            if (article == null) return NotFound();
            return Ok(article);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, [FromForm] string articleName, [FromForm] DateOnly? publishDate, [FromForm] int categoryId, [FromForm] int authorId, [FromForm] IFormFile textFile)
        {
            var existingArticle = await _context.Articles.FindAsync(id);
            if (existingArticle == null) return NotFound();
            existingArticle.ArticleName = articleName;
            existingArticle.PublishDate = publishDate;
            existingArticle.CategoryId = categoryId;
            existingArticle.AuthorId = authorId;
            if (textFile != null)
            {
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(textFile.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);
                var localPath = Path.Combine("/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await textFile.CopyToAsync(stream);
                }
                existingArticle.Text = localPath;
            }
            _context.Entry(existingArticle).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id)) return NotFound();
                else throw;
            }
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle([FromForm] string articleName, [FromForm] DateOnly? publishDate, [FromForm] int categoryId, [FromForm] int authorId, [FromForm] IFormFile textFile)
        {
            var newArticle = new Article
            {
                ArticleName = articleName,
                PublishDate = publishDate,
                CategoryId = categoryId,
                AuthorId = authorId
            };
            if (textFile == null || textFile.Length == 0) return BadRequest("File is required.");
            if (Path.GetExtension(textFile.FileName) != ".docx") return BadRequest("Only .docx files are allowed.");
            var article = new Article
            {
                ArticleName = articleName,
                PublishDate = publishDate,
                CategoryId = categoryId,
                AuthorId = authorId
            };
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(textFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var localPath = Path.Combine("/uploads", uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await textFile.CopyToAsync(stream);
            }
            article.Text = localPath;
            int maxArticleId = _context.Articles.Max(c => (int?)c.ArticleId) ?? 0;
            article.ArticleId = maxArticleId + 1;
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return CreatedAtAction("GetArticle", new { id = article.ArticleId }, article);
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return NoContent();
        }
        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchArticles([FromBody] JsonElement request)
        {
            if (!request.TryGetProperty("queryText", out var queryTextElement) || queryTextElement.ValueKind != JsonValueKind.String) return BadRequest("Invalid or missing 'queryText'.");
            string queryText = queryTextElement.GetString();
            var results = await _luceneService.SearchArticleAsync(queryText);
            return Ok(results);
        }
    }
}
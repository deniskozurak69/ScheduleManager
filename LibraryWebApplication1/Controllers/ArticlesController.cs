using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
namespace LibraryWebApplication1.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly DblibraryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _memoryCache;
        public ArticlesController(DblibraryContext context, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _memoryCache = memoryCache;
        }
        private async Task<List<Article>> GetCachedArticlesAsync()
        {
            string cacheKey = "articles";
            if (!_memoryCache.TryGetValue(cacheKey, out List<Article> articles))
            {
                articles = await _context.Articles.Include(a => a.Author)
                .Include(a => a.CategoryNavigation)
                    .ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _memoryCache.Set(cacheKey, articles, cacheOptions);
            }
            return articles;
        }
        public async Task<IActionResult> Index()
        {
            var articles = await GetCachedArticlesAsync();
            return View(articles);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var articles = await GetCachedArticlesAsync();
            var article = articles.FirstOrDefault(m => m.ArticleId == id);
            if (article == null) return NotFound();
            return View(article);
        }
        public IActionResult Create(int? categoryId)
        {
            ViewBag.Authors = new SelectList(_context.Users, "UserId", "Username");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            if (categoryId != null)
            {
                var category = _context.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
                ViewBag.CategoryId = categoryId;
                ViewBag.CategoryName = category?.Name;
                var article = new Article
                {
                    CategoryId = categoryId,
                    CategoryNavigation = category
                };
                return View(article);
            }
            else
            {
                var article = new Article();
                return View(article);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthorId,ArticleName,PublishDate,CategoryId")] Article article, IFormFile textFile)
        {
            if (textFile != null && textFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(textFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await textFile.CopyToAsync(stream);
                }
                article.Text = Path.Combine("/uploads", uniqueFileName);
            }
            else
            {
                ModelState.AddModelError("Text", "File is required.");
            }
            int maxArticleId = _context.Articles.Max(c => (int?)c.ArticleId) ?? 0;
            article.ArticleId = maxArticleId + 1;
            _context.Add(article);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return RedirectToAction(nameof(Index));
            /*ViewBag.Authors = new SelectList(_context.Users, "UserId", "Username", article.AuthorId);
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", article.CategoryId);
            return View(article);*/
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();
            ViewData["AuthorId"] = new SelectList(_context.Users, "UserId", "Username", article.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", article.CategoryId);
            ViewData["AuthorName"] = _context.Users.FirstOrDefault(u => u.UserId == article.AuthorId)?.Username;
            ViewData["CategoryName"] = _context.Categories.FirstOrDefault(c => c.CategoryId == article.CategoryId)?.Name;
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,AuthorId,ArticleName,CategoryId,PublishDate")] Article article, IFormFile textFile)
        {
            if (id != article.ArticleId) return NotFound();
            try
            {
                if (textFile != null && textFile.Length > 0)
                {
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(textFile.FileName);
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await textFile.CopyToAsync(stream);
                    }
                    article.Text = Path.Combine("/uploads", fileName);
                }
                _context.Update(article);
                await _context.SaveChangesAsync();
                _memoryCache.Remove("articles");
                _memoryCache.Remove("categories_with_articles");
                _memoryCache.Remove("users_with_articles");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(article.ArticleId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
            ViewData["AuthorId"] = new SelectList(_context.Users, "UserId", "UserId", article.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", article.CategoryId);
            return View(article);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.CategoryNavigation)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            if (article == null) return NotFound();
            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return NotFound();
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("articles");
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("users_with_articles");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAutocompleteData(string term)
        {
            var articles = await _context.Articles
                .Where(u => u.ArticleName.Contains(term))
                .Select(u => new { u.ArticleId, u.ArticleName })
                .ToListAsync();
            foreach (var article in articles)
            {
                Console.WriteLine($"Article: {article.ArticleName}");
            }
            return Json(articles);
        }
        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}


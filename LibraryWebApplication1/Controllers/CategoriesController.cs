using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
namespace LibraryWebApplication1.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DblibraryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _memoryCache;
        public CategoriesController(DblibraryContext context, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _memoryCache = memoryCache;
        }
        private async Task<List<Category>> GetCachedCategoriesAsync()
        {
            string cacheKey = "categories_with_articles";
            if (!_memoryCache.TryGetValue(cacheKey, out List<Category> categories))
            {
                categories = await _context.Categories
                    .Include(c => c.Articles) 
                    .ThenInclude(a => a.Author)
                    .ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _memoryCache.Set(cacheKey, categories, cacheOptions);
            }
            return categories;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await GetCachedCategoriesAsync();
            return View(categories);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var categories = await GetCachedCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null) return NotFound();
            return View(category);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, Microsoft.AspNetCore.Http.IFormFile descriptionFile)
        {
            if (descriptionFile != null && descriptionFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(descriptionFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await descriptionFile.CopyToAsync(stream);
                }
                int maxCategoryId = _context.Categories.Max(c => (int?)c.CategoryId) ?? 0;
                category.CategoryId = maxCategoryId + 1;
                category.Description = Path.Combine("/uploads", uniqueFileName);
                _context.Add(category);
                await _context.SaveChangesAsync();
                _memoryCache.Remove("categories_with_articles");
                _memoryCache.Remove("articles");
                _memoryCache.Remove("users_with_articles");
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description")] Category category, IFormFile uploadedFile)
        {
            if (id != category.CategoryId) return NotFound();
                try
                {
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
                        string path = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(stream);
                        }
                    category.Description = Path.Combine("/uploads", uniqueFileName);
                    _context.Update(category);
                    _memoryCache.Remove("categories_with_articles");
                    _memoryCache.Remove("articles");
                    _memoryCache.Remove("users_with_articles");
                    await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
                return View(category);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            var articlesToDelete = _context.Articles.Where(a => a.CategoryId == category.CategoryId);
            _context.Articles.RemoveRange(articlesToDelete);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _memoryCache.Remove("categories_with_articles");
            _memoryCache.Remove("articles");
            _memoryCache.Remove("users_with_articles");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAutocompleteData(string term)
        {
            var categories = await _context.Categories
                .Where(u => u.Name.Contains(term))
                .Select(u => new { u.CategoryId, u.Name })
                .ToListAsync();
            foreach (var category in categories)
            {
                Console.WriteLine($"Category: {category.Name}");
            }
            return Json(categories);
        }
        public async Task<IActionResult> RelatedArticles(int? id)
        {
            if (id == null) return NotFound();
            var articlesInCategory = await _context.Articles
                .Where(a => a.CategoryId == id)
                .ToListAsync();
            var categoryName = _context.Categories
                .Where(c => c.CategoryId == id)
                .Select(c => c.Name)
                .FirstOrDefault();
            ViewBag.CategoryName = categoryName;
            return View("RelatedArticles", articlesInCategory);
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}



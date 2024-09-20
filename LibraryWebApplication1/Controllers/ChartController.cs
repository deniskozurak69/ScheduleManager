using LibraryWebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LibraryWebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly DblibraryContext _context;
        public ChartController(DblibraryContext context)
        {
            _context = context;
        }

        [HttpGet("ArticlesByCategories")]
        public JsonResult ArticlesByCategories()
        {
            var categories = _context.Categories.ToList();
            List<object> catArticle = new List<object>();
            catArticle.Add(new[] { "Категорія", "Кількість статей" });
            int cnt;
            foreach (var c in categories)
            {
                var articles = _context.Articles.Where(a => a.CategoryId == c.CategoryId);
                cnt = articles.Count();
                catArticle.Add(new object[] { c.Name, cnt });
            }
            return new JsonResult(catArticle);
        }

        [HttpGet("ArticlesByUsers")]
        public JsonResult ArticlesByUsers()
        {
            var users = _context.Users.ToList();
            List<object> usArticle = new List<object>();
            usArticle.Add(new[] { "Користувач", "Кількість статей" });
            int cnt;
            foreach (var c in users)
            {
                var articles = _context.Articles.Where(a => a.AuthorId == c.UserId);
                cnt = articles.Count();
                usArticle.Add(new object[] { c.Username, cnt });
            }
            return new JsonResult(usArticle);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;

namespace LibraryWebApplication1.Controllers
{
    public class GroupsController : Controller
    {
        private readonly DblibraryContext _context;

        public GroupsController(DblibraryContext context)
        {
            _context = context;
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            var dblibraryContext = _context.Groups.Include(g => g.Course).Include(g => g.Specialty);
            return View(await dblibraryContext.ToListAsync());
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Course)
                .Include(g => g.Specialty)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // GET: Groups/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName");
            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "SpecialtyName");
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupId,GroupName,CourseId,SpecialtyId")] Group group)
        {
            int maxId = _context.Groups.Max(c => (int?)c.GroupId) ?? 0;
            group.GroupId = maxId + 1;
            if (ModelState.IsValid)
            {
                _context.Add(group);
                await _context.ScheduleParts.ExecuteDeleteAsync();
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", group.CourseId);
            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "SpecialtyName", group.SpecialtyId);
            return View(group);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", group.CourseId);
            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "SpecialtyName", group.SpecialtyId);
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,GroupName,CourseId,SpecialtyId")] Group group)
        {
            if (id != group.GroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.GroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", group.CourseId);
            ViewData["SpecialtyId"] = new SelectList(_context.Specialties, "SpecialtyId", "SpecialtyName", group.SpecialtyId);
            return View(group);
        }

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Course)
                .Include(g => g.Specialty)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            await _context.ScheduleParts.ExecuteDeleteAsync();
            var gsToDelete = _context.GroupSubjects.Where(a => a.GroupId == id);
            _context.GroupSubjects.RemoveRange(gsToDelete);
            if (group != null)
            {
                _context.Groups.Remove(group);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.GroupId == id);
        }
    }
}

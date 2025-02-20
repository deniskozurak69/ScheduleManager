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
    public class GroupSubjectsController : Controller
    {
        private readonly DblibraryContext _context;

        public GroupSubjectsController(DblibraryContext context)
        {
            _context = context;
        }

        // GET: GroupSubjects
        public async Task<IActionResult> Index()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            var groups = await _context.Groups.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync();
            var groupSubjects = await _context.GroupSubjects.ToListAsync();
            ViewBag.Groups = groups;
            ViewBag.Subjects = subjects;
            ViewBag.GroupSubjects = groupSubjects;
            var dblibraryContext = _context.GroupSubjects.Include(g => g.Group).Include(g => g.Subject);
            return View(await dblibraryContext.ToListAsync());
        }

        // GET: GroupSubjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupSubject = await _context.GroupSubjects
                .Include(g => g.Group)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (groupSubject == null)
            {
                return NotFound();
            }

            return View(groupSubject);
        }

        // GET: GroupSubjects/Create
        public IActionResult Create()
        {
            ViewData["GroupId"] = new SelectList(_context.Groups, "GroupId", "GroupId");
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name");
            return View();
        }

        // POST: GroupSubjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,SubjectId,GroupId")] GroupSubject groupSubject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupSubject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GroupId"] = new SelectList(_context.Groups, "GroupId", "GroupId", groupSubject.GroupId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", groupSubject.SubjectId);
            return View(groupSubject);
        }

        // GET: GroupSubjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupSubject = await _context.GroupSubjects.FindAsync(id);
            if (groupSubject == null)
            {
                return NotFound();
            }
            ViewData["GroupId"] = new SelectList(_context.Groups, "GroupId", "GroupId", groupSubject.GroupId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", groupSubject.SubjectId);
            return View(groupSubject);
        }

        // POST: GroupSubjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItemId,SubjectId,GroupId")] GroupSubject groupSubject)
        {
            if (id != groupSubject.ItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupSubject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupSubjectExists(groupSubject.ItemId))
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
            ViewData["GroupId"] = new SelectList(_context.Groups, "GroupId", "GroupId", groupSubject.GroupId);
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", groupSubject.SubjectId);
            return View(groupSubject);
        }

        // GET: GroupSubjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupSubject = await _context.GroupSubjects
                .Include(g => g.Group)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (groupSubject == null)
            {
                return NotFound();
            }

            return View(groupSubject);
        }

        // POST: GroupSubjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupSubject = await _context.GroupSubjects.FindAsync(id);
            if (groupSubject != null)
            {
                _context.GroupSubjects.Remove(groupSubject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupSubjectExists(int id)
        {
            return _context.GroupSubjects.Any(e => e.ItemId == id);
        }

        [HttpPost]
        public async Task<IActionResult> Save(List<int> selectedGroupSubjects)
        {
            await _context.ScheduleParts.ExecuteDeleteAsync();
            _context.GroupSubjects.RemoveRange(_context.GroupSubjects);
            await _context.SaveChangesAsync();
            foreach (var id in selectedGroupSubjects)
            {
                int groupId = id / 1000; 
                int subjectId = id % 1000;
                var newGroupSubject = new GroupSubject
                {
                    ItemId= groupId*1000+subjectId,
                    GroupId = groupId,
                    SubjectId = subjectId
                };
                _context.GroupSubjects.Add(newGroupSubject);
            }
            foreach(var group in _context.Groups)
            {
                var newGroupSubject = new GroupSubject
                {
                    ItemId = group.GroupId * 1000,
                    GroupId = group.GroupId,
                    SubjectId = 0
                };
                _context.GroupSubjects.Add(newGroupSubject);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

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
    public class TeacherSubjectsController : Controller
    {
        private readonly DblibraryContext _context;

        public TeacherSubjectsController(DblibraryContext context)
        {
            _context = context;
        }

        // GET: TeacherSubjects
        public async Task<IActionResult> Index()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            var teachers = await _context.Users.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync();
            var teacherSubjects = await _context.TeacherSubjects.ToListAsync();
            ViewBag.Teachers = teachers;
            ViewBag.Subjects = subjects;
            ViewBag.TeacherSubjects = teacherSubjects;
            var dblibraryContext = _context.TeacherSubjects.Include(t => t.Subject).Include(t => t.Teacher);
            return View(await dblibraryContext.ToListAsync());
        }

        // GET: TeacherSubjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherSubject = await _context.TeacherSubject
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (teacherSubject == null)
            {
                return NotFound();
            }

            return View(teacherSubject);
        }

        // GET: TeacherSubjects/Create
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Users, "UserId", "Name");
            return View();
        }

        // POST: TeacherSubjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,SubjectId,Name,TeacherId")] TeacherSubject teacherSubject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacherSubject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", teacherSubject.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Users, "UserId", "Name", teacherSubject.TeacherId);
            return View(teacherSubject);
        }

        // GET: TeacherSubjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherSubject = await _context.TeacherSubject.FindAsync(id);
            if (teacherSubject == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", teacherSubject.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Users, "UserId", "Name", teacherSubject.TeacherId);
            return View(teacherSubject);
        }

        // POST: TeacherSubjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItemId,SubjectId,Name,TeacherId")] TeacherSubject teacherSubject)
        {
            if (id != teacherSubject.ItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacherSubject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherSubjectExists(teacherSubject.ItemId))
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
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", teacherSubject.SubjectId);
            ViewData["TeacherId"] = new SelectList(_context.Users, "UserId", "Name", teacherSubject.TeacherId);
            return View(teacherSubject);
        }

        // GET: TeacherSubjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherSubject = await _context.TeacherSubject
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (teacherSubject == null)
            {
                return NotFound();
            }

            return View(teacherSubject);
        }

        // POST: TeacherSubjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacherSubject = await _context.TeacherSubject.FindAsync(id);
            if (teacherSubject != null)
            {
                _context.TeacherSubject.Remove(teacherSubject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherSubjectExists(int id)
        {
            return _context.TeacherSubject.Any(e => e.ItemId == id);
        }

        [HttpPost]
        public async Task<IActionResult> Save(List<int> selectedTeacherSubjects)
        {
            _context.ScheduleParts.RemoveRange(_context.ScheduleParts);
            _context.TeacherSubjects.RemoveRange(_context.TeacherSubjects);
            await _context.SaveChangesAsync();
            foreach (var id in selectedTeacherSubjects)
            {
                int teacherId = id / 1000;
                int subjectId = id % 1000;
                var teacher = _context.Users.SingleOrDefault(u => u.UserId == teacherId);
                var subject = _context.Subjects.SingleOrDefault(u => u.SubjectId == subjectId);
                var newTeacherSubject = new TeacherSubject
                {
                    ItemId = teacherId * 1000 + subjectId,
                    TeacherId = teacherId,
                    SubjectId = subjectId,
                    Name = (subject.Name + '(' + teacher.Surname + ')')
                };
                _context.TeacherSubjects.Add(newTeacherSubject);
            }
            await _context.SaveChangesAsync();
            var noSubject = new TeacherSubject
            {
                ItemId = 0,
                TeacherId = 0,
                SubjectId = 0,
                Name = "no lesson"
            };
            _context.TeacherSubjects.Add(noSubject);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

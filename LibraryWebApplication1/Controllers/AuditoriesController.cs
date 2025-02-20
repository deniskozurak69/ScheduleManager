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
    public class AuditoriesController : Controller
    {
        private readonly DblibraryContext _context;

        public AuditoriesController(DblibraryContext context)
        {
            _context = context;
        }

        // GET: Auditories
        public async Task<IActionResult> Index()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            return View(await _context.Auditories.ToListAsync());
        }

        // GET: Auditories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditory = await _context.Auditories
                .FirstOrDefaultAsync(m => m.AuditoryId == id);
            if (auditory == null)
            {
                return NotFound();
            }

            return View(auditory);
        }

        // GET: Auditories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Auditories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuditoryId,AuditoryName")] Auditory auditory)
        {
            int maxId = _context.Auditories.Max(c => (int?)c.AuditoryId) ?? 0;
            auditory.AuditoryId = maxId + 1;
            if (ModelState.IsValid)
            {
                _context.Add(auditory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(auditory);
        }

        // GET: Auditories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditory = await _context.Auditories.FindAsync(id);
            if (auditory == null)
            {
                return NotFound();
            }
            return View(auditory);
        }

        // POST: Auditories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AuditoryId,AuditoryName")] Auditory auditory)
        {
            if (id != auditory.AuditoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auditory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditoryExists(auditory.AuditoryId))
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
            return View(auditory);
        }

        // GET: Auditories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditory = await _context.Auditories
                .FirstOrDefaultAsync(m => m.AuditoryId == id);
            if (auditory == null)
            {
                return NotFound();
            }

            return View(auditory);
        }

        // POST: Auditories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auditory = await _context.Auditories.FindAsync(id);
            await _context.ScheduleParts.ExecuteDeleteAsync();
            if (auditory != null)
            {
                _context.Auditories.Remove(auditory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditoryExists(int id)
        {
            return _context.Auditories.Any(e => e.AuditoryId == id);
        }
    }
}

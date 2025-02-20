using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
using ClosedXML.Excel;

namespace LibraryWebApplication1.Controllers
{
    public class SchedulePartsController : Controller
    {
        private readonly DblibraryContext _context;

        public SchedulePartsController(DblibraryContext context)
        {
            _context = context;
        }

        // GET: ScheduleParts
        public async Task<IActionResult> Index(string? dayOfWeek, int? courseId, int? specialtyId, int? groupId)
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            var groups = await _context.Groups.ToListAsync();
            var specialties = await _context.Specialties.ToListAsync();
            var courses = await _context.Courses.ToListAsync();
            var lectures = await _context.Lectures.ToListAsync();            
            if (!string.IsNullOrEmpty(dayOfWeek))
            {
                lectures = lectures.Where(i => i.DayOfWeek==dayOfWeek).ToList();
            }
            if (courseId.HasValue)
            {
                groups = groups.Where(i => i.CourseId == courseId).ToList();
            }
            if (specialtyId.HasValue)
            {
                groups = groups.Where(i => i.SpecialtyId == specialtyId).ToList();
            }
            if (groupId.HasValue)
            {
                groups = groups.Where(i => i.GroupId == groupId).ToList();
            }
            ViewBag.Groups = groups;
            ViewBag.Lectures = lectures;
            ViewBag.Courses = courses;
            ViewBag.Specialties = specialties;
            var scheduleParts = await _context.ScheduleParts
                .Include(s => s.Lecture)
                .Include(s => s.TeacherSubject)
                .Include(s => s.Group)
                .Include(s => s.Auditory)
                .ToListAsync();
            var activeScheduleParts = scheduleParts.Where(sp => sp.ScheduleId == sp.ActiveSchedule).ToList();
            return View(activeScheduleParts);
        }
        

        // GET: ScheduleParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedulePart = await _context.ScheduleParts
                .Include(s => s.Lecture)
                .Include(s => s.TeacherSubject)
                .FirstOrDefaultAsync(m => m.PartId == id);
            if (schedulePart == null)
            {
                return NotFound();
            }

            return View(schedulePart);
        }

        // GET: ScheduleParts/Create
        public IActionResult Create()
        {
            ViewData["LectureId"] = new SelectList(_context.Lectures, "LectureId", "LectureId");
            ViewData["SubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name");
            return View();
        }

        // POST: ScheduleParts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PartId,DayOfWeek,LessonNumber,BeginTime,EndTime,TeacherSubjectId,ScheduleId,ActiveSchedule,LectureId")] SchedulePart schedulePart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedulePart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LectureId"] = new SelectList(_context.Lectures, "LectureId", "LectureId", schedulePart.LectureId);
            ViewData["TeacherSubjectId"] = new SelectList(_context.TeacherSubjects, "SubjectId", "Name", schedulePart.TeacherSubjectId);
            return View(schedulePart);
        }

        // GET: ScheduleParts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedulePart = await _context.ScheduleParts.FindAsync(id);
            if (schedulePart == null)
            {
                return NotFound();
            }
            ViewData["LectureId"] = new SelectList(_context.Lectures, "LectureId", "LectureId", schedulePart.LectureId);
            ViewData["TeacherSubjectId"] = new SelectList(_context.TeacherSubjects, "TeacherSubjectId", "Name", schedulePart.TeacherSubjectId);
            return View(schedulePart);
        }

        // POST: ScheduleParts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PartId,DayOfWeek,LessonNumber,BeginTime,EndTime,TeacherSubjectId,ScheduleId,ActiveSchedule,LectureId")] SchedulePart schedulePart)
        {
            if (id != schedulePart.PartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedulePart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SchedulePartExists(schedulePart.PartId))
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
            ViewData["LectureId"] = new SelectList(_context.Lectures, "LectureId", "LectureId", schedulePart.LectureId);
            ViewData["TeacherSubjectId"] = new SelectList(_context.Subjects, "SubjectId", "Name", schedulePart.TeacherSubjectId);
            return View(schedulePart);
        }

        // GET: ScheduleParts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedulePart = await _context.ScheduleParts
                .Include(s => s.Lecture)
                .Include(s => s.TeacherSubject)
                .FirstOrDefaultAsync(m => m.PartId == id);
            if (schedulePart == null)
            {
                return NotFound();
            }

            return View(schedulePart);
        }

        // POST: ScheduleParts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedulePart = await _context.ScheduleParts.FindAsync(id);
            if (schedulePart != null)
            {
                _context.ScheduleParts.Remove(schedulePart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SchedulePartExists(int id)
        {
            return _context.ScheduleParts.Any(e => e.PartId == id);
        }

        public async Task<IActionResult> DownloadSchedule()
        {
            var scheduleParts = await _context.ScheduleParts
                .Include(s => s.Lecture)
                .Include(s => s.TeacherSubject)
                .Include(s => s.Group)
                .Include(s => s.Auditory)
                .ToListAsync();

            var activeScheduleParts = scheduleParts.Where(sp => sp.ScheduleId == sp.ActiveSchedule).ToList();
            var groups = await _context.Groups.ToListAsync();
            var lectures = await _context.Lectures.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                // Загальний розклад
                var worksheet = workbook.Worksheets.Add("General Schedule");
                worksheet.Cell(1, 1).Value = "DayOfWeek";
                worksheet.Cell(1, 2).Value = "LessonNumber";
                worksheet.Cell(1, 3).Value = "BeginTime";
                worksheet.Cell(1, 4).Value = "EndTime";
                int col = 5;
                foreach (var group in groups)
                {
                    worksheet.Cell(1, col).Value = group.GroupName + " lesson";
                    worksheet.Cell(1, col + 1).Value = group.GroupName + " auditory";
                    col += 2;
                }

                int row = 2;
                foreach (var item in lectures)
                {
                    worksheet.Cell(row, 1).Value = item.DayOfWeek;
                    worksheet.Cell(row, 2).Value = item.LessonNumber;
                    worksheet.Cell(row, 3).Value = item.BeginTime?.ToString(@"hh\:mm");
                    worksheet.Cell(row, 4).Value = item.EndTime?.ToString(@"hh\:mm");
                    int col2 = 5;
                    foreach (var group in groups)
                    {
                        var schedulePart = activeScheduleParts.FirstOrDefault(s => s.GroupId == group.GroupId && s.LectureId == item.LectureId);
                        if (schedulePart != null)
                        {
                            worksheet.Cell(row, col2).Value = schedulePart.TeacherSubject?.Name;
                            worksheet.Cell(row, col2 + 1).Value = schedulePart.Auditory?.AuditoryName;
                        }
                        col2 += 2;
                    }
                    row++;
                }
                worksheet.Columns().AdjustToContents();

                // Окремі аркуші для кожної групи
                foreach (var group in groups)
                {
                    var groupSheet = workbook.Worksheets.Add(group.GroupName);
                    groupSheet.Cell(1, 1).Value = "DayOfWeek";
                    groupSheet.Cell(1, 2).Value = "LessonNumber";
                    groupSheet.Cell(1, 3).Value = "BeginTime";
                    groupSheet.Cell(1, 4).Value = "EndTime";
                    groupSheet.Cell(1, 5).Value = "Subject";
                    groupSheet.Cell(1, 6).Value = "Auditory";

                    row = 2;
                    foreach (var item in lectures)
                    {
                        var schedulePart = activeScheduleParts.FirstOrDefault(s => s.GroupId == group.GroupId && s.LectureId == item.LectureId);
                        if (schedulePart != null)
                        {
                            groupSheet.Cell(row, 1).Value = item.DayOfWeek;
                            groupSheet.Cell(row, 2).Value = item.LessonNumber;
                            groupSheet.Cell(row, 3).Value = item.BeginTime?.ToString(@"hh\:mm");
                            groupSheet.Cell(row, 4).Value = item.EndTime?.ToString(@"hh\:mm");
                            groupSheet.Cell(row, 5).Value = schedulePart.TeacherSubject?.Name;
                            groupSheet.Cell(row, 6).Value = schedulePart.Auditory?.AuditoryName;
                            row++;
                        }
                    }
                    groupSheet.Columns().AdjustToContents();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Schedule.xlsx");
                }
            }
        }

    }
}

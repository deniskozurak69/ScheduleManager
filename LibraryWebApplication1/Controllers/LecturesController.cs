using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryWebApplication1.Models;
using Google.Apis.Drive.v3.Data;
using DocumentFormat.OpenXml.InkML;

namespace LibraryWebApplication1.Controllers
{
    public class LecturesController : Controller
    {
        private readonly DblibraryContext _context;
        public static int[,] prev;
        public static int[,] prevAuditories;
        public static List<string> results;
        public static bool check;

        public LecturesController(DblibraryContext context)
        {
            _context = context;
        }

        // GET: Lectures
        public async Task<IActionResult> Index()
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            if (loggedUser != null) ViewBag.LoggedUserId = loggedUser.UserId;
            else ViewBag.LoggedUserId = -1;
            ViewBag.Subjects = await _context.Subjects.ToListAsync();
            ViewBag.ScheduleCheckResults = results;
            if(prev==null)
            {
                prev = new int[31,31];
                for (int i = 1; i <= 30; ++i)
                {
                    for (int j = 1; j <= 30; ++j) prev[i, j] = 0;
                }
            }
            if (prevAuditories == null)
            {
                prevAuditories = new int[31, 31];
                for (int i = 1; i <= 30; ++i)
                {
                    for (int j = 1; j <= 30; ++j) prevAuditories[i, j] = 0;
                }
            }
            ViewBag.Prev = prev;
            ViewBag.PrevAuditories = prevAuditories;
            var groups = await _context.Groups.ToListAsync();
            var subjs = await _context.Subjects.ToListAsync();
            var auditories = await _context.Auditories.ToListAsync();
            ViewBag.Groups = groups;
            ViewBag.Auditories = auditories;
            var groupSubjects = new Dictionary<int, List<Subject>>();
            var groupBridges = new Dictionary<int, List<TeacherSubject>>();
            foreach (var group in groups)
            {
                var subjects = await _context.GroupSubjects
                    .Where(gs => gs.GroupId == group.GroupId)
                    .Select(gs => gs.Subject)
                    .ToListAsync();
                groupSubjects[group.GroupId] = subjects;
            }
            foreach (var group in groups)
            {
                var items = await _context.TeacherSubjects
                .Where(ts => _context.GroupSubjects
                .Any(gs => gs.SubjectId == ts.SubjectId && gs.GroupId == group.GroupId))
                .OrderBy(u => u.TeacherId)
                .ToListAsync();
                groupBridges[group.GroupId] = items;
                var noLesson = _context.TeacherSubjects.SingleOrDefault(u => u.ItemId == 0);
                //groupBridges[group.GroupId].Add(noLesson);
            }
            ViewBag.GroupSubjects = groupSubjects;
            ViewBag.GroupBridges = groupBridges;
            ViewBag.Checked = check;
            return View(await _context.Lecture.ToListAsync());
        }

        // GET: Lectures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lecture
                .FirstOrDefaultAsync(m => m.LectureId == id);
            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // GET: Lectures/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Lectures/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LectureId,DayOfWeek,LessonNumber,BeginTime,EndTime")] Lecture lecture)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lecture);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lecture);
        }

        // GET: Lectures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lecture.FindAsync(id);
            if (lecture == null)
            {
                return NotFound();
            }
            return View(lecture);
        }

        // POST: Lectures/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LectureId,DayOfWeek,LessonNumber,BeginTime,EndTime")] Lecture lecture)
        {
            if (id != lecture.LectureId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lecture);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LectureExists(lecture.LectureId))
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
            return View(lecture);
        }

        // GET: Lectures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lecture
                .FirstOrDefaultAsync(m => m.LectureId == id);
            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // POST: Lectures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lecture = await _context.Lecture.FindAsync(id);
            if (lecture != null)
            {
                _context.Lecture.Remove(lecture);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LectureExists(int id)
        {
            return _context.Lecture.Any(e => e.LectureId == id);
        }
        public IActionResult Configure()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Configure(int weeks, int maxLectures, List<TimeSpan> startTimes, List<TimeSpan> endTimes)
        {
            prev = new int[31,31];
            for (int i = 1; i <= 30; ++i)
            {
                for (int j = 1; j <= 30; ++j) prev[i,j] = 0;
            }
            if (maxLectures < 1 || startTimes.Count != maxLectures || endTimes.Count != maxLectures || maxLectures>5)
            {
                ModelState.AddModelError("", "Некоректні дані. Переконайтеся, що всі поля заповнені.");
                return View();
            }

            for (int i = 0; i < maxLectures; i++)
            {
                if (startTimes[i] >= endTimes[i] || (i > 0 && startTimes[i] <= endTimes[i - 1]))
                {
                    ModelState.AddModelError("", "Некоректний час для лекції.");
                    return View();
                }
            }
            await _context.Requests.ExecuteDeleteAsync();
            await _context.ScheduleParts.ExecuteDeleteAsync();
            await _context.Lectures.ExecuteDeleteAsync();
            var daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            var lectures = new List<Lecture>();
            int currentId = 1;

            foreach (var day in daysOfWeek)
            {
                for (int i = 0; i < maxLectures; i++)
                {
                    lectures.Add(new Lecture
                    {
                        LectureId = currentId,
                        DayOfWeek = day,
                        TotalWeeks=weeks,
                        LessonNumber = i + 1,
                        BeginTime = startTimes[i],
                        EndTime = endTimes[i]
                    });
                    ++currentId;
                }
            }

            await _context.Lecture.AddRangeAsync(lectures);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Lectures");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseLectures(List<int> selectedLectures)
        {
            var loggedUser = _context.Users.SingleOrDefault(u => u.IsLogged == 1);
            int teacherId = loggedUser.UserId;

            if (teacherId < 0)
            {
                return Unauthorized();
            }
            var existingRequests = _context.Requests.Where(r => r.TeacherId == teacherId);
            _context.Requests.RemoveRange(existingRequests);
            await _context.SaveChangesAsync();
            if (selectedLectures == null || !selectedLectures.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one lecture.");
                return RedirectToAction(nameof(Index));
            }
            int maxRequestId = _context.Requests.Any() ? _context.Requests.Max(r => r.RequestId) : 0;
            var newRequests = selectedLectures.Select((lectureId, index) => new Request
            {
                RequestId = maxRequestId + index + 1,
                TeacherId = teacherId,
                LectureId = lectureId
            }).ToList();

            _context.Requests.AddRange(newRequests);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessSchedule(
    Dictionary<int, Dictionary<int, int?>> selectedSubjects,
    Dictionary<int, Dictionary<int, int?>> selectedAuditories,
    string action)
        {
            if (selectedSubjects == null || selectedSubjects.Count == 0)
            {
                return RedirectToAction("Index");
            }

            int maxScheduleId = _context.ScheduleParts.Max(c => (int?)c.ScheduleId) ?? 0;
            int maxPartId = _context.ScheduleParts.Max(c => (int?)c.PartId) ?? 0;
            results = new List<string>();
            check = false;

            if (action == "Check")
            {
                check = true;
                var tempScheduleParts = new List<SchedulePart>();
                prev = new int[31, 31];
                prevAuditories = new int[31, 31];

                foreach (var groupPair in selectedSubjects)
                {
                    int groupId = groupPair.Key;
                    foreach (var lecturePair in groupPair.Value)
                    {
                        int lectureId = lecturePair.Key;
                        int? subjectId = lecturePair.Value;
                        int? auditoryId = selectedAuditories.ContainsKey(groupId) && selectedAuditories[groupId].ContainsKey(lectureId)
                            ? selectedAuditories[groupId][lectureId]
                            : (int?)null;

                        prev[groupId, lectureId] = subjectId ?? 0;
                        prevAuditories[groupId, lectureId] = auditoryId ?? 0;

                        if (subjectId.HasValue)
                        {
                            tempScheduleParts.Add(new SchedulePart
                            {
                                PartId = lectureId,
                                DayOfWeek = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.DayOfWeek,
                                LessonNumber = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.LessonNumber,
                                BeginTime = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.BeginTime,
                                EndTime = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.EndTime,
                                LectureId = lectureId,
                                Lecture = _context.Lectures.SingleOrDefault(u => u.LectureId == lectureId),
                                TeacherSubjectId = subjectId,
                                TeacherSubject = _context.TeacherSubjects.SingleOrDefault(u => u.ItemId == subjectId),
                                ScheduleId = maxScheduleId + 1,
                                GroupId = groupId,
                                Group = _context.Groups.SingleOrDefault(u => u.GroupId == groupId),
                                AuditoryId = auditoryId,
                                Auditory = _context.Auditories.SingleOrDefault(u => u.AuditoryId == auditoryId)
                            });
                        }
                    }
                }

                var users = _context.Users.Where(t => t.UserId > 0);
                var allSubjects = await _context.Subjects.Select(s => s.SubjectId).ToHashSetAsync();
                var requests = await _context.Requests.ToListAsync();

                var lessons = tempScheduleParts.Where(t => t.TeacherSubject.SubjectId > 0);
                lessons = lessons.Where(t => t.Auditory.AuditoryId > 0);

                var teacherConflicts = lessons
                    .GroupBy(sp => new { sp.TeacherSubject.TeacherId, sp.LectureId, sp.DayOfWeek, sp.LessonNumber })
                    .Where(g => g.Count() > 1)
                    .ToList();
                foreach (var conflict in teacherConflicts)
                {
                    check = false;
                    var tch= _context.Users.SingleOrDefault(u => u.UserId == conflict.Key.TeacherId);
                    results.Add($"Error: teacher {tch.Name} {tch.Surname} has more than one lesson in {conflict.Key.DayOfWeek}, lecture number {conflict.Key.LessonNumber}.");
                }

                var roomConflicts = lessons
                .GroupBy(sp => new { sp.Auditory.AuditoryId, sp.LectureId, sp.DayOfWeek, sp.LessonNumber })
                .Where(group => group.Count() > 1).ToList();
                foreach (var conflict in roomConflicts)
                {
                    check = false;
                    var aud = _context.Auditories.SingleOrDefault(u => u.AuditoryId == conflict.Key.AuditoryId);
                    results.Add($"Error: auditory {aud.AuditoryName} has more than one lesson in {conflict.Key.DayOfWeek}, lecture number {conflict.Key.LessonNumber}.");
                }

                var missingLessons = tempScheduleParts
                .Where(sp => (sp.TeacherSubjectId == 0 && sp.AuditoryId > 0));
                foreach(var sp in missingLessons)
                {
                    check = false;
                    results.Add($"Error: set auditory for group {sp.Group.GroupName}, {sp.DayOfWeek}, lecture {sp.LessonNumber}, but didn't set a subject.");
                }

                var missingRooms = tempScheduleParts
                .Where(sp => (sp.TeacherSubjectId > 0 && sp.AuditoryId == 0));
                foreach (var sp in missingRooms)
                {
                    check = false;
                    results.Add($"Error: set subject for group {sp.Group.GroupName}, {sp.DayOfWeek}, lecture {sp.LessonNumber}, but didn't set an auditory.");
                }

                var totalWeeks = _context.Lectures.Select(l => l.TotalWeeks).FirstOrDefault();
                var validGS = _context.GroupSubjects.Where(gs => gs.SubjectId > 0).ToList();
                var insufficientLectures = validGS
                    .Select(gs => new
                    {
                        GroupId = gs.GroupId,
                        SubjectId = gs.SubjectId,
                        TotalPlannedHours = lessons
                            .Where(sp => sp.GroupId == gs.GroupId && sp.TeacherSubject.SubjectId == gs.SubjectId)
                            .Sum(sp => (sp.EndTime.Value - sp.BeginTime.Value).TotalHours) * totalWeeks,
                        RequiredHours = _context.Subjects
                            .Where(s => s.SubjectId == gs.SubjectId)
                            .Select(s => s.TotalHours)
                            .FirstOrDefault()
                    })
                    .Where(result => result.TotalPlannedHours < result.RequiredHours)
                    .ToList();
                foreach (var il in insufficientLectures)
                {
                    check = false;
                    var sbj = _context.Subjects.SingleOrDefault(u => u.SubjectId == il.SubjectId);
                    var grp = _context.Groups.SingleOrDefault(u => u.GroupId == il.GroupId);
                    results.Add($"Error: too few lectures of subject {sbj.Name} for group {grp.GroupName}");
                }

                foreach (var user in users)
                {
                    bool suits = true;
                    foreach (var schedulePart in tempScheduleParts)
                    {
                        var teachersubject = await _context.TeacherSubjects.FindAsync(schedulePart.TeacherSubjectId);
                        if (teachersubject?.TeacherId == user.UserId)
                        {
                            bool requestExists = requests.Any(r => r.TeacherId == user.UserId && r.LectureId == schedulePart.PartId);
                            if (!requestExists)
                            {
                                if(check) results.Add($"This schedule doesn't suit for user {user.Name} {user.Surname}");
                                suits = false;
                                break;
                            }
                        }
                    }
                    if (suits)
                    {
                        if (check) results.Add($"This schedule suits for user {user.Name} {user.Surname}");
                    }
                }           

                /*var subjectsInSchedule = tempScheduleParts.Select(sp => sp.TeacherSubject.SubjectId.Value).ToHashSet();
                var missingSubjects = allSubjects.Except(subjectsInSchedule).ToList();
                results.Add(missingSubjects.Any() ? "This schedule is incomplete" : "This schedule contains all subjects.");*/
            }

            if (action == "Save" || action == "Activate")
            {
                var newScheduleParts = new List<SchedulePart>();

                foreach (var groupPair in selectedSubjects)
                {
                    foreach (var lecturePair in groupPair.Value)
                    {
                        int lectureId = lecturePair.Key;
                        int? subjectId = lecturePair.Value;
                        int? auditoryId = selectedAuditories.ContainsKey(groupPair.Key) && selectedAuditories[groupPair.Key].ContainsKey(lectureId)
                            ? selectedAuditories[groupPair.Key][lectureId]
                            : (int?)null;

                        if (subjectId.HasValue)
                        {
                            newScheduleParts.Add(new SchedulePart
                            {
                                PartId = lectureId + maxPartId + 30 * groupPair.Key,
                                DayOfWeek = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.DayOfWeek,
                                LessonNumber = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.LessonNumber,
                                BeginTime = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.BeginTime,
                                EndTime = _context.Lectures.FirstOrDefault(l => l.LectureId == lectureId)?.EndTime,
                                LectureId = lectureId,
                                TeacherSubjectId = subjectId,
                                ScheduleId = maxScheduleId + 1,
                                ActiveSchedule = action == "Activate" ? maxScheduleId + 1 : (int?)null,
                                GroupId = groupPair.Key,
                                AuditoryId = auditoryId
                            });
                        }
                    }
                }

                _context.ScheduleParts.AddRange(newScheduleParts);

                if (action == "Activate")
                {
                    foreach (var item in _context.ScheduleParts)
                    {
                        item.ActiveSchedule = maxScheduleId + 1;
                    }
                }

                await _context.SaveChangesAsync();
            }

            ViewBag.ScheduleCheckResults = results;
            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class AnnouncementsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public AnnouncementsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(int? courseId)
        {
            var role = CurrentUserRole?.ToLower();
            var query = _context.Announcements.Include(a => a.Course).Include(a => a.Creator).AsQueryable();

            if (IsStudent)
            {
                var enrolled = await _context.Enrollments
                    .Where(e => e.UserId == CurrentUserId!.Value && e.Status == "active")
                    .Select(e => e.CourseId).ToListAsync();
                query = query.Where(a => enrolled.Contains(a.CourseId)
                    && (a.TargetRole == "all" || a.TargetRole == "student" || a.TargetRole == null));
            }
            else if (IsLecturer)
                query = query.Where(a => a.Course != null
                    && a.Course.Modules.Any(m => m.LecturerId == CurrentUserId));

            if (courseId.HasValue) query = query.Where(a => a.CourseId == courseId.Value);

            return View(await query.OrderByDescending(a => a.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Include(c => c.Modules)
                    .Where(c => c.Modules.Any(m => m.LecturerId == CurrentUserId)).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title");
            ViewBag.TargetRoles = new SelectList(new[] { "all", "student", "lecturer" });
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementModel ann)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (ModelState.IsValid)
            {
                ann.CreatedBy = CurrentUserId!.Value;
                ann.CreatedAt = DateTime.UtcNow;
                _context.Announcements.Add(ann);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Announcement created.";
                return RedirectToAction(nameof(Index));
            }
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Include(c => c.Modules)
                    .Where(c => c.Modules.Any(m => m.LecturerId == CurrentUserId)).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title");
            ViewBag.TargetRoles = new SelectList(new[] { "all", "student", "lecturer" });
            return View(ann);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var ann = await _context.Announcements.FindAsync(id);
            if (ann == null) return NotFound();
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Include(c => c.Modules)
                    .Where(c => c.Modules.Any(m => m.LecturerId == CurrentUserId)).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title", ann.CourseId);
            ViewBag.TargetRoles = new SelectList(new[] { "all", "student", "lecturer" }, ann.TargetRole);
            return View(ann);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementModel ann)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (id != ann.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(ann);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Announcement updated.";
                return RedirectToAction(nameof(Index));
            }
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Include(c => c.Modules)
                    .Where(c => c.Modules.Any(m => m.LecturerId == CurrentUserId)).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title", ann.CourseId);
            ViewBag.TargetRoles = new SelectList(new[] { "all", "student", "lecturer" }, ann.TargetRole);
            return View(ann);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var ann = await _context.Announcements.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);
            if (ann == null) return NotFound();
            return View(ann);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var ann = await _context.Announcements.FindAsync(id);
            if (ann != null) { _context.Announcements.Remove(ann); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Announcement deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

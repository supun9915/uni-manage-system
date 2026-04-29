using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class EnrollmentsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public EnrollmentsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(int? courseId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var query = _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Include(e => e.Batch)
                .AsQueryable();
            if (IsLecturer)
                query = query.Where(e => e.Course != null && e.Course.CreatedBy == CurrentUserId);
            if (courseId.HasValue) query = query.Where(e => e.CourseId == courseId.Value);
            ViewBag.CourseId = courseId;
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title");
            return View(await query.OrderByDescending(e => e.EnrolledAt).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin) return Forbid();
            var students = await _context.Users.Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.Name.ToLower() == "student" && u.IsActive)
                .ToListAsync();
            ViewBag.Students = new SelectList(students.Select(u => new { u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" }), "Id", "Name");
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title");
            ViewBag.Batches = new SelectList(await _context.BatchModels.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnrollmentModel enrollment)
        {
            if (!IsAdmin) return Forbid();
            if (ModelState.IsValid)
            {
                // Check max students
                var batch = await _context.BatchModels.FindAsync(enrollment.BatchId);
                if (batch?.MaxStudents.HasValue == true)
                {
                    var count = await _context.Enrollments.CountAsync(e => e.BatchId == enrollment.BatchId && e.Status == "active");
                    if (count >= batch.MaxStudents.Value)
                    {
                        ModelState.AddModelError("", "Batch has reached maximum student capacity.");
                        await PopulateCreateViewBags();
                        return View(enrollment);
                    }
                }
                // Prevent duplicate
                if (await _context.Enrollments.AnyAsync(e => e.UserId == enrollment.UserId && e.CourseId == enrollment.CourseId && e.Status == "active"))
                {
                    ModelState.AddModelError("", "Student is already enrolled in this course.");
                    await PopulateCreateViewBags();
                    return View(enrollment);
                }
                enrollment.EnrolledAt = DateTime.UtcNow;
                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student enrolled.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateCreateViewBags();
            return View(enrollment);
        }

        private async Task PopulateCreateViewBags()
        {
            var students = await _context.Users.Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.Name.ToLower() == "student" && u.IsActive)
                .ToListAsync();
            ViewBag.Students = new SelectList(students.Select(u => new { u.Id, Name = $"{u.FirstName} {u.LastName} ({u.Email})" }), "Id", "Name");
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title");
            ViewBag.Batches = new SelectList(await _context.BatchModels.ToListAsync(), "Id", "Name");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin) return Forbid();
            var e = await _context.Enrollments.Include(en => en.User).Include(en => en.Course).FirstOrDefaultAsync(en => en.Id == id);
            if (e == null) return NotFound();
            return View(e);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin) return Forbid();
            var e = await _context.Enrollments.FindAsync(id);
            if (e != null) { _context.Enrollments.Remove(e); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Enrollment removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}

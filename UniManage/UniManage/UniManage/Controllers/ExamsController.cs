using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class ExamsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public ExamsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(int? moduleId)
        {
            var query = _context.Exams
                .Include(e => e.Module).ThenInclude(m => m!.Course)
                .AsQueryable();

            if (IsStudent)
            {
                var enrolled = await _context.Enrollments
                    .Where(e => e.UserId == CurrentUserId!.Value && e.Status == "active")
                    .Select(e => e.CourseId).ToListAsync();
                query = query.Where(e => e.Module != null && enrolled.Contains(e.Module.CourseId));
            }
            else if (IsLecturer)
                query = query.Where(e => e.Module != null && e.Module.LecturerId == CurrentUserId);

            if (moduleId.HasValue) query = query.Where(e => e.ModuleId == moduleId.Value);

            return View(await query.OrderByDescending(e => e.ExamDate).ToListAsync());
        }

        public async Task<IActionResult> Create(int? moduleId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(
                modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} \u2013 {m.Title}" }),
                "Id", "Name", moduleId);
            return View(new ExamModel { ModuleId = moduleId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamModel exam)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (ModelState.IsValid)
            {
                exam.CreatedBy = CurrentUserId!.Value;
                exam.CreatedAt = DateTime.UtcNow;
                _context.Exams.Add(exam);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Exam created.";
                return RedirectToAction(nameof(Index));
            }
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(
                modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} \u2013 {m.Title}" }),
                "Id", "Name", exam.ModuleId);
            return View(exam);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(
                modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} \u2013 {m.Title}" }),
                "Id", "Name", exam.ModuleId);
            return View(exam);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamModel exam)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (id != exam.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(exam);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Exam updated.";
                return RedirectToAction(nameof(Index));
            }
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(
                modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} \u2013 {m.Title}" }),
                "Id", "Name", exam.ModuleId);
            return View(exam);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var exam = await _context.Exams
                .Include(e => e.Module)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (exam == null) return NotFound();
            return View(exam);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Exam deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

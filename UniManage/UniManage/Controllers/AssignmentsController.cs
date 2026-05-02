using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class AssignmentsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public AssignmentsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(int? moduleId)
        {
            var query = _context.Assignments
                .Include(a => a.Module).ThenInclude(m => m!.Course)
                .AsQueryable();

            if (IsStudent)
            {
                var enrolled = await _context.Enrollments
                    .Where(e => e.UserId == CurrentUserId!.Value && e.Status == "active")
                    .Select(e => e.CourseId).ToListAsync();
                query = query.Where(a => a.Module != null && enrolled.Contains(a.Module.CourseId));
            }
            else if (IsLecturer)
                query = query.Where(a => a.Module != null && a.Module.LecturerId == CurrentUserId);

            if (moduleId.HasValue) query = query.Where(a => a.ModuleId == moduleId.Value);

            ViewBag.ModuleId = moduleId;
            return View(await query.OrderByDescending(a => a.DeadlineDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Module).ThenInclude(m => m!.Course)
                .Include(a => a.Submissions).ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null) return NotFound();

            if (IsStudent)
            {
                ViewBag.MySubmission = assignment.Submissions
                    .FirstOrDefault(s => s.StudentId == CurrentUserId);
            }
            return View(assignment);
        }

        public async Task<IActionResult> Create(int? moduleId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} � {m.Title}" }), "Id", "Name", moduleId);
            return View(new AssignmentModel { ModuleId = moduleId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Create(AssignmentModel assignment, IFormFile? file)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();

            // Clear programmatically assigned fields from ModelState
            ModelState.Remove(nameof(AssignmentModel.FileData));
            ModelState.Remove(nameof(AssignmentModel.FileName));
            ModelState.Remove(nameof(AssignmentModel.FileSize));
            ModelState.Remove(nameof(AssignmentModel.ContentType));
            ModelState.Remove(nameof(AssignmentModel.CreatedBy));
            ModelState.Remove(nameof(AssignmentModel.CreatedAt));

            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                assignment.FileData    = ms.ToArray();
                assignment.FileName    = Path.GetFileName(file.FileName);
                assignment.FileSize    = (int)(file.Length / 1024);
                assignment.ContentType = file.ContentType;
            }
            else if (file != null && file.Length == 0)
            {
                ModelState.AddModelError("file", "The uploaded file is empty. Please upload a valid document.");
            }
            if (ModelState.IsValid)
            {
                assignment.CreatedBy = CurrentUserId!.Value;
                assignment.CreatedAt = DateTime.UtcNow;
                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Assignment created.";
                return RedirectToAction(nameof(Index), new { moduleId = assignment.ModuleId });
            }
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} � {m.Title}" }), "Id", "Name", assignment.ModuleId);
            return View(assignment);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} � {m.Title}" }), "Id", "Name", assignment.ModuleId);
            return View(assignment);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Edit(int id, AssignmentModel assignment, IFormFile? file)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (id != assignment.Id) return NotFound();

            // Clear programmatically assigned fields from ModelState
            ModelState.Remove(nameof(AssignmentModel.FileData));
            ModelState.Remove(nameof(AssignmentModel.FileName));
            ModelState.Remove(nameof(AssignmentModel.FileSize));
            ModelState.Remove(nameof(AssignmentModel.ContentType));
            ModelState.Remove(nameof(AssignmentModel.CreatedBy));
            ModelState.Remove(nameof(AssignmentModel.CreatedAt));

            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                assignment.FileData    = ms.ToArray();
                assignment.FileName    = Path.GetFileName(file.FileName);
                assignment.FileSize    = (int)(file.Length / 1024);
                assignment.ContentType = file.ContentType;
            }
            else
            {
                var existing = await _context.Assignments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                assignment.FileData    = existing?.FileData;
                assignment.FileName    = existing?.FileName;
                assignment.FileSize    = existing?.FileSize;
                assignment.ContentType = existing?.ContentType;
            }
            if (ModelState.IsValid)
            {
                _context.Update(assignment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Assignment updated.";
                return RedirectToAction(nameof(Index), new { moduleId = assignment.ModuleId });
            }
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} � {m.Title}" }), "Id", "Name", assignment.ModuleId);
            return View(assignment);
        }

        public async Task<IActionResult> Download(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null || assignment.FileData == null) return NotFound();
            return File(assignment.FileData, assignment.ContentType ?? "application/octet-stream",
                        assignment.FileName ?? "assignment");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var a = await _context.Assignments.Include(a => a.Module).FirstOrDefaultAsync(a => a.Id == id);
            if (a == null) return NotFound();
            return View(a);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var a = await _context.Assignments.FindAsync(id);
            int? mId = a?.ModuleId;
            if (a != null) { _context.Assignments.Remove(a); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Assignment deleted.";
            return RedirectToAction(nameof(Index), new { moduleId = mId });
        }
    }
}

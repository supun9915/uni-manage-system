using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class AssignmentSubmissionsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AssignmentSubmissionsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Lecturer/Admin: view all submissions for an assignment
        public async Task<IActionResult> Index(int assignmentId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var assignment = await _context.Assignments
                .Include(a => a.Module).ThenInclude(m => m!.Course)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
            if (assignment == null) return NotFound();
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
            ViewBag.Assignment = assignment;
            return View(submissions);
        }

        // Student: submit form
        [HttpGet]
        public async Task<IActionResult> Submit(int assignmentId)
        {
            if (!IsStudent) return Forbid();
            var assignment = await _context.Assignments
                .Include(a => a.Module)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
            if (assignment == null) return NotFound();
            var existing = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == CurrentUserId);
            ViewBag.Assignment = assignment;
            ViewBag.Existing = existing;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int assignmentId, IFormFile? file)
        {
            if (!IsStudent) return Forbid();
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return NotFound();

            var existing = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == CurrentUserId);

            string? fileUrl = existing?.FileUrl;
            string? fileName = existing?.FileName;
            int? fileSize = existing?.FileSize;

            if (file != null && file.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "uploads", "submissions");
                Directory.CreateDirectory(dir);
                var fn = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                using (var fs = new FileStream(Path.Combine(dir, fn), FileMode.Create))
                    await file.CopyToAsync(fs);
                fileUrl = $"/uploads/submissions/{fn}";
                fileName = file.FileName;
                fileSize = (int)(file.Length / 1024);
            }

            bool isLate = assignment.DeadlineDate.HasValue && DateTime.UtcNow > assignment.DeadlineDate.Value;

            if (existing != null)
            {
                existing.FileUrl = fileUrl;
                existing.FileName = fileName;
                existing.FileSize = fileSize;
                existing.SubmittedAt = DateTime.UtcNow;
                existing.Status = isLate ? "late" : "submitted";
            }
            else
            {
                _context.AssignmentSubmissions.Add(new AssignmentSubmissionModel
                {
                    AssignmentId = assignmentId,
                    StudentId = CurrentUserId!.Value,
                    FileUrl = fileUrl,
                    FileName = fileName,
                    FileSize = fileSize,
                    SubmittedAt = DateTime.UtcNow,
                    Status = isLate ? "late" : "submitted"
                });
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assignment submitted.";
            return RedirectToAction("Details", "Assignments", new { id = assignmentId });
        }

        // Lecturer: grade a submission
        [HttpGet]
        public async Task<IActionResult> Grade(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment).ThenInclude(a => a!.Module)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (submission == null) return NotFound();
            return View(submission);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int id, int marksObtained, string? feedback)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var submission = await _context.AssignmentSubmissions.FindAsync(id);
            if (submission == null) return NotFound();
            submission.MarksObtained = marksObtained;
            submission.Feedback = feedback;
            submission.Status = "graded";
            submission.GradedBy = CurrentUserId!.Value;
            submission.GradedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Submission graded.";
            return RedirectToAction(nameof(Index), new { assignmentId = submission.AssignmentId });
        }

        // Student: my grades
        public async Task<IActionResult> MyGrades()
        {
            if (!IsStudent) return Forbid();
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment).ThenInclude(a => a!.Module).ThenInclude(m => m!.Course)
                .Where(s => s.StudentId == CurrentUserId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
            return View(submissions);
        }
    }
}

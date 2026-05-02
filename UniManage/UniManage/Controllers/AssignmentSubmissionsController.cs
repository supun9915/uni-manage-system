using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class AssignmentSubmissionsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AssignmentSubmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lecturer/Admin: view all submissions (optionally filtered by assignment)
        public async Task<IActionResult> Index(int? assignmentId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();

            // If a specific assignment is requested, show only that assignment's submissions
            if (assignmentId.HasValue && assignmentId.Value > 0)
            {
                var assignment = await _context.Assignments
                    .Include(a => a.Module).ThenInclude(m => m!.Course)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId.Value);
                if (assignment == null) return NotFound();

                var submissions = await _context.AssignmentSubmissions
                    .Include(s => s.Student)
                    .Where(s => s.AssignmentId == assignmentId.Value)
                    .OrderByDescending(s => s.SubmittedAt)
                    .ToListAsync();

                ViewBag.Assignment = assignment;
                ViewBag.Assignments = await GetAccessibleAssignmentsAsync();
                return View(submissions);
            }

            // No specific assignment — show all submissions across accessible modules
            var allSubmissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment).ThenInclude(a => a!.Module).ThenInclude(m => m!.Course)
                .Where(s => IsAdmin
                    ? true
                    : s.Assignment != null && s.Assignment.Module != null
                      && s.Assignment.Module.LecturerId == CurrentUserId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            ViewBag.Assignment = null;
            ViewBag.Assignments = await GetAccessibleAssignmentsAsync();
            return View(allSubmissions);
        }

        private async Task<List<AssignmentModel>> GetAccessibleAssignmentsAsync()
        {
            var query = _context.Assignments
                .Include(a => a.Module).ThenInclude(m => m!.Course)
                .AsQueryable();

            if (IsLecturer)
                query = query.Where(a => a.Module != null && a.Module.LecturerId == CurrentUserId);

            return await query.OrderBy(a => a.Title).ToListAsync();
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
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Submit(int assignmentId, IFormFile? file)
        {
            if (!IsStudent) return Forbid();
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return NotFound();

            var existing = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == CurrentUserId);

            string? fileName = existing?.FileName;
            int? fileSize = existing?.FileSize;
            string? contentType = existing?.ContentType;
            byte[]? fileData = existing?.FileData;

            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                fileData    = ms.ToArray();
                fileName    = file.FileName;
                fileSize    = (int)(file.Length / 1024);
                contentType = file.ContentType;
            }
            else if (file != null && file.Length == 0)
            {
                TempData["Error"] = "The uploaded file is empty. Please upload a valid Word document or file.";
                ViewBag.Assignment = assignment;
                ViewBag.Existing = existing;
                return View();
            }

            bool isLate = assignment.DeadlineDate.HasValue && DateTime.UtcNow > assignment.DeadlineDate.Value;

            if (existing != null)
            {
                existing.FileData    = fileData;
                existing.FileName    = fileName;
                existing.FileSize    = fileSize;
                existing.ContentType = contentType;
                existing.SubmittedAt = DateTime.UtcNow;
                existing.Status = isLate ? "late" : "submitted";
            }
            else
            {
                _context.AssignmentSubmissions.Add(new AssignmentSubmissionModel
                {
                    AssignmentId = assignmentId,
                    StudentId    = CurrentUserId!.Value,
                    FileData     = fileData,
                    FileName     = fileName,
                    FileSize     = fileSize,
                    ContentType  = contentType,
                    SubmittedAt  = DateTime.UtcNow,
                    Status       = isLate ? "late" : "submitted"
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

        public async Task<IActionResult> Download(int id)
        {
            var submission = await _context.AssignmentSubmissions.FindAsync(id);
            if (submission == null || submission.FileData == null) return NotFound();
            return File(submission.FileData, submission.ContentType ?? "application/octet-stream",
                        submission.FileName ?? "submission");
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

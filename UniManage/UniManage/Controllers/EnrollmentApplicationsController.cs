using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class EnrollmentApplicationsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ??????????????????????????????????????????????????????????
        // Student: browse published courses and see which are available to apply for
        // ??????????????????????????????????????????????????????????
        public async Task<IActionResult> Browse()
        {
            if (!IsStudent) return Forbid();

            var courses = await _context.Courses
                .Include(c => c.Department)
                .Where(c => c.Status == "published")
                .OrderBy(c => c.Title)
                .ToListAsync();

            // Courses the student is already enrolled in (active)
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserId == CurrentUserId && e.Status == "active")
                .Select(e => e.CourseId)
                .ToListAsync();

            // Pending or approved applications
            var appliedCourseIds = await _context.EnrollmentApplications
                .Where(a => a.StudentId == CurrentUserId && (a.Status == "pending" || a.Status == "approved"))
                .Select(a => a.CourseId)
                .ToListAsync();

            ViewBag.EnrolledCourseIds = enrolledCourseIds;
            ViewBag.AppliedCourseIds = appliedCourseIds;
            return View(courses);
        }

        // ??????????????????????????????????????????????????????????
        // Student: application form
        // ??????????????????????????????????????????????????????????
        [HttpGet]
        public async Task<IActionResult> Apply(int courseId)
        {
            if (!IsStudent) return Forbid();

            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Status == "published");

            if (course == null) return NotFound();

            // Already enrolled?
            if (await _context.Enrollments.AnyAsync(e => e.UserId == CurrentUserId && e.CourseId == courseId && e.Status == "active"))
            {
                TempData["Error"] = "You are already enrolled in this course.";
                return RedirectToAction(nameof(Browse));
            }

            // Already has a pending/approved application?
            var existing = await _context.EnrollmentApplications
                .FirstOrDefaultAsync(a => a.StudentId == CurrentUserId && a.CourseId == courseId && (a.Status == "pending" || a.Status == "approved"));

            if (existing != null)
            {
                TempData["Error"] = "You already have a pending or approved application for this course.";
                return RedirectToAction(nameof(Browse));
            }

            ViewBag.Course = course;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Apply(int courseId, string documentType, string? studentNote, IFormFile? documentFile)
        {
            if (!IsStudent) return Forbid();

            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Status == "published");

            if (course == null) return NotFound();

            if (string.IsNullOrWhiteSpace(documentType))
            {
                ModelState.AddModelError("documentType", "Document type is required.");
                ViewBag.Course = course;
                return View();
            }

            byte[]? fileData = null;
            string? fileName = null;
            string? contentType = null;

            if (documentFile != null && documentFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await documentFile.CopyToAsync(ms);
                fileData = ms.ToArray();
                fileName = documentFile.FileName;
                contentType = documentFile.ContentType;
            }
            else
            {
                ModelState.AddModelError("documentFile", "Please upload your prerequisite document.");
                ViewBag.Course = course;
                return View();
            }

            var application = new EnrollmentApplicationModel
            {
                StudentId = CurrentUserId!.Value,
                CourseId = courseId,
                DocumentType = documentType,
                StudentNote = studentNote,
                DocumentData = fileData,
                DocumentFileName = fileName,
                DocumentContentType = contentType,
                Status = "pending",
                AppliedAt = DateTime.UtcNow
            };

            _context.EnrollmentApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application has been submitted. Please wait for admin approval.";
            return RedirectToAction(nameof(MyApplications));
        }

        // ??????????????????????????????????????????????????????????
        // Student: see their own applications
        // ??????????????????????????????????????????????????????????
        public async Task<IActionResult> MyApplications()
        {
            if (!IsStudent) return Forbid();

            var apps = await _context.EnrollmentApplications
                .Include(a => a.Course).ThenInclude(c => c!.Department)
                .Where(a => a.StudentId == CurrentUserId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(apps);
        }

        // ??????????????????????????????????????????????????????????
        // Admin/Lecturer: view all applications
        // ??????????????????????????????????????????????????????????
        public async Task<IActionResult> Index(string? status)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();

            var query = _context.EnrollmentApplications
                .Include(a => a.Student)
                .Include(a => a.Course)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            if (IsLecturer)
                query = query.Where(a => a.Course != null && a.Course.CreatedBy == CurrentUserId);

            ViewBag.StatusFilter = status;
            return View(await query.OrderByDescending(a => a.AppliedAt).ToListAsync());
        }

        // ??????????????????????????????????????????????????????????
        // Admin: review a single application
        // ??????????????????????????????????????????????????????????
        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();

            var app = await _context.EnrollmentApplications
                .Include(a => a.Student)
                .Include(a => a.Course).ThenInclude(c => c!.Department)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();
            ViewBag.Batches = await _context.BatchModels.Where(b => b.CourseId == app.CourseId && b.Status == "active").ToListAsync();
            return View(app);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, string decision, string? adminNote, int? batchId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();

            var app = await _context.EnrollmentApplications
                .Include(a => a.Student)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();

            app.Status = decision == "approve" ? "approved" : "rejected";
            app.AdminNote = adminNote;
            app.ReviewedAt = DateTime.UtcNow;
            app.ReviewedBy = CurrentUserId;

            // If approved: create an Enrollment
            if (decision == "approve")
            {
                // Batch must be provided
                if (batchId == null)
                {
                    var loadedApp = await _context.EnrollmentApplications
                        .Include(a => a.Student)
                        .Include(a => a.Course).ThenInclude(c => c!.Department)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    ModelState.AddModelError("batchId", "Please select a batch when approving.");
                    ViewBag.Batches = new SelectList(await _context.BatchModels.Where(b => b.CourseId == app.CourseId).ToListAsync(), "Id", "Name");
                    return View(loadedApp);
                }

                // Check batch capacity
                var batch = await _context.BatchModels.FindAsync(batchId);
                if (batch?.MaxStudents.HasValue == true)
                {
                    var count = await _context.Enrollments.CountAsync(e => e.BatchId == batchId && e.Status == "active");
                    if (count >= batch.MaxStudents.Value)
                    {
                        TempData["Error"] = "The selected batch is full.";
                        return RedirectToAction(nameof(Review), new { id });
                    }
                }

                // Prevent duplicate enrollment
                bool alreadyEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.UserId == app.StudentId && e.CourseId == app.CourseId && e.Status == "active");

                if (!alreadyEnrolled)
                {
                    _context.Enrollments.Add(new EnrollmentModel
                    {
                        UserId = app.StudentId,
                        CourseId = app.CourseId,
                        BatchId = batchId.Value,
                        Status = "active",
                        EnrolledAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = decision == "approve" ? "Application approved and student enrolled." : "Application rejected.";
            return RedirectToAction(nameof(Index));
        }

        // ??????????????????????????????????????????????????????????
        // Download document
        // ??????????????????????????????????????????????????????????
        public async Task<IActionResult> DownloadDocument(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();

            var app = await _context.EnrollmentApplications.FindAsync(id);
            if (app == null || app.DocumentData == null) return NotFound();

            return File(app.DocumentData, app.DocumentContentType ?? "application/octet-stream", app.DocumentFileName ?? "document");
        }
    }
}

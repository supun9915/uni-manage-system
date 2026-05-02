using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;

namespace UniManage.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var role = CurrentUserRole?.ToLower();
            int uid = CurrentUserId!.Value;

            if (role == "student")
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Course).ThenInclude(c => c!.Modules)
                    .Include(e => e.Batch)
                    .Where(e => e.UserId == uid && e.Status == "active")
                    .ToListAsync();

                var courseIds = enrollments.Select(e => e.CourseId).ToList();

                var assignments = await _context.Assignments
                    .Include(a => a.Module)
                    .Where(a => a.Module != null && courseIds.Contains(a.Module.CourseId))
                    .OrderBy(a => a.DeadlineDate)
                    .Take(5)
                    .ToListAsync();

                var submissions = await _context.AssignmentSubmissions
                    .Include(s => s.Assignment)
                    .Where(s => s.StudentId == uid)
                    .ToListAsync();

                var announcements = await _context.Announcements
                    .Include(a => a.Course)
                    .Where(a => courseIds.Contains(a.CourseId) &&
                                (a.TargetRole == "all" || a.TargetRole == "student" || a.TargetRole == null))
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                ViewBag.Enrollments = enrollments;
                ViewBag.Assignments = assignments;
                ViewBag.Submissions = submissions;
                ViewBag.Announcements = announcements;
                return View("Student");
            }
            else if (role == "lecturer")
            {
                // Modules directly assigned to this lecturer
                var assignedModuleIds = await _context.Modules
                    .Where(m => m.LecturerId == uid)
                    .Select(m => m.Id)
                    .ToListAsync();

                // Course IDs: only from assigned modules
                var assignedCourseIds = await _context.Modules
                    .Where(m => m.LecturerId == uid)
                    .Select(m => m.CourseId)
                    .Distinct()
                    .ToListAsync();

                var courses = await _context.Courses
                    .Include(c => c.Department)
                    .Where(c => assignedCourseIds.Contains(c.Id))
                    .ToListAsync();

                var recentSubmissions = await _context.AssignmentSubmissions
                    .Include(s => s.Assignment).ThenInclude(a => a!.Module)
                    .Include(s => s.Student)
                    .Where(s => s.Assignment != null && s.Assignment.Module != null
                                && s.Assignment.Module.LecturerId == uid)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(10)
                    .ToListAsync();

                var pendingGrading = recentSubmissions.Count(s => s.Status == "submitted");

                var assignments = await _context.Assignments
                    .Include(a => a.Module)
                    .Where(a => a.Module != null && a.Module.LecturerId == uid)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                ViewBag.Courses = courses;
                ViewBag.RecentSubmissions = recentSubmissions;
                ViewBag.PendingGrading = pendingGrading;
                ViewBag.Assignments = assignments;
                return View("Lecturer");
            }
            else
            {
                // Admin
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.TotalCourses = await _context.Courses.CountAsync();
                ViewBag.TotalStudents = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Student");
                ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
                ViewBag.RecentUsers = await _context.Users
                    .Include(u => u.Role)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                ViewBag.CourseEnrollments = await _context.Courses
                    .Include(c => c.Department)
                    .Select(c => new { c.Title, Count = c.Enrollments.Count })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();
                return View("Admin");
            }
        }
    }
}

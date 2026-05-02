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
                // Admin – summary counts
                ViewBag.TotalUsers       = await _context.Users.CountAsync();
                ViewBag.TotalCourses     = await _context.Courses.CountAsync();
                ViewBag.TotalStudents    = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Student");
                ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
                ViewBag.TotalLecturers   = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Lecturer");
                ViewBag.TotalAdmins      = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Admin");
                ViewBag.TotalDepartments = await _context.Departments.CountAsync();
                ViewBag.TotalMessages    = await _context.Messages.CountAsync();
                ViewBag.TotalAssignments = await _context.Assignments.CountAsync();
                ViewBag.TotalExams       = await _context.Exams.CountAsync();

                // Recent users
                ViewBag.RecentUsers = await _context.Users
                    .Include(u => u.Role)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Top 6 courses by enrollment count
                var courseEnrollments = await _context.Courses
                    .Select(c => new { c.Title, Count = c.Enrollments.Count })
                    .OrderByDescending(x => x.Count)
                    .Take(6)
                    .ToListAsync();
                ViewBag.CourseEnrollmentLabels = System.Text.Json.JsonSerializer.Serialize(courseEnrollments.Select(x => x.Title).ToList());
                ViewBag.CourseEnrollmentData   = System.Text.Json.JsonSerializer.Serialize(courseEnrollments.Select(x => x.Count).ToList());

                // User role breakdown
                var roleBreakdown = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role != null)
                    .GroupBy(u => u.Role!.Name)
                    .Select(g => new { Role = g.Key, Count = g.Count() })
                    .ToListAsync();
                ViewBag.RoleLabels = System.Text.Json.JsonSerializer.Serialize(roleBreakdown.Select(x => x.Role).ToList());
                ViewBag.RoleData   = System.Text.Json.JsonSerializer.Serialize(roleBreakdown.Select(x => x.Count).ToList());

                // Monthly enrollments – last 6 months
                var sixMonthsAgo = DateTime.Now.AddMonths(-5);
                var monthlyEnrollments = await _context.Enrollments
                    .Where(e => e.EnrolledAt >= sixMonthsAgo)
                    .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                var monthlyLabels = new List<string>();
                var monthlyData   = new List<int>();
                for (int i = 5; i >= 0; i--)
                {
                    var d = DateTime.Now.AddMonths(-i);
                    monthlyLabels.Add(d.ToString("MMM yyyy"));
                    monthlyData.Add(monthlyEnrollments.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Count ?? 0);
                }
                ViewBag.MonthlyEnrollmentLabels = System.Text.Json.JsonSerializer.Serialize(monthlyLabels);
                ViewBag.MonthlyEnrollmentData   = System.Text.Json.JsonSerializer.Serialize(monthlyData);

                // Pending enrollment applications
                ViewBag.PendingApplications = await _context.EnrollmentApplications.CountAsync(a => a.Status == "pending");
                ViewBag.ActiveEnrollments   = await _context.Enrollments.CountAsync(e => e.Status == "active");

                return View("Admin");
            }
        }
    }
}

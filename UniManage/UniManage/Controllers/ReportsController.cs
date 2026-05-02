using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.ViewModels;

namespace UniManage.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (IsStudent)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        public async Task<IActionResult> CoursePopularity()
        {
            if (IsStudent)
                return RedirectToAction("Index", "Dashboard");

            var query = _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Creator)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (IsLecturer)
                query = query.Where(c => c.Modules.Any(m => m.LecturerId == CurrentUserId));

            var courses = await query
                .OrderByDescending(c => c.Enrollments.Count)
                .ToListAsync();

            ViewBag.GeneratedAt = DateTime.Now;
            return View(courses);
        }

        public async Task<IActionResult> StudentPerformance(int? courseId)
        {
            if (IsStudent)
                return RedirectToAction("Index", "Dashboard");

            var courseQuery = _context.Courses
                .Include(c => c.Department)
                .AsQueryable();

            if (IsLecturer)
                courseQuery = courseQuery.Where(c => c.Modules.Any(m => m.LecturerId == CurrentUserId));

            var courses = await courseQuery.OrderBy(c => c.Title).ToListAsync();

            var resultsQuery = _context.ExamResults
                .Include(r => r.Student)
                .Include(r => r.Exam).ThenInclude(e => e!.Module).ThenInclude(m => m!.Course)
                .AsQueryable();

            if (IsLecturer)
                resultsQuery = resultsQuery.Where(r =>
                    r.Exam != null && r.Exam.Module != null &&
                    r.Exam.Module.LecturerId == CurrentUserId);

            if (courseId.HasValue)
                resultsQuery = resultsQuery.Where(r =>
                    r.Exam != null && r.Exam.Module != null &&
                    r.Exam.Module.CourseId == courseId.Value);

            var results = await resultsQuery.ToListAsync();

            var submissionsQuery = _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment).ThenInclude(a => a!.Module).ThenInclude(m => m!.Course)
                .Where(s => s.MarksObtained.HasValue)
                .AsQueryable();

            if (IsLecturer)
                submissionsQuery = submissionsQuery.Where(s =>
                    s.Assignment != null && s.Assignment.Module != null &&
                    s.Assignment.Module.LecturerId == CurrentUserId);

            if (courseId.HasValue)
                submissionsQuery = submissionsQuery.Where(s =>
                    s.Assignment != null && s.Assignment.Module != null &&
                    s.Assignment.Module.CourseId == courseId.Value);

            var submissions = await submissionsQuery.ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;
            ViewBag.ExamResults = results;
            ViewBag.Submissions = submissions;
            ViewBag.GeneratedAt = DateTime.Now;
            return View();
        }

        public async Task<IActionResult> WorkloadAnalysis()
        {
            if (IsStudent)
                return RedirectToAction("Index", "Dashboard");

            var modulesQuery = _context.Modules
                .Include(m => m.Course).ThenInclude(c => c!.Department)
                .Include(m => m.Lecturer)
                .Include(m => m.Assignments)
                .Include(m => m.Exams)
                .Include(m => m.CourseMaterials)
                .AsQueryable();

            if (IsLecturer)
                modulesQuery = modulesQuery.Where(m => m.LecturerId == CurrentUserId);

            var modules = await modulesQuery
                .OrderBy(m => m.Course != null ? m.Course.Title : "")
                .ThenBy(m => m.OrderIndex)
                .ToListAsync();

            var lecturerWorkload = modules
                .Where(m => m.Lecturer != null)
                .GroupBy(m => m.Lecturer!)
                .Select(g => new LecturerWorkloadItem
                {
                    Lecturer = g.Key,
                    ModuleCount = g.Count(),
                    AssignmentCount = g.Sum(m => m.Assignments.Count),
                    ExamCount = g.Sum(m => m.Exams.Count),
                    MaterialCount = g.Sum(m => m.CourseMaterials.Count)
                })
                .OrderByDescending(x => x.ModuleCount)
                .ToList();

            ViewBag.Modules = modules;
            ViewBag.LecturerWorkload = lecturerWorkload;
            ViewBag.GeneratedAt = DateTime.Now;
            return View();
        }
    }
}

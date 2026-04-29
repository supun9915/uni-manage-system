using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public CoursesController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var query = _context.Courses.Include(c => c.Department).AsQueryable();

            if (IsLecturer)
                query = query.Where(c => c.CreatedBy == CurrentUserId);
            else if (IsStudent)
            {
                var enrolled = await _context.Enrollments
                    .Where(e => e.UserId == CurrentUserId!.Value && e.Status == "active")
                    .Select(e => e.CourseId).ToListAsync();
                query = query.Where(c => enrolled.Contains(c.Id));
            }

            return View(await query.OrderBy(c => c.Title).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Modules).ThenInclude(m => m.CourseMaterials)
                .Include(c => c.Modules).ThenInclude(m => m.Assignments)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();
            ViewBag.Enrollments = await _context.Enrollments
                .Include(e => e.User).Include(e => e.Batch)
                .Where(e => e.CourseId == id).ToListAsync();
            return View(course);
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin) return Forbid();
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseModel course)
        {
            if (!IsAdmin) return Forbid();
            if (ModelState.IsValid)
            {
                course.CreatedBy = CurrentUserId!.Value;
                course.CreatedAt = DateTime.UtcNow;
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course created.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            return View(course);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin) return Forbid();
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", course.DepartmentId);
            return View(course);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseModel course)
        {
            if (!IsAdmin) return Forbid();
            if (id != course.Id) return NotFound();
            if (ModelState.IsValid)
            {
                course.CreatedBy = CurrentUserId!.Value;
                _context.Update(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course updated.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", course.DepartmentId);
            return View(course);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin) return Forbid();
            var course = await _context.Courses.Include(c => c.Department).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin) return Forbid();
            var course = await _context.Courses.FindAsync(id);
            if (course != null) { _context.Courses.Remove(course); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Course deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

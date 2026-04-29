using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class ModulesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public ModulesController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(int? courseId)
        {
            var query = _context.Modules.Include(m => m.Course).AsQueryable();

            if (IsStudent)
            {
                var enrolled = await _context.Enrollments
                    .Where(e => e.UserId == CurrentUserId!.Value && e.Status == "active")
                    .Select(e => e.CourseId).ToListAsync();
                query = query.Where(m => enrolled.Contains(m.CourseId) && m.IsPublished);
            }
            else if (IsLecturer)
                query = query.Where(m => m.Course != null && m.Course.CreatedBy == CurrentUserId);

            if (courseId.HasValue) query = query.Where(m => m.CourseId == courseId.Value);

            ViewBag.CourseId = courseId;
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title");
            return View(await query.OrderBy(m => m.CourseId).ThenBy(m => m.OrderIndex).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Where(c => c.CreatedBy == CurrentUserId).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ModuleModel module)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Module created.";
                return RedirectToAction(nameof(Index), new { courseId = module.CourseId });
            }
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Where(c => c.CreatedBy == CurrentUserId).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title");
            return View(module);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var module = await _context.Modules.FindAsync(id);
            if (module == null) return NotFound();
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Where(c => c.CreatedBy == CurrentUserId).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title", module.CourseId);
            return View(module);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ModuleModel module)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (id != module.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(module);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Module updated.";
                return RedirectToAction(nameof(Index), new { courseId = module.CourseId });
            }
            var courses = IsAdmin
                ? await _context.Courses.ToListAsync()
                : await _context.Courses.Where(c => c.CreatedBy == CurrentUserId).ToListAsync();
            ViewBag.Courses = new SelectList(courses, "Id", "Title", module.CourseId);
            return View(module);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var module = await _context.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == id);
            if (module == null) return NotFound();
            return View(module);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var module = await _context.Modules.FindAsync(id);
            int? cId = module?.CourseId;
            if (module != null) { _context.Modules.Remove(module); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Module deleted.";
            return RedirectToAction(nameof(Index), new { courseId = cId });
        }
    }
}

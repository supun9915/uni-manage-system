using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class BatchesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public BatchesController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(int? courseId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var query = _context.BatchModels.Include(b => b.Course).AsQueryable();
            if (courseId.HasValue) query = query.Where(b => b.CourseId == courseId.Value);
            ViewBag.CourseId = courseId;
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title");
            return View(await query.OrderByDescending(b => b.Id).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin) return Forbid();
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BatchModel batch)
        {
            if (!IsAdmin) return Forbid();
            if (ModelState.IsValid)
            {
                _context.BatchModels.Add(batch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Batch created.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title");
            return View(batch);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin) return Forbid();
            var batch = await _context.BatchModels.FindAsync(id);
            if (batch == null) return NotFound();
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title", batch.CourseId);
            return View(batch);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BatchModel batch)
        {
            if (!IsAdmin) return Forbid();
            if (id != batch.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(batch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Batch updated.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Courses = new SelectList(await _context.Courses.ToListAsync(), "Id", "Title", batch.CourseId);
            return View(batch);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin) return Forbid();
            var batch = await _context.BatchModels.Include(b => b.Course).FirstOrDefaultAsync(b => b.Id == id);
            if (batch == null) return NotFound();
            return View(batch);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin) return Forbid();
            var batch = await _context.BatchModels.FindAsync(id);
            if (batch != null) { _context.BatchModels.Remove(batch); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Batch deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

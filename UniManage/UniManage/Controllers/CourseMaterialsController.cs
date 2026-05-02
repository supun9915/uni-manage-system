using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class CourseMaterialsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CourseMaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? moduleId)
        {
            var query = _context.CourseMaterials.Include(m => m.Module).ThenInclude(m => m!.Course).AsQueryable();

            if (IsStudent)
            {
                var enrolled = await _context.Enrollments
                    .Where(e => e.UserId == CurrentUserId!.Value && e.Status == "active")
                    .Select(e => e.CourseId).ToListAsync();
                query = query.Where(cm => cm.Module != null && enrolled.Contains(cm.Module.CourseId));
            }
            else if (IsLecturer)
                query = query.Where(cm => cm.Module != null && cm.Module.LecturerId == CurrentUserId);

            if (moduleId.HasValue) query = query.Where(cm => cm.ModuleId == moduleId.Value);

            ViewBag.ModuleId = moduleId;
            ViewBag.Modules = new SelectList(await _context.Modules.Include(m => m.Course).ToListAsync(), "Id", "Title");
            return View(await query.OrderByDescending(cm => cm.UploadedAt).ToListAsync());
        }

        public async Task<IActionResult> Create(int? moduleId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} � {m.Title}" }), "Id", "Name", moduleId);
            var model = new CourseMaterialModel { ModuleId = moduleId ?? 0 };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]          // 50 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Create(CourseMaterialModel material, IFormFile? file)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();

            // Clear programmatically assigned fields from ModelState
            ModelState.Remove(nameof(CourseMaterialModel.FileData));
            ModelState.Remove(nameof(CourseMaterialModel.FileName));
            ModelState.Remove(nameof(CourseMaterialModel.ContentType));
            ModelState.Remove(nameof(CourseMaterialModel.FileType));
            ModelState.Remove(nameof(CourseMaterialModel.FileSize));
            ModelState.Remove(nameof(CourseMaterialModel.UploadedAt));

            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                material.FileData    = ms.ToArray();
                material.FileName    = Path.GetFileName(file.FileName);
                material.ContentType = file.ContentType;
                material.FileSize    = (int)(file.Length / 1024);
                material.FileType    = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
            }
            else
            {
                ModelState.AddModelError("file", "Please upload a file.");
            }

            if (ModelState.IsValid)
            {
                material.UploadedAt = DateTime.UtcNow;
                _context.CourseMaterials.Add(material);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Material uploaded.";
                return RedirectToAction(nameof(Index), new { moduleId = material.ModuleId });
            }

            var modules = IsAdmin
                ? await _context.Modules.Include(m => m.Course).ToListAsync()
                : await _context.Modules.Include(m => m.Course)
                    .Where(m => m.LecturerId == CurrentUserId).ToListAsync();
            ViewBag.Modules = new SelectList(modules.Select(m => new { m.Id, Name = $"{m.Course?.Title} � {m.Title}" }), "Id", "Name", material.ModuleId);
            return View(material);
        }

        public async Task<IActionResult> Download(int id)
        {
            var material = await _context.CourseMaterials.FindAsync(id);
            if (material == null || material.FileData == null) return NotFound();
            return File(material.FileData, material.ContentType ?? "application/octet-stream",
                        material.FileName ?? "download");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var material = await _context.CourseMaterials.Include(m => m.Module).FirstOrDefaultAsync(m => m.Id == id);
            if (material == null) return NotFound();
            return View(material);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var material = await _context.CourseMaterials.FindAsync(id);
            int? mId = material?.ModuleId;
            if (material != null) { _context.CourseMaterials.Remove(material); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Material deleted.";
            return RedirectToAction(nameof(Index), new { moduleId = mId });
        }
    }
}

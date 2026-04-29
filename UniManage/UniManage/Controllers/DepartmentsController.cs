using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class DepartmentsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public DepartmentsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin) return Forbid();
            return View(await _context.Departments.ToListAsync());
        }

        public IActionResult Create()
        {
            if (!IsAdmin) return Forbid();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentModel dept)
        {
            if (!IsAdmin) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Departments.Add(dept);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department created.";
                return RedirectToAction(nameof(Index));
            }
            return View(dept);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin) return Forbid();
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentModel dept)
        {
            if (!IsAdmin) return Forbid();
            if (id != dept.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(dept);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(dept);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin) return Forbid();
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin) return Forbid();
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null) { _context.Departments.Remove(dept); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Department deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

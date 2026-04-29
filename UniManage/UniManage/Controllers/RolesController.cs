using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class RolesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public RolesController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin) return Forbid();
            return View(await _context.Roles.ToListAsync());
        }

        public IActionResult Create()
        {
            if (!IsAdmin) return Forbid();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleModel role)
        {
            if (!IsAdmin) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Role created.";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin) return Forbid();
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleModel role)
        {
            if (!IsAdmin) return Forbid();
            if (id != role.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Role updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin) return Forbid();
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin) return Forbid();
            var role = await _context.Roles.FindAsync(id);
            if (role != null) { _context.Roles.Remove(role); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Role deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

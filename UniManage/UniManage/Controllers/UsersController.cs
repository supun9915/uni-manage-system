using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserModel> _hasher;

        public UsersController(ApplicationDbContext context, IPasswordHasher<UserModel> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index(string? search)
        {
            if (!IsAdmin) return Forbid();
            var query = _context.Users.Include(u => u.Role).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
            ViewBag.Search = search;
            return View(await query.OrderBy(u => u.FirstName).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin) return Forbid();
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user, string password)
        {
            if (!IsAdmin) return Forbid();
            ModelState.Remove("PasswordHash");
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use.");
                    ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name");
                    return View(user);
                }
                user.PasswordHash = _hasher.HashPassword(user, password);
                user.CreatedAt = DateTime.UtcNow;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User created.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name");
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin) return Forbid();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserModel user, string? newPassword)
        {
            if (!IsAdmin) return Forbid();
            if (id != user.Id) return NotFound();
            ModelState.Remove("PasswordHash");
            if (ModelState.IsValid)
            {
                var existing = await _context.Users.FindAsync(id);
                if (existing == null) return NotFound();
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.Email = user.Email;
                existing.RoleId = user.RoleId;
                existing.Phone = user.Phone;
                existing.IsActive = user.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(newPassword))
                    existing.PasswordHash = _hasher.HashPassword(existing, newPassword);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
            return View(user);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin) return Forbid();
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin) return Forbid();
            var user = await _context.Users.FindAsync(id);
            if (user != null) { _context.Users.Remove(user); await _context.SaveChangesAsync(); }
            TempData["Success"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

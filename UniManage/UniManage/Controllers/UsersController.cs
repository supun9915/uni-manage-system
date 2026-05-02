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

        private static string? ValidatePassword(string pwd)
        {
            if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 6)
                return "Password must be at least 6 characters.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(pwd, @"[A-Z]"))
                return "Password must contain at least one uppercase letter.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(pwd, @"[a-z]"))
                return "Password must contain at least one lowercase letter.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(pwd, @"[0-9]"))
                return "Password must contain at least one number.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(pwd, @"[^a-zA-Z0-9]"))
                return "Password must contain at least one special character (e.g. @, #, !, $).";
            return null;
        }

        public UsersController(ApplicationDbContext context, IPasswordHasher<UserModel> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index(string? search, int? roleId)
        {
            if (!IsAdmin) return Forbid();
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.Email.Contains(search) ||
                    (u.NIC != null && u.NIC.Contains(search)) ||
                    (u.UID != null && u.UID.Contains(search)));

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            ViewBag.Search = search;
            ViewBag.RoleId = roleId;
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name");
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
            var pwdError = ValidatePassword(password);
            if (pwdError != null) ModelState.AddModelError("password", pwdError);

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
                user.UID = await GenerateUIDAsync(user.RoleId);
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
                existing.NIC = user.NIC;
                existing.IsActive = user.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    var pwdErr = ValidatePassword(newPassword);
                    if (pwdErr != null)
                    {
                        ModelState.AddModelError("newPassword", pwdErr);
                        ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
                        return View(user);
                    }
                    existing.PasswordHash = _hasher.HashPassword(existing, newPassword);
                }
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

        private async Task<string> GenerateUIDAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            var prefix = (role?.Name?.ToLower()) switch {
                "student"       => "STU",
                "lecturer"      => "LEC",
                "administrator" => "ADM",
                _               => "USR"
            };
            int year = DateTime.UtcNow.Year;
            string pattern = $"{prefix}{year}";

            // Find the highest existing index for this prefix+year to avoid reuse after deletions
            var lastIndex = await _context.Users
                .Where(u => u.UID != null && u.UID.StartsWith(pattern))
                .Select(u => u.UID!)
                .ToListAsync();

            int maxIndex = lastIndex
                .Select(uid => int.TryParse(uid.Substring(pattern.Length), out int idx) ? idx : 999)
                .DefaultIfEmpty(999)
                .Max();

            return $"{prefix}{year}{maxIndex + 1}";
        }
    }
}

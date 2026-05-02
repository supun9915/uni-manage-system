using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;
using UniManage.Services;

namespace UniManage.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserModel> _hasher;
        private readonly IEmailService _emailService;

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

        public AccountController(ApplicationDbContext context, IPasswordHasher<UserModel> hasher, IEmailService emailService)
        {
            _context = context;
            _hasher = hasher;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role?.Name?.ToLower() ?? "student");
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ??? Forgot Password ?????????????????????????????????????????????????

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter your email address.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null)
            {
                // Do not reveal whether email exists
                ViewBag.Success = "If that email is registered, an OTP has been sent.";
                return View();
            }

            // Invalidate any previous OTPs
            var existing = _context.PasswordResetOtps.Where(o => o.Email == email && !o.IsUsed);
            _context.PasswordResetOtps.RemoveRange(existing);

            var otp = new PasswordResetOtpModel
            {
                Email = email,
                OtpCode = new Random().Next(100000, 999999).ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            _context.PasswordResetOtps.Add(otp);
            await _context.SaveChangesAsync();

            var html = $@"
                <div style='font-family:Inter,sans-serif;max-width:480px;margin:auto;'>
                  <div style='background:linear-gradient(90deg,#0f172a,#1e3a8a);padding:28px 32px;border-radius:12px 12px 0 0;text-align:center;'>
                    <h2 style='color:#fff;margin:0;font-size:22px;'>UniManage Password Reset</h2>
                  </div>
                  <div style='background:#f8fafc;padding:32px;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 12px 12px;'>
                    <p style='color:#475569;font-size:14px;'>Hello {user.FirstName},</p>
                    <p style='color:#475569;font-size:14px;'>Use the OTP below to reset your password. It expires in <strong>10 minutes</strong>.</p>
                    <div style='margin:28px 0;text-align:center;'>
                      <span style='display:inline-block;letter-spacing:0.3em;font-size:36px;font-weight:700;color:#1d4ed8;background:#eff6ff;padding:16px 32px;border-radius:12px;border:2px dashed #bfdbfe;'>{otp.OtpCode}</span>
                    </div>
                    <p style='color:#94a3b8;font-size:12px;text-align:center;'>If you did not request this, please ignore this email.</p>
                  </div>
                </div>";

            await _emailService.SendAsync(email, "Your UniManage Password Reset OTP", html);

            TempData["OtpEmail"] = email;
            return RedirectToAction("VerifyOtp");
        }

        // ??? Verify OTP ???????????????????????????????????????????????????????

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (TempData["OtpEmail"] is string email)
            {
                TempData.Keep("OtpEmail");
                ViewBag.Email = email;
            }
            else
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            ViewBag.Email = email;

            if (string.IsNullOrWhiteSpace(otp))
            {
                ViewBag.Error = "Please enter the OTP.";
                return View();
            }

            var record = await _context.PasswordResetOtps
                .Where(o => o.Email == email && o.OtpCode == otp.Trim() && !o.IsUsed && !o.IsVerified)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null || record.ExpiresAt < DateTime.UtcNow)
            {
                ViewBag.Error = "Invalid or expired OTP. Please try again.";
                return View();
            }

            record.IsVerified = true;
            await _context.SaveChangesAsync();

            TempData["VerifiedEmail"] = email;
            return RedirectToAction("ResetPassword");
        }

        // ??? Reset Password ???????????????????????????????????????????????????

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["VerifiedEmail"] is string email)
            {
                TempData.Keep("VerifiedEmail");
                ViewBag.Email = email;
                return View();
            }
            return RedirectToAction("ForgotPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
        {
            ViewBag.Email = email;

            var pwdError = ValidatePassword(newPassword);
            if (pwdError != null)
            {
                ViewBag.Error = pwdError;
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            var record = await _context.PasswordResetOtps
                .Where(o => o.Email == email && o.IsVerified && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                TempData["Error"] = "Session expired. Please restart the password reset process.";
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("ForgotPassword");
            }

            user.PasswordHash = _hasher.HashPassword(user, newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            record.IsUsed = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password updated successfully. You can now sign in.";
            return RedirectToAction("Login");
        }
    }
}

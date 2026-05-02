using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class MessagesController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ??? Inbox ????????????????????????????????????????????????????????????
        public async Task<IActionResult> Inbox()
        {
            // Root messages where current user is either sender or receiver
            var messages = await _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u!.Role)
                .Include(m => m.Receiver).ThenInclude(u => u!.Role)
                .Where(m => m.ParentMessageId == null
                            && ((m.ReceiverId == CurrentUserId!.Value && !m.IsDeletedByReceiver)
                                || (m.SenderId == CurrentUserId!.Value && !m.IsDeletedBySender)))
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            ViewBag.UnreadCount = messages.Count(m => m.ReceiverId == CurrentUserId!.Value && !m.IsReadByReceiver);
            return View(messages);
        }

        // ??? Sent ?????????????????????????????????????????????????????????????
        public async Task<IActionResult> Sent()
        {
            var messages = await _context.Messages
                .Include(m => m.Receiver).ThenInclude(u => u!.Role)
                .Where(m => m.SenderId == CurrentUserId!.Value
                            && m.ParentMessageId == null
                            && !m.IsDeletedBySender)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        // ??? Read thread ??????????????????????????????????????????????????????
        public async Task<IActionResult> Read(int id)
        {
            var root = await _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u!.Role)
                .Include(m => m.Receiver).ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (root == null) return NotFound();

            // Only sender or receiver may read
            if (root.SenderId != CurrentUserId && root.ReceiverId != CurrentUserId)
                return Forbid();

            // Mark as read when receiver opens it
            if (root.ReceiverId == CurrentUserId && !root.IsReadByReceiver)
            {
                root.IsReadByReceiver = true;
                await _context.SaveChangesAsync();
            }

            // Load full thread (replies) ordered by time
            var thread = await _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u!.Role)
                .Include(m => m.Receiver).ThenInclude(u => u!.Role)
                .Where(m => m.Id == id || m.ParentMessageId == id)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.Root = root;
            return View(thread);
        }

        // ??? Compose ??????????????????????????????????????????????????????????
        [HttpGet]
        public async Task<IActionResult> Compose(int? replyToId, int? toUserId)
        {
            await PopulateRecipientsAsync();

            if (replyToId.HasValue)
            {
                var original = await _context.Messages
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(m => m.Id == replyToId.Value);
                if (original != null)
                {
                    ViewBag.ReplyTo = original;
                    ViewBag.DefaultSubject = original.Subject.StartsWith("Re: ")
                        ? original.Subject : $"Re: {original.Subject}";
                    ViewBag.DefaultReceiverId = original.SenderId;
                }
            }
            else if (toUserId.HasValue)
            {
                ViewBag.DefaultReceiverId = toUserId.Value;
            }

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(int receiverId, string subject, string body, int? replyToId)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                ViewBag.Error = "Subject and message body are required.";
                await PopulateRecipientsAsync();
                return View();
            }

            var receiver = await _context.Users.FindAsync(receiverId);
            if (receiver == null)
            {
                ViewBag.Error = "Selected recipient not found.";
                await PopulateRecipientsAsync();
                return View();
            }

            var message = new MessageModel
            {
                SenderId = CurrentUserId!.Value,
                ReceiverId = receiverId,
                Subject = subject.Trim(),
                Body = body.Trim(),
                ParentMessageId = replyToId,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully.";

            // If it's a reply, go back to the thread
            return replyToId.HasValue
                ? RedirectToAction(nameof(Read), new { id = replyToId.Value })
                : RedirectToAction(nameof(Sent));
        }

        // ??? Delete ???????????????????????????????????????????????????????????
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string returnTo = "inbox")
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return NotFound();

            if (message.SenderId == CurrentUserId)
                message.IsDeletedBySender = true;
            if (message.ReceiverId == CurrentUserId)
                message.IsDeletedByReceiver = true;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Message deleted.";
            return RedirectToAction(returnTo == "sent" ? nameof(Sent) : nameof(Inbox));
        }

        // ??? Helper: unread count for layout badge ????????????????????????????
        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == CurrentUserId!.Value
                                 && !m.IsReadByReceiver
                                 && !m.IsDeletedByReceiver);
        }

        // ??? Private helpers ??????????????????????????????????????????????????
        private async Task PopulateRecipientsAsync()
        {
            List<UserModel> recipients;

            if (IsStudent)
            {
                // Students can only message lecturers assigned to their modules
                var enrolledCourseIds = await _context.Enrollments
                    .Where(e => e.UserId == CurrentUserId!.Value && e.Status == "active")
                    .Select(e => e.CourseId)
                    .ToListAsync();

                var lecturerIds = await _context.Modules
                    .Where(m => enrolledCourseIds.Contains(m.CourseId) && m.LecturerId.HasValue)
                    .Select(m => m.LecturerId!.Value)
                    .Distinct()
                    .ToListAsync();

                recipients = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => lecturerIds.Contains(u.Id) && u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();
            }
            else if (IsLecturer)
            {
                // Lecturers can message students enrolled in their assigned modules
                var assignedModuleCourseIds = await _context.Modules
                    .Where(m => m.LecturerId == CurrentUserId)
                    .Select(m => m.CourseId)
                    .Distinct()
                    .ToListAsync();

                var studentIds = await _context.Enrollments
                    .Where(e => assignedModuleCourseIds.Contains(e.CourseId) && e.Status == "active")
                    .Select(e => e.UserId)
                    .Distinct()
                    .ToListAsync();

                recipients = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => studentIds.Contains(u.Id) && u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();
            }
            else
            {
                // Admin can message anyone
                recipients = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Id != CurrentUserId && u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();
            }

            ViewBag.Recipients = new SelectList(
                recipients.Select(u => new
                {
                    u.Id,
                    Name = $"{u.FirstName} {u.LastName} ({u.Role?.Name ?? ""})"
                }),
                "Id", "Name");
        }
    }
}

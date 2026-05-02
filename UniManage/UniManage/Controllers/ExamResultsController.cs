using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Controllers
{
    public class ExamResultsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public ExamResultsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(int? examId)
        {
            var query = _context.ExamResults
                .Include(r => r.Exam).ThenInclude(e => e!.Module).ThenInclude(m => m!.Course)
                .Include(r => r.Student)
                .AsQueryable();

            if (IsStudent)
                query = query.Where(r => r.StudentId == CurrentUserId);
            else if (IsLecturer)
                query = query.Where(r => r.Exam != null && r.Exam.Module != null
                    && r.Exam.Module.LecturerId == CurrentUserId);

            if (examId.HasValue) query = query.Where(r => r.ExamId == examId.Value);

            ViewBag.ExamId = examId;
            return View(await query.OrderByDescending(r => r.SubmittedAt).ToListAsync());
        }

        public async Task<IActionResult> Create(int examId)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var exam = await _context.Exams.Include(e => e.Module).FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null) return NotFound();
            ViewBag.Exam = exam;

            var enrolledStudents = await _context.Enrollments
                .Include(e => e.User)
                .Where(e => e.Status == "active" && e.CourseId == exam.Module!.CourseId)
                .Select(e => e.User)
                .ToListAsync();
            ViewBag.Students = new SelectList(enrolledStudents.Select(u => new { u!.Id, Name = $"{u.FirstName} {u.LastName}" }), "Id", "Name");
            return View(new ExamResultModel { ExamId = examId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamResultModel result)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (ModelState.IsValid)
            {
                result.SubmittedAt = DateTime.UtcNow;
                var exam = await _context.Exams.FindAsync(result.ExamId);
                if (exam != null && result.MarksObtained.HasValue)
                    result.Status = result.MarksObtained >= (exam.PassMarks ?? exam.TotalMarks / 2) ? "pass" : "fail";
                _context.ExamResults.Add(result);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Result recorded.";
                return RedirectToAction(nameof(Index), new { examId = result.ExamId });
            }
            var ex = await _context.Exams.Include(e => e.Module).FirstOrDefaultAsync(e => e.Id == result.ExamId);
            ViewBag.Exam = ex;
            var enrolled = await _context.Enrollments.Include(e => e.User)
                .Where(e => e.Status == "active" && e.CourseId == ex!.Module!.CourseId)
                .Select(e => e.User).ToListAsync();
            ViewBag.Students = new SelectList(enrolled.Select(u => new { u!.Id, Name = $"{u.FirstName} {u.LastName}" }), "Id", "Name");
            return View(result);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var result = await _context.ExamResults.Include(r => r.Exam).Include(r => r.Student).FirstOrDefaultAsync(r => r.Id == id);
            if (result == null) return NotFound();
            return View(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamResultModel result)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            if (id != result.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var exam = await _context.Exams.FindAsync(result.ExamId);
                if (exam != null && result.MarksObtained.HasValue)
                    result.Status = result.MarksObtained >= (exam.PassMarks ?? exam.TotalMarks / 2) ? "pass" : "fail";
                _context.Update(result);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Result updated.";
                return RedirectToAction(nameof(Index), new { examId = result.ExamId });
            }
            return View(result);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var result = await _context.ExamResults.Include(r => r.Exam).Include(r => r.Student).FirstOrDefaultAsync(r => r.Id == id);
            if (result == null) return NotFound();
            return View(result);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin && !IsLecturer) return Forbid();
            var result = await _context.ExamResults.FindAsync(id);
            int? eId = result?.ExamId;
            if (result != null) { _context.ExamResults.Remove(result); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Result deleted.";
            return RedirectToAction(nameof(Index), new { examId = eId });
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? NIC { get; set; }

        [MaxLength(20)]
        public string? UID { get; set; }

        public string? ProfilePicture { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(RoleId))]
        public RoleModel? Role { get; set; }

        public ICollection<UserDepartmentModel> UserDepartments { get; set; } = new List<UserDepartmentModel>();
        public ICollection<EnrollmentModel> Enrollments { get; set; } = new List<EnrollmentModel>();
        public ICollection<CourseModel> CreatedCourses { get; set; } = new List<CourseModel>();
        public ICollection<AssignmentSubmissionModel> AssignmentSubmissions { get; set; } = new List<AssignmentSubmissionModel>();
        public ICollection<ExamResultModel> ExamResults { get; set; } = new List<ExamResultModel>();
        public ICollection<AnnouncementModel> Announcements { get; set; } = new List<AnnouncementModel>();
        public ICollection<ExamModel> CreatedExams { get; set; } = new List<ExamModel>();
        public ICollection<AssignmentModel> CreatedAssignments { get; set; } = new List<AssignmentModel>();
    }
}

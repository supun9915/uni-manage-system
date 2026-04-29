using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class CourseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Code { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "draft"; // draft, published, archived

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(DepartmentId))]
        public DepartmentModel? Department { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public UserModel? Creator { get; set; }

        public ICollection<BatchModel> Batches { get; set; } = new List<BatchModel>();
        public ICollection<EnrollmentModel> Enrollments { get; set; } = new List<EnrollmentModel>();
        public ICollection<ModuleModel> Modules { get; set; } = new List<ModuleModel>();
        public ICollection<AnnouncementModel> Announcements { get; set; } = new List<AnnouncementModel>();
    }
}

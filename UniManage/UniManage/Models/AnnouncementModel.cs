using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class AnnouncementModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? CreatedBy { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? TargetRole { get; set; } // all, student, lecturer

        public DateTime? PublishDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(CourseId))]
        public CourseModel? Course { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public UserModel? Creator { get; set; }
    }
}

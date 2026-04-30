using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class AssignmentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ModuleId { get; set; }

        public int? CreatedBy { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Instructions { get; set; }

        public int MaxMarks { get; set; } = 100;

        public DateTime? ReleaseDate { get; set; }

        public DateTime? DeadlineDate { get; set; }

        public bool AllowLateSubmit { get; set; } = false;

        public byte[]? FileData { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        public int? FileSize { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(ModuleId))]
        public ModuleModel? Module { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public UserModel? Creator { get; set; }

        public ICollection<AssignmentSubmissionModel> Submissions { get; set; } = new List<AssignmentSubmissionModel>();
    }
}

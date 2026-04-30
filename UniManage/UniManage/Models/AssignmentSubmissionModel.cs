using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class AssignmentSubmissionModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public byte[]? FileData { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        public int? FileSize { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public int? MarksObtained { get; set; }

        public string? Feedback { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "submitted"; // submitted, graded, late

        public int? GradedBy { get; set; }

        public DateTime? GradedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(AssignmentId))]
        public AssignmentModel? Assignment { get; set; }

        [ForeignKey(nameof(StudentId))]
        public UserModel? Student { get; set; }
    }
}

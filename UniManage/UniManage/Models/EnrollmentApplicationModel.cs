using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class EnrollmentApplicationModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        /// <summary>Type of prerequisite document submitted (e.g. "Transcript", "Certificate")</summary>
        [Required, MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        public byte[]? DocumentData { get; set; }

        [MaxLength(255)]
        public string? DocumentFileName { get; set; }

        [MaxLength(100)]
        public string? DocumentContentType { get; set; }

        /// <summary>Any notes the student wants to add</summary>
        public string? StudentNote { get; set; }

        /// <summary>pending | approved | rejected</summary>
        [MaxLength(20)]
        public string Status { get; set; } = "pending";

        public string? AdminNote { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedBy { get; set; }

        // Navigation
        [ForeignKey(nameof(StudentId))]
        public UserModel? Student { get; set; }

        [ForeignKey(nameof(CourseId))]
        public CourseModel? Course { get; set; }

        [ForeignKey(nameof(ReviewedBy))]
        public UserModel? Reviewer { get; set; }
    }
}

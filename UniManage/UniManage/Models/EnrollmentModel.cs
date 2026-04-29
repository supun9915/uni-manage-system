using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class EnrollmentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BatchId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "active";

        // Navigation
        [ForeignKey(nameof(UserId))]
        public UserModel? User { get; set; }

        [ForeignKey(nameof(BatchId))]
        public BatchModel? Batch { get; set; }

        [ForeignKey(nameof(CourseId))]
        public CourseModel? Course { get; set; }
    }
}

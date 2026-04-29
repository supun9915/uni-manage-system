using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class BatchModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int? MaxStudents { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "active";

        // Navigation
        [ForeignKey(nameof(CourseId))]
        public CourseModel? Course { get; set; }

        public ICollection<EnrollmentModel> Enrollments { get; set; } = new List<EnrollmentModel>();
    }
}

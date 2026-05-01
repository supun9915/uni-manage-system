using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class ModuleModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int OrderIndex { get; set; }

        public bool IsPublished { get; set; } = false;

        public int? LecturerId { get; set; }

        // Navigation
        [ForeignKey(nameof(CourseId))]
        public CourseModel? Course { get; set; }

        [ForeignKey(nameof(LecturerId))]
        public UserModel? Lecturer { get; set; }

        public ICollection<CourseMaterialModel> CourseMaterials { get; set; } = new List<CourseMaterialModel>();
        public ICollection<AssignmentModel> Assignments { get; set; } = new List<AssignmentModel>();
        public ICollection<ExamModel> Exams { get; set; } = new List<ExamModel>();
    }
}

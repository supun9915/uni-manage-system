using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class ExamModel
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

        public int TotalMarks { get; set; } = 100;

        public int? PassMarks { get; set; }

        public int? DurationMinutes { get; set; }

        public DateTime? ExamDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(ModuleId))]
        public ModuleModel? Module { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public UserModel? Creator { get; set; }

        public ICollection<ExamResultModel> ExamResults { get; set; } = new List<ExamResultModel>();
    }
}

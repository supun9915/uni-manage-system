using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class ExamResultModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public int? MarksObtained { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // pass, fail, absent

        public DateTime? SubmittedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(ExamId))]
        public ExamModel? Exam { get; set; }

        [ForeignKey(nameof(StudentId))]
        public UserModel? Student { get; set; }
    }
}

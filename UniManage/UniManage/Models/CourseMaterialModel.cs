using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class CourseMaterialModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? FileType { get; set; } // pdf, video, link, etc

        public int? FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(ModuleId))]
        public ModuleModel? Module { get; set; }
    }
}

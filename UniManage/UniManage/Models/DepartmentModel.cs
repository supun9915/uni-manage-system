using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class DepartmentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Code { get; set; }

        public string? Description { get; set; }

        // Navigation
        public ICollection<UserDepartmentModel> UserDepartments { get; set; } = new List<UserDepartmentModel>();
        public ICollection<CourseModel> Courses { get; set; } = new List<CourseModel>();
    }
}

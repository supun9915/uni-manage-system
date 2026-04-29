using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class UserDepartmentModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        // Navigation
        [ForeignKey(nameof(UserId))]
        public UserModel? User { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public DepartmentModel? Department { get; set; }
    }
}

using UniManage.Models;

namespace UniManage.ViewModels
{
    public class LecturerWorkloadItem
    {
        public UserModel Lecturer { get; set; } = null!;
        public int ModuleCount { get; set; }
        public int AssignmentCount { get; set; }
        public int ExamCount { get; set; }
        public int MaterialCount { get; set; }
        public int TotalItems => ModuleCount + AssignmentCount + ExamCount + MaterialCount;
    }
}

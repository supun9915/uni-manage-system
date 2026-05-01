using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UniManage.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.UserDepartmentModel>()
                .HasKey(ud => new { ud.UserId, ud.DepartmentId });
        }

        public DbSet<Models.UserModel> Users { get; set; }
        public DbSet<Models.RoleModel> Roles { get; set; }
        public DbSet<Models.DepartmentModel> Departments { get; set; }
        public DbSet<Models.UserDepartmentModel> UserDepartments { get; set; }
        public DbSet<Models.CourseModel> Courses { get; set; }
        public DbSet<Models.ModuleModel> Modules { get; set; }
        public DbSet<Models.CourseMaterialModel> CourseMaterials { get; set; }
        public DbSet<Models.AssignmentModel> Assignments { get; set; }
        public DbSet<Models.ExamModel> Exams { get; set; }
        public DbSet<Models.EnrollmentModel> Enrollments { get; set; }
        public DbSet<Models.AssignmentSubmissionModel> AssignmentSubmissions { get; set; }
        public DbSet<Models.ExamResultModel> ExamResults { get; set; }
        public DbSet<Models.AnnouncementModel> Announcements { get; set; }
        public DbSet<Models.BatchModel> BatchModels { get; set; }
        public DbSet<Models.EnrollmentApplicationModel> EnrollmentApplications { get; set; }
    }
}

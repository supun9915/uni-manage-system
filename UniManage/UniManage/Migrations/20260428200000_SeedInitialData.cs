using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManage.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // Roles
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name", "Description" },
                values: new object[,]
                {
                    { 1, "Administrator", "Full system access" },
                    { 2, "Lecturer",      "Can manage courses, modules, assignments and exams" },
                    { 3, "Student",       "Can enroll in courses and submit assignments" }
                });

            // Users – super admin + lecturers
            var hasher = new PasswordHasher<Models.UserModel>();
            var adminHash    = hasher.HashPassword(new Models.UserModel(), "superadmin");
            var lecturerHash = hasher.HashPassword(new Models.UserModel(), "lecturer123");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "RoleId", "FirstName", "LastName", "Email", "PasswordHash", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    // Admin
                    { 1, 1, "Super",   "Admin",    "superadmin@gmail.com",      adminHash,    true, now },
                    // Computing lecturers
                    { 2, 2, "James",   "Anderson", "j.anderson@unimanage.ac.lk", lecturerHash, true, now },
                    { 3, 2, "Sarah",   "Mitchell", "s.mitchell@unimanage.ac.lk", lecturerHash, true, now },
                    { 4, 2, "David",   "Clarke",   "d.clarke@unimanage.ac.lk",   lecturerHash, true, now },
                    // Business Management lecturers
                    { 5, 2, "Emily",   "Roberts",  "e.roberts@unimanage.ac.lk",  lecturerHash, true, now },
                    { 6, 2, "Michael", "Thompson", "m.thompson@unimanage.ac.lk", lecturerHash, true, now },
                    { 7, 2, "Priya",   "Sharma",   "p.sharma@unimanage.ac.lk",   lecturerHash, true, now }
                });

            // Departments – Computing and Business Management only
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Name", "Code", "Description" },
                values: new object[,]
                {
                    { 1, "Computing",           "CSE", "Department of Computer Science and Software Engineering" },
                    { 2, "Business Management", "BUS", "Department of Business and Management Studies"          }
                });

            // Lecturer ? Department assignments
            migrationBuilder.InsertData(
                table: "UserDepartments",
                columns: new[] { "UserId", "DepartmentId" },
                values: new object[,]
                {
                    // Computing lecturers
                    { 2, 1 }, { 3, 1 }, { 4, 1 },
                    // Business Management lecturers
                    { 5, 2 }, { 6, 2 }, { 7, 2 }
                });

            // Courses
            // CreatedBy maps each course to its assigned lecturer
            var courseColumns = new[] { "Id", "DepartmentId", "CreatedBy", "Title", "Code", "Description", "Thumbnail", "Status", "CreatedAt" };

            migrationBuilder.InsertData(
                table: "Courses",
                columns: courseColumns,
                values: new object[,]
                {
                    // Computing (dept 1) — lecturers: James Anderson(2), Sarah Mitchell(3), David Clarke(4)
                    {  1, 1, 2, "BSc Software Engineering",      "CSE101", "Software development lifecycle and engineering practices", "se.jpg",        "published", now },
                    {  2, 1, 2, "BSc Computer Science",          "CSE102", "Core computing concepts and programming",                  "cs.jpg",        "published", now },
                    {  3, 1, 3, "BSc Information Technology",    "CSE103", "IT infrastructure and systems management",                 "it.jpg",        "published", now },
                    {  4, 1, 3, "BSc Cyber Security",            "CSE104", "Network security and ethical hacking",                     "cyber.jpg",     "published", now },
                    {  5, 1, 4, "BSc Artificial Intelligence",   "CSE105", "AI concepts and intelligent systems",                      "ai.jpg",        "published", now },
                    // Business Management (dept 2) — lecturers: Emily Roberts(5), Michael Thompson(6), Priya Sharma(7)
                    {  6, 2, 5, "BA Business Administration",    "BUS201", "Business management and leadership",                       "ba.jpg",        "published", now },
                    {  7, 2, 5, "BA Marketing Management",       "BUS202", "Marketing strategies and branding",                        "marketing.jpg", "published", now },
                    {  8, 2, 6, "BA Human Resource Management",  "BUS203", "HR practices and employee management",                     "hr.jpg",        "published", now },
                    {  9, 2, 6, "BA Accounting and Finance",     "BUS204", "Financial reporting and accounting",                       "finance.jpg",   "published", now },
                    { 10, 2, 7, "BA International Business",     "BUS205", "Global trade and business operations",                     "intl.jpg",      "published", now }
                });

            // Modules (4 per course, courses 1-10)
            // Lecturer assignment is through the parent course's CreatedBy field:
            //   Courses 1,2 ? James Anderson | Courses 3,4 ? Sarah Mitchell | Course 5 ? David Clarke
            //   Courses 6,7 ? Emily Roberts  | Courses 8,9 ? Michael Thompson | Course 10 ? Priya Sharma
            var moduleColumns = new[] { "Id", "CourseId", "Title", "Description", "OrderIndex", "IsPublished" };

            migrationBuilder.InsertData(
                table: "Modules",
                columns: moduleColumns,
                values: new object[,]
                {
                    // Course 1 – Software Engineering  (Lecturer: James Anderson)
                    {  1,  1, "Programming Fundamentals",  "Basics of C# and .NET programming",            1, true },
                    {  2,  1, "OOP Concepts",              "Object-oriented design principles",             2, true },
                    {  3,  1, "Web Development",           "ASP.NET MVC and Razor Pages development",      3, true },
                    {  4,  1, "Software Testing",          "Testing methodologies and QA",                  4, true },
                    // Course 2 – Computer Science  (Lecturer: James Anderson)
                    {  5,  2, "Data Structures",           "Arrays, lists, trees and graphs",               1, true },
                    {  6,  2, "Algorithms",                "Algorithm design and complexity analysis",       2, true },
                    {  7,  2, "Operating Systems",         "Processes, memory and file systems",            3, true },
                    {  8,  2, "Databases",                 "Relational database design and SQL",            4, true },
                    // Course 3 – Information Technology  (Lecturer: Sarah Mitchell)
                    {  9,  3, "Networking Basics",         "OSI model and network fundamentals",            1, true },
                    { 10,  3, "System Administration",     "Managing servers and IT systems",               2, true },
                    { 11,  3, "Cloud Computing",           "Cloud services, AWS and Azure deployment",      3, true },
                    { 12,  3, "Cyber Security Basics",     "Security principles and best practices",        4, true },
                    // Course 4 – Cyber Security  (Lecturer: Sarah Mitchell)
                    { 13,  4, "Network Security",          "Firewalls, VPNs and network protection",        1, true },
                    { 14,  4, "Ethical Hacking",           "Penetration testing methodologies",             2, true },
                    { 15,  4, "Cryptography",              "Symmetric and asymmetric encryption",           3, true },
                    { 16,  4, "Digital Forensics",         "Incident investigation techniques",             4, true },
                    // Course 5 – Artificial Intelligence  (Lecturer: David Clarke)
                    { 17,  5, "AI Fundamentals",           "Introduction to artificial intelligence",       1, true },
                    { 18,  5, "Machine Learning",          "Supervised and unsupervised ML algorithms",     2, true },
                    { 19,  5, "Neural Networks",           "Deep learning and backpropagation",             3, true },
                    { 20,  5, "AI Applications",           "Real-world AI use cases and deployment",        4, true },
                    // Course 6 – Business Administration  (Lecturer: Emily Roberts)
                    { 21,  6, "Management Principles",     "Core management theory and practice",           1, true },
                    { 22,  6, "Organizational Behavior",   "Workplace psychology and team dynamics",        2, true },
                    { 23,  6, "Business Ethics",           "Ethical decision making in business",           3, true },
                    { 24,  6, "Strategic Management",      "Business strategy and competitive advantage",   4, true },
                    // Course 7 – Marketing Management  (Lecturer: Emily Roberts)
                    { 25,  7, "Marketing Basics",          "Marketing fundamentals and the 4Ps",            1, true },
                    { 26,  7, "Digital Marketing",         "SEO, social media and online marketing",        2, true },
                    { 27,  7, "Consumer Behavior",         "Customer psychology and buying decisions",      3, true },
                    { 28,  7, "Brand Management",          "Brand building and positioning strategies",     4, true },
                    // Course 8 – Human Resource Management  (Lecturer: Michael Thompson)
                    { 29,  8, "HR Fundamentals",           "Core HR concepts and functions",                1, true },
                    { 30,  8, "Recruitment & Selection",   "Talent acquisition and hiring processes",       2, true },
                    { 31,  8, "Employee Relations",        "Workforce management and labour law",           3, true },
                    { 32,  8, "Performance Management",    "KPIs, appraisals and evaluations",              4, true },
                    // Course 9 – Accounting and Finance  (Lecturer: Michael Thompson)
                    { 33,  9, "Accounting Basics",         "Financial accounting and bookkeeping",          1, true },
                    { 34,  9, "Cost Accounting",           "Cost analysis and budgeting",                   2, true },
                    { 35,  9, "Financial Reporting",       "Preparing financial statements",                3, true },
                    { 36,  9, "Taxation",                  "Tax systems and compliance",                    4, true },
                    // Course 10 – International Business  (Lecturer: Priya Sharma)
                    { 37, 10, "Global Business",           "International trade and globalisation",         1, true },
                    { 38, 10, "Export and Import",         "Trade operations and customs",                  2, true },
                    { 39, 10, "Cross-cultural Management", "Managing a diverse global workforce",           3, true },
                    { 40, 10, "International Finance",     "Foreign exchange and global finance",           4, true }
                });

            // Batches – SEP and JAN intake for every course
            var batchColumns = new[] { "Id", "CourseId", "Name", "StartDate", "EndDate", "MaxStudents", "Status" };

            migrationBuilder.InsertData(
                table: "BatchModels",
                columns: batchColumns,
                values: new object[,]
                {
                    // Course 1 – Software Engineering
                    {  1,  1, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 40, "active" },
                    {  2,  1, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 40, "active" },
                    // Course 2 – Computer Science
                    {  3,  2, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 40, "active" },
                    {  4,  2, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 40, "active" },
                    // Course 3 – Information Technology
                    {  5,  3, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 40, "active" },
                    {  6,  3, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 40, "active" },
                    // Course 4 – Cyber Security
                    {  7,  4, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 35, "active" },
                    {  8,  4, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 35, "active" },
                    // Course 5 – Artificial Intelligence
                    {  9,  5, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 30, "active" },
                    { 10,  5, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 30, "active" },
                    // Course 6 – Business Administration
                    { 11,  6, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 50, "active" },
                    { 12,  6, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 50, "active" },
                    // Course 7 – Marketing Management
                    { 13,  7, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 45, "active" },
                    { 14,  7, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 45, "active" },
                    // Course 8 – Human Resource Management
                    { 15,  8, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 40, "active" },
                    { 16,  8, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 40, "active" },
                    // Course 9 – Accounting and Finance
                    { 17,  9, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 40, "active" },
                    { 18,  9, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 40, "active" },
                    // Course 10 – International Business
                    { 19, 10, "SEP 2024 Batch", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30), 35, "active" },
                    { 20, 10, "JAN 2025 Batch", new DateOnly(2025, 1, 6), new DateOnly(2025, 12, 31), 35, "active" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var batchIds    = Enumerable.Range(1, 20).Cast<object>().ToArray();
            var moduleIds   = Enumerable.Range(1, 40).Cast<object>().ToArray();
            var courseIds   = Enumerable.Range(1, 10).Cast<object>().ToArray();
            var lecturerIds = new object[] { 2, 3, 4, 5, 6, 7 };

            migrationBuilder.DeleteData(table: "BatchModels",     keyColumn: "Id", keyValues: batchIds);
            migrationBuilder.DeleteData(table: "Modules",         keyColumn: "Id", keyValues: moduleIds);
            migrationBuilder.DeleteData(table: "Courses",         keyColumn: "Id", keyValues: courseIds);
            migrationBuilder.DeleteData(table: "UserDepartments", keyColumn: "UserId", keyValues: lecturerIds);
            migrationBuilder.DeleteData(table: "Users",           keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7 });
            migrationBuilder.DeleteData(table: "Departments",     keyColumn: "Id", keyValues: new object[] { 1, 2 });
            migrationBuilder.DeleteData(table: "Roles",           keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
        }
    }
}

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

            // ?? Roles ????????????????????????????????????????????????????????????
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name", "Description" },
                values: new object[,]
                {
                    { 1, "Administrator", "Full system access" },
                    { 2, "Lecturer",      "Can manage courses, modules, assignments and exams" },
                    { 3, "Student",       "Can enroll in courses and submit assignments" }
                });

            // ?? Super-admin user ?????????????????????????????????????????????????
            var hasher = new PasswordHasher<Models.UserModel>();
            var passwordHash = hasher.HashPassword(new Models.UserModel(), "superadmin");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "RoleId", "FirstName", "LastName", "Email", "PasswordHash", "IsActive", "CreatedAt" },
                values: new object[] { 1, 1, "Super", "Admin", "superadmin@gmail.com", passwordHash, true, now });

            // ?? Departments ??????????????????????????????????????????????????????
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Name", "Code", "Description" },
                values: new object[,]
                {
                    { 1, "Computing",          "CSE", "Department of Computer Science and Software Engineering" },
                    { 2, "Business Management","BUS", "Department of Business and Management Studies"          },
                    { 3, "Engineering",        "ENG", "Department of Engineering and Technology"               },
                    { 4, "Design",             "DES", "Department of Creative Design and Media"                },
                    { 5, "Data Science",       "DS",  "Department of Data Science and Analytics"               }
                });

            // ?? Courses ??????????????????????????????????????????????????????????
            var courseColumns = new[] { "Id", "DepartmentId", "CreatedBy", "Title", "Code", "Description", "Thumbnail", "Status", "CreatedAt" };

            migrationBuilder.InsertData(
                table: "Courses",
                columns: courseColumns,
                values: new object[,]
                {
                    // ?? Computing (dept 1)
                    {  1, 1, 1, "BSc Software Engineering",      "CSE101", "Software development lifecycle and engineering practices", "se.jpg",           "published", now },
                    {  2, 1, 1, "BSc Computer Science",          "CSE102", "Core computing concepts and programming",                  "cs.jpg",           "published", now },
                    {  3, 1, 1, "BSc Information Technology",    "CSE103", "IT infrastructure and systems management",                 "it.jpg",           "published", now },
                    {  4, 1, 1, "BSc Cyber Security",            "CSE104", "Network security and ethical hacking",                     "cyber.jpg",        "published", now },
                    {  5, 1, 1, "BSc Artificial Intelligence",   "CSE105", "AI concepts and intelligent systems",                      "ai.jpg",           "published", now },
                    // ?? Business (dept 2)
                    {  6, 2, 1, "BA Business Administration",    "BUS201", "Business management and leadership",                       "ba.jpg",           "published", now },
                    {  7, 2, 1, "BA Marketing Management",       "BUS202", "Marketing strategies and branding",                        "marketing.jpg",    "published", now },
                    {  8, 2, 1, "BA Human Resource Management",  "BUS203", "HR practices and employee management",                     "hr.jpg",           "published", now },
                    {  9, 2, 1, "BA Accounting and Finance",     "BUS204", "Financial reporting and accounting",                       "finance.jpg",      "published", now },
                    { 10, 2, 1, "BA International Business",     "BUS205", "Global trade and business operations",                     "intl.jpg",         "published", now },
                    // ?? Engineering (dept 3)
                    { 11, 3, 1, "BEng Mechanical Engineering",   "ENG301", "Mechanical systems and design",                            "me.jpg",           "published", now },
                    { 12, 3, 1, "BEng Civil Engineering",        "ENG302", "Infrastructure and construction engineering",              "ce.jpg",           "published", now },
                    { 13, 3, 1, "BEng Electrical Engineering",   "ENG303", "Electrical systems and circuits",                          "ee.jpg",           "published", now },
                    { 14, 3, 1, "BEng Electronics Engineering",  "ENG304", "Electronic devices and systems",                           "elec.jpg",         "published", now },
                    { 15, 3, 1, "BEng Mechatronics Engineering", "ENG305", "Integration of mechanical and electronic systems",          "mechatronics.jpg", "published", now },
                    // ?? Design (dept 4)
                    { 16, 4, 1, "BA Graphic Design",             "DES401", "Visual communication and design principles",               "graphic.jpg",      "published", now },
                    { 17, 4, 1, "BA Multimedia Design",          "DES402", "Digital media and animation",                              "multimedia.jpg",   "published", now },
                    { 18, 4, 1, "BA UI/UX Design",               "DES403", "User interface and experience design",                     "uiux.jpg",         "published", now },
                    { 19, 4, 1, "BA Interior Design",            "DES404", "Interior space planning and design",                       "interior.jpg",     "published", now },
                    { 20, 4, 1, "BA Fashion Design",             "DES405", "Clothing and fashion industry design",                     "fashion.jpg",      "published", now },
                    // ?? Data Science (dept 5)
                    { 21, 5, 1, "BSc Data Science",              "DS501",  "Data analysis and visualization",                          "ds.jpg",           "published", now },
                    { 22, 5, 1, "BSc Machine Learning",          "DS502",  "Machine learning algorithms and models",                   "ml.jpg",           "published", now },
                    { 23, 5, 1, "BSc Big Data Analytics",        "DS503",  "Big data processing technologies",                         "bigdata.jpg",      "published", now },
                    { 24, 5, 1, "BSc Business Analytics",        "DS504",  "Data-driven business decision making",                     "ba.jpg",           "published", now },
                    { 25, 5, 1, "BSc Artificial Intelligence",   "DS505",  "Advanced AI and neural networks",                          "ai2.jpg",          "published", now }
                });

            // ?? Modules ??????????????????????????????????????????????????????????
            var moduleColumns = new[] { "Id", "CourseId", "Title", "Description", "OrderIndex", "IsPublished" };

            migrationBuilder.InsertData(
                table: "Modules",
                columns: moduleColumns,
                values: new object[,]
                {
                    // Course 1 – Software Engineering
                    {   1,  1, "Programming Fundamentals", "Basics of C# programming",              1, true },
                    {   2,  1, "OOP Concepts",             "Object-oriented design principles",      2, true },
                    {   3,  1, "Web Development",          "ASP.NET MVC development",                3, true },
                    {   4,  1, "Software Testing",         "Testing methodologies and QA",           4, true },
                    // Course 2 – Computer Science
                    {   5,  2, "Data Structures",          "Core data structures",                   1, true },
                    {   6,  2, "Algorithms",               "Algorithm design and analysis",          2, true },
                    {   7,  2, "Operating Systems",        "Processes and memory",                   3, true },
                    {   8,  2, "Databases",                "Relational database systems",            4, true },
                    // Course 3 – Information Technology
                    {   9,  3, "Networking Basics",        "Network fundamentals",                   1, true },
                    {  10,  3, "System Administration",    "Managing IT systems",                    2, true },
                    {  11,  3, "Cloud Computing",          "Cloud services and deployment",          3, true },
                    {  12,  3, "Cyber Security Basics",    "Security principles",                    4, true },
                    // Course 4 – Cyber Security
                    {  13,  4, "Network Security",         "Securing networks",                      1, true },
                    {  14,  4, "Ethical Hacking",          "Penetration testing basics",             2, true },
                    {  15,  4, "Cryptography",             "Encryption techniques",                  3, true },
                    {  16,  4, "Digital Forensics",        "Investigation techniques",               4, true },
                    // Course 5 – Artificial Intelligence
                    {  17,  5, "AI Fundamentals",          "Introduction to AI",                     1, true },
                    {  18,  5, "Machine Learning",         "ML algorithms",                          2, true },
                    {  19,  5, "Neural Networks",          "Deep learning basics",                   3, true },
                    {  20,  5, "AI Applications",          "Real-world AI usage",                    4, true },
                    // Course 6 – Business Administration
                    {  21,  6, "Management Principles",    "Core management theory",                 1, true },
                    {  22,  6, "Organizational Behavior",  "Workplace behavior",                     2, true },
                    {  23,  6, "Business Ethics",          "Ethical decision making",                3, true },
                    {  24,  6, "Strategic Management",     "Business strategies",                    4, true },
                    // Course 7 – Marketing Management
                    {  25,  7, "Marketing Basics",         "Marketing fundamentals",                 1, true },
                    {  26,  7, "Digital Marketing",        "Online marketing tools",                 2, true },
                    {  27,  7, "Consumer Behavior",        "Customer psychology",                    3, true },
                    {  28,  7, "Brand Management",         "Brand strategies",                       4, true },
                    // Course 8 – Human Resource Management
                    {  29,  8, "HR Fundamentals",          "HR concepts",                            1, true },
                    {  30,  8, "Recruitment",              "Hiring processes",                       2, true },
                    {  31,  8, "Employee Relations",       "Workforce management",                   3, true },
                    {  32,  8, "Performance Management",   "Evaluations and KPIs",                   4, true },
                    // Course 9 – Accounting and Finance
                    {  33,  9, "Accounting Basics",        "Financial accounting",                   1, true },
                    {  34,  9, "Cost Accounting",          "Cost analysis",                          2, true },
                    {  35,  9, "Financial Reporting",      "Reports and statements",                 3, true },
                    {  36,  9, "Taxation",                 "Tax systems",                            4, true },
                    // Course 10 – International Business
                    {  37, 10, "Global Business",          "International trade",                    1, true },
                    {  38, 10, "Export/Import",            "Trade operations",                       2, true },
                    {  39, 10, "Cross-cultural Mgmt",      "Global workforce",                       3, true },
                    {  40, 10, "International Finance",    "Global finance",                         4, true },
                    // Course 11 – Mechanical Engineering
                    {  41, 11, "Thermodynamics",           "Heat and energy systems",                1, true },
                    {  42, 11, "Fluid Mechanics",          "Fluid behavior",                         2, true },
                    {  43, 11, "Machine Design",           "Mechanical design",                      3, true },
                    {  44, 11, "Manufacturing",            "Production processes",                   4, true },
                    // Course 12 – Civil Engineering
                    {  45, 12, "Structural Engineering",   "Building structures",                    1, true },
                    {  46, 12, "Geotechnics",              "Soil mechanics",                         2, true },
                    {  47, 12, "Transportation",           "Road systems",                           3, true },
                    {  48, 12, "Hydraulics",               "Water systems",                          4, true },
                    // Course 13 – Electrical Engineering
                    {  49, 13, "Circuit Theory",           "Electrical circuits",                    1, true },
                    {  50, 13, "Power Systems",            "Electric power systems",                 2, true },
                    {  51, 13, "Control Systems",          "Automation basics",                      3, true },
                    {  52, 13, "Electrical Machines",      "Motors and generators",                  4, true },
                    // Course 14 – Electronics Engineering
                    {  53, 14, "Analog Electronics",       "Analog circuits",                        1, true },
                    {  54, 14, "Digital Electronics",      "Digital logic",                          2, true },
                    {  55, 14, "Embedded Systems",         "Microcontrollers",                       3, true },
                    {  56, 14, "Signal Processing",        "Signals and systems",                    4, true },
                    // Course 15 – Mechatronics Engineering
                    {  57, 15, "Robotics",                 "Robot systems",                          1, true },
                    {  58, 15, "Automation",               "Industrial automation",                  2, true },
                    {  59, 15, "Control Engineering",      "System control",                         3, true },
                    {  60, 15, "Mechatronic Design",       "Integrated systems",                     4, true },
                    // Course 16 – Graphic Design
                    {  61, 16, "Design Principles",        "Visual fundamentals",                    1, true },
                    {  62, 16, "Typography",               "Text design",                            2, true },
                    {  63, 16, "Branding",                 "Visual identity",                        3, true },
                    {  64, 16, "Illustration",             "Creative drawing",                       4, true },
                    // Course 17 – Multimedia Design
                    {  65, 17, "Animation",                "Motion graphics",                        1, true },
                    {  66, 17, "Video Editing",            "Editing tools",                          2, true },
                    {  67, 17, "3D Modeling",              "3D design",                              3, true },
                    {  68, 17, "Multimedia Tools",         "Creative software",                      4, true },
                    // Course 18 – UI/UX Design
                    {  69, 18, "UX Research",              "User research methods",                  1, true },
                    {  70, 18, "Wireframing",              "Design layouts",                         2, true },
                    {  71, 18, "Prototyping",              "Interactive design",                     3, true },
                    {  72, 18, "Usability Testing",        "User testing",                           4, true },
                    // Course 19 – Interior Design
                    {  73, 19, "Interior Concepts",        "Design basics",                          1, true },
                    {  74, 19, "Space Planning",           "Layout design",                          2, true },
                    {  75, 19, "Lighting Design",          "Lighting systems",                       3, true },
                    {  76, 19, "Furniture Design",         "Furniture concepts",                     4, true },
                    // Course 20 – Fashion Design
                    {  77, 20, "Fashion Illustration",     "Clothing sketches",                      1, true },
                    {  78, 20, "Textiles",                 "Fabric studies",                         2, true },
                    {  79, 20, "Garment Construction",     "Clothing design",                        3, true },
                    {  80, 20, "Fashion Marketing",        "Fashion business",                       4, true },
                    // Course 21 – Data Science
                    {  81, 21, "Data Analysis",            "Data exploration",                       1, true },
                    {  82, 21, "Statistics",               "Statistical methods",                    2, true },
                    {  83, 21, "Data Visualization",       "Charts and dashboards",                  3, true },
                    {  84, 21, "Python for Data",          "Python programming",                     4, true },
                    // Course 22 – Machine Learning
                    {  85, 22, "Supervised Learning",      "Regression & classification",            1, true },
                    {  86, 22, "Unsupervised Learning",    "Clustering methods",                     2, true },
                    {  87, 22, "Model Evaluation",         "Performance metrics",                    3, true },
                    {  88, 22, "ML Projects",              "Real-world ML",                          4, true },
                    // Course 23 – Big Data Analytics
                    {  89, 23, "Big Data Intro",           "Big data concepts",                      1, true },
                    {  90, 23, "Hadoop",                   "Distributed storage",                    2, true },
                    {  91, 23, "Spark",                    "Data processing",                        3, true },
                    {  92, 23, "Data Pipelines",           "ETL processes",                          4, true },
                    // Course 24 – Business Analytics
                    {  93, 24, "Business Intelligence",    "BI tools",                               1, true },
                    {  94, 24, "Data Warehousing",         "Data storage",                           2, true },
                    {  95, 24, "Analytics Tools",          "Analysis tools",                         3, true },
                    {  96, 24, "Decision Making",          "Data-driven decisions",                  4, true },
                    // Course 25 – AI (Data Science dept)
                    {  97, 25, "Deep Learning",            "Neural networks",                        1, true },
                    {  98, 25, "NLP",                      "Language processing",                    2, true },
                    {  99, 25, "Computer Vision",          "Image processing",                       3, true },
                    { 100, 25, "AI Systems",               "AI deployment",                          4, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var moduleIds = Enumerable.Range(1, 100).Cast<object>().ToArray();
            var courseIds  = Enumerable.Range(1, 25).Cast<object>().ToArray();

            migrationBuilder.DeleteData(table: "Modules",     keyColumn: "Id", keyValues: moduleIds);
            migrationBuilder.DeleteData(table: "Courses",     keyColumn: "Id", keyValues: courseIds);
            migrationBuilder.DeleteData(table: "Users",       keyColumn: "Id", keyValues: new object[] { 1 });
            migrationBuilder.DeleteData(table: "Departments", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5 });
            migrationBuilder.DeleteData(table: "Roles",       keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
        }
    }
}

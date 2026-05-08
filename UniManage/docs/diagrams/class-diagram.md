# Class Diagram

Domain model class relationships for the UniManage system.

```mermaid
classDiagram
    direction TB

    class RoleModel {
        +int Id
        +string Name
        +string? Description
        +ICollection~UserModel~ Users
    }

    class UserModel {
        +int Id
        +int RoleId
        +string FirstName
        +string LastName
        +string Email
        +string PasswordHash
        +string? Phone
        +string? NIC
        +string? UID
        +string? ProfilePicture
        +bool IsActive
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +RoleModel? Role
        +ICollection~UserDepartmentModel~ UserDepartments
        +ICollection~EnrollmentModel~ Enrollments
        +ICollection~CourseModel~ CreatedCourses
        +ICollection~AssignmentSubmissionModel~ AssignmentSubmissions
        +ICollection~ExamResultModel~ ExamResults
        +ICollection~AnnouncementModel~ Announcements
        +ICollection~ExamModel~ CreatedExams
        +ICollection~AssignmentModel~ CreatedAssignments
    }

    class DepartmentModel {
        +int Id
        +string Name
        +string? Code
        +string? Description
        +ICollection~UserDepartmentModel~ UserDepartments
        +ICollection~CourseModel~ Courses
    }

    class UserDepartmentModel {
        +int UserId
        +int DepartmentId
        +UserModel? User
        +DepartmentModel? Department
    }

    class CourseModel {
        +int Id
        +int DepartmentId
        +int CreatedBy
        +string Title
        +string? Code
        +string? Description
        +string? Thumbnail
        +string Status
        +string? PrerequisiteCourses
        +DateTime CreatedAt
        +DepartmentModel? Department
        +UserModel? Creator
        +ICollection~BatchModel~ Batches
        +ICollection~EnrollmentModel~ Enrollments
        +ICollection~ModuleModel~ Modules
        +ICollection~AnnouncementModel~ Announcements
    }

    class BatchModel {
        +int Id
        +int CourseId
        +string Name
        +DateOnly? StartDate
        +DateOnly? EndDate
        +int? MaxStudents
        +string Status
        +CourseModel? Course
        +ICollection~EnrollmentModel~ Enrollments
    }

    class EnrollmentModel {
        +int Id
        +int UserId
        +int BatchId
        +int CourseId
        +DateTime EnrolledAt
        +string Status
        +UserModel? User
        +BatchModel? Batch
        +CourseModel? Course
    }

    class EnrollmentApplicationModel {
        +int Id
        +int StudentId
        +int CourseId
        +string DocumentType
        +byte[]? DocumentData
        +string? DocumentFileName
        +string? DocumentContentType
        +string? StudentNote
        +string Status
        +string? AdminNote
        +DateTime AppliedAt
        +DateTime? ReviewedAt
        +int? ReviewedBy
        +UserModel? Student
        +CourseModel? Course
        +UserModel? Reviewer
    }

    class ModuleModel {
        +int Id
        +int CourseId
        +string Title
        +string? Description
        +int OrderIndex
        +bool IsPublished
        +int? LecturerId
        +CourseModel? Course
        +UserModel? Lecturer
        +ICollection~CourseMaterialModel~ CourseMaterials
        +ICollection~AssignmentModel~ Assignments
        +ICollection~ExamModel~ Exams
    }

    class CourseMaterialModel {
        +int Id
        +int ModuleId
        +string Title
        +byte[]? FileData
        +string? FileName
        +string? ContentType
        +string? FileType
        +int? FileSize
        +DateTime UploadedAt
        +ModuleModel? Module
    }

    class AssignmentModel {
        +int Id
        +int ModuleId
        +int? CreatedBy
        +string Title
        +string? Description
        +string? Instructions
        +int MaxMarks
        +DateTime? ReleaseDate
        +DateTime? DeadlineDate
        +bool AllowLateSubmit
        +byte[]? FileData
        +string? FileName
        +int? FileSize
        +string? ContentType
        +DateTime CreatedAt
        +ModuleModel? Module
        +UserModel? Creator
        +ICollection~AssignmentSubmissionModel~ Submissions
    }

    class AssignmentSubmissionModel {
        +int Id
        +int AssignmentId
        +int StudentId
        +byte[]? FileData
        +string? FileName
        +int? FileSize
        +string? ContentType
        +DateTime SubmittedAt
        +int? MarksObtained
        +string? Feedback
        +string Status
        +int? GradedBy
        +DateTime? GradedAt
        +AssignmentModel? Assignment
        +UserModel? Student
    }

    class ExamModel {
        +int Id
        +int ModuleId
        +int? CreatedBy
        +string Title
        +string? Description
        +int TotalMarks
        +int? PassMarks
        +int? DurationMinutes
        +DateTime? ExamDate
        +DateTime CreatedAt
        +ModuleModel? Module
        +UserModel? Creator
        +ICollection~ExamResultModel~ ExamResults
    }

    class ExamResultModel {
        +int Id
        +int ExamId
        +int StudentId
        +int? MarksObtained
        +string? Status
        +DateTime? SubmittedAt
        +ExamModel? Exam
        +UserModel? Student
    }

    class AnnouncementModel {
        +int Id
        +int CourseId
        +int? CreatedBy
        +string Title
        +string Content
        +string? TargetRole
        +DateTime? PublishDate
        +DateTime CreatedAt
        +CourseModel? Course
        +UserModel? Creator
    }

    class PasswordResetOtpModel {
        +int Id
        +string Email
        +string OtpCode
        +DateTime ExpiresAt
        +bool IsVerified
        +bool IsUsed
        +DateTime CreatedAt
    }

    %% Relationships
    RoleModel "1" --> "0..*" UserModel : has

    UserModel "1" --> "0..*" UserDepartmentModel : belongs to
    DepartmentModel "1" --> "0..*" UserDepartmentModel : contains

    DepartmentModel "1" --> "0..*" CourseModel : offers
    UserModel "1" --> "0..*" CourseModel : creates

    CourseModel "1" --> "0..*" BatchModel : has
    CourseModel "1" --> "0..*" ModuleModel : contains
    CourseModel "1" --> "0..*" AnnouncementModel : has
    CourseModel "1" --> "0..*" EnrollmentModel : tracks
    CourseModel "1" --> "0..*" EnrollmentApplicationModel : receives

    BatchModel "1" --> "0..*" EnrollmentModel : groups
    UserModel "1" --> "0..*" EnrollmentModel : enrolled in

    ModuleModel "1" --> "0..*" CourseMaterialModel : holds
    ModuleModel "1" --> "0..*" AssignmentModel : has
    ModuleModel "1" --> "0..*" ExamModel : has
    UserModel "0..1" --> "0..*" ModuleModel : teaches

    AssignmentModel "1" --> "0..*" AssignmentSubmissionModel : receives
    UserModel "1" --> "0..*" AssignmentSubmissionModel : submits

    ExamModel "1" --> "0..*" ExamResultModel : records
    UserModel "1" --> "0..*" ExamResultModel : sits

    UserModel "1" --> "0..*" AnnouncementModel : creates
    UserModel "1" --> "0..*" EnrollmentApplicationModel : applies
```

## Controller — Model Mapping

| Controller                         | Primary Models Used                            |
| ---------------------------------- | ---------------------------------------------- |
| `AccountController`                | `UserModel`, `PasswordResetOtpModel`           |
| `DashboardController`              | `UserModel`, `CourseModel`, `EnrollmentModel`  |
| `CoursesController`                | `CourseModel`, `DepartmentModel`               |
| `BatchesController`                | `BatchModel`, `CourseModel`                    |
| `ModulesController`                | `ModuleModel`, `CourseModel`                   |
| `CourseMaterialsController`        | `CourseMaterialModel`, `ModuleModel`           |
| `AssignmentsController`            | `AssignmentModel`, `ModuleModel`               |
| `AssignmentSubmissionsController`  | `AssignmentSubmissionModel`, `AssignmentModel` |
| `ExamResultsController`            | `ExamResultModel`, `ExamModel`                 |
| `EnrollmentsController`            | `EnrollmentModel`, `BatchModel`, `CourseModel` |
| `EnrollmentApplicationsController` | `EnrollmentApplicationModel`, `CourseModel`    |
| `AnnouncementsController`          | `AnnouncementModel`, `CourseModel`             |
| `DepartmentsController`            | `DepartmentModel`                              |
| `UsersController`                  | `UserModel`, `RoleModel`, `DepartmentModel`    |
| `RolesController`                  | `RoleModel`                                    |
| `ReportsController`                | Multiple models (read-only aggregates)         |

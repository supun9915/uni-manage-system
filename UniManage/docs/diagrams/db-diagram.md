# Database Diagram

Entity-Relationship diagram for the UniManage MySQL database. Schema is managed via Entity Framework Core migrations.

```mermaid
erDiagram

    Roles {
        int Id PK
        varchar(50) Name
        text Description
    }

    Users {
        int Id PK
        int RoleId FK
        varchar(100) FirstName
        varchar(100) LastName
        varchar(255) Email
        text PasswordHash
        varchar(20) Phone
        varchar(20) NIC
        varchar(20) UID
        text ProfilePicture
        tinyint IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }

    PasswordResetOtps {
        int Id PK
        varchar(255) Email
        varchar(6) OtpCode
        datetime ExpiresAt
        tinyint IsVerified
        tinyint IsUsed
        datetime CreatedAt
    }

    Departments {
        int Id PK
        varchar(150) Name
        varchar(20) Code
        text Description
    }

    UserDepartments {
        int UserId PK_FK
        int DepartmentId PK_FK
    }

    Courses {
        int Id PK
        int DepartmentId FK
        int CreatedBy FK
        varchar(200) Title
        varchar(20) Code
        text Description
        text Thumbnail
        varchar(20) Status
        text PrerequisiteCourses
        datetime CreatedAt
    }

    Batches {
        int Id PK
        int CourseId FK
        varchar(100) Name
        date StartDate
        date EndDate
        int MaxStudents
        varchar(20) Status
    }

    Enrollments {
        int Id PK
        int UserId FK
        int BatchId FK
        int CourseId FK
        datetime EnrolledAt
        varchar(20) Status
    }

    EnrollmentApplications {
        int Id PK
        int StudentId FK
        int CourseId FK
        varchar(100) DocumentType
        longblob DocumentData
        varchar(255) DocumentFileName
        varchar(100) DocumentContentType
        text StudentNote
        varchar(20) Status
        text AdminNote
        datetime AppliedAt
        datetime ReviewedAt
        int ReviewedBy FK
    }

    Modules {
        int Id PK
        int CourseId FK
        int LecturerId FK
        varchar(200) Title
        text Description
        int OrderIndex
        tinyint IsPublished
    }

    CourseMaterials {
        int Id PK
        int ModuleId FK
        varchar(200) Title
        longblob FileData
        varchar(255) FileName
        varchar(100) ContentType
        varchar(20) FileType
        int FileSize
        datetime UploadedAt
    }

    Assignments {
        int Id PK
        int ModuleId FK
        int CreatedBy FK
        varchar(200) Title
        text Description
        text Instructions
        int MaxMarks
        datetime ReleaseDate
        datetime DeadlineDate
        tinyint AllowLateSubmit
        longblob FileData
        varchar(255) FileName
        int FileSize
        varchar(100) ContentType
        datetime CreatedAt
    }

    AssignmentSubmissions {
        int Id PK
        int AssignmentId FK
        int StudentId FK
        longblob FileData
        varchar(255) FileName
        int FileSize
        varchar(100) ContentType
        datetime SubmittedAt
        int MarksObtained
        text Feedback
        varchar(20) Status
        int GradedBy
        datetime GradedAt
    }

    Exams {
        int Id PK
        int ModuleId FK
        int CreatedBy FK
        varchar(200) Title
        text Description
        int TotalMarks
        int PassMarks
        int DurationMinutes
        datetime ExamDate
        datetime CreatedAt
    }

    ExamResults {
        int Id PK
        int ExamId FK
        int StudentId FK
        int MarksObtained
        varchar(20) Status
        datetime SubmittedAt
    }

    Announcements {
        int Id PK
        int CourseId FK
        int CreatedBy FK
        varchar(200) Title
        text Content
        varchar(20) TargetRole
        datetime PublishDate
        datetime CreatedAt
    }

    %% Relationships
    Roles ||--o{ Users : "has"

    Users ||--o{ UserDepartments : "belongs to"
    Departments ||--o{ UserDepartments : "contains"

    Departments ||--o{ Courses : "offers"
    Users ||--o{ Courses : "creates"

    Courses ||--o{ Batches : "has"
    Courses ||--o{ Modules : "contains"
    Courses ||--o{ Enrollments : "tracks"
    Courses ||--o{ EnrollmentApplications : "receives"
    Courses ||--o{ Announcements : "has"

    Batches ||--o{ Enrollments : "groups"
    Users ||--o{ Enrollments : "enrolled in"

    Users ||--o{ EnrollmentApplications : "applies"
    Users ||--o{ EnrollmentApplications : "reviews"

    Users ||--o{ Modules : "lectures"
    Modules ||--o{ CourseMaterials : "holds"
    Modules ||--o{ Assignments : "has"
    Modules ||--o{ Exams : "has"

    Assignments ||--o{ AssignmentSubmissions : "receives"
    Users ||--o{ AssignmentSubmissions : "submits"

    Exams ||--o{ ExamResults : "records"
    Users ||--o{ ExamResults : "sits"

    Users ||--o{ Announcements : "creates"
```

## Table Summary

| Table                    | Rows | Notes                                       |
| ------------------------ | ---- | ------------------------------------------- |
| `Roles`                  | ~3   | student, lecturer, administrator            |
| `Users`                  | Many | Polymorphic — role differentiates behaviour |
| `Departments`            | Many | Faculty/school groupings                    |
| `UserDepartments`        | Many | Composite PK junction table                 |
| `Courses`                | Many | Status: draft / published / archived        |
| `Batches`                | Many | Per-course intake groups                    |
| `Enrollments`            | Many | Student ↔ Batch ↔ Course                    |
| `EnrollmentApplications` | Many | Status: pending / approved / rejected       |
| `Modules`                | Many | Ordered units within a course               |
| `CourseMaterials`        | Many | Binary files stored as `longblob`           |
| `Assignments`            | Many | Binary attachment optional                  |
| `AssignmentSubmissions`  | Many | Status: submitted / graded / late           |
| `Exams`                  | Many | Scheduled per module                        |
| `ExamResults`            | Many | Status: pass / fail / absent                |
| `Announcements`          | Many | Target: all / student / lecturer            |
| `PasswordResetOtps`      | Many | Expiry-controlled, single-use OTP           |

## Key Constraints

- All FK relationships use `int` surrogate keys with auto-increment.
- `UserDepartments` uses a **composite primary key** (`UserId`, `DepartmentId`).
- File data (materials, assignments, submissions, enrollment documents) is stored as **`longblob`** directly in MySQL.
- Soft-delete pattern used via `IsActive` on `Users` and `Status` fields on courses, batches, and enrollments.

# Database Structure – UniManage

Database engine: **MySQL** (via Pomelo EF Core provider)  
Database name: `UniManageDB`  
Schema managed by: EF Core Code-First Migrations

---

## Entity Relationship Overview

```
Roles ??< Users >??< UserDepartments >?? Departments
                ?
                ???< Enrollments >?? Batches ??< Courses >?? Departments
                ?                                    ?
                ?                              ??< Modules >?? CourseMaterials
                ?                              ?           ??? Assignments ??< AssignmentSubmissions
                ?                              ?           ??? Exams       ??< ExamResults
                ?
                ???< EnrollmentApplications >?? Courses
                ???< Announcements          >?? Courses
                ???< Messages (sender)
                ???< Messages (receiver)
                ???< PasswordResetOtps
```

---

## Tables

### `Roles`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | Role ID |
| `Name` | VARCHAR(50) | NOT NULL | e.g. `administrator`, `lecturer`, `student` |

---

### `Users`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | User ID |
| `RoleId` | INT | NOT NULL, FK ? Roles.Id | User's role |
| `FirstName` | VARCHAR(100) | NOT NULL | |
| `LastName` | VARCHAR(100) | NOT NULL | |
| `Email` | VARCHAR(255) | NOT NULL | Unique login email |
| `PasswordHash` | TEXT | NOT NULL | ASP.NET Identity `PasswordHasher` output |
| `Phone` | VARCHAR(20) | NULL | |
| `NIC` | VARCHAR(20) | NULL | National Identity Card number |
| `UID` | VARCHAR(20) | NULL | University ID |
| `ProfilePicture` | TEXT | NULL | Path or base64 string |
| `IsActive` | TINYINT(1) | DEFAULT 1 | Soft-disable accounts |
| `CreatedAt` | DATETIME | DEFAULT UTC_NOW | |
| `UpdatedAt` | DATETIME | NULL | |

**Relations:** `Users.RoleId ? Roles.Id`

---

### `Departments`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `Name` | VARCHAR(100) | NOT NULL | Department name |
| `Description` | TEXT | NULL | |

---

### `UserDepartments`  *(composite PK join table)*

| Column | Type | Constraints | Description |
|---|---|---|---|
| `UserId` | INT | PK (composite), FK ? Users.Id | |
| `DepartmentId` | INT | PK (composite), FK ? Departments.Id | |

---

### `Courses`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `DepartmentId` | INT | NOT NULL, FK ? Departments.Id | |
| `CreatedBy` | INT | NOT NULL, FK ? Users.Id | Admin who created the course |
| `Title` | VARCHAR(200) | NOT NULL | |
| `Code` | VARCHAR(20) | NULL | e.g. `CS101` |
| `Description` | TEXT | NULL | |
| `Thumbnail` | TEXT | NULL | Image path |
| `Status` | VARCHAR(20) | DEFAULT `draft` | `draft` \| `published` \| `archived` |
| `PrerequisiteCourses` | TEXT | NULL | JSON or comma-separated course IDs |
| `CreatedAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `Courses.DepartmentId ? Departments.Id`, `Courses.CreatedBy ? Users.Id`

---

### `BatchModels`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `CourseId` | INT | NOT NULL, FK ? Courses.Id | |
| `Name` | VARCHAR(100) | NOT NULL | e.g. `Batch 2024` |
| `StartDate` | DATE | NULL | |
| `EndDate` | DATE | NULL | |
| `MaxStudents` | INT | NULL | Capacity cap |
| `Status` | VARCHAR(20) | DEFAULT `active` | `active` \| `closed` |

**Relations:** `BatchModels.CourseId ? Courses.Id`

---

### `Modules`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `CourseId` | INT | NOT NULL, FK ? Courses.Id | |
| `LecturerId` | INT | NULL, FK ? Users.Id | Assigned lecturer |
| `Title` | VARCHAR(200) | NOT NULL | |
| `Description` | TEXT | NULL | |
| `OrderIndex` | INT | NOT NULL | Display order within course |
| `IsPublished` | TINYINT(1) | DEFAULT 0 | Visibility to students |

**Relations:** `Modules.CourseId ? Courses.Id`, `Modules.LecturerId ? Users.Id`

---

### `CourseMaterials`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `ModuleId` | INT | NOT NULL, FK ? Modules.Id | |
| `Title` | VARCHAR(200) | NOT NULL | |
| `Description` | TEXT | NULL | |
| `FileData` | LONGBLOB | NULL | Binary file stored in DB |
| `FileName` | VARCHAR(255) | NULL | Original filename |
| `FileSize` | INT | NULL | Bytes |
| `ContentType` | VARCHAR(100) | NULL | MIME type |
| `UploadedAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `CourseMaterials.ModuleId ? Modules.Id`

---

### `Assignments`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `ModuleId` | INT | NOT NULL, FK ? Modules.Id | |
| `CreatedBy` | INT | NULL, FK ? Users.Id | Lecturer who created it |
| `Title` | VARCHAR(200) | NOT NULL | |
| `Description` | TEXT | NULL | |
| `Instructions` | TEXT | NULL | |
| `MaxMarks` | INT | DEFAULT 100 | |
| `ReleaseDate` | DATETIME | NULL | When students can see it |
| `DeadlineDate` | DATETIME | NULL | Submission deadline |
| `AllowLateSubmit` | TINYINT(1) | DEFAULT 0 | |
| `FileData` | LONGBLOB | NULL | Attached brief file |
| `FileName` | VARCHAR(255) | NULL | |
| `FileSize` | INT | NULL | |
| `ContentType` | VARCHAR(100) | NULL | |
| `CreatedAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `Assignments.ModuleId ? Modules.Id`, `Assignments.CreatedBy ? Users.Id`

---

### `AssignmentSubmissions`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `AssignmentId` | INT | NOT NULL, FK ? Assignments.Id | |
| `StudentId` | INT | NOT NULL, FK ? Users.Id | |
| `FileData` | LONGBLOB | NULL | Student-submitted file |
| `FileName` | VARCHAR(255) | NULL | |
| `FileSize` | INT | NULL | |
| `ContentType` | VARCHAR(100) | NULL | |
| `Comment` | TEXT | NULL | Student's note |
| `Grade` | DECIMAL(5,2) | NULL | Marks awarded |
| `Feedback` | TEXT | NULL | Lecturer's feedback |
| `SubmittedAt` | DATETIME | DEFAULT UTC_NOW | |
| `GradedAt` | DATETIME | NULL | |

**Relations:** `AssignmentSubmissions.AssignmentId ? Assignments.Id`, `AssignmentSubmissions.StudentId ? Users.Id`

---

### `Exams`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `ModuleId` | INT | NOT NULL, FK ? Modules.Id | |
| `CreatedBy` | INT | NULL, FK ? Users.Id | |
| `Title` | VARCHAR(200) | NOT NULL | |
| `Description` | TEXT | NULL | |
| `TotalMarks` | INT | DEFAULT 100 | |
| `PassMarks` | INT | NULL | |
| `DurationMinutes` | INT | NULL | |
| `ExamDate` | DATETIME | NULL | Scheduled date/time |
| `CreatedAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `Exams.ModuleId ? Modules.Id`, `Exams.CreatedBy ? Users.Id`

---

### `ExamResults`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `ExamId` | INT | NOT NULL, FK ? Exams.Id | |
| `StudentId` | INT | NOT NULL, FK ? Users.Id | |
| `MarksObtained` | DECIMAL(5,2) | NOT NULL | |
| `IsPassed` | TINYINT(1) | NULL | Derived or set manually |
| `Remarks` | TEXT | NULL | |
| `RecordedAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `ExamResults.ExamId ? Exams.Id`, `ExamResults.StudentId ? Users.Id`

---

### `Enrollments`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `UserId` | INT | NOT NULL, FK ? Users.Id | Student |
| `BatchId` | INT | NOT NULL, FK ? BatchModels.Id | |
| `CourseId` | INT | NOT NULL, FK ? Courses.Id | |
| `EnrolledAt` | DATETIME | DEFAULT UTC_NOW | |
| `Status` | VARCHAR(20) | DEFAULT `active` | `active` \| `dropped` \| `completed` |

**Relations:** `Enrollments.UserId ? Users.Id`, `Enrollments.BatchId ? BatchModels.Id`, `Enrollments.CourseId ? Courses.Id`

---

### `EnrollmentApplications`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `StudentId` | INT | NOT NULL, FK ? Users.Id | Applicant |
| `CourseId` | INT | NOT NULL, FK ? Courses.Id | Target course |
| `DocumentType` | VARCHAR(100) | NOT NULL | e.g. `Transcript`, `Certificate` |
| `DocumentData` | LONGBLOB | NULL | Uploaded supporting document |
| `DocumentFileName` | VARCHAR(255) | NULL | |
| `DocumentContentType` | VARCHAR(100) | NULL | |
| `StudentNote` | TEXT | NULL | |
| `Status` | VARCHAR(20) | DEFAULT `pending` | `pending` \| `approved` \| `rejected` |
| `AdminNote` | TEXT | NULL | Reviewer's comment |
| `AppliedAt` | DATETIME | DEFAULT UTC_NOW | |
| `ReviewedAt` | DATETIME | NULL | |
| `ReviewedBy` | INT | NULL, FK ? Users.Id | Admin who reviewed |

**Relations:** `EnrollmentApplications.StudentId ? Users.Id`, `.CourseId ? Courses.Id`, `.ReviewedBy ? Users.Id`

---

### `Announcements`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `CourseId` | INT | NOT NULL, FK ? Courses.Id | |
| `CreatedBy` | INT | NULL, FK ? Users.Id | |
| `Title` | VARCHAR(200) | NOT NULL | |
| `Content` | TEXT | NOT NULL | |
| `TargetRole` | VARCHAR(20) | NULL | `all` \| `student` \| `lecturer` |
| `PublishDate` | DATETIME | NULL | Scheduled publish |
| `CreatedAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `Announcements.CourseId ? Courses.Id`, `Announcements.CreatedBy ? Users.Id`

---

### `Messages`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `SenderId` | INT | NOT NULL, FK ? Users.Id | |
| `ReceiverId` | INT | NOT NULL, FK ? Users.Id | |
| `ParentMessageId` | INT | NULL, FK ? Messages.Id | Threading – null = root message |
| `Subject` | VARCHAR(200) | NOT NULL | |
| `Body` | TEXT | NOT NULL | |
| `IsReadByReceiver` | TINYINT(1) | DEFAULT 0 | |
| `IsDeletedBySender` | TINYINT(1) | DEFAULT 0 | Soft-delete |
| `IsDeletedByReceiver` | TINYINT(1) | DEFAULT 0 | Soft-delete |
| `SentAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `Messages.SenderId ? Users.Id`, `.ReceiverId ? Users.Id`, `.ParentMessageId ? Messages.Id` (self-referencing)

---

### `PasswordResetOtps`

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | INT | PK, Auto-increment | |
| `UserId` | INT | NOT NULL, FK ? Users.Id | |
| `OtpCode` | VARCHAR(10) | NOT NULL | 6-digit code |
| `ExpiresAt` | DATETIME | NOT NULL | Short-lived (e.g., 15 minutes) |
| `IsUsed` | TINYINT(1) | DEFAULT 0 | Prevents replay |
| `CreatedAt` | DATETIME | DEFAULT UTC_NOW | |

**Relations:** `PasswordResetOtps.UserId ? Users.Id`

---

### ASP.NET Identity Tables (inherited from `IdentityDbContext`)

Although custom session auth is used for login, the project extends `IdentityDbContext`, which creates the following tables automatically (unused at runtime but present in schema):

`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`

---

## Relationship Summary

| Relationship | Type |
|---|---|
| Role ? Users | One-to-Many |
| Department ? Users | Many-to-Many (via `UserDepartments`) |
| Department ? Courses | One-to-Many |
| Course ? Batches | One-to-Many |
| Course ? Modules | One-to-Many |
| Course ? Enrollments | One-to-Many |
| Course ? Announcements | One-to-Many |
| Course ? EnrollmentApplications | One-to-Many |
| Batch ? Enrollments | One-to-Many |
| Module ? CourseMaterials | One-to-Many |
| Module ? Assignments | One-to-Many |
| Module ? Exams | One-to-Many |
| Assignment ? AssignmentSubmissions | One-to-Many |
| Exam ? ExamResults | One-to-Many |
| Message ? Messages (replies) | Self-referencing One-to-Many |

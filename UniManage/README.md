# UniManage � University Management System

UniManage is an ASP.NET Core 8 MVC web application that manages the full academic lifecycle of a university — users, departments, courses, modules, assignments, exams, enrollments, internal messaging, announcements, and reporting — all protected by a custom session-based authentication layer with role-based access control.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [NuGet Packages](#nuget-packages)
4. [Frontend Libraries](#frontend-libraries)
5. [Solution & Folder Structure](#solution--folder-structure)
6. [Database Structure](#database-structure)
7. [Domain Models](#domain-models)
8. [Controllers & Responsibilities](#controllers--responsibilities)
9. [Authentication & Authorization](#authentication--authorization)
10. [Services](#services)
11. [Getting Started](#getting-started)
12. [Default Seed Data](#default-seed-data)
13. [Architecture & Diagrams](#architecture--diagrams)

---

## Project Overview

UniManage covers the complete academic lifecycle of a university through the following feature areas:

| Feature Area                | Description                                                                                                                                     |
| --------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| **User Management**         | Admin creates and manages users with roles: Administrator, Lecturer, Student. Supports profile pictures, NIC/UID fields, and soft-deactivation. |
| **Role Management**         | Named roles stored in the database; `BaseController` enforces role guards on every request.                                                     |
| **Department Management**   | Departments group users and courses. Users can belong to multiple departments via a junction table.                                             |
| **Course Management**       | Courses belong to a department. Status lifecycle: `draft` → `published` → `archived`. Supports prerequisite course notation.                    |
| **Batch Management**        | Each course has intake batches with start/end dates and max student capacity.                                                                   |
| **Module Management**       | Ordered modules within a course. Each module can be assigned to a specific lecturer. Modules are independently publishable.                     |
| **Course Materials**        | Lecturers upload files (PDF, video, link, etc.) per module. Files stored as binary blobs in the database.                                       |
| **Assignments**             | Lecturers create assignments with release/deadline dates, max marks, and optional file attachments. Late submission toggle supported.           |
| **Assignment Submissions**  | Students upload submission files. Lecturers grade with marks and written feedback. Status: `submitted` / `graded` / `late`.                     |
| **Exams**                   | Lecturers schedule exams per module with duration, pass mark, and total marks.                                                                  |
| **Exam Results**            | Results recorded per student per exam. Status: `pass` / `fail` / `absent`.                                                                      |
| **Enrollment Applications** | Students submit prerequisite documents to apply for a course. Admins approve or reject with notes.                                              |
| **Enrollments**             | Direct enrollment management (admin) linking a student to a batch and course.                                                                   |
| **Announcements**           | Course-scoped announcements with an optional target role filter (`all` / `student` / `lecturer`).                                               |
| **Messaging**               | Threaded internal messaging between any two users. Supports reply chains.                                                                       |
| **Reports**                 | Admin/Lecturer reports: Course Popularity, Student Performance, Lecturer Workload.                                                              |
| **Password Reset**          | 6-digit OTP sent to registered email via SMTP. Single-use, time-limited token.                                                                  |

---

## Technology Stack

| Category             | Technology / Version                                       |
| -------------------- | ---------------------------------------------------------- |
| **Runtime**          | .NET 8                                                     |
| **Framework**        | ASP.NET Core 8 MVC                                         |
| **ORM**              | Entity Framework Core 8                                    |
| **Database**         | MySQL 8.x                                                  |
| **EF MySQL Driver**  | Pomelo.EntityFrameworkCore.MySql 8.0.3                     |
| **Authentication**   | Custom session-based (ASP.NET Core `ISession`)             |
| **Password Hashing** | `Microsoft.AspNetCore.Identity.PasswordHasher<T>`          |
| **Email**            | `System.Net.Mail.SmtpClient` (Gmail SMTP / TLS port 587)   |
| **View Engine**      | Razor Views (`.cshtml`) with Runtime Compilation           |
| **Frontend CSS**     | Bootstrap 5                                                |
| **Frontend JS**      | jQuery 3, jQuery Validation, jQuery Unobtrusive Validation |

---

## NuGet Packages

All packages target **.NET 8**. Defined in `UniManage/UniManage.csproj`.

| Package                                                | Version | Purpose                                                                   |
| ------------------------------------------------------ | ------- | ------------------------------------------------------------------------- |
| `Pomelo.EntityFrameworkCore.MySql`                     | 8.0.3   | EF Core provider for MySQL — primary database driver                      |
| `MySql.Data`                                           | 8.4.0   | MySQL ADO.NET connector (referenced alongside Pomelo)                     |
| `Microsoft.EntityFrameworkCore.Tools`                  | 8.0.23  | CLI tools for `dotnet ef` migrations and scaffolding                      |
| `Microsoft.EntityFrameworkCore.SqlServer`              | 8.0.23  | SQL Server EF provider (available, not active in production)              |
| `Microsoft.EntityFrameworkCore.Sqlite`                 | 8.0.23  | SQLite EF provider (available for local dev/testing)                      |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore`    | 8.0.23  | Identity base classes; `ApplicationDbContext` extends `IdentityDbContext` |
| `Microsoft.AspNetCore.Identity.UI`                     | 8.0.23  | Identity UI scaffolding support                                           |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 8.0.20  | Developer exception page with EF query details                            |
| `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation`    | 8.0.0   | Hot-reload Razor views without restarting the app                         |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design`     | 8.0.23  | `dotnet aspnet-codegenerator` scaffolding tool                            |

---

## Frontend Libraries

Bundled in `wwwroot/lib/` (served as static files):

| Library                           | Purpose                                                      |
| --------------------------------- | ------------------------------------------------------------ |
| **Bootstrap 5**                   | Responsive grid, components, utilities                       |
| **jQuery 3**                      | DOM manipulation, AJAX helpers                               |
| **jquery-validation**             | Client-side form validation                                  |
| **jquery-validation-unobtrusive** | Wires ASP.NET MVC `data-val-*` attributes to jQuery Validate |

---

## Solution & Folder Structure

```
uni-manage-system/
+-- UniManage/                          (Solution root - UniManage.sln)
    +-- UniManage.sln
    +-- README.md
    +-- docs/                           (General documentation)
    |   +-- architecture.md
    |   +-- Authintication.md
    |   +-- database-structure.md
    |   +-- libraries.md
    |   +-- diagrams/                   (Mermaid diagram files)
    |       +-- architecture-diagram.md
    |       +-- class-diagram.md
    |       +-- db-diagram.md
    |       +-- use-case-diagram.md
    +-- UniManage/                      (ASP.NET Core project - UniManage.csproj)
        +-- Program.cs                  (App bootstrap, DI registration, middleware pipeline)
        +-- appsettings.json            (Connection string, SMTP config, logging)
        +-- appsettings.Development.json
        |
        +-- Controllers/                (MVC Controllers - all inherit BaseController)
        |   +-- BaseController.cs       (Session auth guard, role helpers)
        |   +-- AccountController.cs    (Login, Logout, ForgotPassword, OTP, ResetPassword)
        |   +-- DashboardController.cs  (Role-specific dashboard views)
        |   +-- UsersController.cs      (User CRUD - admin only)
        |   +-- RolesController.cs      (Role CRUD - admin only)
        |   +-- DepartmentsController.cs
        |   +-- CoursesController.cs
        |   +-- BatchesController.cs
        |   +-- ModulesController.cs
        |   +-- CourseMaterialsController.cs
        |   +-- AssignmentsController.cs
        |   +-- AssignmentSubmissionsController.cs
        |   +-- ExamResultsController.cs
        |   +-- EnrollmentsController.cs
        |   +-- EnrollmentApplicationsController.cs
        |   +-- AnnouncementsController.cs
        |   +-- ReportsController.cs
        |   +-- HomeController.cs
        |
        +-- Models/                     (EF Core domain models - POCOs)
        |   +-- RoleModel.cs
        |   +-- UserModel.cs
        |   +-- DepartmentModel.cs
        |   +-- UserDepartmentModel.cs  (Junction table with composite PK)
        |   +-- CourseModel.cs
        |   +-- BatchModel.cs
        |   +-- ModuleModel.cs
        |   +-- CourseMaterialModel.cs
        |   +-- AssignmentModel.cs
        |   +-- AssignmentSubmissionModel.cs
        |   +-- ExamModel.cs
        |   +-- ExamResultModel.cs
        |   +-- EnrollmentModel.cs
        |   +-- EnrollmentApplicationModel.cs
        |   +-- AnnouncementModel.cs
        |   +-- MessageModel.cs
        |   +-- PasswordResetOtpModel.cs
        |   +-- ErrorViewModel.cs
        |
        +-- ViewModels/                 (View-specific DTOs)
        |   +-- LecturerWorkloadItem.cs (Used in Reports/LecturerWorkload view)
        |
        +-- Views/                      (Razor Views - .cshtml)
        |   +-- _ViewImports.cshtml
        |   +-- _ViewStart.cshtml
        |   +-- Shared/
        |   |   +-- _Layout.cshtml      (Master layout with Bootstrap nav/sidebar)
        |   |   +-- Error.cshtml
        |   +-- Account/                (Login, ForgotPassword, VerifyOtp, ResetPassword)
        |   +-- Dashboard/              (Admin.cshtml, Lecturer.cshtml, Student.cshtml)
        |   +-- Users/                  (Index, Create, Edit, Details, Delete)
        |   +-- Roles/
        |   +-- Departments/
        |   +-- Courses/
        |   +-- Batches/
        |   +-- Modules/
        |   +-- CourseMaterials/
        |   +-- Assignments/
        |   +-- AssignmentSubmissions/  (Submit, Grade, MyGrades)
        |   +-- Exams/
        |   +-- ExamResults/
        |   +-- Enrollments/
        |   +-- EnrollmentApplications/
        |   +-- Announcements/
        |   +-- Messages/               (Inbox, Sent, Compose, Read - threaded)
        |   +-- Reports/                (CoursePopularity, StudentPerformance, LecturerWorkload)
        |   +-- Home/
        |
        +-- Services/
        |   +-- IEmailService.cs        (Interface: Task SendAsync(to, subject, htmlBody))
        |   +-- EmailService.cs         (SmtpClient implementation - Gmail TLS)
        |
        +-- Data/
        |   +-- ApplicationDbContext.cs (EF Core DbContext with all DbSets)
        |
        +-- Migrations/                 (EF migration history - 8 migrations)
        |   +-- 20260428190328_dbinitialize
        |   +-- 20260428193304_seed-db
        |   +-- 20260428200000_SeedInitialData
        |   +-- 20260429162233_db-seed
        |   +-- 20260501061223_AddNicUidToUsers
        |   +-- 20260501153015_AddLecturerToModule
        |   +-- 20260501170141_AddPrerequisitesAndEnrollmentApplications
        |   +-- 20260502051227_AddPasswordResetOtp
        |   +-- 20260502072805_AddMessages
        |
        +-- wwwroot/
        |   +-- css/                    (Custom stylesheets)
        |   +-- js/                     (Custom scripts)
        |   +-- lib/                    (Bootstrap 5, jQuery, validation libraries)
        |   +-- favicon.ico
        |
        +-- Properties/
            +-- launchSettings.json     (Dev server ports and profiles)
```

---

## Database Structure

The database is **MySQL 8.x**, managed entirely through EF Core migrations. File attachments are stored as `LONGBLOB` columns directly in the database.

### Tables

| Table                    | Primary Key                        | Description                                   |
| ------------------------ | ---------------------------------- | --------------------------------------------- |
| `Roles`                  | `Id` (int, AI)                     | Named roles: administrator, lecturer, student |
| `Users`                  | `Id` (int, AI)                     | All system users; role determines behaviour   |
| `UserDepartments`        | `(UserId, DepartmentId)` composite | Many-to-many junction: user ↔ department      |
| `Departments`            | `Id` (int, AI)                     | Faculty / school groupings                    |
| `Courses`                | `Id` (int, AI)                     | Academic courses under a department           |
| `Batches`                | `Id` (int, AI)                     | Intake groups per course                      |
| `Modules`                | `Id` (int, AI)                     | Ordered units within a course                 |
| `CourseMaterials`        | `Id` (int, AI)                     | File resources attached to a module           |
| `Assignments`            | `Id` (int, AI)                     | Assignments within a module                   |
| `AssignmentSubmissions`  | `Id` (int, AI)                     | Student file submissions for assignments      |
| `Exams`                  | `Id` (int, AI)                     | Scheduled exams within a module               |
| `ExamResults`            | `Id` (int, AI)                     | Per-student exam results                      |
| `Enrollments`            | `Id` (int, AI)                     | Student enrolment in a batch+course           |
| `EnrollmentApplications` | `Id` (int, AI)                     | Student prerequisite applications             |
| `Announcements`          | `Id` (int, AI)                     | Course-scoped announcements                   |
| `Messages`               | `Id` (int, AI)                     | Internal threaded messages between users      |
| `PasswordResetOtps`      | `Id` (int, AI)                     | One-time password reset tokens                |

### Key Relationships

```
Roles ─────────────── Users  (1:many)
Departments ────────── Courses  (1:many)
Departments ────────── UserDepartments ─── Users  (many:many via junction)
Courses ─────────────── Batches  (1:many)
Courses ─────────────── Modules  (1:many)
Courses ─────────────── Enrollments  (1:many)
Courses ─────────────── EnrollmentApplications  (1:many)
Courses ─────────────── Announcements  (1:many)
Batches ─────────────── Enrollments  (1:many)
Users ──────────────── Enrollments  (1:many)
Modules ─────────────── CourseMaterials  (1:many)
Modules ─────────────── Assignments  (1:many)
Modules ─────────────── Exams  (1:many)
Assignments ─────────── AssignmentSubmissions  (1:many)
Exams ───────────────── ExamResults  (1:many)
Messages ────────────── Messages  (self-referencing: ParentMessage / Replies)
```

### Status / Enum Fields

| Table                    | Field        | Allowed Values                    |
| ------------------------ | ------------ | --------------------------------- |
| `Courses`                | `Status`     | `draft`, `published`, `archived`  |
| `Batches`                | `Status`     | `active`, `inactive`              |
| `Enrollments`            | `Status`     | `active`, `inactive`              |
| `EnrollmentApplications` | `Status`     | `pending`, `approved`, `rejected` |
| `AssignmentSubmissions`  | `Status`     | `submitted`, `graded`, `late`     |
| `ExamResults`            | `Status`     | `pass`, `fail`, `absent`          |
| `Announcements`          | `TargetRole` | `all`, `student`, `lecturer`      |
| `CourseMaterials`        | `FileType`   | `pdf`, `video`, `link`, etc.      |

---

## Domain Models

All models live in `UniManage/Models/` and are plain C# classes decorated with Data Annotations. All foreign keys are `int` surrogate keys with auto-increment.

| Model                        | Key Fields                                                                                                                                 |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `RoleModel`                  | `Id`, `Name`, `Description`                                                                                                                |
| `UserModel`                  | `Id`, `RoleId` (FK), `FirstName`, `LastName`, `Email`, `PasswordHash`, `NIC`, `UID`, `ProfilePicture`, `IsActive`                          |
| `DepartmentModel`            | `Id`, `Name`, `Code`, `Description`                                                                                                        |
| `UserDepartmentModel`        | `UserId` (PK+FK), `DepartmentId` (PK+FK)                                                                                                   |
| `CourseModel`                | `Id`, `DepartmentId` (FK), `CreatedBy` (FK→User), `Title`, `Code`, `Status`, `PrerequisiteCourses`                                         |
| `BatchModel`                 | `Id`, `CourseId` (FK), `Name`, `StartDate`, `EndDate`, `MaxStudents`, `Status`                                                             |
| `ModuleModel`                | `Id`, `CourseId` (FK), `LecturerId` (FK→User), `Title`, `OrderIndex`, `IsPublished`                                                        |
| `CourseMaterialModel`        | `Id`, `ModuleId` (FK), `Title`, `FileData` (LONGBLOB), `FileName`, `ContentType`, `FileType`                                               |
| `AssignmentModel`            | `Id`, `ModuleId` (FK), `CreatedBy` (FK→User), `Title`, `MaxMarks`, `ReleaseDate`, `DeadlineDate`, `AllowLateSubmit`, `FileData` (LONGBLOB) |
| `AssignmentSubmissionModel`  | `Id`, `AssignmentId` (FK), `StudentId` (FK→User), `FileData` (LONGBLOB), `MarksObtained`, `Feedback`, `Status`, `GradedBy`                 |
| `ExamModel`                  | `Id`, `ModuleId` (FK), `CreatedBy` (FK→User), `Title`, `TotalMarks`, `PassMarks`, `DurationMinutes`, `ExamDate`                            |
| `ExamResultModel`            | `Id`, `ExamId` (FK), `StudentId` (FK→User), `MarksObtained`, `Status`                                                                      |
| `EnrollmentModel`            | `Id`, `UserId` (FK), `BatchId` (FK), `CourseId` (FK), `EnrolledAt`, `Status`                                                               |
| `EnrollmentApplicationModel` | `Id`, `StudentId` (FK), `CourseId` (FK), `DocumentType`, `DocumentData` (LONGBLOB), `Status`, `AdminNote`, `ReviewedBy` (FK)               |
| `AnnouncementModel`          | `Id`, `CourseId` (FK), `CreatedBy` (FK→User), `Title`, `Content`, `TargetRole`, `PublishDate`                                              |
| `MessageModel`               | `Id`, `SenderId` (FK→User), `ReceiverId` (FK→User), `Subject`, `Body`, `ParentMessageId` (self-ref FK), `IsRead`, `SentAt`                 |
| `PasswordResetOtpModel`      | `Id`, `Email`, `OtpCode` (6-char), `ExpiresAt`, `IsVerified`, `IsUsed`                                                                     |

---

## Controllers & Responsibilities

All controllers inherit `BaseController`. Its `OnActionExecuting` override redirects unauthenticated requests to `/Account/Login`.

| Controller                         | Route Prefix              | Accessible By              | Key Actions                                                      |
| ---------------------------------- | ------------------------- | -------------------------- | ---------------------------------------------------------------- |
| `AccountController`                | `/Account`                | Guest / All                | Login, Logout, ForgotPassword, VerifyOtp, ResetPassword          |
| `DashboardController`              | `/Dashboard`              | All                        | Index (routes to role-specific view: Admin / Lecturer / Student) |
| `UsersController`                  | `/Users`                  | Admin                      | Index, Create, Edit, Details, Delete, ToggleStatus               |
| `RolesController`                  | `/Roles`                  | Admin                      | Index, Create, Edit, Delete                                      |
| `DepartmentsController`            | `/Departments`            | Admin                      | CRUD + user assignment                                           |
| `CoursesController`                | `/Courses`                | Admin, Lecturer            | Index, Create, Edit, Details, Delete, Publish                    |
| `BatchesController`                | `/Batches`                | Admin                      | CRUD per course                                                  |
| `ModulesController`                | `/Modules`                | Admin, Lecturer            | CRUD, assign lecturer, publish/unpublish                         |
| `CourseMaterialsController`        | `/CourseMaterials`        | Admin, Lecturer            | Upload, Download (serves BLOB), Delete                           |
| `AssignmentsController`            | `/Assignments`            | Admin, Lecturer            | Create, Edit, Download brief, Delete                             |
| `AssignmentSubmissionsController`  | `/AssignmentSubmissions`  | Student / Lecturer / Admin | Submit (student), Grade (lecturer/admin), View grades (student)  |
| `ExamResultsController`            | `/ExamResults`            | Admin, Lecturer, Student   | Create exam, Enter results, View own result                      |
| `EnrollmentsController`            | `/Enrollments`            | Admin                      | Direct enrolment management                                      |
| `EnrollmentApplicationsController` | `/EnrollmentApplications` | Student, Admin             | Apply (student), Review + Approve/Reject (admin)                 |
| `AnnouncementsController`          | `/Announcements`          | Admin, Lecturer, Student   | Post, Edit, Delete, View                                         |
| `ReportsController`                | `/Reports`                | Admin, Lecturer            | CoursePopularity, StudentPerformance, LecturerWorkload           |
| `HomeController`                   | `/Home`                   | All                        | Index, Privacy, Error                                            |

---

## Authentication & Authorization

UniManage uses **custom session-based authentication** — not ASP.NET Core Identity middleware.

### Login Flow

1. User submits email + password to `POST /Account/Login`.
2. `AccountController` fetches `UserModel` from the database by email.
3. `PasswordHasher<UserModel>` verifies the password against the stored hash.
4. On success, these values are written to the server-side session:

| Session Key | Type     | Value                                          |
| ----------- | -------- | ---------------------------------------------- |
| `UserId`    | `int`    | Database PK                                    |
| `UserName`  | `string` | `"FirstName LastName"`                         |
| `UserRole`  | `string` | `"administrator"` / `"lecturer"` / `"student"` |
| `RoleId`    | `int`    | Role PK                                        |

5. User is redirected to `/Dashboard/Index`, which routes to the role-specific dashboard view.

### BaseController Guards

```csharp
protected int?    CurrentUserId    // Session["UserId"]
protected string? CurrentUserName  // Session["UserName"]
protected string? CurrentUserRole  // Session["UserRole"]
protected int?    CurrentRoleId    // Session["RoleId"]

protected bool IsAdmin
protected bool IsLecturer
protected bool IsStudent
```

Any controller action that needs a role check calls `IsAdmin`, `IsLecturer`, or `IsStudent` and redirects otherwise. `OnActionExecuting` automatically redirects to `/Account/Login` if `CurrentUserId` is null.

### Session Configuration

```csharp
// Program.cs
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});
```

### Password Reset (OTP)

1. User submits email at `POST /Account/ForgotPassword`.
2. A random 6-digit OTP is generated, saved to `PasswordResetOtps`, and emailed via `IEmailService`.
3. User enters the OTP at `POST /Account/VerifyOtp`.
4. On valid OTP, user sets a new password at `POST /Account/ResetPassword`.
5. The new password is hashed with `PasswordHasher<UserModel>` and saved; the OTP row is marked `IsUsed = true`.

---

## Services

### `IEmailService` / `EmailService`

Located in `UniManage/Services/`.

```csharp
public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody);
}
```

`EmailService` uses `System.Net.Mail.SmtpClient` and reads host/port/credentials from `appsettings.json` under the `Smtp` key. Registered in DI as scoped:

```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server 8.x running on `localhost:3306`

### 1 — Clone and configure

```bash
git clone <repo-url>
cd uni-manage-system/UniManage/UniManage
```

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=UniManageDB;Uid=root;Pwd=YOUR_PASSWORD;"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "User": "your-email@gmail.com",
    "Password": "your-gmail-app-password",
    "FromName": "UniManage"
  }
}
```

> **Security:** Never commit real credentials. Use environment variables or .NET User Secrets (`dotnet user-secrets`) in development and a secrets manager in production.

### 2 — Apply migrations

```bash
dotnet ef database update
```

This runs all 9 migrations in order and seeds initial roles, departments, and a default admin user.

### 3 — Run

```bash
dotnet run
```

The app starts at `https://localhost:{port}` and redirects to `/Account/Login`.

---

## Default Seed Data

The EF migrations include seed data for:

| Seeded Entity   | Details                                                           |
| --------------- | ----------------------------------------------------------------- |
| **Roles**       | `administrator`, `lecturer`, `student`                            |
| **Departments** | Sample faculty departments                                        |
| **Admin User**  | Default administrator account (change password after first login) |

---

| **Course & Module Management** | Hierarchical structure: Course ? Module ? Materials / Assignments / Exams |
| **Enrollment System** | Students apply for courses; admins review and approve applications |
| **Assignments** | Lecturers create assignments with file attachments; students submit work |
| **Exams & Results** | Lecturers create exams; results are recorded per student |
| **Messaging** | Internal threaded messaging between any two users |
| **Announcements** | Course-scoped announcements targeted by role |
| **Reports** | Admin-only: workload analysis, student performance, course popularity |
| **Password Reset** | OTP sent via SMTP email; user resets via OTP verification flow |

---

## Technology Stack

| Category             | Technology                                                           |
| -------------------- | -------------------------------------------------------------------- |
| **Framework**        | ASP.NET Core 8 MVC                                                   |
| **ORM**              | Entity Framework Core 8                                              |
| **Database**         | MySQL (Pomelo provider) � also references SQLite/SQL Server packages |
| **Authentication**   | Custom session-based (no ASP.NET Identity middleware pipeline)       |
| **Password Hashing** | `Microsoft.AspNetCore.Identity` `PasswordHasher<T>`                  |
| **Email**            | `System.Net.Mail.SmtpClient` via `IEmailService` / `EmailService`    |
| **View Engine**      | Razor Views (`.cshtml`) with Runtime Compilation                     |
| **Frontend**         | Bootstrap 5, vanilla JavaScript, jQuery                              |

---

## Solution Structure

```
uni-manage-system/
??? UniManage/                   ? Solution directory
    ??? UniManage/               ? Project directory (UniManage.csproj)
        ??? Controllers/         ? MVC Controllers
        ??? Models/              ? EF Core domain models
        ??? ViewModels/          ? View-specific data transfer objects
        ??? Views/               ? Razor Views (.cshtml)
        ?   ??? Shared/          ? _Layout.cshtml, Error.cshtml
        ?   ??? Account/         ? Login, ForgotPassword, VerifyOtp, ResetPassword
        ?   ??? Dashboard/       ? Admin.cshtml, Lecturer.cshtml, Student.cshtml
        ?   ??? Users/           ? CRUD for user management
        ?   ??? Courses/         ? CRUD for courses
        ?   ??? Modules/         ? CRUD for modules
        ?   ??? Assignments/     ? Assignment management
        ?   ??? AssignmentSubmissions/ ? Submission, grading, student grades
        ?   ??? Exams/           ? Exam management
        ?   ??? ExamResults/     ? Exam result entry
        ?   ??? Enrollments/     ? Direct enrollment management
        ?   ??? EnrollmentApplications/ ? Student application workflow
        ?   ??? Announcements/   ? Course announcements
        ?   ??? Messages/        ? Inbox, Sent, Compose, Read (threaded)
        ?   ??? Reports/         ? Admin reporting views
        ?   ??? Batches/         ? Batch management
        ?   ??? Departments/     ? Department management
        ?   ??? Roles/           ? Role management
        ?   ??? CourseMaterials/ ? Material upload/management
        ??? Services/            ? IEmailService / EmailService
        ??? Data/                ? ApplicationDbContext (EF Core)
        ??? Migrations/          ? EF Core migration history
        ??? Program.cs           ? App bootstrap & DI configuration
        ??? appsettings.json     ? Connection string & SMTP config
```

---

## Architecture & Diagrams

Pre-built Mermaid diagrams are available in `docs/diagrams/`:

| Diagram           | File                                                                           | Description                                                       |
| ----------------- | ------------------------------------------------------------------------------ | ----------------------------------------------------------------- |
| **Architecture**  | [docs/diagrams/architecture-diagram.md](docs/diagrams/architecture-diagram.md) | Layered system architecture + authentication sequence diagram     |
| **Class Diagram** | [docs/diagrams/class-diagram.md](docs/diagrams/class-diagram.md)               | All 17 domain model classes with fields and navigation properties |
| **Database (ER)** | [docs/diagrams/db-diagram.md](docs/diagrams/db-diagram.md)                     | Full ER diagram with all tables, columns, and FK relationships    |
| **Use Case**      | [docs/diagrams/use-case-diagram.md](docs/diagrams/use-case-diagram.md)         | All actors (Guest, Student, Lecturer, Admin) and ~50 use cases    |

Additional documentation:

| Document                                                 | Contents                                                              |
| -------------------------------------------------------- | --------------------------------------------------------------------- |
| [docs/architecture.md](docs/architecture.md)             | MVC architecture, request lifecycle, session auth, role-based routing |
| [docs/database-structure.md](docs/database-structure.md) | Detailed table/column descriptions and relationships                  |
| [docs/libraries.md](docs/libraries.md)                   | NuGet packages with versions and purposes                             |
| [docs/Authintication.md](docs/Authintication.md)         | Authentication and authorization flow details                         |

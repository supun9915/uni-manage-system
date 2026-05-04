# UniManage – University Management System

UniManage is an ASP.NET Core 8 MVC web application that manages the academic lifecycle of a university — users, courses, modules, assignments, exams, enrollments, messaging, and reporting — all protected by a custom session-based authentication layer.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Solution Structure](#solution-structure)
4. [Getting Started](#getting-started)
5. [Detailed Documentation](#detailed-documentation)

---

## Project Overview

| Feature Area | Description |
|---|---|
| **User Management** | Admin can create/manage users with roles: Administrator, Lecturer, Student |
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

| Category | Technology |
|---|---|
| **Framework** | ASP.NET Core 8 MVC |
| **ORM** | Entity Framework Core 8 |
| **Database** | MySQL (Pomelo provider) – also references SQLite/SQL Server packages |
| **Authentication** | Custom session-based (no ASP.NET Identity middleware pipeline) |
| **Password Hashing** | `Microsoft.AspNetCore.Identity` `PasswordHasher<T>` |
| **Email** | `System.Net.Mail.SmtpClient` via `IEmailService` / `EmailService` |
| **View Engine** | Razor Views (`.cshtml`) with Runtime Compilation |
| **Frontend** | Bootstrap 5, vanilla JavaScript, jQuery |

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

## Getting Started

### Prerequisites
- .NET 8 SDK
- MySQL server (default: `localhost:3306`)

### Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=UniManageDB;Uid=root;Pwd=YOUR_PASSWORD;"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "User": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromName": "UniManage"
  }
}
```

### Run

```bash
cd UniManage/UniManage
dotnet ef database update   # apply all migrations
dotnet run
```

The app starts at `https://localhost:{port}` and redirects to `/Account/Login`.

---

## Detailed Documentation

| Document | Contents |
|---|---|
| [docs/architecture.md](docs/architecture.md) | MVC architecture, request lifecycle, session auth, role-based routing |
| [docs/database-structure.md](docs/database-structure.md) | All tables, columns, data types, relationships, ERD description |
| [docs/libraries.md](docs/libraries.md) | All NuGet packages with version & purpose |

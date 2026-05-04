# MVC Architecture – UniManage

## Overview

UniManage follows the **ASP.NET Core MVC** (Model-View-Controller) pattern. Every HTTP request is routed to a controller action, the action builds data (using EF Core), assigns it to a ViewModel / ViewBag, and returns a Razor View that renders HTML.

---

## Request Lifecycle

```
Browser Request
      ?
      ?
  Program.cs  ???  Middleware Pipeline
      ?              ?? UseHttpsRedirection
      ?              ?? UseStaticFiles       (wwwroot/)
      ?              ?? UseRouting
      ?              ?? UseSession           ?? custom auth reads session here
      ?              ?? UseAuthorization
      ?
      ?
  Route Matching  ({controller}/{action}/{id?})
  Default route:  Account/Login
      ?
      ?
  Controller Action
      ?   ?? inherits BaseController (session helpers)
      ?   ?? queries ApplicationDbContext (EF Core ? MySQL)
      ?   ?? populates ViewBag / ViewModel
      ?   ?? returns View(model)
      ?
      ?
  Razor View (.cshtml)
      ?   ?? _Layout.cshtml wraps all content views
      ?   ?? uses @Model or @ViewBag data
      ?   ?? Tag Helpers, HTML Helpers, partial views
      ?
      ?
  HTML Response ? Browser
```

---

## Layer Descriptions

### 1. Program.cs – Application Bootstrap

`Program.cs` configures the DI container and middleware pipeline:

```csharp
// EF Core with MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// MVC + Razor Runtime Compilation
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Session (4-hour idle timeout)
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromHours(4); });

// Custom services
builder.Services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
builder.Services.AddScoped<IEmailService, EmailService>();
```

Default route: `{controller=Account}/{action=Login}/{id?}`

---

### 2. Models – Domain / Database Layer (`Models/`)

EF Core entity classes that map directly to MySQL tables. Each model uses **Data Annotations** for validation and **navigation properties** for relationships.

| Model | Table | Key Relations |
|---|---|---|
| `UserModel` | `Users` | ? `RoleModel`, ? `UserDepartmentModel`(many) |
| `RoleModel` | `Roles` | ? `UserModel`(many) |
| `DepartmentModel` | `Departments` | ? `UserDepartmentModel`, ? `CourseModel` |
| `UserDepartmentModel` | `UserDepartments` | composite PK (`UserId`, `DepartmentId`) |
| `CourseModel` | `Courses` | ? `DepartmentModel`, ? `UserModel`(creator) |
| `BatchModel` | `BatchModels` | ? `CourseModel` |
| `ModuleModel` | `Modules` | ? `CourseModel`, ? `UserModel`(lecturer) |
| `CourseMaterialModel` | `CourseMaterials` | ? `ModuleModel` |
| `AssignmentModel` | `Assignments` | ? `ModuleModel`, ? `UserModel`(creator) |
| `AssignmentSubmissionModel` | `AssignmentSubmissions` | ? `AssignmentModel`, ? `UserModel`(student) |
| `ExamModel` | `Exams` | ? `ModuleModel`, ? `UserModel`(creator) |
| `ExamResultModel` | `ExamResults` | ? `ExamModel`, ? `UserModel`(student) |
| `EnrollmentModel` | `Enrollments` | ? `UserModel`, ? `BatchModel`, ? `CourseModel` |
| `EnrollmentApplicationModel` | `EnrollmentApplications` | ? `UserModel`(student/reviewer), ? `CourseModel` |
| `AnnouncementModel` | `Announcements` | ? `CourseModel`, ? `UserModel`(creator) |
| `MessageModel` | `Messages` | ? `UserModel`(sender/receiver), self-ref(parent) |
| `PasswordResetOtpModel` | `PasswordResetOtps` | ? `UserModel` |

---

### 3. ApplicationDbContext – Data Access (`Data/ApplicationDbContext.cs`)

Extends `IdentityDbContext` (although ASP.NET Identity login flow is **not used**—only `PasswordHasher<T>` is used for hashing).

Defines all `DbSet<T>` properties and one custom `OnModelCreating` rule:

```csharp
// Composite primary key for the many-to-many join table
modelBuilder.Entity<UserDepartmentModel>()
    .HasKey(ud => new { ud.UserId, ud.DepartmentId });
```

---

### 4. BaseController – Session Authentication (`Controllers/BaseController.cs`)

All controllers that require authentication inherit `BaseController` instead of `Controller` directly.

```csharp
public class BaseController : Controller
{
    protected int?    CurrentUserId   => HttpContext.Session.GetInt32("UserId");
    protected string? CurrentUserName => HttpContext.Session.GetString("UserName");
    protected string? CurrentUserRole => HttpContext.Session.GetString("UserRole");
    protected int?    CurrentRoleId   => HttpContext.Session.GetInt32("RoleId");

    protected bool IsAdmin    => CurrentUserRole == "administrator";
    protected bool IsLecturer => CurrentUserRole == "lecturer";
    protected bool IsStudent  => CurrentUserRole == "student";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (CurrentUserId == null)                         // ? guard: redirect to Login
            context.Result = RedirectToAction("Login", "Account");
        // Populates ViewBag for every request (used in _Layout.cshtml)
        ViewBag.CurrentUserId   = CurrentUserId;
        ViewBag.CurrentUserName = CurrentUserName;
        ViewBag.CurrentUserRole = CurrentUserRole?.ToLower();
    }
}
```

`AccountController` does **not** inherit `BaseController` so unauthenticated users can reach Login / ForgotPassword / ResetPassword.

---

### 5. Controllers

| Controller | Inherits | Responsibilities |
|---|---|---|
| `AccountController` | `Controller` | Login, Logout, ForgotPassword, VerifyOtp, ResetPassword |
| `DashboardController` | `BaseController` | Role-aware dashboard (Admin / Lecturer / Student views) |
| `UsersController` | `BaseController` | Full CRUD for users (Admin only) |
| `CoursesController` | `BaseController` | Course CRUD, course details |
| `ModulesController` | `BaseController` | Module CRUD, lecturer assignment |
| `CourseMaterialsController` | `BaseController` | File upload/download of course materials |
| `AssignmentsController` | `BaseController` | Assignment CRUD with optional file attachment |
| `AssignmentSubmissionsController` | `BaseController` | Student submit, lecturer grade, student view grades |
| `ExamsController` | `BaseController` | Exam CRUD |
| `ExamResultsController` | `BaseController` | Exam result entry/edit |
| `EnrollmentsController` | `BaseController` | Direct enrollment management |
| `EnrollmentApplicationsController` | `BaseController` | Student apply, admin browse/review |
| `AnnouncementsController` | `BaseController` | Announcement CRUD |
| `MessagesController` | `BaseController` | Inbox, Sent, Compose, Reply, Read, Delete |
| `ReportsController` | `BaseController` | WorkloadAnalysis, StudentPerformance, CoursePopularity |
| `BatchesController` | `BaseController` | Batch CRUD |
| `DepartmentsController` | `BaseController` | Department CRUD |
| `RolesController` | `BaseController` | Role CRUD |
| `HomeController` | `Controller` | Home / Privacy (minimal) |

---

### 6. ViewModels (`ViewModels/`)

ViewModels carry computed data that does not map 1-to-1 with a single entity.

| ViewModel | Used in | Purpose |
|---|---|---|
| `LecturerWorkloadItem` | `Reports/WorkloadAnalysis` | Per-lecturer module/assignment/exam/material counts & total workload |

---

### 7. Views – Razor Templates (`Views/`)

#### Layout & Shared Partials

- **`Views/Shared/_Layout.cshtml`** – Master layout. Renders the navigation bar using `ViewBag.CurrentUserRole` to show role-appropriate menu items, then renders `@RenderBody()`.
- **`Views/_ViewImports.cshtml`** – Imports `UniManage`, `UniManage.Models` namespaces and `Microsoft.AspNetCore.Mvc.TagHelpers` Tag Helpers globally.
- **`Views/_ViewStart.cshtml`** – Sets `Layout = "_Layout"` as the default layout for all views.

#### Role-Scoped Dashboard Views

The `DashboardController.Index()` action checks `CurrentUserRole` and returns a different view:

```csharp
if (role == "student")  return View("Student");
if (role == "lecturer") return View("Lecturer");
return View("Admin");   // administrator
```

Each dashboard view receives role-specific data via `ViewBag`:

| View | ViewBag Keys |
|---|---|
| `Dashboard/Student.cshtml` | Enrollments, Assignments, Submissions, Announcements |
| `Dashboard/Lecturer.cshtml` | Courses, RecentSubmissions, PendingGrades, Announcements |
| `Dashboard/Admin.cshtml` | TotalUsers, TotalCourses, PendingApplications, RecentUsers |

#### Passing Data to Views

Two patterns are used:

**1. ViewBag (dynamic dictionary)**

```csharp
// Controller
ViewBag.Enrollments = enrollments;
```
```html
<!-- View -->
@foreach (var e in (IEnumerable<EnrollmentModel>)ViewBag.Enrollments) { ... }
```

**2. Typed Model (`@model` directive)**

```csharp
// Controller
return View(lecturerWorkloadList);  // IEnumerable<LecturerWorkloadItem>
```
```html
<!-- View -->
@model IEnumerable<UniManage.ViewModels.LecturerWorkloadItem>
@foreach (var item in Model) { ... }
```

#### Tag Helpers in Forms

All forms use ASP.NET Core Tag Helpers (`asp-action`, `asp-controller`, `asp-for`, `asp-validation-for`):

```html
<form asp-action="Login" asp-controller="Account" method="post">
    <input asp-for="Email" class="form-control" />
    <span asp-validation-for="Email" class="text-danger"></span>
    <button type="submit">Login</button>
</form>
```

---

### 8. Services Layer (`Services/`)

```
IEmailService  (interface)
    ??? EmailService  (implementation using System.Net.Mail.SmtpClient)
```

Registered as **scoped** in `Program.cs`:

```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
```

Used by `AccountController` to send OTP emails during the password-reset flow:

```
ForgotPassword POST ? generate 6-digit OTP ? save to PasswordResetOtps table
                                            ? EmailService.SendAsync(otp email)
VerifyOtp POST  ? validate OTP & expiry
ResetPassword POST ? hash new password ? save ? invalidate OTP
```

---

### 9. Authentication & Authorization (Session-Based)

UniManage does **not** use ASP.NET Identity's cookie middleware. Instead:

1. **Login** (`AccountController.Login POST`):
   - Looks up user by email.
   - Verifies password with `PasswordHasher<UserModel>.VerifyHashedPassword`.
   - Writes `UserId`, `UserName`, `UserEmail`, `UserRole`, `RoleId` into `HttpContext.Session`.

2. **Guard** (`BaseController.OnActionExecuting`):
   - Every controller action that requires auth checks `CurrentUserId != null`.
   - If null ? `RedirectToAction("Login", "Account")`.

3. **Role checks** in controllers and views use `IsAdmin`, `IsLecturer`, `IsStudent` (controller) or `ViewBag.CurrentUserRole` (view).

4. **Logout** clears the entire session with `HttpContext.Session.Clear()`.

---

### 10. Database Migrations

EF Core Code-First migrations are stored in `Migrations/`:

| Migration | Description |
|---|---|
| `dbinitialize` | Initial schema |
| `seed-db` | Initial seed data |
| `SeedInitialData` | Additional seeding |
| `db-seed` | Extra seed records |
| `AddNicUidToUsers` | Adds NIC & UID columns to Users |
| `AddLecturerToModule` | Adds `LecturerId` FK to Modules |
| `AddPrerequisitesAndEnrollmentApplications` | Prerequisites & EnrollmentApplications table |
| `AddPasswordResetOtp` | PasswordResetOtps table |
| `AddMessages` | Messages table |

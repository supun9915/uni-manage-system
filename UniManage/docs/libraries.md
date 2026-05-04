# Libraries & Packages – UniManage

Target Framework: **.NET 8** (`net8.0`)

---

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 8.0.20 | Shows EF Core error pages during development (migration errors, query failures) |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.23 | Provides `IdentityDbContext` base class; `ApplicationDbContext` extends it to inherit Identity schema. Also supplies `PasswordHasher<T>` used for securely hashing & verifying user passwords |
| `Microsoft.AspNetCore.Identity.UI` | 8.0.23 | Razor UI scaffolding for Identity pages (included as dependency; Identity UI pages not actively used since custom session auth is implemented) |
| `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` | 8.0.0 | Enables hot-reload of `.cshtml` Razor views without restarting the app — improves development experience |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.23 | SQLite EF Core provider (available as fallback / testing alternative to MySQL) |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.23 | SQL Server EF Core provider (available as alternative database target) |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.23 | CLI & Package Manager Console tools for `dotnet ef migrations`, `dotnet ef database update`, etc. *(build-time only)* |
| `Microsoft.VisualStudio.Web.CodeGeneration.Design` | 8.0.23 | Visual Studio scaffolding support for generating controllers and views *(build-time only)* |
| `MySql.Data` | 8.4.0 | Official Oracle MySQL ADO.NET connector (pulled in as a dependency of Pomelo) |
| `Pomelo.EntityFrameworkCore.MySql` | 8.0.3 | Community EF Core provider for MySQL/MariaDB. Used in `Program.cs` via `options.UseMySql(...)` with auto server-version detection |

---

## Built-in .NET / ASP.NET Core Features Used

| Feature | How It Is Used |
|---|---|
| `Microsoft.AspNetCore.Http.ISession` | Session storage for `UserId`, `UserName`, `UserRole`, `RoleId` — the foundation of custom auth |
| `System.Net.Mail.SmtpClient` | Sends OTP emails via Gmail SMTP inside `EmailService` |
| `Microsoft.AspNetCore.Mvc.TagHelpers` | `asp-action`, `asp-controller`, `asp-for`, `asp-validation-for` in all Razor forms |
| `Microsoft.AspNetCore.Mvc.Filters.IActionFilter` | `BaseController.OnActionExecuting` — session guard runs before every action |
| EF Core LINQ | All database queries use `_context.Entity.Include(...).Where(...).ToListAsync()` |
| Data Annotations | `[Required]`, `[MaxLength]`, `[Key]`, `[ForeignKey]`, `[DatabaseGenerated]` on all models |

---

## Frontend Libraries (CDN / wwwroot)

| Library | Purpose |
|---|---|
| **Bootstrap 5** | Responsive grid, cards, buttons, badges, modals, tables |
| **jQuery** | DOM manipulation, AJAX helpers used in some views |
| **jQuery Validation** | Client-side form validation (`_ValidationScriptsPartial.cshtml`) |

---

## Configuration Files

| File | Purpose |
|---|---|
| `appsettings.json` | MySQL connection string, SMTP credentials, logging levels |
| `appsettings.Development.json` | Development overrides (if present) |
| `UniManage.csproj` | SDK-style project file with all `<PackageReference>` entries |

# Authentication & Authorization in UniManage

This document explains how the UniManage system handles **who can log in** (authentication) and **what each user is allowed to do** (authorization).

---

## Table of Contents

1. [Overview](#1-overview)
2. [Authentication – How Login Works](#2-authentication--how-login-works)
   - [Session Storage](#21-session-storage)
   - [Login Flow (Step by Step)](#22-login-flow-step-by-step)
   - [Logout](#23-logout)
3. [Password Rules](#3-password-rules)
4. [Password Reset via OTP](#4-password-reset-via-otp)
   - [Step 1 – Request OTP](#step-1--request-otp)
   - [Step 2 – Verify OTP](#step-2--verify-otp)
   - [Step 3 – Set New Password](#step-3--set-new-password)
5. [Authorization – Who Can Do What](#5-authorization--who-can-do-what)
   - [BaseController – The Gatekeeper](#51-basecontroller--the-gatekeeper)
   - [Role Checks Inside Actions](#52-role-checks-inside-actions)
   - [Role Permissions Matrix](#53-role-permissions-matrix)
6. [Service Registration (Program.cs)](#6-service-registration-programcs)
7. [Security Design Decisions](#7-security-design-decisions)
8. [Sequence Diagrams](#8-sequence-diagrams)

---

## 1. Overview

UniManage uses **custom session-based authentication** — not ASP.NET Core Identity sign-in. After a successful login, key user data is written to the server-side session. Every subsequent request reads from the session to identify the user and check their role.

There are **three roles** in the system:

| Role            | Description                                                                               |
| --------------- | ----------------------------------------------------------------------------------------- |
| `administrator` | Full access to all features                                                               |
| `lecturer`      | Access to their own modules, assignments, exams, and student submissions                  |
| `student`       | Read-only access to enrolled course content; can submit assignments and apply for courses |

---

## 2. Authentication – How Login Works

### 2.1 Session Storage

On a successful login, the following keys are written to `HttpContext.Session`:

| Session Key | Type     | Value                                                |
| ----------- | -------- | ---------------------------------------------------- |
| `UserId`    | `int`    | Database primary key of the user                     |
| `UserName`  | `string` | `"{FirstName} {LastName}"`                           |
| `UserEmail` | `string` | User's email address                                 |
| `UserRole`  | `string` | Role name in **lower-case** (e.g. `"administrator"`) |
| `RoleId`    | `int`    | Database primary key of the role                     |

```csharp
// AccountController.cs – after password verification passes
HttpContext.Session.SetInt32("UserId",   user.Id);
HttpContext.Session.SetString("UserName",  $"{user.FirstName} {user.LastName}");
HttpContext.Session.SetString("UserEmail", user.Email);
HttpContext.Session.SetString("UserRole",  user.Role?.Name?.ToLower() ?? "student");
HttpContext.Session.SetInt32("RoleId",   user.RoleId);
```

Session settings (configured in `Program.cs`):

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(4); // expires after 4 h of inactivity
    options.Cookie.HttpOnly    = true;                  // not accessible from JavaScript
    options.Cookie.IsEssential = true;                  // always sent, even without consent
});
```

---

### 2.2 Login Flow (Step by Step)

```
Browser                     AccountController              Database
  |                               |                            |
  |--- GET /Account/Login ------->|                            |
  |<-- Login View (form) ---------|                            |
  |                               |                            |
  |--- POST /Account/Login ------>|                            |
  |    (email, password)          |--- Find user by email ---->|
  |                               |<-- UserModel + Role -------|
  |                               |                            |
  |                               |-- VerifyHashedPassword()   |
  |                               |   (ASP.NET Identity        |
  |                               |    PasswordHasher)         |
  |                               |                            |
  |                               |-- Write session keys       |
  |<-- Redirect /Dashboard -------|                            |
```

**Key points:**

- The user must have `IsActive = true` to be found at all.
- Passwords are hashed using **`PasswordHasher<UserModel>`** (ASP.NET Identity's hasher), so plain-text passwords are never stored.
- The same generic error `"Invalid email or password."` is shown for both a missing user AND a wrong password — this prevents user-enumeration attacks.

---

### 2.3 Logout

```csharp
// AccountController.cs
public IActionResult Logout()
{
    HttpContext.Session.Clear(); // removes all session keys
    return RedirectToAction("Login");
}
```

Calling `Session.Clear()` removes every key, effectively ending the session immediately.

---

## 3. Password Rules

Both the **account creation** and **password reset** flows enforce these rules (defined in the static `ValidatePassword()` method in `AccountController` and `UsersController`):

| Rule              | Minimum Requirement                             |
| ----------------- | ----------------------------------------------- |
| Length            | At least **6 characters**                       |
| Uppercase         | At least **1** uppercase letter (`A–Z`)         |
| Lowercase         | At least **1** lowercase letter (`a–z`)         |
| Digit             | At least **1** number (`0–9`)                   |
| Special character | At least **1** symbol (e.g. `@`, `#`, `!`, `$`) |

```csharp
private static string? ValidatePassword(string pwd)
{
    if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 6)
        return "Password must be at least 6 characters.";
    if (!Regex.IsMatch(pwd, @"[A-Z]"))
        return "Password must contain at least one uppercase letter.";
    if (!Regex.IsMatch(pwd, @"[a-z]"))
        return "Password must contain at least one lowercase letter.";
    if (!Regex.IsMatch(pwd, @"[0-9]"))
        return "Password must contain at least one number.";
    if (!Regex.IsMatch(pwd, @"[^a-zA-Z0-9]"))
        return "Password must contain at least one special character.";
    return null; // password is valid
}
```

---

## 4. Password Reset via OTP

The password reset flow is a **3-step process** that uses a one-time PIN sent via email.

```
[ForgotPassword] --> [VerifyOtp] --> [ResetPassword]
```

The `PasswordResetOtpModel` table tracks each OTP:

| Field        | Purpose                                                      |
| ------------ | ------------------------------------------------------------ |
| `Email`      | Which account the OTP was issued for                         |
| `OtpCode`    | 6-digit random code                                          |
| `ExpiresAt`  | UTC timestamp — OTP is invalid after this point (10 minutes) |
| `IsVerified` | Set to `true` after the user enters the correct OTP          |
| `IsUsed`     | Set to `true` after the password is successfully changed     |
| `CreatedAt`  | When the OTP was generated                                   |

---

### Step 1 – Request OTP

**Route:** `POST /Account/ForgotPassword`

1. The user submits their email address.
2. The system looks up the user (`IsActive = true` required).
3. Any previous **unused** OTPs for that email are deleted.
4. A new 6-digit OTP is generated and saved with a **10-minute expiry**.
5. An HTML email is sent via `IEmailService` (SMTP).
6. The email is passed to the next step via `TempData["OtpEmail"]`.

> **Security note:** If the email does not exist in the database, the same success message is shown — this prevents revealing whether an account exists (user-enumeration protection).

---

### Step 2 – Verify OTP

**Route:** `POST /Account/VerifyOtp`

1. The user enters the 6-digit code they received by email.
2. The system queries for a matching OTP record where:
   - `Email` matches
   - `OtpCode` matches (trimmed)
   - `IsUsed = false`
   - `IsVerified = false`
   - `ExpiresAt > DateTime.UtcNow`
3. If found, `IsVerified` is set to `true`.
4. The verified email is passed to the next step via `TempData["VerifiedEmail"]`.

---

### Step 3 – Set New Password

**Route:** `POST /Account/ResetPassword`

1. The user enters and confirms a new password.
2. Password rules are validated (see [Section 3](#3-password-rules)).
3. The system re-queries for a verified, unused, non-expired OTP record.
4. The user's `PasswordHash` is updated using `PasswordHasher<UserModel>`.
5. The OTP record is marked `IsUsed = true`.
6. The user is redirected to the Login page.

---

## 5. Authorization – Who Can Do What

### 5.1 BaseController – The Gatekeeper

Every controller in the application (except `AccountController`) inherits from `BaseController`:

```csharp
public class BaseController : Controller
{
    // Read session values
    protected int?    CurrentUserId   => HttpContext.Session.GetInt32("UserId");
    protected string? CurrentUserRole => HttpContext.Session.GetString("UserRole");
    protected int?    CurrentRoleId   => HttpContext.Session.GetInt32("RoleId");

    // Convenient role booleans
    protected bool IsAdmin    => string.Equals(CurrentUserRole, "administrator", StringComparison.OrdinalIgnoreCase);
    protected bool IsLecturer => string.Equals(CurrentUserRole, "lecturer",      StringComparison.OrdinalIgnoreCase);
    protected bool IsStudent  => string.Equals(CurrentUserRole, "student",       StringComparison.OrdinalIgnoreCase);

    // Runs before every action — redirects if not logged in
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (CurrentUserId == null)
        {
            context.Result = RedirectToAction("Login", "Account");
            return;
        }
        ViewBag.CurrentUserId   = CurrentUserId;
        ViewBag.CurrentUserName = CurrentUserName;
        ViewBag.CurrentUserRole = CurrentUserRole?.ToLower();
        base.OnActionExecuting(context);
    }
}
```

**`OnActionExecuting` is the authentication guard** — it runs before every action method. If `UserId` is not in the session (user not logged in or session expired), the request is immediately redirected to `Account/Login`.

---

### 5.2 Role Checks Inside Actions

Inside individual action methods, role checks are done with the helper booleans:

```csharp
// Deny access — returns HTTP 403
if (!IsAdmin) return Forbid();

// Filter data — students only see their own enrollments
if (IsStudent)
    query = query.Where(e => e.UserId == CurrentUserId);

// Branch behavior — different view per role
if (IsStudent)   return View("Student");
if (IsLecturer)  return View("Lecturer");
return View("Admin");
```

---

### 5.3 Role Permissions Matrix

| Feature / Action                                        |  Administrator   |         Lecturer         |            Student             |
| ------------------------------------------------------- | :--------------: | :----------------------: | :----------------------------: |
| **Dashboard**                                           | Admin stats view |    Own module summary    | Enrolled courses & assignments |
| **Users** – View list                                   |        ✅        |            ❌            |               ❌               |
| **Users** – Create / Edit / Delete                      |        ✅        |            ❌            |               ❌               |
| **Roles** – CRUD                                        |        ✅        |            ❌            |               ❌               |
| **Departments** – CRUD                                  |        ✅        |            ❌            |               ❌               |
| **Courses** – View                                      |      ✅ All      | ✅ Assigned modules only |        ✅ Enrolled only        |
| **Courses** – Create / Edit / Delete                    |        ✅        |            ❌            |               ❌               |
| **Modules** – View                                      |      ✅ All      |      ✅ Own modules      |    ✅ Published & enrolled     |
| **Modules** – Create                                    |        ✅        |            ❌            |               ❌               |
| **Modules** – Edit / Delete                             |        ✅        |          ✅ Own          |               ❌               |
| **Modules** – Assign Lecturer                           |        ✅        |            ❌            |               ❌               |
| **Course Materials** – View                             |        ✅        |          ✅ Own          |          ✅ Enrolled           |
| **Course Materials** – Upload / Delete                  |        ✅        |          ✅ Own          |               ❌               |
| **Assignments** – View                                  |        ✅        |          ✅ Own          |          ✅ Enrolled           |
| **Assignments** – Create / Edit / Delete                |        ✅        |          ✅ Own          |               ❌               |
| **Submissions** – View all                              |        ✅        |      ✅ Own modules      |               ❌               |
| **Submissions** – Submit                                |        ❌        |            ❌            |               ✅               |
| **Submissions** – Grade                                 |        ✅        |            ✅            |               ❌               |
| **Submissions** – View own grades                       |        ❌        |            ❌            |               ✅               |
| **Batches** – CRUD                                      |        ✅        |            ❌            |               ❌               |
| **Enrollments** – View                                  |        ✅        |      ✅ Own courses      |               ❌               |
| **Enrollments** – Create / Delete                       |        ✅        |            ❌            |               ❌               |
| **Enrollment Applications** – Browse courses            |        ❌        |            ❌            |               ✅               |
| **Enrollment Applications** – Apply                     |        ❌        |            ❌            |               ✅               |
| **Enrollment Applications** – View own                  |        ❌        |            ❌            |               ✅               |
| **Enrollment Applications** – Review / Approve / Reject |        ✅        |            ✅            |               ❌               |
| **Exams** – View                                        |        ✅        |          ✅ Own          |          ✅ Enrolled           |
| **Exams** – Create / Edit / Delete                      |        ✅        |          ✅ Own          |               ❌               |
| **Exam Results** – View                                 |      ✅ All      |          ✅ Own          |          ✅ Own only           |
| **Exam Results** – Create / Edit / Delete               |        ✅        |            ✅            |               ❌               |
| **Announcements** – View                                |        ✅        |          ✅ Own          |          ✅ Enrolled           |
| **Announcements** – Create / Edit / Delete              |        ✅        |            ✅            |               ❌               |
| **Messages** – Send / Read / Delete                     |    ✅ Anyone     |  ✅ Students in modules  |       ✅ Lecturers only        |
| **Reports** – View                                      |        ✅        |       ✅ Own data        |        ❌ (redirected)         |

---

## 6. Service Registration (Program.cs)

The two services that support authentication are registered in the DI container:

```csharp
// Hashes and verifies passwords (ASP.NET Identity algorithm)
builder.Services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();

// Sends password-reset OTP emails via SMTP
builder.Services.AddScoped<IEmailService, EmailService>();
```

SMTP settings are read from `appsettings.json`:

```json
"Smtp": {
  "Host":     "smtp.example.com",
  "Port":     "587",
  "User":     "noreply@example.com",
  "Password": "secret",
  "FromName": "UniManage"
}
```

---

## 7. Security Design Decisions

| Decision                            | Reason                                                                                                              |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| **Session-based auth** (not JWT)    | Simpler server-side approach; sessions are invalidated instantly on logout                                          |
| **ASP.NET Identity PasswordHasher** | Industry-standard adaptive hashing (PBKDF2 with HMAC-SHA256); avoids writing custom crypto                          |
| **`HttpOnly` session cookie**       | Prevents JavaScript from reading the cookie — protects against XSS token theft                                      |
| **Generic login error message**     | `"Invalid email or password."` — does not reveal whether the email exists                                           |
| **Generic OTP request message**     | `"If that email is registered, an OTP has been sent."` — same protection for the reset flow                         |
| **OTP invalidation on re-request**  | Previous unused OTPs are deleted when a new one is requested — prevents OTP accumulation                            |
| **OTP 10-minute expiry**            | Limits the window of opportunity if an OTP is intercepted                                                           |
| **`IsVerified` + `IsUsed` flags**   | Two-stage check: OTP must be verified before the reset form is shown, and marked used after                         |
| **`IsActive` check on login**       | Disabled accounts cannot log in even with the correct password                                                      |
| **CSRF protection**                 | `[ValidateAntiForgeryToken]` on every `POST` action prevents cross-site request forgery                             |
| **`return Forbid()`**               | Returns HTTP 403 for role violations — distinguishes "not authenticated" (401/redirect) from "not authorised" (403) |

---

## 8. Sequence Diagrams

### 8.1 Normal Login

```
User          Browser         AccountController       Session         Database
 |               |                   |                   |               |
 |--enter creds->|                   |                   |               |
 |               |--POST /Login----->|                   |               |
 |               |                   |--query user------>|               |
 |               |                   |<--UserModel + Role----------------|
 |               |                   |--VerifyHashedPassword()           |
 |               |                   |--SetInt32("UserId")-------------->|
 |               |                   |--SetString("UserRole")----------->|
 |               |<--Redirect /Dashboard-|               |               |
```

### 8.2 Request Protected Page (Not Logged In)

```
Browser                 BaseController.OnActionExecuting
   |                              |
   |-- GET /Assignments --------->|
   |                              |-- Session.GetInt32("UserId") == null
   |<-- Redirect /Account/Login --|
```

### 8.3 Password Reset OTP Flow

```
User          Browser         AccountController       EmailService      Database
 |               |                   |                   |               |
 |--enter email->|                   |                   |               |
 |               |--POST ForgotPwd-->|                   |               |
 |               |                   |--delete old OTPs->|               |
 |               |                   |--save new OTP---->|               |
 |               |                   |--SendAsync()----->|               |
 |               |                   |                   |--SMTP email-->|
 |<--(receives OTP in email)---------|                   |               |
 |               |                   |                   |               |
 |--enter OTP--->|                   |                   |               |
 |               |--POST VerifyOtp-->|                   |               |
 |               |                   |--check OTP------->|               |
 |               |                   |--IsVerified=true->|               |
 |               |<--Redirect ResetPassword-|            |               |
 |               |                   |                   |               |
 |--new password->|                  |                   |               |
 |               |--POST ResetPwd--->|                   |               |
 |               |                   |--HashPassword()   |               |
 |               |                   |--update user----->|               |
 |               |                   |--IsUsed=true----->|               |
 |               |<--Redirect Login--|                   |               |
```

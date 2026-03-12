# EventsTrackerApi Refactoring Plan

Based on .NET/C# Best Practices analysis. This document outlines issues found and fixes to implement.

---

## 1. Missing Service Layer ⚠️ HIGH PRIORITY

### Problem
Business logic scattered between Controllers and Repositories. Controllers do too much:
- `AuthController`: JWT token generation (`GenerateJwtToken`), password hashing validation
- `UserController`: File upload logic, password generation
- `InvitationsController`: Email orchestration, validation logic

### Fix
Create a `Services/` layer:

```
Services/
├── Interfaces/
│   ├── IAuthService.cs          # JWT generation, login logic
│   ├── IUserService.cs          # User business logic
│   ├── IInvitationService.cs    # Invitation orchestration
│   └── IPasswordService.cs      # Password hashing/verification
├── AuthService.cs
├── UserService.cs
├── InvitationService.cs
└── PasswordService.cs
```

### Files to Modify
- Create new service files
- Refactor: `AuthController`, `UserController`, `InvitationsController`

---

## 2. Missing XML Documentation 📝 MEDIUM PRIORITY

### Problem
Only ~3 files have XML comments. Most classes/methods lack documentation.

### Fix
Add comprehensive XML doc comments to:
- All public classes and interfaces
- All public methods with params/returns
- DTOs and models

### Example
```csharp
/// <summary>
/// Retrieves a user by their unique identifier.
/// </summary>
/// <param name="id">The unique identifier of the user.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The user entity or null if not found.</returns>
public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
```

---

## 3. No Resource Files for Localization 📝 MEDIUM PRIORITY

### Problem
All strings hardcoded in `Commons.cs` (email HTML templates ~400 lines, error messages in controllers).

### Fix
Create ResourceManager-based localization:

```
Resources/
├── Strings.resx              # Default (Spanish)
├── Strings.es.resx
└── Strings.en.resx
```

Replace hardcoded strings with:
```csharp
_resourceManager.GetString("PasswordRecoveryEmailBody", culture)
```

---

## 4. Static Utility Classes Should Be Services ⚙️ MEDIUM PRIORITY

### Problem
`Commons.cs` has static methods for:
- Password hashing (`CreatePasswordHash`, `VerifyPassword`)
- Password generation (`GeneratePassword`)
- HTML email templates

### Fix
Convert to injectable services:
```csharp
public interface IPasswordService
{
    string HashPassword(string password);
    password, string hash bool VerifyPassword(string);
    string GeneratePassword(int length);
}

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
    
    public bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
    
    public string GeneratePassword(int length)
        => // ... implementation
}
```

---

## 5. Inconsistent Error Handling & Logging 🔧 MEDIUM PRIORITY

### Problems
- `AuthController:104-107`: catches generic `Exception`, returns 500 with `ex.Message` (exposes internals!)
- `UserController:72`: uses `Console.WriteLine` instead of logger
- Some endpoints lack try-catch entirely

### Fix
- Use structured logging: `_logger.LogError(ex, "Failed to {Action}", context)`
- Never expose internal error messages to clients in production
- Add consistent try-catch with proper error responses

---

## 6. Configuration Not Strongly-Typed 🔧 LOW PRIORITY

### Problem
`IConfiguration` accessed via magic strings everywhere (`configuration["Jwt:Key"]`).

### Fix
Create configuration classes:
```csharp
public class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required] public string Key { get; set; } = string.Empty;
    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string Audience { get; set; } = string.Empty;
}
```

Register in `Program.cs`:
```csharp
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
```

---

## 7. Typo in Service Name 🔧 LOW PRIORITY

### Problem
`DivaceTokenService.cs` → should be `DeviceTokenService`

### Fix
Rename file and class.

---

## 8. Missing Null Checks in Constructors 🔧 LOW PRIORITY

### Problem
Primary constructors don't validate injected dependencies with `ArgumentNullException`.

### Fix
Add validation or use a base class/filter:
```csharp
public class AuthController(
    IRepository<User> userRepository,
    IConfiguration configuration,
    IEmailSender emailSender
)
{
    ArgumentNullException.ThrowIfNull(userRepository);
    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentNullException.ThrowIfNull(emailSender);
    // ...
}
```

---

## 9. No Tests 🚨 HIGH PRIORITY - Infrastructure

### Problem
No test project exists.

### Fix
```bash
dotnet new xunit -n EventsTrackerApi.Tests
# Add FluentAssertions, Moq
```

### Recommended Tests
- AuthService: Login success/failure
- UserService: CRUD operations
- InvitationService: Batch creation
- Repositories: Basic CRUD

---

## 10. File Upload Logic Duplicated 📝 MEDIUM PRIORITY

### Problem
`UserController` has inline file handling. `ImageFilesUtils` exists but not used consistently.

### Fix
Use `ImageFilesUtils` consistently, or create `IFileUploadService`.

---

## 11. Inconsistent Namespace Structure 🔧 LOW PRIORITY

### Problem
No clear `{Feature}` subfolder organization.

### Fix
Organize by feature:
```
Controllers/
├── Auth/AuthController.cs
├── Users/UsersController.cs
├── Events/EventsController.cs
└── Invitations/InvitationsController.cs
```

---

## Execution Order

| Phase | Tasks | Effort |
|-------|-------|--------|
| **1** | Create Service interfaces & implementations (Auth, Password, User, Invitation) | High |
| **2** | Refactor Controllers to use Services | High |
| **3** | Add XML documentation to all public APIs | Medium |
| **4** | Create Resource files for localization | Medium |
| **5** | Fix typos, add null checks, improve logging | Low |
| **6** | Add tests | High |

---

## Quick Wins (Start Here)

1. **Fix typo**: Rename `DivaceTokenService` → `DeviceTokenService`
2. **Replace Console.WriteLine**: Change to `_logger.LogInformation` in UserController
3. **Remove ex.Message exposure**: Sanitize error responses in AuthController
4. **Add XML docs**: Start with `IRepository<T>` and base interfaces

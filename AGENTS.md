# AGENTS.md - EventsTrackerApi Development Guide

This file provides guidelines for agents working on the EventsTrackerApi codebase.

## Project Overview

- **Framework**: .NET 8.0 ASP.NET Core Web API
- **Database**: MySQL 8.0+ with Entity Framework Core (Pomelo provider)
- **Authentication**: JWT Bearer tokens
- **Key Dependencies**: EF Core, MySQL, JWT, Swagger, Firebase, MailKit, BCrypt

---

## Build & Run Commands

### Build the project
```bash
dotnet build
```

### Run the API (development)
```bash
dotnet run --project EventsTrackerApi/EventsTrackerApi.csproj
```

### Run with hot reload
```bash
dotnet watch --project EventsTrackerApi/EventsTrackerApi.csproj
```

### Clean build
```bash
dotnet clean && dotnet build
```

### Database Migrations (EF Core)
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
dotnet ef migrations remove
```

### Testing
**No test project exists yet.** To add tests:
```bash
dotnet new xunit -n EventsTrackerApi.Tests
dotnet add EventsTrackerApi.Tests reference EventsTrackerApi/EventsTrackerApi.csproj
dotnet test
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

---

## Architecture (Phases 1-3 Complete)

```
HTTP Request → Controller → Service → Repository → Database
```

### File Organization
```
EventsTrackerApi/
├── Controllers/          # API endpoints (XxxController.cs)
├── Models/               # Entity models, enums (Models/enum/)
├── DTOs/                 # Request/Response objects (feature-folders)
├── Repositories/         # Data access (interfaces + implementations)
│   └── mappers/          # Entity to DTO mappers
├── Service/              # Business services
│   └── Interfaces/       # Service interfaces (IPasswordService, etc.)
├── Job/                  # Background jobs (IHostedService)
├── Data/                 # DbContext
├── Utils/                # Utility classes
├── Resources/            # Localization files (.resx)
├── Migrations/           # EF Core migrations
└── wwwroot/              # Static files (uploads/)
```

---

## Code Style Guidelines

### General Conventions
- **C# Version**: Latest (C# 12), with `ImplicitUsings` and `Nullable` enabled
- **Target Framework**: `net8.0`
- **Architecture**: Controller → Service → Repository pattern with DTOs
- **Dependency Injection**: Constructor injection throughout

### Naming Conventions
| Element | Convention | Example |
|---------|-----------|---------|
| Classes/Models | PascalCase | `Event`, `UserDto` |
| Interfaces | `I` + Entity + `Repository/Service` | `IEventRepository`, `IAuthService` |
| Methods | PascalCase + `Async` suffix | `GetEventsAsync` |
| Properties | PascalCase | `EventId`, `StartDateTime` |
| DTOs | Feature folders under `DTOs/` | `DTOs/Event/EventDto.cs` |

### Service Layer Pattern (Phase 1)
- **Interfaces**: `Service/Interfaces/IAuthService.cs`
- **Implementations**: `Service/AuthService.cs` (with `Service` suffix)
- **Registration**: `builder.Services.AddScoped<IAuthService, AuthService>();`
- All methods: async returning `Task<T>`

### XML Documentation (Phase 2)
All public APIs must have XML docs:
```csharp
/// <summary>
/// Brief description of the class/method.
/// </summary>
/// <param name="parameterName">Description of the parameter.</param>
/// <returns>Description of the return value.</returns>
Task<User?> GetByEmailAsync(string email);
```

### Localization (Phase 3)
- Resource files in `Resources/` folder
- Default: `Strings.resx` (Spanish)
- Translations: `Strings.<culture>.resx` (e.g., `Strings.en.resx`)
- Use `ILocalizationService` to get localized strings
- Use `IEmailTemplateService` for email templates

### Imports
- File-scoped namespaces: `namespace EventsTrackerApi.Controllers;`
- Group: System → Microsoft → Third-party → Project

### Entity Models
- `[Key]`, `[Required]`, `[MaxLength(n)]`, `[ForeignKey]`
- `[JsonIgnore]` for circular references, `[NotMapped]` for computed
- Enums in `Models/enum/` folder

### Controller Patterns
```csharp
[Route("api/events")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventRepository eventRepository,
        IAuthService authService, ILogger<EventsController> logger)
    {
        _eventRepository = eventRepository;
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents(
        [FromQuery] EventsFilterDto request)

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ResponseDto>> CreateEvent([FromForm] CreateEventDto form)
}
```

### Repository Pattern
- Interface in `Repositories/`, implementation with `Repository` suffix
- Use `IRepository<T>` base generic for CRUD
- Mappers in `Repositories/mappers/`

### Error Handling
- `ModelState.IsValid` for request validation
- HTTP status codes: `Ok()` (200), `CreatedAtAction()` (201), `BadRequest()` (400), `Unauthorized()` (401), `Forbid()` (403), `NotFound()` (404), `NoContent()` (204)
- Use `_logger.LogInformation` / `_logger.LogError`, NOT `Console.WriteLine`

### Background Jobs
- `BackgroundService` or `IHostedService`
- Register: `builder.Services.AddHostedService<JobName>();`
- Current: `InvitationNotificationJob`, `EventsExpiredJob`, `EventsSyncJob`

### Authentication
- JWT Bearer: `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`
- Claims: `User.FindFirst("ClaimName")?.Value`

---

## Important Notes

- **Phase 1**: Service Layer complete - business logic centralized in Service layer
- **Phase 2**: XML Documentation complete - all public APIs documented
- **Phase 3**: Localization complete - ResourceManager-based i18n support
- **No tests exist** - Phase 5 in refactor plan
- **No linting** - no StyleCop configured
- **User Secrets**: `dotnet user-secrets init` in development
- **Swagger**: Available at root URL in development mode

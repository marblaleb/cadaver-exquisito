# CadaverExquisito — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a full-stack async collaborative writing app (Exquisite Corpse) — .NET 8 REST API + Flutter mobile app (iOS/Android).

**Architecture:** Clean Architecture backend (Controllers → Services → Repositories → EF Core/PostgreSQL) authenticated via Firebase ID tokens. Flutter frontend using Riverpod + Dio, Soft UI 90s pastel visual style.

**Tech Stack:** .NET 8, EF Core, PostgreSQL, FirebaseAdmin SDK, xUnit, Moq / Flutter 3.x, Riverpod, Dio, Firebase Auth + FCM, Google Fonts (Playfair Display + DM Sans), Phosphor Flutter icons.

---

## File Map

### Backend
```
backend/
├── CadaverExquisito.sln
├── CadaverExquisito.API/
│   ├── CadaverExquisito.API.csproj
│   ├── Program.cs
│   ├── Domain/Entities/
│   │   ├── User.cs
│   │   ├── Cadaver.cs
│   │   └── Fragment.cs
│   ├── Application/
│   │   ├── DTOs/
│   │   │   ├── AuthDtos.cs
│   │   │   ├── CadaverDtos.cs
│   │   │   └── FragmentDtos.cs
│   │   ├── Interfaces/
│   │   │   ├── ICadaverRepository.cs
│   │   │   ├── IFragmentRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   └── INotificationService.cs
│   │   └── Services/
│   │       ├── CadaverService.cs
│   │       ├── FragmentService.cs
│   │       └── NotificationService.cs
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Repositories/
│   │   │       ├── CadaverRepository.cs
│   │   │       ├── FragmentRepository.cs
│   │   │       └── UserRepository.cs
│   │   └── Notifications/
│   │       └── FcmNotificationService.cs
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── CadaversController.cs
│   │   └── FragmentsController.cs
│   └── Middleware/
│       └── FirebaseAuthMiddleware.cs
└── CadaverExquisito.Tests/
    ├── CadaverExquisito.Tests.csproj
    └── Services/
        ├── CadaverServiceTests.cs
        └── FragmentServiceTests.cs
```

### Frontend
```
frontend/cadaver_exquisito_app/
├── pubspec.yaml
└── lib/
    ├── main.dart
    ├── core/
    │   ├── api/
    │   │   ├── api_client.dart
    │   │   └── endpoints.dart
    │   └── theme/
    │       └── app_theme.dart
    └── features/
        ├── auth/
        │   ├── screens/login_screen.dart
        │   └── providers/auth_provider.dart
        ├── cadavers/
        │   ├── screens/
        │   │   ├── main_scaffold.dart
        │   │   ├── participate_tab.dart
        │   │   └── archive_tab.dart
        │   ├── widgets/
        │   │   ├── cadaver_card.dart
        │   │   └── create_cadaver_sheet.dart
        │   └── providers/
        │       ├── available_cadavers_provider.dart
        │       └── completed_cadavers_provider.dart
        └── editor/
            ├── screens/editor_screen.dart
            └── providers/word_count_provider.dart
```

---

## PART 1 — BACKEND

---

### Task 1: Scaffold .NET solution

**Files:**
- Create: `backend/CadaverExquisito.sln`
- Create: `backend/CadaverExquisito.API/CadaverExquisito.API.csproj`
- Create: `backend/CadaverExquisito.Tests/CadaverExquisito.Tests.csproj`

- [ ] **Step 1: Create solution and projects**

```bash
mkdir backend && cd backend
dotnet new sln -n CadaverExquisito
dotnet new webapi -n CadaverExquisito.API --no-openapi false
dotnet new xunit -n CadaverExquisito.Tests
dotnet sln add CadaverExquisito.API/CadaverExquisito.API.csproj
dotnet sln add CadaverExquisito.Tests/CadaverExquisito.Tests.csproj
cd CadaverExquisito.Tests && dotnet add reference ../CadaverExquisito.API/CadaverExquisito.API.csproj
```

- [ ] **Step 2: Add NuGet packages to API**

```bash
cd ../CadaverExquisito.API
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package FirebaseAdmin
dotnet add package Microsoft.EntityFrameworkCore.Design
```

- [ ] **Step 3: Add NuGet packages to Tests**

```bash
cd ../CadaverExquisito.Tests
dotnet add package Moq
dotnet add package FluentAssertions
```

- [ ] **Step 4: Delete boilerplate files**

```bash
cd ../CadaverExquisito.API
rm Controllers/WeatherForecastController.cs WeatherForecast.cs
cd ../CadaverExquisito.Tests
rm UnitTest1.cs
```

- [ ] **Step 5: Verify build**

```bash
cd .. && dotnet build
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "chore: scaffold .NET solution with API and Tests projects"
```

---

### Task 2: Domain entities

**Files:**
- Create: `backend/CadaverExquisito.API/Domain/Entities/User.cs`
- Create: `backend/CadaverExquisito.API/Domain/Entities/Cadaver.cs`
- Create: `backend/CadaverExquisito.API/Domain/Entities/Fragment.cs`

- [ ] **Step 1: Create directory structure**

```bash
mkdir -p CadaverExquisito.API/Domain/Entities
mkdir -p CadaverExquisito.API/Application/DTOs
mkdir -p CadaverExquisito.API/Application/Interfaces
mkdir -p CadaverExquisito.API/Application/Services
mkdir -p CadaverExquisito.API/Infrastructure/Data/Repositories
mkdir -p CadaverExquisito.API/Infrastructure/Notifications
mkdir -p CadaverExquisito.API/Middleware
```

- [ ] **Step 2: Write User entity**

`CadaverExquisito.API/Domain/Entities/User.cs`:
```csharp
namespace CadaverExquisito.API.Domain.Entities;

public class User
{
    public string Id { get; set; } = string.Empty; // Firebase UID
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FcmToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Cadaver> CreatedCadavers { get; set; } = new List<Cadaver>();
    public ICollection<Fragment> Fragments { get; set; } = new List<Fragment>();
}
```

- [ ] **Step 3: Write Cadaver entity**

`CadaverExquisito.API/Domain/Entities/Cadaver.cs`:
```csharp
namespace CadaverExquisito.API.Domain.Entities;

public class Cadaver
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int CurrentTurn { get; set; } = 1;
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string CreatedByUserId { get; set; } = string.Empty;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<Fragment> Fragments { get; set; } = new List<Fragment>();
}
```

- [ ] **Step 4: Write Fragment entity**

`CadaverExquisito.API/Domain/Entities/Fragment.cs`:
```csharp
namespace CadaverExquisito.API.Domain.Entities;

public class Fragment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CadaverId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cadaver Cadaver { get; set; } = null!;
    public User User { get; set; } = null!;
}
```

- [ ] **Step 5: Commit**

```bash
git add .
git commit -m "feat: add domain entities User, Cadaver, Fragment"
```

---

### Task 3: EF Core DbContext + migration

**Files:**
- Create: `backend/CadaverExquisito.API/Infrastructure/Data/AppDbContext.cs`
- Modify: `backend/CadaverExquisito.API/appsettings.json`

- [ ] **Step 1: Write AppDbContext**

`CadaverExquisito.API/Infrastructure/Data/AppDbContext.cs`:
```csharp
using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Cadaver> Cadavers => Set<Cadaver>();
    public DbSet<Fragment> Fragments => Set<Fragment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Cadaver>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.CreatedByUser)
             .WithMany(u => u.CreatedCadavers)
             .HasForeignKey(c => c.CreatedByUserId);
        });

        modelBuilder.Entity<Fragment>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasOne(f => f.Cadaver)
             .WithMany(c => c.Fragments)
             .HasForeignKey(f => f.CadaverId);
            e.HasOne(f => f.User)
             .WithMany(u => u.Fragments)
             .HasForeignKey(f => f.UserId);
            e.HasIndex(f => new { f.CadaverId, f.UserId }).IsUnique();
        });
    }
}
```

- [ ] **Step 2: Add connection string to appsettings.json**

`CadaverExquisito.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cadaver_exquisito;Username=postgres;Password=postgres"
  },
  "Firebase": {
    "ProjectId": "YOUR_FIREBASE_PROJECT_ID"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: Register DbContext in Program.cs (temporary minimal setup)**

`CadaverExquisito.API/Program.cs` (replace all content):
```csharp
using CadaverExquisito.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

- [ ] **Step 4: Create initial migration**

```bash
cd CadaverExquisito.API
dotnet ef migrations add InitialCreate --output-dir Infrastructure/Data/Migrations
```
Expected: `Done. To undo this action, use 'ef migrations remove'`

- [ ] **Step 5: Verify migration files were generated**

```bash
ls Infrastructure/Data/Migrations/
```
Expected: files named `*_InitialCreate.cs` and `AppDbContextModelSnapshot.cs`

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "feat: add EF Core DbContext and initial migration"
```

---

### Task 4: Repository interfaces and implementations

**Files:**
- Create: `backend/CadaverExquisito.API/Application/Interfaces/ICadaverRepository.cs`
- Create: `backend/CadaverExquisito.API/Application/Interfaces/IFragmentRepository.cs`
- Create: `backend/CadaverExquisito.API/Application/Interfaces/IUserRepository.cs`
- Create: `backend/CadaverExquisito.API/Infrastructure/Data/Repositories/CadaverRepository.cs`
- Create: `backend/CadaverExquisito.API/Infrastructure/Data/Repositories/FragmentRepository.cs`
- Create: `backend/CadaverExquisito.API/Infrastructure/Data/Repositories/UserRepository.cs`

- [ ] **Step 1: Write ICadaverRepository**

`CadaverExquisito.API/Application/Interfaces/ICadaverRepository.cs`:
```csharp
using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Interfaces;

public interface ICadaverRepository
{
    Task<List<Cadaver>> GetAvailableForUserAsync(string userId);
    Task<List<Cadaver>> GetPendingForUserAsync(string userId);
    Task<List<Cadaver>> GetCompletedAsync();
    Task<Cadaver?> GetByIdAsync(Guid id);
    Task<Cadaver> CreateAsync(Cadaver cadaver);
    Task UpdateAsync(Cadaver cadaver);
}
```

- [ ] **Step 2: Write IFragmentRepository**

`CadaverExquisito.API/Application/Interfaces/IFragmentRepository.cs`:
```csharp
using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Interfaces;

public interface IFragmentRepository
{
    Task<Fragment?> GetBySequenceOrderAsync(Guid cadaverId, int sequenceOrder);
    Task<bool> UserHasFragmentAsync(Guid cadaverId, string userId);
    Task<Fragment> CreateAsync(Fragment fragment);
    Task<List<Fragment>> GetAllByCadaverOrderedAsync(Guid cadaverId);
    Task<List<string>> GetParticipantUserIdsAsync(Guid cadaverId);
}
```

- [ ] **Step 3: Write IUserRepository**

`CadaverExquisito.API/Application/Interfaces/IUserRepository.cs`:
```csharp
using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task UpsertAsync(User user);
    Task UpdateFcmTokenAsync(string userId, string fcmToken);
    Task<List<string>> GetFcmTokensExcludingParticipantsAsync(Guid cadaverId);
}
```

- [ ] **Step 4: Write CadaverRepository**

`CadaverExquisito.API/Infrastructure/Data/Repositories/CadaverRepository.cs`:
```csharp
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data.Repositories;

public class CadaverRepository(AppDbContext db) : ICadaverRepository
{
    public async Task<List<Cadaver>> GetAvailableForUserAsync(string userId)
    {
        var participatedIds = await db.Fragments
            .Where(f => f.UserId == userId)
            .Select(f => f.CadaverId)
            .ToListAsync();

        return await db.Cadavers
            .Where(c => !c.IsCompleted && !participatedIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Cadaver>> GetPendingForUserAsync(string userId)
    {
        var participatedIds = await db.Fragments
            .Where(f => f.UserId == userId)
            .Select(f => f.CadaverId)
            .ToListAsync();

        return await db.Cadavers
            .Where(c => !c.IsCompleted && participatedIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Cadaver>> GetCompletedAsync() =>
        await db.Cadavers
            .Where(c => c.IsCompleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Cadaver?> GetByIdAsync(Guid id) =>
        await db.Cadavers.FindAsync(id);

    public async Task<Cadaver> CreateAsync(Cadaver cadaver)
    {
        db.Cadavers.Add(cadaver);
        await db.SaveChangesAsync();
        return cadaver;
    }

    public async Task UpdateAsync(Cadaver cadaver)
    {
        db.Cadavers.Update(cadaver);
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 5: Write FragmentRepository**

`CadaverExquisito.API/Infrastructure/Data/Repositories/FragmentRepository.cs`:
```csharp
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data.Repositories;

public class FragmentRepository(AppDbContext db) : IFragmentRepository
{
    public async Task<Fragment?> GetBySequenceOrderAsync(Guid cadaverId, int sequenceOrder) =>
        await db.Fragments
            .FirstOrDefaultAsync(f => f.CadaverId == cadaverId && f.SequenceOrder == sequenceOrder);

    public async Task<bool> UserHasFragmentAsync(Guid cadaverId, string userId) =>
        await db.Fragments.AnyAsync(f => f.CadaverId == cadaverId && f.UserId == userId);

    public async Task<Fragment> CreateAsync(Fragment fragment)
    {
        db.Fragments.Add(fragment);
        await db.SaveChangesAsync();
        return fragment;
    }

    public async Task<List<Fragment>> GetAllByCadaverOrderedAsync(Guid cadaverId) =>
        await db.Fragments
            .Where(f => f.CadaverId == cadaverId)
            .Include(f => f.User)
            .OrderBy(f => f.SequenceOrder)
            .ToListAsync();

    public async Task<List<string>> GetParticipantUserIdsAsync(Guid cadaverId) =>
        await db.Fragments
            .Where(f => f.CadaverId == cadaverId)
            .Select(f => f.UserId)
            .Distinct()
            .ToListAsync();
}
```

- [ ] **Step 6: Write UserRepository**

`CadaverExquisito.API/Infrastructure/Data/Repositories/UserRepository.cs`:
```csharp
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id) =>
        await db.Users.FindAsync(id);

    public async Task UpsertAsync(User user)
    {
        var existing = await db.Users.FindAsync(user.Id);
        if (existing is null)
            db.Users.Add(user);
        else
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
            if (user.FcmToken is not null) existing.FcmToken = user.FcmToken;
        }
        await db.SaveChangesAsync();
    }

    public async Task UpdateFcmTokenAsync(string userId, string fcmToken)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.FcmToken = fcmToken;
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetFcmTokensExcludingParticipantsAsync(Guid cadaverId)
    {
        var participantIds = await db.Fragments
            .Where(f => f.CadaverId == cadaverId)
            .Select(f => f.UserId)
            .ToListAsync();

        return await db.Users
            .Where(u => !participantIds.Contains(u.Id) && u.FcmToken != null)
            .Select(u => u.FcmToken!)
            .ToListAsync();
    }
}
```

- [ ] **Step 7: Commit**

```bash
git add .
git commit -m "feat: add repository interfaces and EF Core implementations"
```

---

### Task 5: DTOs

**Files:**
- Create: `backend/CadaverExquisito.API/Application/DTOs/AuthDtos.cs`
- Create: `backend/CadaverExquisito.API/Application/DTOs/CadaverDtos.cs`
- Create: `backend/CadaverExquisito.API/Application/DTOs/FragmentDtos.cs`

- [ ] **Step 1: Write AuthDtos**

`CadaverExquisito.API/Application/DTOs/AuthDtos.cs`:
```csharp
namespace CadaverExquisito.API.Application.DTOs;

public record RegisterUserRequest(string Name, string Email, string? FcmToken);
public record UpdateFcmTokenRequest(string FcmToken);
```

- [ ] **Step 2: Write CadaverDtos**

`CadaverExquisito.API/Application/DTOs/CadaverDtos.cs`:
```csharp
namespace CadaverExquisito.API.Application.DTOs;

public record CreateCadaverRequest(int MaxParticipants, string Content);

public record CadaverDto(
    Guid Id,
    string Title,
    int MaxParticipants,
    int CurrentTurn,
    DateTime CreatedAt);

public record FullCadaverDto(
    Guid Id,
    string Title,
    List<FullFragmentDto> Fragments);

public record FullFragmentDto(
    string Content,
    int SequenceOrder,
    string AuthorName);
```

- [ ] **Step 3: Write FragmentDtos**

`CadaverExquisito.API/Application/DTOs/FragmentDtos.cs`:
```csharp
namespace CadaverExquisito.API.Application.DTOs;

public record AddFragmentRequest(string Content);

public record FragmentDto(
    Guid Id,
    string Content,
    int SequenceOrder);
```

- [ ] **Step 4: Commit**

```bash
git add .
git commit -m "feat: add application DTOs"
```

---

### Task 6: CadaverService + unit tests

**Files:**
- Create: `backend/CadaverExquisito.API/Application/Services/CadaverService.cs`
- Create: `backend/CadaverExquisito.Tests/Services/CadaverServiceTests.cs`

- [ ] **Step 1: Write failing tests first**

`CadaverExquisito.Tests/Services/CadaverServiceTests.cs`:
```csharp
using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CadaverExquisito.Tests.Services;

public class CadaverServiceTests
{
    private readonly Mock<ICadaverRepository> _cadaverRepo = new();
    private readonly Mock<IFragmentRepository> _fragmentRepo = new();
    private readonly CadaverService _sut;

    public CadaverServiceTests()
    {
        _sut = new CadaverService(_cadaverRepo.Object, _fragmentRepo.Object);
    }

    [Fact]
    public async Task GetAvailableAsync_ReturnsMappedDtos()
    {
        var userId = "user-1";
        var cadaver = new Cadaver
        {
            Id = Guid.NewGuid(),
            Title = "Historia del 18 may 2026",
            MaxParticipants = 3,
            CurrentTurn = 1,
            CreatedAt = new DateTime(2026, 5, 18)
        };
        _cadaverRepo.Setup(r => r.GetAvailableForUserAsync(userId))
            .ReturnsAsync(new List<Cadaver> { cadaver });

        var result = await _sut.GetAvailableAsync(userId);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(cadaver.Id);
        result[0].Title.Should().Be(cadaver.Title);
    }

    [Fact]
    public async Task CreateAsync_SetsTitleFromCreatedAt()
    {
        var userId = "user-1";
        var request = new CreateCadaverRequest(3, "Había una vez...");
        _cadaverRepo.Setup(r => r.CreateAsync(It.IsAny<Cadaver>()))
            .ReturnsAsync((Cadaver c) => c);
        _fragmentRepo.Setup(r => r.CreateAsync(It.IsAny<Fragment>()))
            .ReturnsAsync((Fragment f) => f);

        var result = await _sut.CreateAsync(userId, request);

        result.Title.Should().StartWith("Historia del");
        result.MaxParticipants.Should().Be(3);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd CadaverExquisito.Tests && dotnet test --filter "CadaverServiceTests"
```
Expected: FAIL — `CadaverService` does not exist yet

- [ ] **Step 3: Write CadaverService**

`CadaverExquisito.API/Application/Services/CadaverService.cs`:
```csharp
using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Services;

public class CadaverService(ICadaverRepository cadaverRepo, IFragmentRepository fragmentRepo)
{
    public async Task<List<CadaverDto>> GetAvailableAsync(string userId)
    {
        var cadavers = await cadaverRepo.GetAvailableForUserAsync(userId);
        return cadavers.Select(ToDto).ToList();
    }

    public async Task<List<CadaverDto>> GetPendingAsync(string userId)
    {
        var cadavers = await cadaverRepo.GetPendingForUserAsync(userId);
        return cadavers.Select(ToDto).ToList();
    }

    public async Task<List<CadaverDto>> GetCompletedAsync()
    {
        var cadavers = await cadaverRepo.GetCompletedAsync();
        return cadavers.Select(ToDto).ToList();
    }

    public async Task<CadaverDto> CreateAsync(string userId, CreateCadaverRequest request)
    {
        var now = DateTime.UtcNow;
        var cadaver = new Cadaver
        {
            Title = $"Historia del {now:dd MMM yyyy}",
            MaxParticipants = request.MaxParticipants,
            CurrentTurn = 1,
            CreatedAt = now,
            CreatedByUserId = userId
        };

        var saved = await cadaverRepo.CreateAsync(cadaver);

        var fragment = new Fragment
        {
            CadaverId = saved.Id,
            UserId = userId,
            Content = request.Content,
            SequenceOrder = 1,
            CreatedAt = now
        };
        await fragmentRepo.CreateAsync(fragment);

        saved.CurrentTurn = 2;
        if (saved.CurrentTurn > saved.MaxParticipants)
            saved.IsCompleted = true;

        await cadaverRepo.UpdateAsync(saved);
        return ToDto(saved);
    }

    private static CadaverDto ToDto(Cadaver c) =>
        new(c.Id, c.Title, c.MaxParticipants, c.CurrentTurn, c.CreatedAt);
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test --filter "CadaverServiceTests"
```
Expected: PASS — 2 tests

- [ ] **Step 5: Commit**

```bash
git add .
git commit -m "feat: add CadaverService with unit tests"
```

---

### Task 7: FragmentService + unit tests

**Files:**
- Create: `backend/CadaverExquisito.API/Application/Services/FragmentService.cs`
- Create: `backend/CadaverExquisito.Tests/Services/FragmentServiceTests.cs`

- [ ] **Step 1: Write failing tests**

`CadaverExquisito.Tests/Services/FragmentServiceTests.cs`:
```csharp
using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CadaverExquisito.Tests.Services;

public class FragmentServiceTests
{
    private readonly Mock<ICadaverRepository> _cadaverRepo = new();
    private readonly Mock<IFragmentRepository> _fragmentRepo = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly FragmentService _sut;

    public FragmentServiceTests()
    {
        _sut = new FragmentService(_cadaverRepo.Object, _fragmentRepo.Object, _notifications.Object);
    }

    [Fact]
    public async Task AddFragment_ThrowsIfUserAlreadyParticipated()
    {
        var cadaverId = Guid.NewGuid();
        var userId = "user-1";
        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId))
            .ReturnsAsync(new Cadaver { Id = cadaverId, MaxParticipants = 3, CurrentTurn = 2 });
        _fragmentRepo.Setup(r => r.UserHasFragmentAsync(cadaverId, userId))
            .ReturnsAsync(true);

        var act = () => _sut.AddFragmentAsync(cadaverId, userId, new AddFragmentRequest("texto"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already*");
    }

    [Fact]
    public async Task AddFragment_ThrowsIfContentExceedsWordLimit()
    {
        var cadaverId = Guid.NewGuid();
        var userId = "user-1";
        var longContent = string.Join(" ", Enumerable.Repeat("palabra", 301));

        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId))
            .ReturnsAsync(new Cadaver { Id = cadaverId, MaxParticipants = 3, CurrentTurn = 2 });
        _fragmentRepo.Setup(r => r.UserHasFragmentAsync(cadaverId, userId))
            .ReturnsAsync(false);

        var act = () => _sut.AddFragmentAsync(cadaverId, userId, new AddFragmentRequest(longContent));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*300*");
    }

    [Fact]
    public async Task AddFragment_MarksCompletedWhenLastParticipant()
    {
        var cadaverId = Guid.NewGuid();
        var userId = "user-3";
        var cadaver = new Cadaver { Id = cadaverId, MaxParticipants = 3, CurrentTurn = 3 };

        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId)).ReturnsAsync(cadaver);
        _fragmentRepo.Setup(r => r.UserHasFragmentAsync(cadaverId, userId)).ReturnsAsync(false);
        _fragmentRepo.Setup(r => r.CreateAsync(It.IsAny<Fragment>()))
            .ReturnsAsync((Fragment f) => f);
        _fragmentRepo.Setup(r => r.GetParticipantUserIdsAsync(cadaverId))
            .ReturnsAsync(new List<string> { "user-1", "user-2", "user-3" });
        _notifications.Setup(n => n.SendToUsersAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _notifications.Setup(n => n.SendToNonParticipantsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.AddFragmentAsync(cadaverId, userId, new AddFragmentRequest("último fragmento"));

        cadaver.IsCompleted.Should().BeTrue();
        _cadaverRepo.Verify(r => r.UpdateAsync(It.Is<Cadaver>(c => c.IsCompleted)), Times.Once);
    }

    [Fact]
    public async Task GetLastFragment_ReturnsFragmentAtPreviousTurn()
    {
        var cadaverId = Guid.NewGuid();
        var cadaver = new Cadaver { Id = cadaverId, CurrentTurn = 3 };
        var fragment = new Fragment { Id = Guid.NewGuid(), Content = "texto previo", SequenceOrder = 2 };

        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId)).ReturnsAsync(cadaver);
        _fragmentRepo.Setup(r => r.GetBySequenceOrderAsync(cadaverId, 2)).ReturnsAsync(fragment);

        var result = await _sut.GetLastFragmentAsync(cadaverId);

        result!.Content.Should().Be("texto previo");
        result.SequenceOrder.Should().Be(2);
    }

    [Fact]
    public async Task GetLastFragment_ReturnsNullOnFirstTurn()
    {
        var cadaverId = Guid.NewGuid();
        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId))
            .ReturnsAsync(new Cadaver { Id = cadaverId, CurrentTurn = 1 });

        var result = await _sut.GetLastFragmentAsync(cadaverId);

        result.Should().BeNull();
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test --filter "FragmentServiceTests"
```
Expected: FAIL

- [ ] **Step 3: Write INotificationService interface**

`CadaverExquisito.API/Application/Interfaces/INotificationService.cs`:
```csharp
namespace CadaverExquisito.API.Application.Interfaces;

public interface INotificationService
{
    Task SendToUsersAsync(List<string> userIds, string title, string body);
    Task SendToNonParticipantsAsync(Guid cadaverId, string title, string body);
}
```

- [ ] **Step 4: Write FragmentService**

`CadaverExquisito.API/Application/Services/FragmentService.cs`:
```csharp
using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Services;

public class FragmentService(
    ICadaverRepository cadaverRepo,
    IFragmentRepository fragmentRepo,
    INotificationService notifications)
{
    public async Task<FragmentDto?> GetLastFragmentAsync(Guid cadaverId)
    {
        var cadaver = await cadaverRepo.GetByIdAsync(cadaverId)
            ?? throw new KeyNotFoundException($"Cadaver {cadaverId} not found");

        if (cadaver.CurrentTurn == 1) return null;

        var fragment = await fragmentRepo.GetBySequenceOrderAsync(cadaverId, cadaver.CurrentTurn - 1);
        return fragment is null ? null : new FragmentDto(fragment.Id, fragment.Content, fragment.SequenceOrder);
    }

    public async Task<FragmentDto> AddFragmentAsync(Guid cadaverId, string userId, AddFragmentRequest request)
    {
        var cadaver = await cadaverRepo.GetByIdAsync(cadaverId)
            ?? throw new KeyNotFoundException($"Cadaver {cadaverId} not found");

        if (await fragmentRepo.UserHasFragmentAsync(cadaverId, userId))
            throw new InvalidOperationException("User has already participated in this cadaver.");

        if (request.Content.Split(' ').Length > 300)
            throw new ArgumentException("Content must not exceed 300 words.");

        var fragment = new Fragment
        {
            CadaverId = cadaverId,
            UserId = userId,
            Content = request.Content,
            SequenceOrder = cadaver.CurrentTurn,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await fragmentRepo.CreateAsync(fragment);

        cadaver.CurrentTurn++;
        if (cadaver.CurrentTurn > cadaver.MaxParticipants)
        {
            cadaver.IsCompleted = true;
            await cadaverRepo.UpdateAsync(cadaver);
            _ = NotifyCompletedAsync(cadaverId);
        }
        else
        {
            await cadaverRepo.UpdateAsync(cadaver);
            _ = NotifyNextParticipantsAsync(cadaverId);
        }

        return new FragmentDto(saved.Id, saved.Content, saved.SequenceOrder);
    }

    public async Task<FullCadaverDto> GetFullCadaverAsync(Guid cadaverId)
    {
        var cadaver = await cadaverRepo.GetByIdAsync(cadaverId)
            ?? throw new KeyNotFoundException($"Cadaver {cadaverId} not found");

        if (!cadaver.IsCompleted)
            throw new InvalidOperationException("Cadaver is not completed yet.");

        var fragments = await fragmentRepo.GetAllByCadaverOrderedAsync(cadaverId);
        var fullFragments = fragments.Select(f =>
            new FullFragmentDto(f.Content, f.SequenceOrder, f.User?.Name ?? "Anónimo")).ToList();

        return new FullCadaverDto(cadaver.Id, cadaver.Title, fullFragments);
    }

    private async Task NotifyNextParticipantsAsync(Guid cadaverId)
    {
        try
        {
            await notifications.SendToNonParticipantsAsync(cadaverId, "Cadáver Exquisito", "Hay una historia esperando tu continuación.");
        }
        catch { /* swallow — notification failure should not break the request */ }
    }

    private async Task NotifyCompletedAsync(Guid cadaverId)
    {
        try
        {
            var participantIds = await fragmentRepo.GetParticipantUserIdsAsync(cadaverId);
            await notifications.SendToUsersAsync(participantIds, "Cadáver Exquisito", "La historia en la que participaste está completa.");
        }
        catch { }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

```bash
dotnet test --filter "FragmentServiceTests"
```
Expected: PASS — 5 tests

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "feat: add FragmentService with unit tests"
```

---

### Task 8: Firebase auth middleware

**Files:**
- Create: `backend/CadaverExquisito.API/Middleware/FirebaseAuthMiddleware.cs`

- [ ] **Step 1: Initialize FirebaseApp in Program.cs**

Add after `var builder = WebApplication.CreateBuilder(args);` in `Program.cs`:
```csharp
// Firebase Admin SDK initialization
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.GetApplicationDefault(),
    ProjectId = builder.Configuration["Firebase:ProjectId"]
});
```

Also add at top of `Program.cs`:
```csharp
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
```

- [ ] **Step 2: Write Firebase middleware**

`CadaverExquisito.API/Middleware/FirebaseAuthMiddleware.cs`:
```csharp
using FirebaseAdmin.Auth;

namespace CadaverExquisito.API.Middleware;

public class FirebaseAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..];
            try
            {
                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                context.Items["UserId"] = decoded.Uid;
                context.Items["Email"] = decoded.Claims.GetValueOrDefault("email")?.ToString() ?? string.Empty;
            }
            catch
            {
                // Invalid token — request proceeds unauthenticated; controllers enforce auth
            }
        }
        await next(context);
    }
}

public static class HttpContextExtensions
{
    public static string GetUserId(this HttpContext context)
    {
        var userId = context.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Not authenticated.");
        return userId;
    }
}
```

- [ ] **Step 3: Register middleware in Program.cs**

Add before `app.MapControllers()`:
```csharp
app.UseMiddleware<CadaverExquisito.API.Middleware.FirebaseAuthMiddleware>();
```

- [ ] **Step 4: Commit**

```bash
git add .
git commit -m "feat: add Firebase token validation middleware"
```

---

### Task 9: FCM notification service

**Files:**
- Create: `backend/CadaverExquisito.API/Infrastructure/Notifications/FcmNotificationService.cs`

- [ ] **Step 1: Write FcmNotificationService**

`CadaverExquisito.API/Infrastructure/Notifications/FcmNotificationService.cs`:
```csharp
using CadaverExquisito.API.Application.Interfaces;
using FirebaseAdmin.Messaging;

namespace CadaverExquisito.API.Infrastructure.Notifications;

public class FcmNotificationService(IUserRepository userRepo) : INotificationService
{
    public async Task SendToUsersAsync(List<string> userIdsOrTokens, string title, string body)
    {
        // If called with user IDs (for CadaverCompleted), resolve FCM tokens
        // If called with empty list (for FragmentAdded), fetch from repo
        List<string> tokens;
        if (userIdsOrTokens.Count == 0)
            return;

        // For participant notifications (CadaverCompleted), we receive user IDs
        // Resolve to FCM tokens
        tokens = new List<string>();
        foreach (var id in userIdsOrTokens)
        {
            var user = await userRepo.GetByIdAsync(id);
            if (user?.FcmToken is not null)
                tokens.Add(user.FcmToken);
        }

        if (tokens.Count == 0) return;

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification { Title = title, Body = body }
        };

        await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }

    public async Task SendToNonParticipantsAsync(Guid cadaverId, string title, string body)
    {
        var tokens = await userRepo.GetFcmTokensExcludingParticipantsAsync(cadaverId);
        if (tokens.Count == 0) return;

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification { Title = title, Body = body }
        };

        await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add .
git commit -m "feat: add FCM notification service"
```

---

### Task 10: Controllers + DI wiring + CORS

**Files:**
- Create: `backend/CadaverExquisito.API/Controllers/AuthController.cs`
- Create: `backend/CadaverExquisito.API/Controllers/CadaversController.cs`
- Create: `backend/CadaverExquisito.API/Controllers/FragmentsController.cs`
- Modify: `backend/CadaverExquisito.API/Program.cs`

- [ ] **Step 1: Write AuthController**

`CadaverExquisito.API/Controllers/AuthController.cs`:
```csharp
using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using CadaverExquisito.API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CadaverExquisito.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserRepository userRepo) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var userId = HttpContext.GetUserId();
        var user = new User
        {
            Id = userId,
            Name = request.Name,
            Email = request.Email,
            FcmToken = request.FcmToken
        };
        await userRepo.UpsertAsync(user);
        return Ok();
    }

    [HttpPut("fcm-token")]
    public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequest request)
    {
        var userId = HttpContext.GetUserId();
        await userRepo.UpdateFcmTokenAsync(userId, request.FcmToken);
        return Ok();
    }
}
```

- [ ] **Step 2: Write CadaversController**

`CadaverExquisito.API/Controllers/CadaversController.cs`:
```csharp
using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CadaverExquisito.API.Controllers;

[ApiController]
[Route("api/cadavers")]
public class CadaversController(CadaverService cadaverService, FragmentService fragmentService) : ControllerBase
{
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var userId = HttpContext.GetUserId();
        return Ok(await cadaverService.GetAvailableAsync(userId));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = HttpContext.GetUserId();
        return Ok(await cadaverService.GetPendingAsync(userId));
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompleted() =>
        Ok(await cadaverService.GetCompletedAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCadaverRequest request)
    {
        var userId = HttpContext.GetUserId();
        var result = await cadaverService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetFull), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}/last-fragment")]
    public async Task<IActionResult> GetLastFragment(Guid id)
    {
        HttpContext.GetUserId(); // enforce auth
        var result = await fragmentService.GetLastFragmentAsync(id);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPost("{id:guid}/fragments")]
    public async Task<IActionResult> AddFragment(Guid id, [FromBody] AddFragmentRequest request)
    {
        var userId = HttpContext.GetUserId();
        try
        {
            var result = await fragmentService.AddFragmentAsync(id, userId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/full")]
    public async Task<IActionResult> GetFull(Guid id)
    {
        try
        {
            return Ok(await fragmentService.GetFullCadaverAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

- [ ] **Step 3: Complete Program.cs with DI + CORS**

Replace `Program.cs` completely:
```csharp
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Infrastructure.Data;
using CadaverExquisito.API.Infrastructure.Data.Repositories;
using CadaverExquisito.API.Infrastructure.Notifications;
using CadaverExquisito.API.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.GetApplicationDefault(),
    ProjectId = builder.Configuration["Firebase:ProjectId"]
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICadaverRepository, CadaverRepository>();
builder.Services.AddScoped<IFragmentRepository, FragmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INotificationService, FcmNotificationService>();
builder.Services.AddScoped<CadaverService>();
builder.Services.AddScoped<FragmentService>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseMiddleware<FirebaseAuthMiddleware>();
app.MapControllers();

app.Run();
```

- [ ] **Step 4: Build to verify no errors**

```bash
cd CadaverExquisito.API && dotnet build
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 5: Apply migration to local PostgreSQL**

```bash
dotnet ef database update
```
Expected: `Done.`

- [ ] **Step 6: Run all tests**

```bash
cd ../CadaverExquisito.Tests && dotnet test
```
Expected: PASS — all tests green

- [ ] **Step 7: Commit**

```bash
git add .
git commit -m "feat: add controllers, DI wiring, CORS — backend complete"
```

---

## PART 2 — FLUTTER FRONTEND

---

### Task 11: Flutter project scaffold + dependencies

**Files:**
- Create: `frontend/cadaver_exquisito_app/pubspec.yaml` (modify after scaffold)

- [ ] **Step 1: Create Flutter project**

```bash
cd ../../frontend
flutter create cadaver_exquisito_app --org com.cadaverexquisito --platforms ios,android
cd cadaver_exquisito_app
```

- [ ] **Step 2: Add dependencies to pubspec.yaml**

Replace the `dependencies` section in `pubspec.yaml`:
```yaml
dependencies:
  flutter:
    sdk: flutter

  # State management
  flutter_riverpod: ^2.5.1

  # HTTP client
  dio: ^5.4.3+1

  # Firebase
  firebase_core: ^3.1.0
  firebase_auth: ^5.1.0
  firebase_messaging: ^15.0.0

  # Navigation
  go_router: ^14.1.4

  # Fonts & Icons
  google_fonts: ^6.2.1
  phosphor_flutter: ^2.1.0

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^4.0.0
  build_runner: ^2.4.11
```

- [ ] **Step 3: Install dependencies**

```bash
flutter pub get
```
Expected: no errors

- [ ] **Step 4: Set up Firebase (FlutterFire CLI)**

```bash
dart pub global activate flutterfire_cli
flutterfire configure --project=YOUR_FIREBASE_PROJECT_ID
```
This generates `lib/firebase_options.dart`. Follow CLI prompts for iOS and Android.

- [ ] **Step 5: Delete default boilerplate**

```bash
rm lib/main.dart
```

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "chore: scaffold Flutter project with dependencies"
```

---

### Task 12: App theme (Soft UI + 90s pastel)

**Files:**
- Create: `frontend/cadaver_exquisito_app/lib/core/theme/app_theme.dart`

- [ ] **Step 1: Create directory**

```bash
mkdir -p lib/core/theme
```

- [ ] **Step 2: Write app_theme.dart**

`lib/core/theme/app_theme.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppColors {
  static const background = Color(0xFFF0EBF4);
  static const surface = Color(0xFFE8E2EE);
  static const primary = Color(0xFFA89BB5);
  static const accent = Color(0xFFC4A882);
  static const textDark = Color(0xFF3D3347);
  static const textMuted = Color(0xFF7A7085);
  static const success = Color(0xFF8FB8A0);
  static const shadowLight = Color(0xFFFFFFFF);
  static const shadowDark = Color(0xFFC8BDD4);
}

class AppTheme {
  static ThemeData get theme => ThemeData(
        scaffoldBackgroundColor: AppColors.background,
        colorScheme: ColorScheme.light(
          primary: AppColors.primary,
          secondary: AppColors.accent,
          surface: AppColors.surface,
          onPrimary: Colors.white,
          onSurface: AppColors.textDark,
        ),
        textTheme: GoogleFonts.dmSansTextTheme().copyWith(
          bodyLarge: GoogleFonts.dmSans(color: AppColors.textDark),
          bodyMedium: GoogleFonts.dmSans(color: AppColors.textDark),
          labelSmall: GoogleFonts.dmSans(color: AppColors.textMuted),
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
            elevation: 0,
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: AppColors.surface,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(16),
            borderSide: BorderSide.none,
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(16),
            borderSide: const BorderSide(color: AppColors.primary, width: 1.5),
          ),
        ),
      );
}

// Reusable Soft UI container
class SoftCard extends StatelessWidget {
  const SoftCard({super.key, required this.child, this.padding});
  final Widget child;
  final EdgeInsets? padding;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: padding ?? const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [
          BoxShadow(
            color: AppColors.shadowLight,
            offset: Offset(-4, -4),
            blurRadius: 8,
          ),
          BoxShadow(
            color: AppColors.shadowDark,
            offset: Offset(4, 4),
            blurRadius: 8,
          ),
        ],
      ),
      child: child,
    );
  }
}
```

- [ ] **Step 3: Commit**

```bash
git add .
git commit -m "feat: add Soft UI theme with 90s pastel palette"
```

---

### Task 13: API client

**Files:**
- Create: `frontend/cadaver_exquisito_app/lib/core/api/endpoints.dart`
- Create: `frontend/cadaver_exquisito_app/lib/core/api/api_client.dart`

- [ ] **Step 1: Write endpoints**

`lib/core/api/endpoints.dart`:
```dart
class Endpoints {
  static const _base = 'http://localhost:5000/api'; // change to prod URL when deploying

  static const register = '$_base/auth/register';
  static const updateFcmToken = '$_base/auth/fcm-token';
  static const availableCadavers = '$_base/cadavers/available';
  static const pendingCadavers = '$_base/cadavers/pending';
  static const completedCadavers = '$_base/cadavers/completed';
  static const cadavers = '$_base/cadavers';

  static String lastFragment(String id) => '$_base/cadavers/$id/last-fragment';
  static String addFragment(String id) => '$_base/cadavers/$id/fragments';
  static String fullCadaver(String id) => '$_base/cadavers/$id/full';
}
```

- [ ] **Step 2: Write API client with Firebase token interceptor**

`lib/core/api/api_client.dart`:
```dart
import 'package:dio/dio.dart';
import 'package:firebase_auth/firebase_auth.dart';

final apiClient = _buildClient();

Dio _buildClient() {
  final dio = Dio();
  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        final user = FirebaseAuth.instance.currentUser;
        if (user != null) {
          final token = await user.getIdToken();
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
    ),
  );
  return dio;
}
```

- [ ] **Step 3: Commit**

```bash
git add .
git commit -m "feat: add Dio API client with Firebase token interceptor"
```

---

### Task 14: Auth feature

**Files:**
- Create: `frontend/cadaver_exquisito_app/lib/features/auth/providers/auth_provider.dart`
- Create: `frontend/cadaver_exquisito_app/lib/features/auth/screens/login_screen.dart`

- [ ] **Step 1: Create directories**

```bash
mkdir -p lib/features/auth/providers lib/features/auth/screens
```

- [ ] **Step 2: Write auth provider**

`lib/features/auth/providers/auth_provider.dart`:
```dart
import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final authProvider = StreamProvider<User?>((ref) {
  return FirebaseAuth.instance.authStateChanges();
});

final authActionsProvider = Provider((ref) => AuthActions());

class AuthActions {
  Future<void> signInWithGoogle() async {
    final provider = GoogleAuthProvider();
    await FirebaseAuth.instance.signInWithPopup(provider);
  }

  Future<void> signInWithEmail(String email, String password) async {
    await FirebaseAuth.instance.signInWithEmailAndPassword(
      email: email,
      password: password,
    );
  }

  Future<void> registerWithEmail(String email, String password) async {
    await FirebaseAuth.instance.createUserWithEmailAndPassword(
      email: email,
      password: password,
    );
  }

  Future<void> signOut() => FirebaseAuth.instance.signOut();
}
```

- [ ] **Step 3: Write login screen**

`lib/features/auth/screens/login_screen.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/auth_provider.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isLogin = true;
  String? _error;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _error = null);
    final actions = ref.read(authActionsProvider);
    try {
      if (_isLogin) {
        await actions.signInWithEmail(_emailController.text, _passwordController.text);
      } else {
        await actions.registerWithEmail(_emailController.text, _passwordController.text);
      }
    } catch (e) {
      setState(() => _error = e.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                'Cadáver Exquisito',
                style: GoogleFonts.playfairDisplay(
                  fontSize: 32,
                  fontWeight: FontWeight.bold,
                  color: AppColors.textDark,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'escritura colectiva',
                style: GoogleFonts.dmSans(color: AppColors.textMuted, fontSize: 14),
              ),
              const SizedBox(height: 40),
              SoftCard(
                padding: const EdgeInsets.all(24),
                child: Column(
                  children: [
                    TextField(
                      controller: _emailController,
                      decoration: const InputDecoration(labelText: 'Email'),
                      keyboardType: TextInputType.emailAddress,
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _passwordController,
                      decoration: const InputDecoration(labelText: 'Contraseña'),
                      obscureText: true,
                    ),
                    if (_error != null) ...[
                      const SizedBox(height: 12),
                      Text(_error!, style: const TextStyle(color: Colors.red, fontSize: 12)),
                    ],
                    const SizedBox(height: 24),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: _submit,
                        child: Text(_isLogin ? 'Ingresar' : 'Registrarse'),
                      ),
                    ),
                    TextButton(
                      onPressed: () => setState(() => _isLogin = !_isLogin),
                      child: Text(
                        _isLogin ? '¿No tienes cuenta? Regístrate' : '¿Ya tienes cuenta? Ingresa',
                        style: const TextStyle(color: AppColors.textMuted, fontSize: 12),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
```

- [ ] **Step 4: Commit**

```bash
git add .
git commit -m "feat: add auth provider and login screen"
```

---

### Task 15: Main scaffold + routing

**Files:**
- Create: `frontend/cadaver_exquisito_app/lib/main.dart`
- Create: `frontend/cadaver_exquisito_app/lib/features/cadavers/screens/main_scaffold.dart`

- [ ] **Step 1: Create directories**

```bash
mkdir -p lib/features/cadavers/screens lib/features/cadavers/widgets lib/features/cadavers/providers
mkdir -p lib/features/editor/screens lib/features/editor/providers
```

- [ ] **Step 2: Write main.dart**

`lib/main.dart`:
```dart
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/providers/auth_provider.dart';
import 'features/auth/screens/login_screen.dart';
import 'features/cadavers/screens/main_scaffold.dart';
import 'firebase_options.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Firebase.initializeApp(options: DefaultFirebaseOptions.currentPlatform);
  runApp(const ProviderScope(child: CadaverExquisitoApp()));
}

class CadaverExquisitoApp extends ConsumerWidget {
  const CadaverExquisitoApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authProvider);
    return MaterialApp(
      title: 'Cadáver Exquisito',
      theme: AppTheme.theme,
      home: authState.when(
        data: (user) => user != null ? const MainScaffold() : const LoginScreen(),
        loading: () => const Scaffold(body: Center(child: CircularProgressIndicator())),
        error: (_, __) => const LoginScreen(),
      ),
    );
  }
}
```

- [ ] **Step 3: Write MainScaffold (bottom nav)**

`lib/features/cadavers/screens/main_scaffold.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:phosphor_flutter/phosphor_flutter.dart';
import '../../../core/theme/app_theme.dart';
import 'participate_tab.dart';
import 'archive_tab.dart';

class MainScaffold extends StatefulWidget {
  const MainScaffold({super.key});

  @override
  State<MainScaffold> createState() => _MainScaffoldState();
}

class _MainScaffoldState extends State<MainScaffold> {
  int _currentIndex = 0;

  static const _tabs = [ParticipateTab(), ArchiveTab()];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _tabs[_currentIndex],
      bottomNavigationBar: Container(
        decoration: const BoxDecoration(
          color: AppColors.surface,
          boxShadow: [
            BoxShadow(color: AppColors.shadowDark, offset: Offset(0, -2), blurRadius: 6),
          ],
        ),
        child: BottomNavigationBar(
          currentIndex: _currentIndex,
          onTap: (i) => setState(() => _currentIndex = i),
          backgroundColor: AppColors.surface,
          selectedItemColor: AppColors.primary,
          unselectedItemColor: AppColors.textMuted,
          elevation: 0,
          items: [
            BottomNavigationBarItem(
              icon: PhosphorIcon(PhosphorIconsRegular.pencilSimple),
              activeIcon: PhosphorIcon(PhosphorIconsFill.pencilSimple),
              label: 'Participar',
            ),
            BottomNavigationBarItem(
              icon: PhosphorIcon(PhosphorIconsRegular.bookOpen),
              activeIcon: PhosphorIcon(PhosphorIconsFill.bookOpen),
              label: 'Archivo',
            ),
          ],
        ),
      ),
    );
  }
}
```

- [ ] **Step 4: Commit**

```bash
git add .
git commit -m "feat: add main scaffold with bottom navigation"
```

---

### Task 16: Cadavers providers + Participate tab

**Files:**
- Create: `frontend/.../providers/available_cadavers_provider.dart`
- Create: `frontend/.../screens/participate_tab.dart`
- Create: `frontend/.../widgets/cadaver_card.dart`

- [ ] **Step 1: Write models**

`lib/features/cadavers/providers/available_cadavers_provider.dart`:
```dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';

class CadaverSummary {
  final String id;
  final String title;
  final int maxParticipants;
  final int currentTurn;

  CadaverSummary.fromJson(Map<String, dynamic> j)
      : id = j['id'],
        title = j['title'],
        maxParticipants = j['maxParticipants'],
        currentTurn = j['currentTurn'];
}

final availableCadaversProvider = FutureProvider<List<CadaverSummary>>((ref) async {
  final resp = await apiClient.get(Endpoints.availableCadavers);
  return (resp.data as List).map((j) => CadaverSummary.fromJson(j)).toList();
});

final pendingCadaversProvider = FutureProvider<List<CadaverSummary>>((ref) async {
  final resp = await apiClient.get(Endpoints.pendingCadavers);
  return (resp.data as List).map((j) => CadaverSummary.fromJson(j)).toList();
});
```

- [ ] **Step 2: Write CadaverCard widget**

`lib/features/cadavers/widgets/cadaver_card.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/available_cadavers_provider.dart';

class CadaverCard extends StatelessWidget {
  const CadaverCard({super.key, required this.cadaver, this.onTap, this.isPending = false});
  final CadaverSummary cadaver;
  final VoidCallback? onTap;
  final bool isPending;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: SoftCard(
        padding: const EdgeInsets.all(20),
        child: Row(
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    cadaver.title,
                    style: GoogleFonts.playfairDisplay(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                      color: AppColors.textDark,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    isPending
                        ? 'Turno ${cadaver.currentTurn} de ${cadaver.maxParticipants}'
                        : 'Te toca escribir — turno ${cadaver.currentTurn}',
                    style: GoogleFonts.dmSans(fontSize: 12, color: AppColors.textMuted),
                  ),
                ],
              ),
            ),
            if (!isPending)
              const Icon(Icons.arrow_forward_ios_rounded, size: 14, color: AppColors.primary),
          ],
        ),
      ),
    );
  }
}
```

- [ ] **Step 3: Write ParticipateTab**

`lib/features/cadavers/screens/participate_tab.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/available_cadavers_provider.dart';
import '../widgets/cadaver_card.dart';
import '../widgets/create_cadaver_sheet.dart';
import '../../editor/screens/editor_screen.dart';

class ParticipateTab extends ConsumerWidget {
  const ParticipateTab({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final available = ref.watch(availableCadaversProvider);
    final pending = ref.watch(pendingCadaversProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        title: Text(
          'Cadáver Exquisito',
          style: GoogleFonts.playfairDisplay(color: AppColors.textDark, fontWeight: FontWeight.bold),
        ),
      ),
      floatingActionButton: FloatingActionButton(
        backgroundColor: AppColors.primary,
        onPressed: () => showModalBottomSheet(
          context: context,
          isScrollControlled: true,
          backgroundColor: Colors.transparent,
          builder: (_) => const CreateCadaverSheet(),
        ),
        child: const Icon(Icons.add, color: Colors.white),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _Section(
            title: 'Te toca escribir',
            asyncValue: available,
            emptyMessage: 'No hay historias disponibles ahora.',
            onRefresh: () => ref.invalidate(availableCadaversProvider),
            itemBuilder: (cadaver) => CadaverCard(
              cadaver: cadaver,
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => EditorScreen(cadaverId: cadaver.id)),
              ).then((_) => ref.invalidate(availableCadaversProvider)),
            ),
          ),
          const SizedBox(height: 24),
          _Section(
            title: 'En progreso',
            asyncValue: pending,
            emptyMessage: 'Ninguna historia en progreso.',
            onRefresh: () => ref.invalidate(pendingCadaversProvider),
            itemBuilder: (cadaver) => CadaverCard(cadaver: cadaver, isPending: true),
          ),
        ],
      ),
    );
  }
}

class _Section extends StatelessWidget {
  const _Section({
    required this.title,
    required this.asyncValue,
    required this.emptyMessage,
    required this.itemBuilder,
    required this.onRefresh,
  });
  final String title;
  final AsyncValue asyncValue;
  final String emptyMessage;
  final Widget Function(dynamic) itemBuilder;
  final VoidCallback onRefresh;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(title,
            style: GoogleFonts.dmSans(
                fontSize: 13, fontWeight: FontWeight.w600, color: AppColors.textMuted, letterSpacing: 0.8)),
        const SizedBox(height: 12),
        asyncValue.when(
          data: (items) => items.isEmpty
              ? Text(emptyMessage,
                  style: GoogleFonts.dmSans(color: AppColors.textMuted, fontSize: 13))
              : Column(
                  children: [
                    for (final item in items) ...[
                      itemBuilder(item),
                      const SizedBox(height: 12),
                    ]
                  ],
                ),
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (e, _) => Text('Error: $e'),
        ),
      ],
    );
  }
}
```

- [ ] **Step 4: Commit**

```bash
git add .
git commit -m "feat: add cadaver providers and Participate tab"
```

---

### Task 17: Create Cadaver bottom sheet

**Files:**
- Create: `frontend/.../widgets/create_cadaver_sheet.dart`

- [ ] **Step 1: Write word count provider**

`lib/features/editor/providers/word_count_provider.dart`:
```dart
import 'package:flutter_riverpod/flutter_riverpod.dart';

final wordCountProvider = StateProvider.family<int, String>((ref, text) {
  if (text.trim().isEmpty) return 0;
  return text.trim().split(RegExp(r'\s+')).length;
});
```

- [ ] **Step 2: Write CreateCadaverSheet**

`lib/features/cadavers/widgets/create_cadaver_sheet.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';
import '../../../core/theme/app_theme.dart';
import '../../editor/providers/word_count_provider.dart';

class CreateCadaverSheet extends ConsumerStatefulWidget {
  const CreateCadaverSheet({super.key});

  @override
  ConsumerState<CreateCadaverSheet> createState() => _CreateCadaverSheetState();
}

class _CreateCadaverSheetState extends ConsumerState<CreateCadaverSheet> {
  final _contentController = TextEditingController();
  int _participants = 3;
  bool _loading = false;

  @override
  void dispose() {
    _contentController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final wordCount = ref.read(wordCountProvider(_contentController.text));
    if (wordCount > 300 || wordCount == 0) return;

    setState(() => _loading = true);
    try {
      await apiClient.post(Endpoints.cadavers, data: {
        'maxParticipants': _participants,
        'content': _contentController.text,
      });
      if (mounted) Navigator.pop(context);
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Error: $e')));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final wordCount = ref.watch(wordCountProvider(_contentController.text));
    final overLimit = wordCount > 300;

    return Container(
      padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(context).viewInsets.bottom + 24),
      decoration: const BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Center(
            child: Container(
              width: 40, height: 4,
              decoration: BoxDecoration(color: AppColors.textMuted.withOpacity(0.3), borderRadius: BorderRadius.circular(2)),
            ),
          ),
          const SizedBox(height: 20),
          Text('Nueva historia', style: GoogleFonts.playfairDisplay(fontSize: 22, fontWeight: FontWeight.bold, color: AppColors.textDark)),
          const SizedBox(height: 20),
          Row(
            children: [
              Text('Participantes:', style: GoogleFonts.dmSans(color: AppColors.textMuted)),
              const SizedBox(width: 16),
              for (final n in [2, 3, 4, 5, 6])
                GestureDetector(
                  onTap: () => setState(() => _participants = n),
                  child: Container(
                    margin: const EdgeInsets.only(right: 8),
                    width: 36, height: 36,
                    decoration: BoxDecoration(
                      color: _participants == n ? AppColors.primary : AppColors.surface,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Center(
                      child: Text('$n', style: GoogleFonts.dmSans(
                        color: _participants == n ? Colors.white : AppColors.textDark,
                        fontWeight: FontWeight.w600,
                      )),
                    ),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 16),
          TextField(
            controller: _contentController,
            onChanged: (_) => setState(() {}),
            maxLines: 6,
            style: GoogleFonts.playfairDisplay(color: AppColors.textDark, fontSize: 15),
            decoration: InputDecoration(
              hintText: 'Comienza la historia...',
              hintStyle: GoogleFonts.playfairDisplay(color: AppColors.textMuted),
            ),
          ),
          const SizedBox(height: 8),
          Align(
            alignment: Alignment.centerRight,
            child: Text(
              '$wordCount / 300',
              style: GoogleFonts.dmSans(
                color: overLimit ? Colors.red : AppColors.textMuted,
                fontSize: 12,
              ),
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: (_loading || overLimit || wordCount == 0) ? null : _submit,
              child: _loading
                  ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                  : const Text('Iniciar historia'),
            ),
          ),
        ],
      ),
    );
  }
}
```

- [ ] **Step 3: Commit**

```bash
git add .
git commit -m "feat: add Create Cadaver bottom sheet with word counter"
```

---

### Task 18: Editor screen

**Files:**
- Create: `frontend/.../editor/screens/editor_screen.dart`

- [ ] **Step 1: Write EditorScreen**

`lib/features/editor/screens/editor_screen.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/word_count_provider.dart';

class EditorScreen extends ConsumerStatefulWidget {
  const EditorScreen({super.key, required this.cadaverId});
  final String cadaverId;

  @override
  ConsumerState<EditorScreen> createState() => _EditorScreenState();
}

class _EditorScreenState extends ConsumerState<EditorScreen> {
  final _contentController = TextEditingController();
  String? _previousFragment;
  bool _loadingFragment = true;
  bool _submitting = false;

  @override
  void initState() {
    super.initState();
    _loadLastFragment();
  }

  Future<void> _loadLastFragment() async {
    try {
      final resp = await apiClient.get(Endpoints.lastFragment(widget.cadaverId));
      setState(() {
        _previousFragment = resp.statusCode == 204 ? null : resp.data['content'] as String?;
        _loadingFragment = false;
      });
    } catch (_) {
      setState(() => _loadingFragment = false);
    }
  }

  Future<void> _submit() async {
    setState(() => _submitting = true);
    try {
      await apiClient.post(
        Endpoints.addFragment(widget.cadaverId),
        data: {'content': _contentController.text},
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('¡Fragmento enviado!')),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Error: $e')));
      }
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  void dispose() {
    _contentController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final wordCount = ref.watch(wordCountProvider(_contentController.text));
    final overLimit = wordCount > 300;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        leading: BackButton(color: AppColors.textDark),
        title: Text('Tu turno', style: GoogleFonts.playfairDisplay(color: AppColors.textDark)),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: Center(
              child: Text(
                '$wordCount / 300',
                style: GoogleFonts.dmSans(
                  color: overLimit ? Colors.red : AppColors.textMuted,
                  fontSize: 13,
                ),
              ),
            ),
          ),
        ],
      ),
      body: _loadingFragment
          ? const Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                children: [
                  SoftCard(
                    child: _previousFragment == null
                        ? Text(
                            'Eres el primero. Comienza la historia.',
                            style: GoogleFonts.playfairDisplay(
                                color: AppColors.textMuted,
                                fontSize: 15,
                                fontStyle: FontStyle.italic),
                          )
                        : Text(
                            '...${_previousFragment!}',
                            style: GoogleFonts.playfairDisplay(
                                color: AppColors.textDark, fontSize: 15, height: 1.6),
                          ),
                  ),
                  const SizedBox(height: 16),
                  Expanded(
                    child: TextField(
                      controller: _contentController,
                      onChanged: (_) => setState(() {}),
                      maxLines: null,
                      expands: true,
                      textAlignVertical: TextAlignVertical.top,
                      style: GoogleFonts.playfairDisplay(color: AppColors.textDark, fontSize: 15, height: 1.7),
                      decoration: InputDecoration(
                        hintText: 'Continúa la historia...',
                        hintStyle: GoogleFonts.playfairDisplay(color: AppColors.textMuted),
                        alignLabelWithHint: true,
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: (_submitting || overLimit || wordCount == 0) ? null : _submit,
                      child: _submitting
                          ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                          : const Text('Enviar fragmento'),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add .
git commit -m "feat: add Editor screen with previous fragment preview and word counter"
```

---

### Task 19: Archive tab + full story view

**Files:**
- Create: `frontend/.../providers/completed_cadavers_provider.dart`
- Create: `frontend/.../screens/archive_tab.dart`

- [ ] **Step 1: Write completed cadavers provider**

`lib/features/cadavers/providers/completed_cadavers_provider.dart`:
```dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';

class CompletedCadaver {
  final String id;
  final String title;

  CompletedCadaver.fromJson(Map<String, dynamic> j)
      : id = j['id'],
        title = j['title'];
}

class FullFragment {
  final String content;
  final int sequenceOrder;
  final String authorName;

  FullFragment.fromJson(Map<String, dynamic> j)
      : content = j['content'],
        sequenceOrder = j['sequenceOrder'],
        authorName = j['authorName'];
}

final completedCadaversProvider = FutureProvider<List<CompletedCadaver>>((ref) async {
  final resp = await apiClient.get(Endpoints.completedCadavers);
  return (resp.data as List).map((j) => CompletedCadaver.fromJson(j)).toList();
});

final fullCadaverProvider = FutureProvider.family<List<FullFragment>, String>((ref, id) async {
  final resp = await apiClient.get(Endpoints.fullCadaver(id));
  return (resp.data['fragments'] as List).map((j) => FullFragment.fromJson(j)).toList();
});
```

- [ ] **Step 2: Write ArchiveTab**

`lib/features/cadavers/screens/archive_tab.dart`:
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/completed_cadavers_provider.dart';

class ArchiveTab extends ConsumerWidget {
  const ArchiveTab({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final completed = ref.watch(completedCadaversProvider);
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        title: Text('Archivo', style: GoogleFonts.playfairDisplay(color: AppColors.textDark, fontWeight: FontWeight.bold)),
      ),
      body: completed.when(
        data: (cadavers) => cadavers.isEmpty
            ? Center(child: Text('Aún no hay historias completas.', style: GoogleFonts.dmSans(color: AppColors.textMuted)))
            : ListView.separated(
                padding: const EdgeInsets.all(16),
                itemCount: cadavers.length,
                separatorBuilder: (_, __) => const SizedBox(height: 12),
                itemBuilder: (context, i) {
                  final c = cadavers[i];
                  return GestureDetector(
                    onTap: () => Navigator.push(
                      context,
                      MaterialPageRoute(builder: (_) => _StoryReaderScreen(cadaverId: c.id, title: c.title)),
                    ),
                    child: SoftCard(
                      child: Row(
                        children: [
                          Expanded(
                            child: Text(c.title,
                                style: GoogleFonts.playfairDisplay(fontSize: 16, color: AppColors.textDark)),
                          ),
                          const Icon(Icons.arrow_forward_ios_rounded, size: 14, color: AppColors.primary),
                        ],
                      ),
                    ),
                  );
                },
              ),
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
    );
  }
}

class _StoryReaderScreen extends ConsumerWidget {
  const _StoryReaderScreen({required this.cadaverId, required this.title});
  final String cadaverId;
  final String title;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final fragments = ref.watch(fullCadaverProvider(cadaverId));
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        leading: BackButton(color: AppColors.textDark),
        title: Text(title, style: GoogleFonts.playfairDisplay(color: AppColors.textDark, fontSize: 16)),
      ),
      body: fragments.when(
        data: (frags) => ListView.builder(
          padding: const EdgeInsets.all(20),
          itemCount: frags.length,
          itemBuilder: (context, i) {
            final f = frags[i];
            return Padding(
              padding: const EdgeInsets.only(bottom: 24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    f.content,
                    style: GoogleFonts.playfairDisplay(fontSize: 16, color: AppColors.textDark, height: 1.8),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    '— ${f.authorName}',
                    style: GoogleFonts.dmSans(fontSize: 12, color: AppColors.textMuted, fontStyle: FontStyle.italic),
                  ),
                  if (i < frags.length - 1) ...[
                    const SizedBox(height: 16),
                    Divider(color: AppColors.textMuted.withOpacity(0.2)),
                  ],
                ],
              ),
            );
          },
        ),
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
    );
  }
}
```

- [ ] **Step 3: Verify Flutter build compiles**

```bash
flutter analyze
```
Expected: `No issues found!`

- [ ] **Step 4: Commit**

```bash
git add .
git commit -m "feat: add Archive tab and full story reader — Flutter frontend complete"
```

---

## Final checklist

- [ ] Backend runs: `dotnet run --project CadaverExquisito.API` — Swagger available at `http://localhost:5000/swagger`
- [ ] All backend tests pass: `dotnet test`
- [ ] Flutter app builds: `flutter run`
- [ ] Login works end-to-end with Firebase Auth
- [ ] Creating a cadáver creates a Fragment with SequenceOrder=1 and advances CurrentTurn to 2
- [ ] Available tab excludes cadavers the user already participated in
- [ ] Editor shows previous fragment (or first-turn message)
- [ ] Word counter blocks submit above 300 words
- [ ] Archive shows completed stories with all fragments and authors

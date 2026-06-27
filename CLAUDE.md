# CLAUDE.md — Pixel.Tasks Project Memory

## Project Overview

**Pixel.Tasks** is a **.NET 9 MVC Web Application** (server-rendered with Razor Views) for managing tasks across multiple corporations (e.g., Hotel1, Hotel2). A task creator can create tasks, assign them to employees within specific corporations/sections, and track progress. Tasks support three behavioral modes: Normal (plain), Point (checklist), and Counter (numeric progress). An internal comment system (text/image/file) enables communication between task creators and assignees.

**Solution file:** `Pixel.Tasks.sln`  
**Target framework:** `net9.0`  
**Database:** SQL Server (`TasksDB`) via EF Core 9  
**Auth:** ASP.NET Identity (cookie-based, MVC — NOT JWT)  
**Logging:** Serilog (console + daily rolling files in `Tasks.Presentation/Logs/`)  
**Mapping:** AutoMapper  
**UI Template:** Skote — Admin & Dashboard Template (Bootstrap 5, dark sidebar, Boxicons, MetisMenu, SimpleBar, Toastr)  
**UI Preference:** Client prefers modern/professional UI/UX — use modern elements, creative touches, premium feel  

---

## Architecture — Onion / Clean Architecture

```
Tasks.Domain  (Core — no dependencies on other projects)
    ├── Models/            (BaseModel, ICodedEntity, domain entities)
    ├── Models/Identity/   (AppUser : IdentityUser)
    ├── Enums/             (Gender, TaskCategory, PriorityLevel, WorkTaskStatus, CommentType)
    ├── Authorization/     (Roles, Permissions, RolePermissions — static RBAC constants)
    ├── Repositories/      (IGenericRepository<T>)
    ├── Services/          (ICodeGeneratorService)
    ├── Specifications/    (ISpecifications<T>, BaseSpecifications<T>, per-entity spec folders)
    └── IUnitOfWork.cs

Tasks.Repository  (Infrastructure — references Tasks.Domain)
    ├── Data/TaskContext.cs               (IdentityDbContext<AppUser>)
    ├── Data/Configrations/               (Fluent API per entity — note: "Configrations" typo is established, keep it)
    ├── Data/DataSeed/                    (empty — seed data is in AppIdentityDbContextSeed.cs)
    ├── Data/AppIdentityDbContextSeed.cs  (seeds 4 users: admin, abdelrahman, khaled, omar)
    ├── Data/Migrations/
    ├── GenericRepository.cs
    ├── SpecificationEvalutor.cs          (note: typo is intentional — keep it)
    └── UnitOfWork.cs

Tasks.Services  (Business Logic — references Tasks.Domain)
    └── CodeGeneration/CodeGeneratorService.cs  (generates sequential codes like PXC-000001)

Tasks.Presentation  (MVC Web App — references Tasks.Repository + Tasks.Services)
    ├── Controllers/       (AccountController, HomeController, CorporationController, SectionController, TaskTypeController, UserController)
    ├── ViewModels/        (LoginViewModel, CorporationViewModel, SectionViewModel, TaskTypeViewModel, UserViewModel, AvailableEmployeeViewModel, ErrorViewModel)
    ├── Views/             (Razor views organized per controller + Shared layout partials)
    ├── MappingProfiles/   (CorporationProfile, SectionProfile, TaskTypeProfile)
    ├── Authorization/     (PermissionRequirement, PermissionAuthorizationHandler, PermissionPolicyProvider, AppUserClaimsPrincipalFactory)
    ├── TagHelpers/        (PermissionTagHelper — <permission required="..."> suppresses output for unauthorized users)
    ├── Helpers/           (DocumentSettings — file upload/delete utility)
    ├── Logs/              (Serilog daily rolling log files)
    ├── wwwroot/           (Skote template assets: css, js, lib, back/)
    ├── Program.cs         (startup, DI, pipeline)
    └── appsettings.json
```

### Dependency Flow

```
Tasks.Presentation → Tasks.Services  → Tasks.Domain
Tasks.Presentation → Tasks.Repository → Tasks.Domain
```

**Key rule:** `Tasks.Domain` has ZERO project references to Repository, Service, or Presentation. It only has NuGet: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.

---

## NuGet Packages by Project

| Project | Key Packages |
|---|---|
| **Tasks.Domain** | `Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.17` |
| **Tasks.Repository** | `Microsoft.EntityFrameworkCore.SqlServer 9.0.17` (references Tasks.Domain) |
| **Tasks.Services** | _(no NuGet — only project reference to Tasks.Domain)_ |
| **Tasks.Presentation** | `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1`, `Serilog.AspNetCore 10.0.0`, `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 9.0.17`, `Microsoft.EntityFrameworkCore.Tools 9.0.17`, `Microsoft.VisualStudio.Web.CodeGeneration.Design 9.0.12` |

---

## Domain Models

### Base

- **`BaseModel`** — `int Id` (all DB entities inherit from this)
- **`ICodedEntity`** — marker interface with `string Code { get; set; }` for entities with auto-generated sequential codes

### Identity

- **`AppUser`** : `IdentityUser` — `FirstName`, `LastName`, `FullName` (computed), `Gender` (enum), `IsActive` (default true), `ImageUrl?`, `int? CorporationId`, `int? SectionId`  
  - CorporationId/SectionId are **nullable** — admins/task creators may not belong to any corporation  
  - Navigation: `Corporation?`, `Section?`

### Core Entities

- **`Corporation`** : `BaseModel, ICodedEntity` — `Name`, `NameAr?`, `Code` (unique, auto-generated: PXC-######), `Notes?`  
  Navigation: `ICollection<Section>`, `ICollection<AppUser>`

- **`Section`** : `BaseModel, ICodedEntity` — `Name`, `Code` (unique), `Email?`, `Fax?`, `Phone?`, `Address?`, `Telex?`, `Notes?`, `int CorporationId`  
  Navigation: `Corporation`, `ICollection<AppUser>`

- **`TaskType`** : `BaseModel` — `Name` (unique), `TaskCategory Category` (enum)  
  Admin-managed lookup table. Navigation: `ICollection<WorkTask>`

### Task System

- **`WorkTask`** : `BaseModel, ICodedEntity` — `Title`, `Code` (unique), `Description?`, `Notes?`, `RequestDate`, `DueDate`, `PriorityLevel Priority`, `WorkTaskStatus Status` (default Pending), `int? TargetCount` (Counter-type only), `int TaskTypeId`, `string CreatedByUserId`, `int CorporationId`, `int? SectionId` (null = targets all sections in corp)  
  Navigation: `TaskType`, `CreatedBy (AppUser)`, `Corporation`, `Section?`, `ICollection<TaskAssignment>`, `ICollection<TaskPoint>`, `ICollection<TaskComment>`

- **`TaskAssignment`** : `BaseModel` — `WorkTaskStatus Status` (default Pending), `int? CompletedCount` (Counter-type per-user progress), `DateTime AssignedAt`, `int WorkTaskId`, `string UserId`  
  Unique constraint: `(WorkTaskId, UserId)`. Navigation: `WorkTask`, `User (AppUser)`, `ICollection<TaskPointStatus>`

- **`TaskPoint`** : `BaseModel` — `Description`, `int Order` (display order, rearrangeable), `int WorkTaskId`  
  Navigation: `WorkTask`, `ICollection<TaskPointStatus>`

- **`TaskPointStatus`** : `BaseModel` — `bool IsCompleted` (default false), `DateTime? CompletedAt`, `int TaskAssignmentId`, `int TaskPointId`  
  Unique constraint: `(TaskAssignmentId, TaskPointId)`. Tracks per-user per-point completion independently.

- **`TaskComment`** : `BaseModel` — `Content?` (text), `FileUrl?` (image/file path), `CommentType Type`, `DateTime CreatedAt`, `int WorkTaskId`, `string UserId`  
  Each comment is exactly **one type** (text OR image OR file — never multiple).

---

## Enums

| Enum | Values | Location |
|---|---|---|
| `Gender` | `Male=1`, `Female=2` | `Tasks.Domain/Enums/` |
| `TaskCategory` | `Normal=1`, `Point=2`, `Counter=3` | `Tasks.Domain/Enums/` |
| `PriorityLevel` | `Low=1` 🟢`#10B981`, `Medium=2` 🟡`#F59E0B`, `High=3` 🟠`#F97316`, `Critical=4` 🔴`#EF4444` | `Tasks.Domain/Enums/` |
| `WorkTaskStatus` | `Pending=1`, `InProgress=2`, `Completed=3` | `Tasks.Domain/Enums/` |
| `CommentType` | `Text=1`, `Image=2`, `File=3` | `Tasks.Domain/Enums/` |

---

## Key Patterns

### Generic Repository + Specification

All data access goes through `IGenericRepository<T>` + `ISpecifications<T>`:

```csharp
var spec = new SomeSpecification(params);
var result = await _unitOfWork.Repository<Entity>().GetByIdAsync(spec);
var results = await _unitOfWork.Repository<Entity>().GetAllAsync(spec);
```

**IGenericRepository<T>** methods:
- `GetAllAsync(ISpecifications<T> Spec)` → `IEnumerable<T>`
- `GetByIdAsync(ISpecifications<T> Spec)` → `T`
- `AddAsync(T entity)`
- `Update(T entity)`
- `Delete(T entity)`

**ISpecifications<T>** supports:
- `Criteria` — `Expression<Func<T, bool>>` (WHERE clause)
- `Includes` — `List<Expression<Func<T, object>>>` (eager loading)
- `IncludeStrings` — `List<string>` (string-based ThenInclude chains, e.g., `"Categories.Items"`)
- `OrderBy` / `OrderByDesc`
- `Skip`, `Take`, `IsPaginationEnabled`

**BaseSpecifications<T>** provides:
- Parameterless constructor (get all)
- Constructor with `Expression<Func<T, bool>>` criteria (get by condition)
- Helper methods: `AddInclude(string)`, `SetOrderBy(...)`, `SetOrderByDesc(...)`, `ApplyPagination(skip, take)`

### Unit of Work

```csharp
_unitOfWork.Repository<Entity>()  // Get or create repository
await _unitOfWork.CompleteAsync()  // SaveChangesAsync
```

Uses a `Hashtable` to cache repository instances per entity type name.

#### Transaction Pattern

For methods with **multiple DB operations that must succeed or fail as one unit**, wrap them in an explicit transaction:

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    // ... multiple Repository calls + CompleteAsync calls ...
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

**When to use transactions:**
- A method calls `CompleteAsync()` more than once (e.g., need a DB-generated Id between saves)
- Two or more unrelated entities must be written atomically

**When NOT to use transactions:**
- A method has only a single `CompleteAsync()` — EF Core's SaveChanges is already atomic

### SpecificationEvalutor

Located in `Tasks.Repository/SpecificationEvalutor.cs` (note: typo in filename is intentional — keep it). Builds the EF Core query from a specification by applying Criteria → OrderBy → OrderByDesc → Pagination → Includes → IncludeStrings.

### Code Generation (ICodeGeneratorService)

Generates sequential unique codes for any `ICodedEntity`. Entity-agnostic — the caller supplies the prefix.

| Entity | Prefix | Example Code |
|---|---|---|
| Corporation | `PXC` | `PXC-000001` |

Padding: 6-digit zero-padded for 1–999,999; plain number above that.

---

## Existing Specifications

| Specification | Entity | Purpose |
|---|---|---|
| `CorporationSpec()` | `Corporation` | Get all corporations |
| `CorporationSpec(int id)` | `Corporation` | Get corporation by ID |
| `CorporationByNameSpec(string name)` | `Corporation` | Find by exact name (case-insensitive) — used for remote uniqueness validation |
| `SectionSpec()` | `Section` | Get all sections (includes Corporation + Users) |
| `SectionSpec(int id)` | `Section` | Get section by ID (includes Corporation + Users) |
| `SectionByNameSpec(string name)` | `Section` | Find by exact name — remote uniqueness validation |
| `SectionByCorporationSpec(int corporationId)` | `Section` | Get all sections for a corporation (ordered by Name) — used in User create/edit dropdown |
| `TaskTypeSpec()` | `TaskType` | Get all task types |
| `TaskTypeSpec(int id)` | `TaskType` | Get task type by ID |
| `TaskTypeByNameSpec(string name)` | `TaskType` | Find by exact name — remote uniqueness validation |

---

## Service Layer — Business Logic

| Interface | Implementation | Purpose |
|---|---|---|
| `ICodeGeneratorService` | `CodeGeneratorService` | Generates sequential codes (PXC-######) for coded entities |

_More services to be added as features are built._

---

## AutoMapper Configuration

Defined in `Tasks.Presentation/MappingProfiles/CorporationProfile.cs`:

- `Corporation → CorporationViewModel` (for display)
- `CorporationViewModel → Corporation` (for Create/Edit) — `Code` is **ignored** (auto-generated, not mapped from VM)

---

## Database

### DbContext — `TaskContext` : `IdentityDbContext<AppUser>`

**DbSets:** `Corporations`, `Sections`, `TaskTypes`, `WorkTasks`, `TaskAssignments`, `TaskPoints`, `TaskPointStatuses`, `TaskComments`

**Configurations** (Fluent API via `IEntityTypeConfiguration<T>`):
- Located in `Tasks.Repository/Data/Configrations/` (note: "Configrations" typo is established, keep it)
- One config file per entity: `AppUserConfig`, `CorporationConfig`, `SectionConfig`, `TaskTypeConfig`, `WorkTaskConfig`, `TaskAssignmentConfig`, `TaskPointConfig`, `TaskPointStatusConfig`, `TaskCommentConfig`
- Applied via `modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())`

### FK Cascade Strategy (SQL Server)

SQL Server forbids multiple cascade paths reaching the same table. Strategy:

| Relationship | Delete Behavior | Reason |
|---|---|---|
| Corporation → Section | **Cascade** | Section is fully owned by Corporation |
| Section → WorkTask | **NoAction** | Avoids cycle: Corp→Section→WorkTask AND Corp→WorkTask |
| AppUser → Corporation/Section | **NoAction** | Avoids cycle through WorkTask |
| AppUser → WorkTask (creator) | **NoAction** | Avoids cycle via Corporation |
| AppUser → TaskAssignment | **NoAction** | Avoids cycle via WorkTask |
| AppUser → TaskComment | **NoAction** | Avoids cycle via WorkTask |
| WorkTask → TaskAssignment/Point/Comment | **Cascade** | Child records fully owned by task |
| TaskType → WorkTask | **Cascade** | No cycle path through TaskType |
| TaskAssignment → TaskPointStatus | **Cascade** | Fully owned |
| TaskPoint → TaskPointStatus | **NoAction** | Avoids cycle: WorkTask→Assignment→PointStatus AND WorkTask→Point→PointStatus |

### Seeding

- **`AppIdentityDbContextSeed`** — Seeds 4 users on first run (admin, abdelrahman, khaled, omar). No roles seeded yet (RBAC planned for later).
- Runs at startup in `Program.cs` after `MigrateAsync()`

### Connection String

```
Server=.;Database=TasksDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;
```

---

## Core Business Rules

1. **Corporations** have many **Sections**; sections belong to exactly one corporation.
2. **Users** belong to at most one Corporation and at most one Section (both nullable — admins may be unattached).
3. **Task creation flow:** Select Corporation → (optionally) Section → User(s). If only Corporation is selected (no section), the task targets all users in all sections of that corporation.
4. **Task types** determine behavior: **Normal** (plain), **Point** (checklist with ordered items), **Counter** (numeric target with per-user completed count).
5. **TaskAssignment** has its own `Status` — each assigned user progresses independently. The overall `WorkTask.Status` reflects aggregate state.
6. **Point-type tasks:** Each assigned user independently marks individual points as done — allows partial completion.
7. **Counter-type tasks:** `TargetCount` set on WorkTask; each `TaskAssignment` has its own `CompletedCount`.
8. **Comments** function as internal chat — each comment is exactly one type: text, image, or file.
9. **RBAC** (Roles + Permissions) is planned but NOT implemented yet. All users are in one `AppUser` table; roles will be added later.

---

## Program.cs — DI Registration & Pipeline

### Services Registered

```csharp
// Mapping
AddAutoMapper(CorporationProfile)

// Data & Repositories
AddDbContext<TaskContext>(SqlServer)
AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
AddScoped<IUnitOfWork, UnitOfWork>()

// Business Services
AddScoped<ICodeGeneratorService, CodeGeneratorService>()

// MVC
AddControllersWithViews() + AddRazorRuntimeCompilation()

// Identity
AddIdentity<AppUser, IdentityRole>(password policy) + AddEntityFrameworkStores<TaskContext>()

// Cookie Auth
LoginPath = /Account/Login, AccessDeniedPath = /Account/AccessDenied, 8h expiry, sliding
```

### Middleware Pipeline

```
ExceptionHandler (prod) → HTTPS → Routing → Authentication → Authorization → StaticAssets → MapControllerRoute
```

Default route: `{controller=Account}/{action=Login}/{id?}` — starts on Login page.

---

## Controllers & Views

### AccountController
| Action | Method | Auth | Description |
|---|---|---|---|
| `Login` | GET | Anonymous | Show login page (redirects to Home if already authenticated) |
| `Login` | POST | Anonymous | Validate credentials (supports email or username), sign in via cookie |
| `SignOut` | GET | Authorize | Sign out and redirect to Login |
| `AccessDenied` | GET | Anonymous | 403 page shown when a user lacks the required permission |

### HomeController (`[Authorize]`)
| Action | Description |
|---|---|
| `Index` | Dashboard (currently empty placeholder) |
| `Privacy` | Privacy page |
| `Error` | Error page |

### CorporationController (`[Authorize(Policy = Permissions.Corporations.Manage)]`)
| Action | Method | Description |
|---|---|---|
| `Index` | GET | List all corporations (DataTable view) |
| `Details` | GET | View single corporation details |
| `Create` | GET/POST | Create form + save. Code auto-generated via `ICodeGeneratorService` (prefix: `PXC`) |
| `Edit` | GET/POST | Edit form. Code is read-only (never edited) |
| `Delete` | POST | Delete via AJAX, returns JSON `{ success, message }` |
| `CheckUniqueName` | GET | Remote validation — checks corporation name uniqueness (excludes current ID during edit) |

### SectionController (`[Authorize(Policy = Permissions.Sections.Manage)]`)
| Action | Method | Description |
|---|---|---|
| `Index` | GET | List all sections (DataTable) |
| `Details` | GET | View section + assigned employees |
| `Create` | GET/POST | Create + assign employees; code auto-generated (prefix: `PXS`) |
| `Edit` | GET/POST | Edit + reconcile employee assignments |
| `Delete` | POST | Detach employees, then delete; AJAX JSON response |
| `CheckUniqueName` | GET | Remote validation |
| `GetAvailableEmployees` | GET | Returns active unassigned employees for a corporation (AJAX) |

### TaskTypeController (`[Authorize(Policy = Permissions.TaskTypes.Manage)]`)
| Action | Method | Description |
|---|---|---|
| `Index` | GET | List all task types |
| `Details` | GET | View task type |
| `Create` | GET/POST | Create task type |
| `Edit` | GET/POST | Edit task type |
| `Delete` | POST | Delete via AJAX |
| `CheckUniqueName` | GET | Remote validation |

### UserController (`[Authorize(Policy = Permissions.Users.Manage)]`)
| Action | Method | Description |
|---|---|---|
| `Index` | GET | List all users (DataTable) with role/corp/section/status columns |
| `Create` | GET/POST | Create user with role, corp, section, password |
| `Edit` | GET/POST | Edit user info, role, assignment, optional password change |
| `ToggleActive` | POST | Activate/deactivate user; AJAX JSON response |
| `GetSectionsByCorporation` | GET | Returns sections for a corporation dropdown (AJAX) |

### Views Structure

```
Views/
├── Account/Login.cshtml, AccessDenied.cshtml  (Skote auth layout via _AuthLayout.cshtml)
├── Corporation/                   (Index, Create, Edit, Details)
├── Section/                       (Index, Create, Edit, Details)
├── TaskType/                      (Index, Create, Edit, Details)
├── User/                          (Index, Create, Edit)
├── Home/Index.cshtml, Privacy.cshtml
└── Shared/
    ├── _Layout.cshtml             (Main Skote admin layout: sidebar + nav + content + footer)
    ├── _AuthLayout.cshtml         (Login/register page layout — no sidebar)
    ├── _Sidebar.cshtml            (Left nav — menu items gated by <permission> TagHelper)
    ├── _Nav.cshtml                (Top navbar)
    ├── _Head.cshtml               (CSS includes)
    ├── _Scripts.cshtml            (JS includes)
    ├── _Footer.cshtml
    ├── _Notifications.cshtml      (TempData → Toastr notifications)
    ├── _RightSideBar.cshtml       (Skote theme settings sidebar)
    └── _ValidationScriptsPartial.cshtml
```

---

## Authorization System

### Roles (defined in `Tasks.Domain/Authorization/Roles.cs`)
- `Roles.Admin` = `"Admin"` — task creators; full access
- `Roles.Employee` = `"Employee"` — assignees; limited access

### Permissions (defined in `Tasks.Domain/Authorization/Permissions.cs`)
Grouped by feature. Claim type: `Permissions.ClaimType = "permission"`.

| Permission Constant | Value | Admin | Employee |
|---|---|:---:|:---:|
| `Permissions.Corporations.Manage` | `Corporations.Manage` | ✓ | |
| `Permissions.Sections.Manage` | `Sections.Manage` | ✓ | |
| `Permissions.TaskTypes.Manage` | `TaskTypes.Manage` | ✓ | |
| `Permissions.Users.Manage` | `Users.Manage` | ✓ | |
| `Permissions.Tasks.Create` | `Tasks.Create` | ✓ | |
| `Permissions.Tasks.ViewAll` | `Tasks.ViewAll` | ✓ | |
| `Permissions.Tasks.ViewAssigned` | `Tasks.ViewAssigned` | | ✓ |
| `Permissions.Tasks.Comment` | `Tasks.Comment` | ✓ | ✓ |
| `Permissions.Tasks.UpdateProgress` | `Tasks.UpdateProgress` | | ✓ |

### How it works (end-to-end)
1. **Seed:** `AppIdentityDbContextSeed` creates `Admin` and `Employee` roles, then assigns users.
2. **Cookie:** `AppUserClaimsPrincipalFactory` fires at login — expands the user's role into individual `permission` claims stored in the auth cookie. No per-request DB hit.
3. **Server guard:** `[Authorize(Policy = Permissions.Corporations.Manage)]` — `PermissionPolicyProvider` auto-creates a policy for any permission constant; `PermissionAuthorizationHandler` checks `HasClaim("permission", ...)`.
4. **Client guard:** `<permission required="@Permissions.Corporations.Manage">…</permission>` — `PermissionTagHelper` suppresses output when the user lacks the claim. Used in `_Sidebar.cshtml` and action-button areas. Server and client guards are driven by the same claims, so they can never drift.
5. **Admin → not assignable to tasks:** Domain rule (not a permission) — the user picker for task assignment queries only the `Employee` role via `UserManager.GetUsersInRoleAsync(Roles.Employee)`, so admins never appear. POST re-validates.

### Adding a new permission
1. Add a constant to `Permissions.cs`.
2. Add it to `RolePermissions._map` for the relevant role(s).
3. Decorate the controller/action: `[Authorize(Policy = Permissions.NewFeature.Action)]`.
4. Gate the sidebar/button: `<permission required="@Permissions.NewFeature.Action">`.
No policy registration needed — `PermissionPolicyProvider` auto-wires it.

---

## ViewModels

- **`LoginViewModel`** — `EmailOrUserName` (required), `Password` (required), `RememberMe`
- **`CorporationViewModel`** — `Id`, `Code?` (read-only), `Name` (required, 2-200 chars, `[Remote]` unique check), `NameAr?` (max 200), `Notes?` (max 5000)
- **`SectionViewModel`** — `Id`, `Code?`, `Name`, `CorporationId`, `Email?`, `Fax?`, `Phone?`, `Address?`, `Telex?`, `Notes?`, `SelectedUserIds`, display helpers (`CorporationName`, `MemberCount`, `Corporations`, `AvailableEmployees`)
- **`TaskTypeViewModel`** — `Id`, `Name` (required, unique), `Category` (TaskCategory enum)
- **`UserViewModel`** — `Id?`, `FirstName`, `LastName`, `UserName`, `Email`, `PhoneNumber?`, `Gender`, `Role` (required), `CorporationId?`, `SectionId?`, `IsActive`, `Password?`/`ConfirmPassword?` (create-only / optional on edit), display helpers (`CorporationName`, `SectionName`, `Corporations`, `Sections`, `Roles`)
- **`AvailableEmployeeViewModel`** — `Id`, `FullName`, `Email`, `IsSelected` — used in Section create/edit employee picker
- **`ErrorViewModel`** — `RequestId`

---

## Helpers

- **`DocumentSettings`** (static) — `UplaodFile(IFormFile, folderName)` saves to `wwwroot/Files/{folder}` with GUID-prefixed filename; `DeleteFile(fileName, folderName)` removes from disk.

---

## File Structure Reference

```
Pixel.Tasks/
├── Pixel.Tasks.sln
├── CLAUDE.md
├── Tasks.Domain/
│   ├── Tasks.Domain.csproj
│   ├── IUnitOfWork.cs
│   ├── Enums/
│   │   ├── Gender.cs
│   │   ├── TaskCategory.cs
│   │   ├── PriorityLevel.cs
│   │   ├── WorkTaskStatus.cs
│   │   └── CommentType.cs
│   ├── Models/
│   │   ├── BaseModel.cs
│   │   ├── ICodedEntity.cs
│   │   ├── Corporation.cs
│   │   ├── Section.cs
│   │   ├── TaskType.cs
│   │   ├── WorkTask.cs
│   │   ├── TaskAssignment.cs
│   │   ├── TaskPoint.cs
│   │   ├── TaskPointStatus.cs
│   │   ├── TaskComment.cs
│   │   └── Identity/
│   │       └── AppUser.cs
│   ├── Repositories/
│   │   └── IGenericRepository.cs
│   ├── Services/
│   │   └── ICodeGeneratorService.cs
│   ├── Authorization/
│   │   ├── Roles.cs
│   │   ├── Permissions.cs
│   │   └── RolePermissions.cs
│   └── Specifications/
│       ├── BaseSpecifications.cs
│       ├── ISpecifications.cs
│       ├── CorporationSpec/
│       │   ├── CorporationSpec.cs
│       │   └── CorporationByNameSpec.cs
│       ├── SectionSpec/
│       │   ├── SectionSpec.cs
│       │   ├── SectionByNameSpec.cs
│       │   └── SectionByCorporationSpec.cs
│       └── TaskTypeSpec/
│           ├── TaskTypeSpec.cs
│           └── TaskTypeByNameSpec.cs
├── Tasks.Repository/
│   ├── Tasks.Repository.csproj
│   ├── GenericRepository.cs
│   ├── SpecificationEvalutor.cs
│   ├── UnitOfWork.cs
│   └── Data/
│       ├── TaskContext.cs
│       ├── AppIdentityDbContextSeed.cs
│       ├── Configrations/
│       │   ├── AppUserConfig.cs
│       │   ├── CorporationConfig.cs
│       │   ├── SectionConfig.cs
│       │   ├── TaskTypeConfig.cs
│       │   ├── WorkTaskConfig.cs
│       │   ├── TaskAssignmentConfig.cs
│       │   ├── TaskPointConfig.cs
│       │   ├── TaskPointStatusConfig.cs
│       │   └── TaskCommentConfig.cs
│       ├── DataSeed/  (empty — seed is in AppIdentityDbContextSeed.cs)
│       └── Migrations/
├── Tasks.Services/
│   ├── Tasks.Services.csproj
│   └── CodeGeneration/
│       └── CodeGeneratorService.cs
└── Tasks.Presentation/
    ├── Tasks.Presentation.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Authorization/
    │   ├── PermissionRequirement.cs
    │   ├── PermissionAuthorizationHandler.cs
    │   ├── PermissionPolicyProvider.cs
    │   └── AppUserClaimsPrincipalFactory.cs
    ├── TagHelpers/
    │   └── PermissionTagHelper.cs
    ├── Controllers/
    │   ├── AccountController.cs
    │   ├── HomeController.cs
    │   ├── CorporationController.cs
    │   ├── SectionController.cs
    │   ├── TaskTypeController.cs
    │   └── UserController.cs
    ├── ViewModels/
    │   ├── LoginViewModel.cs
    │   ├── CorporationViewModel.cs
    │   ├── SectionViewModel.cs
    │   ├── TaskTypeViewModel.cs
    │   ├── UserViewModel.cs
    │   ├── AvailableEmployeeViewModel.cs
    │   └── ErrorViewModel.cs
    ├── MappingProfiles/
    │   ├── CorporationProfile.cs
    │   ├── SectionProfile.cs
    │   └── TaskTypeProfile.cs
    ├── Helpers/
    │   └── DocumentSettings.cs
    ├── Views/
    │   ├── _ViewImports.cshtml
    │   ├── _ViewStart.cshtml
    │   ├── Account/Login.cshtml, AccessDenied.cshtml
    │   ├── Home/Index.cshtml, Privacy.cshtml
    │   ├── Corporation/Index.cshtml, Create.cshtml, Edit.cshtml, Details.cshtml
    │   ├── Section/Index.cshtml, Create.cshtml, Edit.cshtml, Details.cshtml
    │   ├── TaskType/Index.cshtml, Create.cshtml, Edit.cshtml, Details.cshtml
    │   ├── User/Index.cshtml, Create.cshtml, Edit.cshtml
    │   └── Shared/ (_Layout, _AuthLayout, _Sidebar, _Nav, _Head, _Scripts, _Footer, _Notifications, _RightSideBar, _ValidationScriptsPartial, Error)
    ├── Logs/
    └── wwwroot/ (Skote template: css/, js/, lib/, back/, favicon.ico)
```

---

## Coding Conventions

1. **PascalCase** for public properties, class names, method names, namespace parts
2. **camelCase** for private fields (e.g., `private readonly TaskContext dbContext;`)
3. **_camelCase** for injected dependencies in services/controllers (e.g., `_unitOfWork`, `_mapper`)
4. **Spec suffix** for specification classes (e.g., `CorporationSpec`)
5. **SpecParams suffix** for specification parameter classes (e.g., `ProductSpecParams`)
6. **ViewModel suffix** for MVC view models (e.g., `CorporationViewModel`)
7. **Config suffix** for EF configurations (e.g., `CorporationConfig`)
8. **Profile suffix** for AutoMapper profiles (e.g., `CorporationProfile`)
9. Service interfaces defined in `Tasks.Domain/Services/`, implementations in `Tasks.Services/`
10. Repository interface in `Tasks.Domain/Repositories/`, implementation in `Tasks.Repository/`
11. Enums in `Tasks.Domain/Enums/`
12. `DateTime.UtcNow` used everywhere for timestamps
13. All entities that are DB-persisted inherit from `BaseModel`
14. String-based includes (`IncludeStrings`) used for multi-level navigation (ThenInclude equivalent)
15. Comments only when intent is not obvious; remove pointless or redundant comments
16. Controllers use `[Authorize(Policy = Permissions.X.Y)]` for permission-based access; `[Authorize]` for authenticated-only (dashboard); login/access-denied actions use `[AllowAnonymous]`
17. Delete actions return JSON for AJAX calls; Create/Edit use POST + redirect pattern
18. `[Remote]` attribute on ViewModels for client-side uniqueness validation
19. Authorization constants (`Roles`, `Permissions`) live in `Tasks.Domain/Authorization/` — no web/infra deps

---

## UI/UX Guidelines

- **Template:** Skote Admin & Dashboard (Bootstrap 5)
- **Client preference:** Modern, professional, premium-feeling UI/UX
- **Sidebar:** Dark mode (`data-sidebar="dark"`), Boxicons for menu icons
- **Notifications:** Toastr for success/error/warning messages via `TempData`
- **Tables:** DataTables plugin for server-side CRUD listing pages
- **Forms:** Bootstrap card-based forms with client-side validation (`jquery.validate` + `jquery.validate.unobtrusive`)
- **Delete:** SweetAlert2 confirmation → AJAX POST → Toastr notification
- **Auth layout:** `_AuthLayout.cshtml` (separate from admin layout, no sidebar)
- **Admin layout:** `_Layout.cshtml` (sidebar + topnav + footer + right sidebar theme settings)

---

## Important Notes

- **`SpecificationEvalutor.cs`** filename has a typo (missing 'a' in Evaluator) — this is the established convention, do NOT rename it
- **`Data/Configrations/`** folder has a typo — this is the established convention, do NOT rename it
- **`WorkTask`** is the entity name for "Task" — avoids collision with `System.Threading.Tasks.Task`
- **No roles/permissions are implemented yet** — RBAC (Roles + Permissions) is planned for later
- **No API endpoints** — this is a server-rendered MVC app (Razor Views), NOT a Web API
- **Seed data does NOT assign roles** — only creates users
- **DocumentSettings.UplaodFile** has a typo in the method name — keep it as-is

---

## Gotchas & Past Mistakes for Claude

> **This section tracks mistakes Claude has made on this project so they are never repeated.**
> Add entries here whenever Claude makes an error during development.

### 2026-06-27 — SQL Server cascade delete cycles
**What went wrong:** All FKs were set to `Cascade` delete, causing SQL Server error 1785 (multiple cascade paths) when running migrations.  
**Root cause:** Corporation → Section → WorkTask AND Corporation → WorkTask (direct) create multiple cascade paths to the same table. SQL Server forbids this.  
**Correct approach:** Use `DeleteBehavior.NoAction` for "reference" FKs (AppUser→Corp/Section, WorkTask→Corp/Section/CreatedBy, etc.) and keep `Cascade` only for true "ownership" FKs (WorkTask→Assignments/Points/Comments). See FK Cascade Strategy table above.

### 2026-06-27 — EF Core HasDefaultValue type mismatch for enums
**What went wrong:** Used `.HasDefaultValue(1)` (int) for a property typed as `WorkTaskStatus` (enum), causing EF Core design-time error: "Cannot set default value '1' of type 'int' on property of type 'WorkTaskStatus'".  
**Root cause:** EF Core enforces exact type matching between the default value and the property type.  
**Correct approach:** Remove `HasDefaultValue()` entirely — the C# model already initializes `Status = WorkTaskStatus.Pending`, so EF always inserts the correct value without a DB-level default.

### 2026-06-27 — Corporation.cs edit not persisting (CRLF mismatch)
**What went wrong:** `replace_file_content` silently failed to match content due to CRLF vs LF line ending mismatch in `Corporation.cs`.  
**Root cause:** The file had mixed line endings; the replacement target string used LF but the file had CRLF.  
**Correct approach:** When a targeted replace fails silently, verify the file content with `view_file` and use `write_to_file` with `Overwrite=true` as a fallback.

<!-- 
FORMAT FOR NEW ENTRIES:
### [Date] — Short description
**What went wrong:** Describe the mistake.
**Root cause:** Why it happened.
**Correct approach:** What should have been done instead.
-->

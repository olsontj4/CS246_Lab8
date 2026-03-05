# Setting Up MySQL Database with Identity and Repository Pattern

## Overview

This guide provides a complete workflow for adding MySQL database support, ASP.NET Core Identity authentication, and the Repository Pattern to the CodeReviews application.

**Key Components:**
- **MySQL Database** - Primary data store
- **ASP.NET Core Identity** - User authentication and authorization
- **Repository Pattern** - Data access abstraction layer
- **Unit Testing** - Using fake repositories

---

## Architecture Overview

```
???????????????????????????????????????????????????????????
?                    Presentation Layer                    ?
?  ????????????????????        ????????????????????????  ?
?  ?  MVC Controllers ?        ?  Identity Razor Pages?  ?
?  ?  (Your Domain)   ?        ?  (Authentication)    ?  ?
?  ????????????????????        ????????????????????????  ?
??????????????????????????????????????????????????????????
            ?                            ?
??????????????????????????????????????????????????????????
?           ?      Business Layer        ?               ?
?  ????????????????????        ???????????????????      ?
?  ?   Repositories   ?        ?  UserManager/   ?      ?
?  ?   (Interfaces)   ?        ?  SignInManager  ?      ?
?  ????????????????????        ???????????????????      ?
??????????????????????????????????????????????????????????
            ?                            ?
??????????????????????????????????????????????????????????
?           ?       Data Layer           ?               ?
?  ????????????????????        ???????????????????      ?
?  ?  Repository      ?        ?  Identity       ?      ?
?  ?  Implementations ?        ?  Tables         ?      ?
?  ????????????????????        ???????????????????      ?
?           ??????????????????????????????               ?
?                ApplicationDbContext                    ?
?                  (EF Core + MySQL)                     ?
??????????????????????????????????????????????????????????
```

---

## Phase 1: Add MySQL Support

### Step 1: Install Required NuGet Packages

```bash
# MySQL provider for Entity Framework Core
dotnet add package Pomelo.EntityFrameworkCore.MySql

# Identity packages
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools

# For scaffolding Identity
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
```

**Package Purposes:**
- `Pomelo.EntityFrameworkCore.MySql` - MySQL database provider
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - Identity with EF Core
- `Microsoft.EntityFrameworkCore.Tools` - Migration tools
- `dotnet-aspnet-codegenerator` - Scaffolding CLI tool

---

## Phase 2: Scaffold Identity with MySQL

### Step 2: Create Custom ApplicationUser

Create this **before** scaffolding so Identity can use it.

**File: `Data/ApplicationUser.cs`**

```csharp
using Microsoft.AspNetCore.Identity;

namespace CodeReviews.Data;

/// <summary>
/// Custom Identity user with student-specific properties
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Custom properties for code review app
    public string? Name { get; set; }
    public string? GitHubId { get; set; }
    public string? Institution { get; set; }
    public string? LabPartner { get; set; }
}
```

**Why extend IdentityUser?**
- Adds custom properties to the user table
- Maintains compatibility with Identity system
- Single table for all user data

---

### Step 3: Create ApplicationDbContext

**File: `Data/ApplicationDbContext.cs`**

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeReviews.Data;

/// <summary>
/// Database context for Identity and domain entities
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Domain entities will be added here later
    // public DbSet<Assignment> Assignments { get; set; }
    // public DbSet<CodeReview> CodeReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Custom configurations will be added here
    }
}
```

---

### Step 4: Configure MySQL Connection

**File: `appsettings.json`**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=CodeReviews;User=root;Password=yourpassword;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Connection String Parameters:**
- `Server=localhost` - MySQL server location
- `Port=3306` - Default MySQL port
- `Database=CodeReviews` - Database name
- `User=root` - MySQL username
- `Password=yourpassword` - MySQL password (change this!)

**For Development:** Consider using user secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=CodeReviews;User=root;Password=yourpassword;"
```

---

### Step 5: Configure Services in Program.cs

**File: `Program.cs`**

```csharp
using CodeReviews.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure MySQL database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString, 
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));

// Add Identity with custom user
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedAccount = false; // Set to true for email confirmation
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization middleware (ORDER MATTERS!)
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Required for Identity pages

app.Run();
```

---

### Step 6: Scaffold Identity Pages

#### Option A: Using Visual Studio

1. **Right-click on project** in Solution Explorer
2. Select **Add > New Scaffolded Item...**
3. Select **Identity** from the left panel
4. Click **Add**
5. Select pages to override (recommended: all Account pages)
6. Select **ApplicationDbContext** from the dropdown
7. Click **Add**

#### Option B: Using Command Line

```bash
# Scaffold specific pages
dotnet aspnet-codegenerator identity \
  -dc CodeReviews.Data.ApplicationDbContext \
  --files "Account.Register;Account.Login;Account.Logout;Account.RegisterConfirmation"

# Or scaffold all Identity pages
dotnet aspnet-codegenerator identity \
  -dc CodeReviews.Data.ApplicationDbContext \
  --useDefaultUI
```

**What gets created:**
```
Areas/
??? Identity/
    ??? Pages/
        ??? _ViewStart.cshtml
        ??? Account/
            ??? Register.cshtml
            ??? Register.cshtml.cs
            ??? Login.cshtml
            ??? Login.cshtml.cs
            ??? Logout.cshtml
            ??? Logout.cshtml.cs
            ??? ... (other pages)
```

---

### Step 7: Create and Apply Initial Migration

```bash
# Create migration
dotnet ef migrations add InitialIdentityCreate

# Apply migration to database
dotnet ef database update
```

**This creates Identity tables:**
- `AspNetUsers` - User accounts (with your custom fields)
- `AspNetRoles` - Roles
- `AspNetUserRoles` - User-role relationships
- `AspNetUserClaims` - User claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - Authentication tokens
- `AspNetRoleClaims` - Role claims

---

### Step 8: Add Login Partial to Layout

**File: `Views/Shared/_Layout.cshtml`**

Add this to the navbar:

```html
<ul class="navbar-nav flex-grow-1">
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
    </li>
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="ReadinessChecker" asp-action="Index">Readiness Checker</a>
    </li>
</ul>
<partial name="_LoginPartial" />
```

**Create the login partial: `Views/Shared/_LoginPartial.cshtml`**

```html
@using Microsoft.AspNetCore.Identity
@using CodeReviews.Data

@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager

<ul class="navbar-nav">
@if (SignInManager.IsSignedIn(User))
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Manage/Index" title="Manage">
            Hello @User.Identity?.Name!
        </a>
    </li>
    <li class="nav-item">
        <form class="form-inline" asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })">
            <button type="submit" class="nav-link btn btn-link text-dark">Logout</button>
        </form>
    </li>
}
else
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Register">Register</a>
    </li>
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Login">Login</a>
    </li>
}
</ul>
```

---

## Phase 3: Implement Repository Pattern

### Step 9: Create Domain Models

**File: `Models/Assignment.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace CodeReviews.Models;

/// <summary>
/// Represents a course assignment that requires code review
/// </summary>
public class Assignment
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Course { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Section { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Institution { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string AssignmentName { get; set; } = string.Empty;

    [Range(1, 100)]
    public int AssignmentNumber { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<CodeReview> CodeReviews { get; set; } = new List<CodeReview>();
}
```

**File: `Models/CodeReview.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using CodeReviews.Data;

namespace CodeReviews.Models;

/// <summary>
/// Represents a code review request and its matching
/// </summary>
public class CodeReview
{
    public int Id { get; set; }

    // Student requesting review
    [Required]
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    // Assignment being reviewed
    [Required]
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    // Repository information
    [Required]
    [Url]
    public string RepoUrl { get; set; } = string.Empty;

    // Dates
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReadyDate { get; set; }

    // Reviewer assignment
    public string? ReviewerId { get; set; }
    public ApplicationUser? Reviewer { get; set; }

    // Status tracking
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Matched, InReview, Completed

    // Review metadata
    public DateTime? ReviewStartedDate { get; set; }
    public DateTime? ReviewCompletedDate { get; set; }

    [StringLength(2000)]
    public string? ReviewNotes { get; set; }
}
```

---

### Step 10: Create Repository Interfaces

**File: `Repositories/IAssignmentRepository.cs`**

```csharp
using CodeReviews.Models;

namespace CodeReviews.Repositories;

/// <summary>
/// Repository interface for Assignment entities
/// </summary>
public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(int id);
    Task<IEnumerable<Assignment>> GetAllAsync();
    Task<IEnumerable<Assignment>> GetByCourseAsync(string course);
    Task<IEnumerable<Assignment>> GetByInstitutionAsync(string institution);
    Task AddAsync(Assignment assignment);
    Task UpdateAsync(Assignment assignment);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

**File: `Repositories/ICodeReviewRepository.cs`**

```csharp
using CodeReviews.Models;

namespace CodeReviews.Repositories;

/// <summary>
/// Repository interface for CodeReview entities
/// </summary>
public interface ICodeReviewRepository
{
    Task<CodeReview?> GetByIdAsync(int id);
    Task<IEnumerable<CodeReview>> GetAllAsync();
    Task<IEnumerable<CodeReview>> GetByStudentIdAsync(string studentId);
    Task<IEnumerable<CodeReview>> GetByReviewerIdAsync(string reviewerId);
    Task<IEnumerable<CodeReview>> GetPendingReviewsAsync();
    Task<IEnumerable<CodeReview>> GetByAssignmentIdAsync(int assignmentId);
    Task AddAsync(CodeReview review);
    Task UpdateAsync(CodeReview review);
    Task DeleteAsync(int id);
    Task<CodeReview?> FindMatchingReviewAsync(string studentId, int assignmentId);
}
```

---

### Step 11: Implement Repositories

**File: `Repositories/AssignmentRepository.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using CodeReviews.Data;
using CodeReviews.Models;

namespace CodeReviews.Repositories;

/// <summary>
/// Implementation of Assignment repository using Entity Framework Core
/// </summary>
public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public AssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Assignment?> GetByIdAsync(int id)
    {
        return await _context.Assignments
            .Include(a => a.CodeReviews)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Assignment>> GetAllAsync()
    {
        return await _context.Assignments
            .OrderBy(a => a.Course)
            .ThenBy(a => a.AssignmentNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assignment>> GetByCourseAsync(string course)
    {
        return await _context.Assignments
            .Where(a => a.Course == course)
            .OrderBy(a => a.AssignmentNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assignment>> GetByInstitutionAsync(string institution)
    {
        return await _context.Assignments
            .Where(a => a.Institution == institution)
            .OrderBy(a => a.Course)
            .ThenBy(a => a.AssignmentNumber)
            .ToListAsync();
    }

    public async Task AddAsync(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Assignment assignment)
    {
        _context.Assignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment != null)
        {
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Assignments.AnyAsync(a => a.Id == id);
    }
}
```

**File: `Repositories/CodeReviewRepository.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using CodeReviews.Data;
using CodeReviews.Models;

namespace CodeReviews.Repositories;

/// <summary>
/// Implementation of CodeReview repository using Entity Framework Core
/// </summary>
public class CodeReviewRepository : ICodeReviewRepository
{
    private readonly ApplicationDbContext _context;

    public CodeReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CodeReview?> GetByIdAsync(int id)
    {
        return await _context.CodeReviews
            .Include(cr => cr.Student)
            .Include(cr => cr.Reviewer)
            .Include(cr => cr.Assignment)
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<IEnumerable<CodeReview>> GetAllAsync()
    {
        return await _context.CodeReviews
            .Include(cr => cr.Student)
            .Include(cr => cr.Reviewer)
            .Include(cr => cr.Assignment)
            .OrderByDescending(cr => cr.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CodeReview>> GetByStudentIdAsync(string studentId)
    {
        return await _context.CodeReviews
            .Include(cr => cr.Reviewer)
            .Include(cr => cr.Assignment)
            .Where(cr => cr.StudentId == studentId)
            .OrderByDescending(cr => cr.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CodeReview>> GetByReviewerIdAsync(string reviewerId)
    {
        return await _context.CodeReviews
            .Include(cr => cr.Student)
            .Include(cr => cr.Assignment)
            .Where(cr => cr.ReviewerId == reviewerId)
            .OrderByDescending(cr => cr.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CodeReview>> GetPendingReviewsAsync()
    {
        return await _context.CodeReviews
            .Include(cr => cr.Student)
            .Include(cr => cr.Assignment)
            .Where(cr => cr.Status == "Pending")
            .OrderBy(cr => cr.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CodeReview>> GetByAssignmentIdAsync(int assignmentId)
    {
        return await _context.CodeReviews
            .Include(cr => cr.Student)
            .Include(cr => cr.Reviewer)
            .Where(cr => cr.AssignmentId == assignmentId)
            .OrderByDescending(cr => cr.SubmittedDate)
            .ToListAsync();
    }

    public async Task AddAsync(CodeReview review)
    {
        _context.CodeReviews.Add(review);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CodeReview review)
    {
        _context.CodeReviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _context.CodeReviews.FindAsync(id);
        if (review != null)
        {
            _context.CodeReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CodeReview?> FindMatchingReviewAsync(string studentId, int assignmentId)
    {
        // Find a pending review from a different student for the same assignment
        // Prioritize lab partners
        var currentStudent = await _context.Users.FindAsync(studentId);
        
        // First try to match with lab partner
        if (!string.IsNullOrEmpty(currentStudent?.LabPartner))
        {
            var partnerReview = await _context.CodeReviews
                .Include(cr => cr.Student)
                .Include(cr => cr.Assignment)
                .FirstOrDefaultAsync(cr => 
                    cr.AssignmentId == assignmentId &&
                    cr.Status == "Pending" &&
                    cr.StudentId != studentId &&
                    cr.Student!.LabPartner == currentStudent.Name);
                    
            if (partnerReview != null)
                return partnerReview;
        }
        
        // Otherwise find any available review
        return await _context.CodeReviews
            .Include(cr => cr.Student)
            .Include(cr => cr.Assignment)
            .FirstOrDefaultAsync(cr => 
                cr.AssignmentId == assignmentId &&
                cr.Status == "Pending" &&
                cr.StudentId != studentId);
    }
}
```

---

### Step 12: Update ApplicationDbContext with Domain Models

**File: `Data/ApplicationDbContext.cs`**

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CodeReviews.Models;

namespace CodeReviews.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Domain entities
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<CodeReview> CodeReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Assignment
        builder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Course, e.Section, e.AssignmentNumber });
            
            entity.HasMany(e => e.CodeReviews)
                .WithOne(cr => cr.Assignment)
                .HasForeignKey(cr => cr.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CodeReview
        builder.Entity<CodeReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Configure Student relationship
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure Reviewer relationship
            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure Assignment relationship
            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.CodeReviews)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for common queries
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.ReviewerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SubmittedDate);
        });
    }
}
```

---

### Step 13: Register Repositories in Program.cs

Add this after the Identity configuration:

```csharp
// Register repositories
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ICodeReviewRepository, CodeReviewRepository>();
```

---

### Step 14: Create Migration for Domain Models

```bash
# Create migration
dotnet ef migrations add AddDomainModels

# Apply migration
dotnet ef database update
```

---

## Phase 4: Create Controllers with Repositories

### Step 15: Create Assignment Controller

**File: `Controllers/AssignmentController.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeReviews.Models;
using CodeReviews.Repositories;

namespace CodeReviews.Controllers;

[Authorize]
public class AssignmentController : Controller
{
    private readonly IAssignmentRepository _assignmentRepository;

    public AssignmentController(IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    // GET: Assignment
    public async Task<IActionResult> Index()
    {
        var assignments = await _assignmentRepository.GetAllAsync();
        return View(assignments);
    }

    // GET: Assignment/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }
        return View(assignment);
    }

    // GET: Assignment/Create
    [Authorize(Roles = "Instructor")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Assignment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Create(Assignment assignment)
    {
        if (!ModelState.IsValid)
        {
            return View(assignment);
        }

        await _assignmentRepository.AddAsync(assignment);
        return RedirectToAction(nameof(Index));
    }

    // GET: Assignment/Edit/5
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Edit(int id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }
        return View(assignment);
    }

    // POST: Assignment/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Edit(int id, Assignment assignment)
    {
        if (id != assignment.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(assignment);
        }

        await _assignmentRepository.UpdateAsync(assignment);
        return RedirectToAction(nameof(Index));
    }

    // GET: Assignment/Delete/5
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }
        return View(assignment);
    }

    // POST: Assignment/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _assignmentRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
```

---

### Step 16: Create CodeReview Controller

**File: `Controllers/CodeReviewController.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CodeReviews.Models;
using CodeReviews.Repositories;
using CodeReviews.Data;

namespace CodeReviews.Controllers;

[Authorize]
public class CodeReviewController : Controller
{
    private readonly ICodeReviewRepository _codeReviewRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public CodeReviewController(
        ICodeReviewRepository codeReviewRepository,
        IAssignmentRepository assignmentRepository,
        UserManager<ApplicationUser> userManager)
    {
        _codeReviewRepository = codeReviewRepository;
        _assignmentRepository = assignmentRepository;
        _userManager = userManager;
    }

    // GET: CodeReview
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var myReviews = await _codeReviewRepository.GetByStudentIdAsync(userId);
        var myReviewTasks = await _codeReviewRepository.GetByReviewerIdAsync(userId);
        
        ViewBag.MyReviewTasks = myReviewTasks;
        return View(myReviews);
    }

    // GET: CodeReview/Request
    public async Task<IActionResult> Request()
    {
        ViewBag.Assignments = await _assignmentRepository.GetAllAsync();
        return View();
    }

    // POST: CodeReview/Request
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Request(CodeReview review)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        review.StudentId = userId;
        review.SubmittedDate = DateTime.UtcNow;
        review.Status = "Pending";

        if (!ModelState.IsValid)
        {
            ViewBag.Assignments = await _assignmentRepository.GetAllAsync();
            return View(review);
        }

        await _codeReviewRepository.AddAsync(review);

        // Try to find a match
        var match = await _codeReviewRepository.FindMatchingReviewAsync(userId, review.AssignmentId);
        if (match != null)
        {
            // Create mutual review assignments
            review.ReviewerId = match.StudentId;
            review.Status = "Matched";
            await _codeReviewRepository.UpdateAsync(review);

            match.ReviewerId = userId;
            match.Status = "Matched";
            await _codeReviewRepository.UpdateAsync(match);

            TempData["Success"] = "Your review has been matched!";
        }
        else
        {
            TempData["Info"] = "Your review request has been submitted. You'll be matched when another student is ready.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: CodeReview/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var review = await _codeReviewRepository.GetByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (review.StudentId != userId && review.ReviewerId != userId)
        {
            return Forbid();
        }

        return View(review);
    }
}
```

---

## Phase 5: Testing with Fake Repositories

### Step 17: Create Fake Repositories

**File: `CodeReviews.Tests/Fakes/FakeAssignmentRepository.cs`**

```csharp
using CodeReviews.Models;
using CodeReviews.Repositories;

namespace CodeReviews.Tests.Fakes;

public class FakeAssignmentRepository : IAssignmentRepository
{
    private readonly List<Assignment> _assignments = new();
    private int _nextId = 1;

    public Task<Assignment?> GetByIdAsync(int id)
    {
        return Task.FromResult(_assignments.FirstOrDefault(a => a.Id == id));
    }

    public Task<IEnumerable<Assignment>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Assignment>>(_assignments);
    }

    public Task<IEnumerable<Assignment>> GetByCourseAsync(string course)
    {
        return Task.FromResult<IEnumerable<Assignment>>(
            _assignments.Where(a => a.Course == course));
    }

    public Task<IEnumerable<Assignment>> GetByInstitutionAsync(string institution)
    {
        return Task.FromResult<IEnumerable<Assignment>>(
            _assignments.Where(a => a.Institution == institution));
    }

    public Task AddAsync(Assignment assignment)
    {
        assignment.Id = _nextId++;
        _assignments.Add(assignment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Assignment assignment)
    {
        var existing = _assignments.FirstOrDefault(a => a.Id == assignment.Id);
        if (existing != null)
        {
            _assignments.Remove(existing);
            _assignments.Add(assignment);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == id);
        if (assignment != null)
        {
            _assignments.Remove(assignment);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(int id)
    {
        return Task.FromResult(_assignments.Any(a => a.Id == id));
    }
}
```

**File: `CodeReviews.Tests/Fakes/FakeCodeReviewRepository.cs`**

```csharp
using CodeReviews.Models;
using CodeReviews.Repositories;

namespace CodeReviews.Tests.Fakes;

public class FakeCodeReviewRepository : ICodeReviewRepository
{
    private readonly List<CodeReview> _reviews = new();
    private int _nextId = 1;

    public Task<CodeReview?> GetByIdAsync(int id)
    {
        return Task.FromResult(_reviews.FirstOrDefault(r => r.Id == id));
    }

    public Task<IEnumerable<CodeReview>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<CodeReview>>(_reviews);
    }

    public Task<IEnumerable<CodeReview>> GetByStudentIdAsync(string studentId)
    {
        return Task.FromResult<IEnumerable<CodeReview>>(
            _reviews.Where(r => r.StudentId == studentId));
    }

    public Task<IEnumerable<CodeReview>> GetByReviewerIdAsync(string reviewerId)
    {
        return Task.FromResult<IEnumerable<CodeReview>>(
            _reviews.Where(r => r.ReviewerId == reviewerId));
    }

    public Task<IEnumerable<CodeReview>> GetPendingReviewsAsync()
    {
        return Task.FromResult<IEnumerable<CodeReview>>(
            _reviews.Where(r => r.Status == "Pending"));
    }

    public Task<IEnumerable<CodeReview>> GetByAssignmentIdAsync(int assignmentId)
    {
        return Task.FromResult<IEnumerable<CodeReview>>(
            _reviews.Where(r => r.AssignmentId == assignmentId));
    }

    public Task AddAsync(CodeReview review)
    {
        review.Id = _nextId++;
        _reviews.Add(review);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CodeReview review)
    {
        var existing = _reviews.FirstOrDefault(r => r.Id == review.Id);
        if (existing != null)
        {
            _reviews.Remove(existing);
            _reviews.Add(review);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var review = _reviews.FirstOrDefault(r => r.Id == id);
        if (review != null)
        {
            _reviews.Remove(review);
        }
        return Task.CompletedTask;
    }

    public Task<CodeReview?> FindMatchingReviewAsync(string studentId, int assignmentId)
    {
        return Task.FromResult(_reviews.FirstOrDefault(r => 
            r.AssignmentId == assignmentId &&
            r.Status == "Pending" &&
            r.StudentId != studentId));
    }
}
```

---

### Step 18: Create Controller Tests

**File: `CodeReviews.Tests/Controllers/AssignmentControllerTests.cs`**

```csharp
using Xunit;
using Microsoft.AspNetCore.Mvc;
using CodeReviews.Controllers;
using CodeReviews.Models;
using CodeReviews.Tests.Fakes;

namespace CodeReviews.Tests.Controllers;

public class AssignmentControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfAssignments()
    {
        // Arrange
        var fakeRepo = new FakeAssignmentRepository();
        await fakeRepo.AddAsync(new Assignment { AssignmentName = "Lab 1", Course = "CS295N" });
        await fakeRepo.AddAsync(new Assignment { AssignmentName = "Lab 2", Course = "CS295N" });
        var controller = new AssignmentController(fakeRepo);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Assignment>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Details_ExistingId_ReturnsViewWithAssignment()
    {
        // Arrange
        var fakeRepo = new FakeAssignmentRepository();
        await fakeRepo.AddAsync(new Assignment 
        { 
            AssignmentName = "Lab 1", 
            Course = "CS295N",
            Section = "01",
            Institution = "LCC"
        });
        var controller = new AssignmentController(fakeRepo);

        // Act
        var result = await controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Assignment>(viewResult.Model);
        Assert.Equal("Lab 1", model.AssignmentName);
        Assert.Equal("CS295N", model.Course);
    }

    [Fact]
    public async Task Details_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var fakeRepo = new FakeAssignmentRepository();
        var controller = new AssignmentController(fakeRepo);

        // Act
        var result = await controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidAssignment_RedirectsToIndex()
    {
        // Arrange
        var fakeRepo = new FakeAssignmentRepository();
        var controller = new AssignmentController(fakeRepo);
        var assignment = new Assignment
        {
            AssignmentName = "Lab 1",
            Course = "CS295N",
            Section = "01",
            Institution = "LCC",
            AssignmentNumber = 1
        };

        // Act
        var result = await controller.Create(assignment);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        
        var allAssignments = await fakeRepo.GetAllAsync();
        Assert.Single(allAssignments);
    }

    [Fact]
    public async Task GetByCourse_ReturnsFilteredAssignments()
    {
        // Arrange
        var fakeRepo = new FakeAssignmentRepository();
        await fakeRepo.AddAsync(new Assignment { AssignmentName = "Lab 1", Course = "CS295N" });
        await fakeRepo.AddAsync(new Assignment { AssignmentName = "Lab 2", Course = "CS295N" });
        await fakeRepo.AddAsync(new Assignment { AssignmentName = "Lab 1", Course = "CS133J" });

        // Act
        var result = await fakeRepo.GetByCourseAsync("CS295N");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal("CS295N", a.Course));
    }
}
```

---

## Final Project Structure

```
CodeReviews/
??? Areas/
?   ??? Identity/
?       ??? Pages/              ? Identity UI (Razor Pages)
?           ??? Account/
?               ??? Login.cshtml
?               ??? Register.cshtml
?               ??? ...
??? Controllers/
?   ??? AssignmentController.cs
?   ??? CodeReviewController.cs
?   ??? ReadinessCheckerController.cs
??? Data/
?   ??? ApplicationDbContext.cs
?   ??? ApplicationUser.cs
??? Models/
?   ??? Assignment.cs
?   ??? CodeReview.cs
?   ??? ReadinessChecklistViewModel.cs
?   ??? ReadinessResultViewModel.cs
??? Repositories/
?   ??? IAssignmentRepository.cs
?   ??? AssignmentRepository.cs
?   ??? ICodeReviewRepository.cs
?   ??? CodeReviewRepository.cs
??? Views/
?   ??? Assignment/
?   ??? CodeReview/
?   ??? ReadinessChecker/
?   ??? Shared/
?       ??? _Layout.cshtml
?       ??? _LoginPartial.cshtml
??? Program.cs

CodeReviews.Tests/
??? Fakes/
?   ??? FakeAssignmentRepository.cs
?   ??? FakeCodeReviewRepository.cs
??? Controllers/
    ??? AssignmentControllerTests.cs
    ??? CodeReviewControllerTests.cs
    ??? ReadinessCheckerControllerTests.cs
```

---

## Summary Checklist

### Phase 1: MySQL Setup ?
- [ ] Install Pomelo.EntityFrameworkCore.MySql
- [ ] Install Identity and EF Tools packages
- [ ] Create connection string in appsettings.json

### Phase 2: Identity Setup ?
- [ ] Create ApplicationUser class
- [ ] Create ApplicationDbContext
- [ ] Configure services in Program.cs
- [ ] Scaffold Identity pages
- [ ] Run initial migration
- [ ] Add login partial to layout

### Phase 3: Repository Pattern ?
- [ ] Create domain models (Assignment, CodeReview)
- [ ] Create repository interfaces
- [ ] Implement repositories
- [ ] Update ApplicationDbContext
- [ ] Register repositories in Program.cs
- [ ] Run domain models migration

### Phase 4: Controllers ?
- [ ] Create AssignmentController
- [ ] Create CodeReviewController
- [ ] Add authorization attributes
- [ ] Create corresponding views

### Phase 5: Testing ?
- [ ] Create fake repositories
- [ ] Write controller unit tests
- [ ] Verify all tests pass

---

## Key Benefits of This Architecture

1. **Testability**
   - Controllers can be tested without database
   - Fake repositories are simple and fast
   - No complex mocking frameworks needed

2. **Maintainability**
   - Clear separation of concerns
   - Easy to modify data access logic
   - Repository interfaces document data operations

3. **Identity Integration**
   - Standard ASP.NET Core Identity features
   - Custom user properties for domain needs
   - Secure authentication out of the box

4. **Database Flexibility**
   - Repository pattern abstracts database
   - Could switch from MySQL to PostgreSQL
   - Easy to add caching layer

5. **Educational Value**
   - Teaches industry best practices
   - Demonstrates SOLID principles
   - Shows real-world architecture patterns

---

## Common Issues and Solutions

### Issue: Migration fails with "Table already exists"

**Solution:**
```bash
# Drop database and recreate
dotnet ef database drop
dotnet ef database update
```

### Issue: Identity pages not found (404)

**Solution:** Ensure `app.MapRazorPages()` is in Program.cs after routing middleware.

### Issue: Can't login after registration

**Solution:** Check `RequireConfirmedAccount` setting:
```csharp
options.SignIn.RequireConfirmedAccount = false; // For development
```

### Issue: Repository not injected in controller

**Solution:** Verify repository is registered in Program.cs:
```csharp
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
```

### Issue: Foreign key constraint error

**Solution:** Check OnDelete behavior in ApplicationDbContext:
```csharp
.OnDelete(DeleteBehavior.Restrict); // For user relationships
.OnDelete(DeleteBehavior.Cascade);  // For owned relationships
```

---

## Additional Resources

- [Pomelo MySQL Provider Documentation](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [Repository Pattern in ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/repository-pattern)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [MySQL Connection String Reference](https://www.connectionstrings.com/mysql/)

---

**Document Version:** 1.0  
**Last Updated:** 2026-02-03  
**Author:** GitHub Copilot for CS295N Course

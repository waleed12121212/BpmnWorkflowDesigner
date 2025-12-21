# BPMN Workflow Designer - Technical Architecture Document

## 📋 Executive Summary

This document outlines the complete technical architecture for the **BPMN Workflow Designer**, an enterprise-grade web application built with ASP.NET Blazor and Radzen UI that enables organizations to visually design, edit, store, and manage BPMN 2.0 business process diagrams.

---

## 🏗️ 1. System Architecture Overview

### 1.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Browser                          │
│  ┌───────────────────────────────────────────────────────┐  │
│  │         Blazor WebAssembly / Server App               │  │
│  │  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  │  │
│  │  │   Radzen    │  │   bpmn-js    │  │   Auth      │  │  │
│  │  │   UI        │  │   Modeler    │  │   Module    │  │  │
│  │  └─────────────┘  └──────────────┘  └─────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ HTTPS / JWT
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   ASP.NET Core Web API                      │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Controllers │ Services │ Repositories │ Middleware   │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ EF Core
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              SQL Server / PostgreSQL Database               │
│     Tables: Users, Roles, Workflows, Departments, Logs      │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Technology Stack

#### Frontend
- **Framework**: Blazor WebAssembly (recommended for scalability) or Blazor Server
- **UI Library**: Radzen Blazor Components
- **BPMN Editor**: bpmn-js (via JavaScript Interop)
- **State Management**: Fluxor (optional) or built-in Blazor state
- **HTTP Client**: HttpClient with JWT token injection

#### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **Authentication**: JWT Bearer Tokens
- **ORM**: Entity Framework Core 8.0
- **Validation**: FluentValidation
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI

#### Database
- **Primary**: SQL Server 2019+ or PostgreSQL 14+
- **Caching**: Redis (optional for session/token management)

#### DevOps
- **Containerization**: Docker
- **CI/CD**: GitHub Actions / Azure DevOps
- **Hosting**: Azure App Service / IIS / Linux containers

---

## 🎨 2. Frontend Architecture (Blazor + Radzen)

### 2.1 Blazor Application Type Decision

**Recommendation: Blazor WebAssembly (WASM)**

**Rationale:**
- Better scalability (client-side rendering)
- Reduced server load
- Offline capabilities (future enhancement)
- Better user experience for diagram editing (no SignalR latency)

**Trade-offs:**
- Larger initial download size
- Requires API for all data operations

### 2.2 Frontend Components Structure

```
Client/
├── Pages/
│   ├── Index.razor                    # Landing page
│   ├── Login.razor                    # Authentication
│   ├── Dashboard.razor                # User dashboard
│   ├── WorkflowList.razor             # List all workflows
│   ├── WorkflowEditor.razor           # Main BPMN editor page
│   └── Admin/
│       ├── UserManagement.razor
│       └── RoleManagement.razor
├── Components/
│   ├── BpmnModeler.razor              # BPMN editor wrapper
│   ├── WorkflowCard.razor             # Workflow display card
│   ├── ToolbarActions.razor           # Editor toolbar
│   ├── PropertiesPanel.razor          # BPMN element properties
│   └── Shared/
│       ├── MainLayout.razor
│       ├── NavMenu.razor
│       └── AuthorizeView.razor
├── Services/
│   ├── IWorkflowService.cs
│   ├── WorkflowService.cs             # API calls for workflows
│   ├── IAuthService.cs
│   ├── AuthService.cs                 # Authentication logic
│   └── BpmnInteropService.cs          # JS Interop for bpmn-js
├── Models/
│   ├── WorkflowDto.cs
│   ├── UserDto.cs
│   ├── LoginRequest.cs
│   └── ApiResponse.cs
├── wwwroot/
│   ├── js/
│   │   ├── bpmn-modeler.js            # bpmn-js initialization
│   │   └── interop.js                 # JS helper functions
│   ├── css/
│   │   └── app.css                    # Custom styles
│   └── index.html
└── Program.cs
```

### 2.3 BPMN Editor Integration (bpmn-js)

#### 2.3.1 JavaScript Interop Setup

**File: wwwroot/js/bpmn-modeler.js**

```javascript
// Global modeler instance
let bpmnModeler = null;

window.BpmnModeler = {
    initialize: function(containerId) {
        const container = document.getElementById(containerId);
        
        bpmnModeler = new BpmnJS({
            container: container,
            keyboard: {
                bindTo: document
            },
            additionalModules: [
                // Add custom modules if needed
            ]
        });
        
        return true;
    },
    
    importXML: async function(xml) {
        try {
            await bpmnModeler.importXML(xml);
            return { success: true };
        } catch (err) {
            return { success: false, error: err.message };
        }
    },
    
    exportXML: async function() {
        try {
            const { xml } = await bpmnModeler.saveXML({ format: true });
            return { success: true, xml: xml };
        } catch (err) {
            return { success: false, error: err.message };
        }
    },
    
    exportSVG: async function() {
        try {
            const { svg } = await bpmnModeler.saveSVG();
            return { success: true, svg: svg };
        } catch (err) {
            return { success: false, error: err.message };
        }
    },
    
    createNewDiagram: async function() {
        const emptyDiagram = `<?xml version="1.0" encoding="UTF-8"?>
            <bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
                              xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"
                              xmlns:dc="http://www.omg.org/spec/DD/20100524/DC"
                              id="Definitions_1" targetNamespace="http://bpmn.io/schema/bpmn">
                <bpmn:process id="Process_1" isExecutable="false">
                    <bpmn:startEvent id="StartEvent_1"/>
                </bpmn:process>
                <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_1"/>
                </bpmndi:BPMNDiagram>
            </bpmn:definitions>`;
        
        return await this.importXML(emptyDiagram);
    },
    
    destroy: function() {
        if (bpmnModeler) {
            bpmnModeler.destroy();
            bpmnModeler = null;
        }
    }
};
```

#### 2.3.2 Blazor Component Wrapper

**File: Components/BpmnModeler.razor**

```razor
@inject IJSRuntime JSRuntime
@implements IAsyncDisposable

<div class="bpmn-editor-container">
    <div class="toolbar">
        <RadzenButton Icon="save" Text="Save" Click="@SaveWorkflow" ButtonStyle="ButtonStyle.Primary" />
        <RadzenButton Icon="download" Text="Export XML" Click="@ExportXML" ButtonStyle="ButtonStyle.Secondary" />
        <RadzenButton Icon="image" Text="Export SVG" Click="@ExportSVG" ButtonStyle="ButtonStyle.Secondary" />
        <RadzenButton Icon="undo" Text="Undo" Click="@Undo" ButtonStyle="ButtonStyle.Light" />
        <RadzenButton Icon="redo" Text="Redo" Click="@Redo" ButtonStyle="ButtonStyle.Light" />
    </div>
    
    <div id="@ContainerId" class="bpmn-canvas"></div>
</div>

@code {
    [Parameter] public string WorkflowId { get; set; }
    [Parameter] public EventCallback<string> OnSave { get; set; }
    
    private string ContainerId = $"bpmn-container-{Guid.NewGuid()}";
    private IJSObjectReference? moduleReference;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            moduleReference = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/bpmn-modeler.js");
            
            await JSRuntime.InvokeVoidAsync("BpmnModeler.initialize", ContainerId);
            
            if (string.IsNullOrEmpty(WorkflowId))
            {
                await JSRuntime.InvokeVoidAsync("BpmnModeler.createNewDiagram");
            }
            else
            {
                // Load existing workflow
                await LoadWorkflow(WorkflowId);
            }
        }
    }
    
    private async Task SaveWorkflow()
    {
        var result = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportXML");
        if (result.Success)
        {
            await OnSave.InvokeAsync(result.Xml);
        }
    }
    
    private async Task ExportXML()
    {
        var result = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportXML");
        if (result.Success)
        {
            // Trigger download
            await JSRuntime.InvokeVoidAsync("downloadFile", "workflow.bpmn", result.Xml, "application/xml");
        }
    }
    
    private async Task ExportSVG()
    {
        var result = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportSVG");
        if (result.Success)
        {
            await JSRuntime.InvokeVoidAsync("downloadFile", "workflow.svg", result.Svg, "image/svg+xml");
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (moduleReference != null)
        {
            await JSRuntime.InvokeVoidAsync("BpmnModeler.destroy");
            await moduleReference.DisposeAsync();
        }
    }
    
    private class ExportResult
    {
        public bool Success { get; set; }
        public string Xml { get; set; }
        public string Svg { get; set; }
        public string Error { get; set; }
    }
}
```

### 2.4 Radzen UI Integration

#### Key Components Usage:

1. **RadzenCard**: Workflow cards in dashboard
2. **RadzenDataGrid**: Workflow list with filtering/sorting
3. **RadzenDialog**: Confirmation dialogs, properties editing
4. **RadzenButton**: All action buttons
5. **RadzenDropDown**: Role selection, department selection
6. **RadzenTextBox**: Search, naming workflows
7. **RadzenNotification**: Success/error messages
8. **RadzenSplitter**: Editor layout (canvas + properties panel)

---

## 🔧 3. Backend Architecture (ASP.NET Core Web API)

### 3.1 Clean Architecture Layers

```
Server/
├── BpmnWorkflow.API/                  # Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── WorkflowsController.cs
│   │   ├── UsersController.cs
│   │   └── DepartmentsController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── JwtMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── BpmnWorkflow.Application/          # Application Layer
│   ├── DTOs/
│   │   ├── WorkflowDto.cs
│   │   ├── CreateWorkflowRequest.cs
│   │   ├── UpdateWorkflowRequest.cs
│   │   └── UserDto.cs
│   ├── Interfaces/
│   │   ├── IWorkflowService.cs
│   │   ├── IAuthService.cs
│   │   └── IUserService.cs
│   ├── Services/
│   │   ├── WorkflowService.cs
│   │   ├── AuthService.cs
│   │   └── UserService.cs
│   ├── Validators/
│   │   └── CreateWorkflowValidator.cs
│   └── Mappings/
│       └── AutoMapperProfile.cs
│
├── BpmnWorkflow.Domain/               # Domain Layer
│   ├── Entities/
│   │   ├── Workflow.cs
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Department.cs
│   │   └── AuditLog.cs
│   ├── Enums/
│   │   └── UserRole.cs
│   └── Interfaces/
│       ├── IWorkflowRepository.cs
│       └── IUserRepository.cs
│
└── BpmnWorkflow.Infrastructure/       # Infrastructure Layer
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   └── Migrations/
    ├── Repositories/
    │   ├── WorkflowRepository.cs
    │   └── UserRepository.cs
    └── Security/
        ├── JwtTokenGenerator.cs
        └── PasswordHasher.cs
```

### 3.2 Domain Models

#### 3.2.1 Workflow Entity

```csharp
namespace BpmnWorkflow.Domain.Entities
{
    public class Workflow
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BpmnXml { get; set; }  // BPMN 2.0 XML content
        public string SvgPreview { get; set; }  // SVG for thumbnails
        
        // Metadata
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
        
        public Guid? DepartmentId { get; set; }
        public Department Department { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        
        // Version control
        public int Version { get; set; }
        public bool IsPublished { get; set; }
        
        // Tags for categorization
        public string Tags { get; set; }  // JSON array or comma-separated
        
        // Soft delete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
```

#### 3.2.2 User Entity

```csharp
namespace BpmnWorkflow.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        
        public Guid? DepartmentId { get; set; }
        public Department Department { get; set; }
        
        public ICollection<Workflow> Workflows { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; }
    }
}
```

#### 3.2.3 Role Entity

```csharp
namespace BpmnWorkflow.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }  // Designer, Viewer, Admin
        public string Description { get; set; }
        
        // Permissions
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanView { get; set; }
        public bool CanPublish { get; set; }
        public bool CanManageUsers { get; set; }
        
        public ICollection<User> Users { get; set; }
    }
}
```

#### 3.2.4 Department Entity

```csharp
namespace BpmnWorkflow.Domain.Entities
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        
        public ICollection<User> Users { get; set; }
        public ICollection<Workflow> Workflows { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
```

#### 3.2.5 Audit Log Entity

```csharp
namespace BpmnWorkflow.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; }  // Created, Updated, Deleted, Viewed
        public string EntityType { get; set; }  // Workflow, User, etc.
        public Guid EntityId { get; set; }
        public string Changes { get; set; }  // JSON of changes
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
    }
}
```

### 3.3 Database Schema

```sql
-- Users Table
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FullName NVARCHAR(200),
    RoleId UNIQUEIDENTIFIER NOT NULL,
    DepartmentId UNIQUEIDENTIFIER,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2,
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_Users_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);

-- Roles Table
CREATE TABLE Roles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    CanCreate BIT DEFAULT 0,
    CanEdit BIT DEFAULT 0,
    CanDelete BIT DEFAULT 0,
    CanView BIT DEFAULT 1,
    CanPublish BIT DEFAULT 0,
    CanManageUsers BIT DEFAULT 0
);

-- Departments Table
CREATE TABLE Departments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(50) UNIQUE,
    Description NVARCHAR(1000),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1
);

-- Workflows Table
CREATE TABLE Workflows (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(300) NOT NULL,
    Description NVARCHAR(2000),
    BpmnXml NVARCHAR(MAX) NOT NULL,
    SvgPreview NVARCHAR(MAX),
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    DepartmentId UNIQUEIDENTIFIER,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100),
    UpdatedBy NVARCHAR(100),
    Version INT DEFAULT 1,
    IsPublished BIT DEFAULT 0,
    Tags NVARCHAR(500),
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME2,
    CONSTRAINT FK_Workflows_Users FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    CONSTRAINT FK_Workflows_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);

-- AuditLogs Table
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    Changes NVARCHAR(MAX),
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(50),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Indexes for performance
CREATE INDEX IX_Workflows_OwnerId ON Workflows(OwnerId);
CREATE INDEX IX_Workflows_DepartmentId ON Workflows(DepartmentId);
CREATE INDEX IX_Workflows_IsDeleted ON Workflows(IsDeleted);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
```

---

## 🔌 4. API Endpoints Design

### 4.1 Authentication Endpoints

```
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh-token
POST   /api/auth/logout
GET    /api/auth/me
```

### 4.2 Workflow Endpoints

```
GET    /api/workflows                    # List all workflows (with filtering)
GET    /api/workflows/{id}               # Get workflow by ID
POST   /api/workflows                    # Create new workflow
PUT    /api/workflows/{id}               # Update workflow
DELETE /api/workflows/{id}               # Soft delete workflow
GET    /api/workflows/{id}/xml           # Get BPMN XML
GET    /api/workflows/{id}/svg           # Get SVG preview
POST   /api/workflows/{id}/publish       # Publish workflow
GET    /api/workflows/{id}/versions      # Get version history
```

### 4.3 User Management Endpoints

```
GET    /api/users                        # List users (Admin only)
GET    /api/users/{id}                   # Get user details
POST   /api/users                        # Create user (Admin only)
PUT    /api/users/{id}                   # Update user
DELETE /api/users/{id}                   # Deactivate user
```

### 4.4 Department Endpoints

```
GET    /api/departments                  # List departments
GET    /api/departments/{id}             # Get department
POST   /api/departments                  # Create department
PUT    /api/departments/{id}             # Update department
```

### 4.5 Example Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(IWorkflowService workflowService, ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetWorkflows(
        [FromQuery] string search = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool includeDeleted = false)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var workflows = await _workflowService.GetWorkflowsAsync(userId, search, departmentId, includeDeleted);
        return Ok(workflows);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var workflow = await _workflowService.GetWorkflowByIdAsync(id, userId);
        
        if (workflow == null)
            return NotFound();
            
        return Ok(workflow);
    }

    [HttpPost]
    [Authorize(Policy = "CanCreate")]
    public async Task<ActionResult<WorkflowDto>> CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var workflow = await _workflowService.CreateWorkflowAsync(request, userId);
        
        return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<WorkflowDto>> UpdateWorkflow(Guid id, [FromBody] UpdateWorkflowRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var workflow = await _workflowService.UpdateWorkflowAsync(id, request, userId);
        
        if (workflow == null)
            return NotFound();
            
        return Ok(workflow);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> DeleteWorkflow(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var success = await _workflowService.DeleteWorkflowAsync(id, userId);
        
        if (!success)
            return NotFound();
            
        return NoContent();
    }

    [HttpGet("{id}/xml")]
    public async Task<ActionResult> GetWorkflowXml(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var xml = await _workflowService.GetWorkflowXmlAsync(id, userId);
        
        if (xml == null)
            return NotFound();
            
        return Content(xml, "application/xml");
    }
}
```

---

## 🔐 5. Authentication & Authorization

### 5.1 JWT Token Structure

```json
{
  "sub": "user-guid",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Designer",
  "permissions": ["CanCreate", "CanEdit", "CanView"],
  "departmentId": "dept-guid",
  "exp": 1234567890,
  "iss": "BpmnWorkflowDesigner",
  "aud": "BpmnWorkflowDesigner"
}
```

### 5.2 Role-Based Access Control (RBAC)

#### Predefined Roles:

1. **Viewer**
   - CanView: ✓
   - CanCreate: ✗
   - CanEdit: ✗
   - CanDelete: ✗
   - CanPublish: ✗
   - CanManageUsers: ✗

2. **Designer**
   - CanView: ✓
   - CanCreate: ✓
   - CanEdit: ✓ (own workflows)
   - CanDelete: ✓ (own workflows)
   - CanPublish: ✗
   - CanManageUsers: ✗

3. **Admin**
   - CanView: ✓
   - CanCreate: ✓
   - CanEdit: ✓ (all workflows)
   - CanDelete: ✓ (all workflows)
   - CanPublish: ✓
   - CanManageUsers: ✓

### 5.3 Authorization Policies

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanCreate", policy => 
        policy.RequireClaim("permissions", "CanCreate"));
    
    options.AddPolicy("CanEdit", policy => 
        policy.RequireClaim("permissions", "CanEdit"));
    
    options.AddPolicy("CanDelete", policy => 
        policy.RequireClaim("permissions", "CanDelete"));
    
    options.AddPolicy("CanPublish", policy => 
        policy.RequireClaim("permissions", "CanPublish"));
    
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
});
```

### 5.4 JWT Configuration

```csharp
// appsettings.json
{
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key-here-minimum-32-characters",
    "Issuer": "BpmnWorkflowDesigner",
    "Audience": "BpmnWorkflowDesigner",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}

// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });
```

---

## 🔄 6. BPMN Editor Lifecycle

### 6.1 Editor Initialization Flow

```
1. User navigates to /workflow/new or /workflow/{id}
   ↓
2. WorkflowEditor.razor component loads
   ↓
3. OnAfterRenderAsync triggers
   ↓
4. JS Interop initializes bpmn-js modeler
   ↓
5. If new: Load empty BPMN template
   If existing: Fetch XML from API → Import into modeler
   ↓
6. User edits diagram visually
   ↓
7. On Save: Export XML → Send to API → Store in database
```

### 6.2 Save Workflow Process

```csharp
// Client-side (Blazor)
private async Task SaveWorkflow()
{
    var exportResult = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportXML");
    
    if (!exportResult.Success)
    {
        NotificationService.Notify(NotificationSeverity.Error, "Export failed");
        return;
    }
    
    var svgResult = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportSVG");
    
    var request = new UpdateWorkflowRequest
    {
        Name = workflowName,
        Description = workflowDescription,
        BpmnXml = exportResult.Xml,
        SvgPreview = svgResult.Svg,
        Tags = selectedTags
    };
    
    var response = await WorkflowService.UpdateWorkflowAsync(workflowId, request);
    
    if (response.IsSuccess)
    {
        NotificationService.Notify(NotificationSeverity.Success, "Workflow saved successfully");
    }
}
```

### 6.3 Load Workflow Process

```csharp
private async Task LoadWorkflow(string workflowId)
{
    var workflow = await WorkflowService.GetWorkflowByIdAsync(workflowId);
    
    if (workflow != null)
    {
        workflowName = workflow.Name;
        workflowDescription = workflow.Description;
        
        var importResult = await JSRuntime.InvokeAsync<ImportResult>(
            "BpmnModeler.importXML", 
            workflow.BpmnXml
        );
        
        if (!importResult.Success)
        {
            NotificationService.Notify(NotificationSeverity.Error, 
                $"Failed to load workflow: {importResult.Error}");
        }
    }
}
```

---

## 📁 7. Complete Folder Structure

### 7.1 Frontend (Blazor WASM)

```
BpmnWorkflow.Client/
├── wwwroot/
│   ├── index.html
│   ├── css/
│   │   ├── app.css
│   │   ├── bpmn-editor.css
│   │   └── radzen-overrides.css
│   ├── js/
│   │   ├── bpmn-modeler.js
│   │   ├── interop.js
│   │   └── file-download.js
│   ├── lib/
│   │   ├── bpmn-js/
│   │   │   ├── bpmn-modeler.development.js
│   │   │   └── bpmn-modeler.production.min.js
│   │   └── diagram-js.css
│   └── favicon.ico
├── Pages/
│   ├── Index.razor
│   ├── Login.razor
│   ├── Register.razor
│   ├── Dashboard.razor
│   ├── Workflows/
│   │   ├── WorkflowList.razor
│   │   ├── WorkflowEditor.razor
│   │   ├── WorkflowDetails.razor
│   │   └── WorkflowHistory.razor
│   └── Admin/
│       ├── UserManagement.razor
│       ├── RoleManagement.razor
│       └── DepartmentManagement.razor
├── Components/
│   ├── BpmnModeler.razor
│   ├── WorkflowCard.razor
│   ├── WorkflowToolbar.razor
│   ├── PropertiesPanel.razor
│   ├── SearchBar.razor
│   └── Shared/
│       ├── MainLayout.razor
│       ├── NavMenu.razor
│       ├── LoginDisplay.razor
│       └── LoadingSpinner.razor
├── Services/
│   ├── Interfaces/
│   │   ├── IWorkflowService.cs
│   │   ├── IAuthService.cs
│   │   ├── IUserService.cs
│   │   └── IBpmnInteropService.cs
│   └── Implementations/
│       ├── WorkflowService.cs
│       ├── AuthService.cs
│       ├── UserService.cs
│       └── BpmnInteropService.cs
├── Models/
│   ├── DTOs/
│   │   ├── WorkflowDto.cs
│   │   ├── UserDto.cs
│   │   ├── DepartmentDto.cs
│   │   └── RoleDto.cs
│   ├── Requests/
│   │   ├── LoginRequest.cs
│   │   ├── CreateWorkflowRequest.cs
│   │   └── UpdateWorkflowRequest.cs
│   └── Responses/
│       ├── ApiResponse.cs
│       └── AuthResponse.cs
├── Helpers/
│   ├── AuthenticationStateProvider.cs
│   ├── HttpInterceptor.cs
│   └── LocalStorageService.cs
├── Program.cs
└── _Imports.razor
```

### 7.2 Backend (ASP.NET Core)

```
BpmnWorkflow.Server/
├── BpmnWorkflow.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── WorkflowsController.cs
│   │   ├── UsersController.cs
│   │   ├── DepartmentsController.cs
│   │   └── AuditController.cs
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   ├── RequestLoggingMiddleware.cs
│   │   └── JwtMiddleware.cs
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── ApplicationBuilderExtensions.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── BpmnWorkflow.Application/
│   ├── DTOs/
│   │   ├── WorkflowDto.cs
│   │   ├── CreateWorkflowRequest.cs
│   │   ├── UpdateWorkflowRequest.cs
│   │   ├── UserDto.cs
│   │   └── DepartmentDto.cs
│   ├── Interfaces/
│   │   ├── IWorkflowService.cs
│   │   ├── IAuthService.cs
│   │   ├── IUserService.cs
│   │   └── IAuditService.cs
│   ├── Services/
│   │   ├── WorkflowService.cs
│   │   ├── AuthService.cs
│   │   ├── UserService.cs
│   │   └── AuditService.cs
│   ├── Validators/
│   │   ├── CreateWorkflowValidator.cs
│   │   ├── UpdateWorkflowValidator.cs
│   │   └── LoginRequestValidator.cs
│   ├── Mappings/
│   │   └── AutoMapperProfile.cs
│   └── Exceptions/
│       ├── NotFoundException.cs
│       ├── UnauthorizedException.cs
│       └── ValidationException.cs
│
├── BpmnWorkflow.Domain/
│   ├── Entities/
│   │   ├── Workflow.cs
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Department.cs
│   │   └── AuditLog.cs
│   ├── Enums/
│   │   ├── UserRole.cs
│   │   └── AuditAction.cs
│   ├── Interfaces/
│   │   ├── IWorkflowRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── IDepartmentRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Common/
│       └── BaseEntity.cs
│
└── BpmnWorkflow.Infrastructure/
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   ├── DbInitializer.cs
    │   └── Migrations/
    ├── Repositories/
    │   ├── WorkflowRepository.cs
    │   ├── UserRepository.cs
    │   ├── DepartmentRepository.cs
    │   └── UnitOfWork.cs
    ├── Security/
    │   ├── JwtTokenGenerator.cs
    │   ├── PasswordHasher.cs
    │   └── PermissionHandler.cs
    └── Configurations/
        ├── WorkflowConfiguration.cs
        ├── UserConfiguration.cs
        └── RoleConfiguration.cs
```

---

## 🛡️ 8. Security Considerations

### 8.1 Authentication Security

1. **Password Security**
   - Use BCrypt or PBKDF2 for password hashing
   - Minimum password requirements: 8 characters, uppercase, lowercase, number, special char
   - Implement account lockout after failed attempts

2. **JWT Security**
   - Short-lived access tokens (60 minutes)
   - Refresh tokens stored securely (HttpOnly cookies or secure storage)
   - Token rotation on refresh
   - Blacklist revoked tokens (Redis cache)

3. **HTTPS Only**
   - Enforce HTTPS in production
   - HSTS headers enabled
   - Secure cookie flags

### 8.2 Authorization Security

1. **Resource-Based Authorization**
   - Users can only edit/delete their own workflows (unless Admin)
   - Department-based access control
   - Ownership verification in service layer

2. **API Security**
   - CORS configuration for specific origins
   - Rate limiting (e.g., 100 requests/minute per user)
   - Input validation and sanitization
   - SQL injection prevention (EF Core parameterized queries)

### 8.3 Data Security

1. **XML Validation**
   - Validate BPMN XML against BPMN 2.0 schema
   - Sanitize XML to prevent XXE attacks
   - Size limits on uploaded XML (e.g., 5MB max)

2. **Audit Logging**
   - Log all CRUD operations
   - Track user actions with timestamps
   - IP address logging
   - Retention policy (e.g., 1 year)

### 8.4 Example Security Implementation

```csharp
// Password Hashing
public class PasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

// Resource Authorization
public class WorkflowAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Workflow>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Workflow resource)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var isAdmin = context.User.IsInRole("Admin");
        
        if (requirement.Name == "Edit" || requirement.Name == "Delete")
        {
            if (resource.OwnerId == userId || isAdmin)
            {
                context.Succeed(requirement);
            }
        }
        
        return Task.CompletedTask;
    }
}
```

---

## ⚡ 9. Performance Optimization

### 9.1 Frontend Performance

1. **Lazy Loading**
   - Load bpmn-js only on editor pages
   - Lazy load Radzen components
   - Code splitting for routes

2. **Caching**
   - Cache workflow list in memory
   - LocalStorage for user preferences
   - Service Worker for offline support (future)

3. **Optimization**
   - Minimize JS bundle size
   - Use production build of bpmn-js
   - Compress SVG previews
   - Debounce auto-save operations

### 9.2 Backend Performance

1. **Database Optimization**
   - Proper indexing on foreign keys
   - Pagination for list queries
   - Use projections (select only needed fields)
   - Connection pooling

2. **Caching Strategy**
   - Redis for frequently accessed data
   - Cache user permissions
   - Cache department lists
   - Invalidate cache on updates

3. **Query Optimization**
```csharp
// Good: Use projection
public async Task<IEnumerable<WorkflowDto>> GetWorkflowsAsync()
{
    return await _context.Workflows
        .Where(w => !w.IsDeleted)
        .Select(w => new WorkflowDto
        {
            Id = w.Id,
            Name = w.Name,
            Description = w.Description,
            // Don't load BpmnXml here (large field)
            OwnerName = w.Owner.FullName,
            DepartmentName = w.Department.Name
        })
        .ToListAsync();
}

// Bad: Load entire entities
public async Task<IEnumerable<Workflow>> GetWorkflowsAsync()
{
    return await _context.Workflows
        .Include(w => w.Owner)
        .Include(w => w.Department)
        .ToListAsync(); // Loads all data including large XML
}
```

### 9.3 Scalability Considerations

1. **Horizontal Scaling**
   - Stateless API design
   - Load balancer ready
   - Distributed caching (Redis)

2. **Database Scaling**
   - Read replicas for queries
   - Partitioning by department
   - Archive old workflows

3. **File Storage**
   - Consider blob storage for large diagrams
   - CDN for static assets

---

## 🚀 10. Deployment Strategy

### 10.1 Development Environment

```yaml
# docker-compose.yml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  api:
    build:
      context: ./Server
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=BpmnWorkflow;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
      - Redis__ConnectionString=redis:6379
    depends_on:
      - sqlserver
      - redis

  client:
    build:
      context: ./Client
      dockerfile: Dockerfile
    ports:
      - "5001:80"
    environment:
      - API_URL=http://api:80

volumes:
  sqldata:
```

### 10.2 Production Deployment (Azure)

1. **Azure App Service** for API
2. **Azure Static Web Apps** for Blazor WASM
3. **Azure SQL Database** for data
4. **Azure Redis Cache** for caching
5. **Azure Application Insights** for monitoring

### 10.3 CI/CD Pipeline

```yaml
# .github/workflows/deploy.yml
name: Deploy BPMN Workflow Designer

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish API
      run: dotnet publish Server/BpmnWorkflow.API -c Release -o ./api-publish
    
    - name: Publish Client
      run: dotnet publish Client/BpmnWorkflow.Client -c Release -o ./client-publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: bpmn-workflow-api
        package: ./api-publish
```

---

## 📊 11. Monitoring & Logging

### 11.1 Logging Strategy

```csharp
// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File("logs/bpmn-workflow-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
    .CreateLogger();
```

### 11.2 Metrics to Track

1. **Application Metrics**
   - API response times
   - Workflow save/load times
   - User login frequency
   - Error rates

2. **Business Metrics**
   - Number of workflows created per day
   - Active users
   - Most used BPMN elements
   - Department usage statistics

---

## 🧪 12. Testing Strategy

### 12.1 Unit Tests

```csharp
public class WorkflowServiceTests
{
    [Fact]
    public async Task CreateWorkflow_ValidRequest_ReturnsWorkflowDto()
    {
        // Arrange
        var mockRepo = new Mock<IWorkflowRepository>();
        var service = new WorkflowService(mockRepo.Object);
        var request = new CreateWorkflowRequest { Name = "Test Workflow" };
        
        // Act
        var result = await service.CreateWorkflowAsync(request, Guid.NewGuid());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Workflow", result.Name);
    }
}
```

### 12.2 Integration Tests

```csharp
public class WorkflowsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    [Fact]
    public async Task GetWorkflows_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/workflows");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

### 12.3 E2E Tests (Playwright/Selenium)

- Test complete workflow creation flow
- Test login/logout
- Test permissions enforcement

---

## 📝 13. Implementation Roadmap

### Phase 1: Foundation (Weeks 1-2)
- [ ] Setup project structure
- [ ] Configure database and migrations
- [ ] Implement authentication system
- [ ] Create basic API endpoints

### Phase 2: Core Features (Weeks 3-4)
- [ ] Integrate bpmn-js in Blazor
- [ ] Implement workflow CRUD operations
- [ ] Build workflow editor page
- [ ] Add save/load functionality

### Phase 3: UI/UX (Weeks 5-6)
- [ ] Design with Radzen components
- [ ] Create dashboard
- [ ] Implement workflow list with search/filter
- [ ] Add export functionality (XML/SVG)

### Phase 4: Advanced Features (Weeks 7-8)
- [ ] Role-based access control
- [ ] Audit logging
- [ ] Version history
- [ ] Department management

### Phase 5: Polish & Deploy (Weeks 9-10)
- [ ] Performance optimization
- [ ] Security hardening
- [ ] Comprehensive testing
- [ ] Documentation
- [ ] Production deployment

---

## ✅ 14. BPMN 2.0 Compliance Checklist

- [x] Support for Start Events
- [x] Support for End Events
- [x] Support for Tasks (User, Service, Script)
- [x] Support for Gateways (Exclusive, Parallel, Inclusive)
- [x] Support for Sequence Flows
- [x] Support for Pools and Lanes
- [x] Valid BPMN 2.0 XML export
- [x] XML schema validation
- [x] Proper namespace declarations

---

## 🎯 15. Success Criteria

1. **Functional**
   - Users can create, edit, save, and load BPMN diagrams
   - Role-based permissions work correctly
   - Export to XML and SVG functions properly

2. **Performance**
   - Page load time < 2 seconds
   - Diagram save time < 1 second
   - API response time < 200ms (95th percentile)

3. **Security**
   - No critical vulnerabilities (OWASP Top 10)
   - All endpoints properly authenticated
   - Audit logs capture all actions

4. **Usability**
   - Non-technical users can create simple workflows
   - Intuitive UI with minimal training
   - Responsive design (desktop and tablet)

---

## 📚 16. References & Resources

- **BPMN 2.0 Specification**: https://www.omg.org/spec/BPMN/2.0/
- **bpmn-js Documentation**: https://bpmn.io/toolkit/bpmn-js/
- **Blazor Documentation**: https://learn.microsoft.com/en-us/aspnet/core/blazor/
- **Radzen Blazor Components**: https://blazor.radzen.com/
- **Clean Architecture**: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html

---

**Document Version**: 1.0  
**Last Updated**: 2025-12-18  
**Author**: Senior Software Architect  
**Status**: Ready for Implementation

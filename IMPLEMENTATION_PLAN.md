# BPMN Workflow Designer - Implementation Plan

## 📋 Overview

This document provides a step-by-step implementation plan for building the BPMN Workflow Designer application. Each phase includes specific tasks, deliverables, and acceptance criteria.

---

## 🎯 Project Phases

### Phase 1: Project Setup & Foundation (Week 1-2)

#### 1.1 Development Environment Setup

**Tasks:**
- [ ] Install prerequisites (.NET 8 SDK, Node.js, SQL Server/PostgreSQL)
- [ ] Setup Git repository with proper .gitignore
- [ ] Create solution structure with projects
- [ ] Configure Docker development environment
- [ ] Setup IDE (Visual Studio / VS Code / Rider)

**Deliverables:**
- Working development environment
- Docker Compose configuration
- README.md with setup instructions

**Commands:**
```bash
# Create solution
dotnet new sln -n BpmnWorkflowDesigner

# Create projects
dotnet new webapi -n BpmnWorkflow.API -o Server/BpmnWorkflow.API
dotnet new classlib -n BpmnWorkflow.Application -o Server/BpmnWorkflow.Application
dotnet new classlib -n BpmnWorkflow.Domain -o Server/BpmnWorkflow.Domain
dotnet new classlib -n BpmnWorkflow.Infrastructure -o Server/BpmnWorkflow.Infrastructure
dotnet new blazorwasm -n BpmnWorkflow.Client -o Client/BpmnWorkflow.Client

# Add projects to solution
dotnet sln add Server/BpmnWorkflow.API/BpmnWorkflow.API.csproj
dotnet sln add Server/BpmnWorkflow.Application/BpmnWorkflow.Application.csproj
dotnet sln add Server/BpmnWorkflow.Domain/BpmnWorkflow.Domain.csproj
dotnet sln add Server/BpmnWorkflow.Infrastructure/BpmnWorkflow.Infrastructure.csproj
dotnet sln add Client/BpmnWorkflow.Client/BpmnWorkflow.Client.csproj

# Add project references
dotnet add Server/BpmnWorkflow.API reference Server/BpmnWorkflow.Application
dotnet add Server/BpmnWorkflow.Application reference Server/BpmnWorkflow.Domain
dotnet add Server/BpmnWorkflow.Infrastructure reference Server/BpmnWorkflow.Domain
```

#### 1.2 Install NuGet Packages

**Backend Packages:**
```bash
# API Project
cd Server/BpmnWorkflow.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Application Project
cd ../BpmnWorkflow.Application
dotnet add package AutoMapper
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

# Infrastructure Project
cd ../BpmnWorkflow.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package BCrypt.Net-Next
dotnet add package StackExchange.Redis

# Domain Project (no external dependencies initially)
```

**Frontend Packages:**
```bash
cd ../../Client/BpmnWorkflow.Client
dotnet add package Radzen.Blazor
dotnet add package Microsoft.AspNetCore.Components.WebAssembly.Authentication
dotnet add package Blazored.LocalStorage
```

#### 1.3 Database Setup

**Tasks:**
- [ ] Create database schema design
- [ ] Setup Entity Framework Core
- [ ] Create domain entities
- [ ] Configure entity relationships
- [ ] Create initial migration
- [ ] Seed initial data (roles, admin user)

**Deliverables:**
- Complete domain model
- EF Core DbContext
- Initial migration
- Database seeding script

**Key Files to Create:**
```
Server/BpmnWorkflow.Domain/Entities/
├── Workflow.cs
├── User.cs
├── Role.cs
├── Department.cs
└── AuditLog.cs

Server/BpmnWorkflow.Infrastructure/Data/
├── ApplicationDbContext.cs
├── DbInitializer.cs
└── Configurations/
    ├── WorkflowConfiguration.cs
    ├── UserConfiguration.cs
    ├── RoleConfiguration.cs
    └── DepartmentConfiguration.cs
```

**Migration Commands:**
```bash
cd Server/BpmnWorkflow.API
dotnet ef migrations add InitialCreate --project ../BpmnWorkflow.Infrastructure
dotnet ef database update
```

---

### Phase 2: Authentication & Authorization (Week 2-3)

#### 2.1 JWT Authentication Implementation

**Tasks:**
- [ ] Create JWT token generator service
- [ ] Implement password hashing service
- [ ] Create authentication service
- [ ] Build login/register endpoints
- [ ] Configure JWT middleware
- [ ] Implement refresh token mechanism

**Deliverables:**
- Working authentication system
- Login/Register API endpoints
- JWT token generation and validation

**Key Files:**
```
Server/BpmnWorkflow.Infrastructure/Security/
├── JwtTokenGenerator.cs
├── PasswordHasher.cs
└── RefreshTokenService.cs

Server/BpmnWorkflow.Application/Services/
└── AuthService.cs

Server/BpmnWorkflow.API/Controllers/
└── AuthController.cs
```

**Example Implementation:**

```csharp
// JwtTokenGenerator.cs
public class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    
    public string GenerateToken(User user, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };
        
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"])),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### 2.2 Role-Based Authorization

**Tasks:**
- [ ] Define authorization policies
- [ ] Create permission constants
- [ ] Implement authorization handlers
- [ ] Add role seeding
- [ ] Test authorization rules

**Deliverables:**
- Authorization policies configured
- Role-based access control working
- Permission system implemented

---

### Phase 3: Core API Development (Week 3-4)

#### 3.1 Repository Pattern Implementation

**Tasks:**
- [ ] Create generic repository interface
- [ ] Implement workflow repository
- [ ] Implement user repository
- [ ] Implement department repository
- [ ] Create unit of work pattern

**Key Files:**
```
Server/BpmnWorkflow.Domain/Interfaces/
├── IRepository.cs
├── IWorkflowRepository.cs
├── IUserRepository.cs
└── IUnitOfWork.cs

Server/BpmnWorkflow.Infrastructure/Repositories/
├── Repository.cs
├── WorkflowRepository.cs
├── UserRepository.cs
└── UnitOfWork.cs
```

#### 3.2 Business Logic Services

**Tasks:**
- [ ] Create workflow service
- [ ] Create user service
- [ ] Create department service
- [ ] Create audit service
- [ ] Implement AutoMapper profiles
- [ ] Add FluentValidation validators

**Key Files:**
```
Server/BpmnWorkflow.Application/Services/
├── WorkflowService.cs
├── UserService.cs
├── DepartmentService.cs
└── AuditService.cs

Server/BpmnWorkflow.Application/Validators/
├── CreateWorkflowValidator.cs
├── UpdateWorkflowValidator.cs
└── CreateUserValidator.cs

Server/BpmnWorkflow.Application/Mappings/
└── AutoMapperProfile.cs
```

#### 3.3 API Controllers

**Tasks:**
- [ ] Create WorkflowsController
- [ ] Create UsersController
- [ ] Create DepartmentsController
- [ ] Create AuditController
- [ ] Add Swagger documentation
- [ ] Implement error handling middleware

**API Endpoints to Implement:**

```csharp
// WorkflowsController
[HttpGet] GetWorkflows(string search, Guid? departmentId, int page, int pageSize)
[HttpGet("{id}")] GetWorkflow(Guid id)
[HttpPost] CreateWorkflow(CreateWorkflowRequest request)
[HttpPut("{id}")] UpdateWorkflow(Guid id, UpdateWorkflowRequest request)
[HttpDelete("{id}")] DeleteWorkflow(Guid id)
[HttpGet("{id}/xml")] GetWorkflowXml(Guid id)
[HttpGet("{id}/svg")] GetWorkflowSvg(Guid id)
[HttpPost("{id}/publish")] PublishWorkflow(Guid id)
[HttpGet("{id}/versions")] GetWorkflowVersions(Guid id)

// UsersController
[HttpGet] GetUsers(int page, int pageSize)
[HttpGet("{id}")] GetUser(Guid id)
[HttpPost] CreateUser(CreateUserRequest request)
[HttpPut("{id}")] UpdateUser(Guid id, UpdateUserRequest request)
[HttpDelete("{id}")] DeleteUser(Guid id)

// DepartmentsController
[HttpGet] GetDepartments()
[HttpGet("{id}")] GetDepartment(Guid id)
[HttpPost] CreateDepartment(CreateDepartmentRequest request)
[HttpPut("{id}")] UpdateDepartment(Guid id, UpdateDepartmentRequest request)
```

**Acceptance Criteria:**
- All endpoints return proper HTTP status codes
- Validation errors return 400 with details
- Unauthorized access returns 401
- Forbidden access returns 403
- Not found returns 404
- Server errors return 500 with logged details

---

### Phase 4: Frontend Foundation (Week 4-5)

#### 4.1 Blazor Project Setup

**Tasks:**
- [ ] Configure Blazor WASM project
- [ ] Install Radzen Blazor components
- [ ] Setup HTTP client with JWT interceptor
- [ ] Create authentication state provider
- [ ] Implement local storage service
- [ ] Configure routing

**Key Files:**
```
Client/BpmnWorkflow.Client/
├── Program.cs
├── App.razor
├── _Imports.razor
├── wwwroot/
│   └── index.html
└── Helpers/
    ├── CustomAuthenticationStateProvider.cs
    ├── AuthenticationHeaderHandler.cs
    └── LocalStorageService.cs
```

**Program.cs Configuration:**
```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]) 
});

// Add Radzen
builder.Services.AddRadzenComponents();

// Add authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Add application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IUserService, UserService>();

await builder.Build().RunAsync();
```

#### 4.2 Authentication Pages

**Tasks:**
- [ ] Create Login page
- [ ] Create Register page
- [ ] Create logout functionality
- [ ] Implement "Remember Me" feature
- [ ] Add password reset (optional)

**Key Files:**
```
Client/BpmnWorkflow.Client/Pages/
├── Login.razor
├── Register.razor
└── Logout.razor
```

#### 4.3 Layout & Navigation

**Tasks:**
- [ ] Create main layout with Radzen
- [ ] Build navigation menu
- [ ] Add user profile dropdown
- [ ] Implement breadcrumbs
- [ ] Add loading indicators

**Key Files:**
```
Client/BpmnWorkflow.Client/Components/Shared/
├── MainLayout.razor
├── NavMenu.razor
├── LoginDisplay.razor
└── LoadingSpinner.razor
```

---

### Phase 5: BPMN Editor Integration (Week 5-6)

#### 5.1 bpmn-js Setup

**Tasks:**
- [ ] Download bpmn-js library
- [ ] Create JavaScript interop wrapper
- [ ] Initialize modeler in Blazor component
- [ ] Test basic diagram creation
- [ ] Implement zoom controls

**Files to Create:**
```
Client/BpmnWorkflow.Client/wwwroot/
├── lib/
│   └── bpmn-js/
│       ├── bpmn-modeler.production.min.js
│       └── diagram-js.css
├── js/
│   ├── bpmn-modeler.js
│   └── interop.js
└── css/
    └── bpmn-editor.css
```

**bpmn-modeler.js Implementation:**
```javascript
window.BpmnModeler = {
    modeler: null,
    
    initialize: function(containerId) {
        const container = document.getElementById(containerId);
        
        this.modeler = new BpmnJS({
            container: container,
            keyboard: { bindTo: document },
            additionalModules: []
        });
        
        return true;
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
        
        try {
            await this.modeler.importXML(emptyDiagram);
            return { success: true };
        } catch (err) {
            return { success: false, error: err.message };
        }
    },
    
    importXML: async function(xml) {
        try {
            await this.modeler.importXML(xml);
            return { success: true };
        } catch (err) {
            return { success: false, error: err.message };
        }
    },
    
    exportXML: async function() {
        try {
            const { xml } = await this.modeler.saveXML({ format: true });
            return { success: true, xml: xml };
        } catch (err) {
            return { success: false, error: err.message };
        }
    },
    
    exportSVG: async function() {
        try {
            const { svg } = await this.modeler.saveSVG();
            return { success: true, svg: svg };
        } catch (err) {
            return { success: false, error: err.message };
        }
    },
    
    zoomIn: function() {
        this.modeler.get('zoomScroll').stepZoom(1);
    },
    
    zoomOut: function() {
        this.modeler.get('zoomScroll').stepZoom(-1);
    },
    
    zoomReset: function() {
        this.modeler.get('zoomScroll').reset();
    },
    
    destroy: function() {
        if (this.modeler) {
            this.modeler.destroy();
            this.modeler = null;
        }
    }
};
```

#### 5.2 Blazor BPMN Component

**Tasks:**
- [ ] Create BpmnModeler.razor component
- [ ] Implement JS interop calls
- [ ] Add toolbar with actions
- [ ] Handle component lifecycle
- [ ] Implement auto-save (optional)

**BpmnModeler.razor:**
```razor
@inject IJSRuntime JSRuntime
@inject IWorkflowService WorkflowService
@inject NotificationService NotificationService
@implements IAsyncDisposable

<div class="bpmn-editor-wrapper">
    <div class="bpmn-toolbar">
        <RadzenStack Orientation="Orientation.Horizontal" Gap="0.5rem" AlignItems="AlignItems.Center">
            <RadzenButton Icon="save" Text="Save" Click="@SaveWorkflow" ButtonStyle="ButtonStyle.Primary" />
            <RadzenButton Icon="download" Text="Export XML" Click="@ExportXML" ButtonStyle="ButtonStyle.Secondary" />
            <RadzenButton Icon="image" Text="Export SVG" Click="@ExportSVG" ButtonStyle="ButtonStyle.Secondary" />
            <RadzenSeparator />
            <RadzenButton Icon="zoom_in" Click="@ZoomIn" ButtonStyle="ButtonStyle.Light" Size="ButtonSize.Small" />
            <RadzenButton Icon="zoom_out" Click="@ZoomOut" ButtonStyle="ButtonStyle.Light" Size="ButtonSize.Small" />
            <RadzenButton Icon="fit_screen" Click="@ZoomReset" ButtonStyle="ButtonStyle.Light" Size="ButtonSize.Small" />
        </RadzenStack>
    </div>
    
    <div id="@ContainerId" class="bpmn-canvas"></div>
</div>

@code {
    [Parameter] public string WorkflowId { get; set; }
    [Parameter] public string WorkflowName { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    
    private string ContainerId = $"bpmn-container-{Guid.NewGuid()}";
    private bool isInitialized = false;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("BpmnModeler.initialize", ContainerId);
            
            if (string.IsNullOrEmpty(WorkflowId))
            {
                await JSRuntime.InvokeAsync<object>("BpmnModeler.createNewDiagram");
            }
            else
            {
                await LoadWorkflow();
            }
            
            isInitialized = true;
        }
    }
    
    private async Task LoadWorkflow()
    {
        var workflow = await WorkflowService.GetWorkflowByIdAsync(WorkflowId);
        if (workflow != null)
        {
            var result = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.importXML", workflow.BpmnXml);
            if (!result.Success)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Failed to load workflow", result.Error);
            }
        }
    }
    
    private async Task SaveWorkflow()
    {
        var xmlResult = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportXML");
        if (!xmlResult.Success)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Failed to export XML");
            return;
        }
        
        var svgResult = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportSVG");
        
        var request = new UpdateWorkflowRequest
        {
            Name = WorkflowName,
            BpmnXml = xmlResult.Xml,
            SvgPreview = svgResult.Success ? svgResult.Svg : null
        };
        
        if (string.IsNullOrEmpty(WorkflowId))
        {
            await WorkflowService.CreateWorkflowAsync(request);
        }
        else
        {
            await WorkflowService.UpdateWorkflowAsync(WorkflowId, request);
        }
        
        NotificationService.Notify(NotificationSeverity.Success, "Workflow saved successfully");
        await OnSaved.InvokeAsync();
    }
    
    private async Task ExportXML()
    {
        var result = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportXML");
        if (result.Success)
        {
            await JSRuntime.InvokeVoidAsync("downloadFile", $"{WorkflowName}.bpmn", result.Xml, "application/xml");
        }
    }
    
    private async Task ExportSVG()
    {
        var result = await JSRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportSVG");
        if (result.Success)
        {
            await JSRuntime.InvokeVoidAsync("downloadFile", $"{WorkflowName}.svg", result.Svg, "image/svg+xml");
        }
    }
    
    private async Task ZoomIn() => await JSRuntime.InvokeVoidAsync("BpmnModeler.zoomIn");
    private async Task ZoomOut() => await JSRuntime.InvokeVoidAsync("BpmnModeler.zoomOut");
    private async Task ZoomReset() => await JSRuntime.InvokeVoidAsync("BpmnModeler.zoomReset");
    
    public async ValueTask DisposeAsync()
    {
        if (isInitialized)
        {
            await JSRuntime.InvokeVoidAsync("BpmnModeler.destroy");
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

#### 5.3 Workflow Editor Page

**Tasks:**
- [ ] Create WorkflowEditor.razor page
- [ ] Add workflow metadata form (name, description)
- [ ] Integrate BpmnModeler component
- [ ] Add properties panel (optional)
- [ ] Implement navigation guards

**WorkflowEditor.razor:**
```razor
@page "/workflow/new"
@page "/workflow/{WorkflowId}"
@inject IWorkflowService WorkflowService
@inject NavigationManager NavigationManager
@attribute [Authorize(Policy = "CanCreate")]

<PageTitle>@(string.IsNullOrEmpty(WorkflowId) ? "New Workflow" : "Edit Workflow")</PageTitle>

<RadzenStack Gap="1rem">
    <RadzenCard>
        <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.SpaceBetween">
            <RadzenText TextStyle="TextStyle.H4">
                @(string.IsNullOrEmpty(WorkflowId) ? "Create New Workflow" : "Edit Workflow")
            </RadzenText>
            <RadzenButton Icon="arrow_back" Text="Back to List" Click="@(() => NavigationManager.NavigateTo("/workflows"))" 
                          ButtonStyle="ButtonStyle.Light" />
        </RadzenStack>
    </RadzenCard>
    
    <RadzenCard>
        <RadzenStack Gap="1rem">
            <RadzenFormField Text="Workflow Name" Variant="Variant.Outlined">
                <RadzenTextBox @bind-Value="@workflowName" Style="width: 100%;" />
            </RadzenFormField>
            
            <RadzenFormField Text="Description" Variant="Variant.Outlined">
                <RadzenTextArea @bind-Value="@workflowDescription" Rows="3" Style="width: 100%;" />
            </RadzenFormField>
            
            <RadzenFormField Text="Department" Variant="Variant.Outlined">
                <RadzenDropDown @bind-Value="@selectedDepartmentId" Data="@departments" 
                                TextProperty="Name" ValueProperty="Id" Style="width: 100%;" />
            </RadzenFormField>
        </RadzenStack>
    </RadzenCard>
    
    <RadzenCard Style="height: 600px;">
        <BpmnModeler WorkflowId="@WorkflowId" WorkflowName="@workflowName" OnSaved="@OnWorkflowSaved" />
    </RadzenCard>
</RadzenStack>

@code {
    [Parameter] public string WorkflowId { get; set; }
    
    private string workflowName = "Untitled Workflow";
    private string workflowDescription = "";
    private Guid? selectedDepartmentId;
    private List<DepartmentDto> departments = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Load departments
        departments = await WorkflowService.GetDepartmentsAsync();
        
        if (!string.IsNullOrEmpty(WorkflowId))
        {
            var workflow = await WorkflowService.GetWorkflowByIdAsync(WorkflowId);
            if (workflow != null)
            {
                workflowName = workflow.Name;
                workflowDescription = workflow.Description;
                selectedDepartmentId = workflow.DepartmentId;
            }
        }
    }
    
    private void OnWorkflowSaved()
    {
        NavigationManager.NavigateTo("/workflows");
    }
}
```

---

### Phase 6: Workflow Management UI (Week 6-7)

#### 6.1 Dashboard Page

**Tasks:**
- [ ] Create dashboard with statistics
- [ ] Show recent workflows
- [ ] Display user activity
- [ ] Add quick actions

**Dashboard.razor:**
```razor
@page "/"
@page "/dashboard"
@attribute [Authorize]

<PageTitle>Dashboard</PageTitle>

<RadzenStack Gap="1rem">
    <RadzenText TextStyle="TextStyle.H3">Dashboard</RadzenText>
    
    <RadzenRow Gap="1rem">
        <RadzenColumn Size="12" SizeMD="3">
            <RadzenCard>
                <RadzenStack Gap="0.5rem">
                    <RadzenText TextStyle="TextStyle.Overline">Total Workflows</RadzenText>
                    <RadzenText TextStyle="TextStyle.H4">@totalWorkflows</RadzenText>
                </RadzenStack>
            </RadzenCard>
        </RadzenColumn>
        
        <RadzenColumn Size="12" SizeMD="3">
            <RadzenCard>
                <RadzenStack Gap="0.5rem">
                    <RadzenText TextStyle="TextStyle.Overline">My Workflows</RadzenText>
                    <RadzenText TextStyle="TextStyle.H4">@myWorkflows</RadzenText>
                </RadzenStack>
            </RadzenCard>
        </RadzenColumn>
        
        <RadzenColumn Size="12" SizeMD="3">
            <RadzenCard>
                <RadzenStack Gap="0.5rem">
                    <RadzenText TextStyle="TextStyle.Overline">Published</RadzenText>
                    <RadzenText TextStyle="TextStyle.H4">@publishedWorkflows</RadzenText>
                </RadzenStack>
            </RadzenCard>
        </RadzenColumn>
        
        <RadzenColumn Size="12" SizeMD="3">
            <RadzenCard>
                <RadzenStack Gap="0.5rem">
                    <RadzenText TextStyle="TextStyle.Overline">Departments</RadzenText>
                    <RadzenText TextStyle="TextStyle.H4">@totalDepartments</RadzenText>
                </RadzenStack>
            </RadzenCard>
        </RadzenColumn>
    </RadzenRow>
    
    <RadzenCard>
        <RadzenStack Gap="1rem">
            <RadzenText TextStyle="TextStyle.H5">Recent Workflows</RadzenText>
            <RadzenDataList Data="@recentWorkflows" TItem="WorkflowDto">
                <Template Context="workflow">
                    <WorkflowCard Workflow="@workflow" />
                </Template>
            </RadzenDataList>
        </RadzenStack>
    </RadzenCard>
</RadzenStack>

@code {
    private int totalWorkflows;
    private int myWorkflows;
    private int publishedWorkflows;
    private int totalDepartments;
    private List<WorkflowDto> recentWorkflows = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Load statistics
        var stats = await WorkflowService.GetStatisticsAsync();
        totalWorkflows = stats.TotalWorkflows;
        myWorkflows = stats.MyWorkflows;
        publishedWorkflows = stats.PublishedWorkflows;
        totalDepartments = stats.TotalDepartments;
        
        // Load recent workflows
        recentWorkflows = await WorkflowService.GetRecentWorkflowsAsync(5);
    }
}
```

#### 6.2 Workflow List Page

**Tasks:**
- [ ] Create workflow list with RadzenDataGrid
- [ ] Add search functionality
- [ ] Implement filtering (by department, status)
- [ ] Add sorting
- [ ] Implement pagination
- [ ] Add bulk actions (optional)

**WorkflowList.razor:**
```razor
@page "/workflows"
@attribute [Authorize]

<PageTitle>Workflows</PageTitle>

<RadzenStack Gap="1rem">
    <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.SpaceBetween">
        <RadzenText TextStyle="TextStyle.H3">Workflows</RadzenText>
        <AuthorizeView Policy="CanCreate">
            <RadzenButton Icon="add" Text="New Workflow" Click="@CreateNewWorkflow" ButtonStyle="ButtonStyle.Primary" />
        </AuthorizeView>
    </RadzenStack>
    
    <RadzenCard>
        <RadzenStack Gap="1rem">
            <RadzenTextBox Placeholder="Search workflows..." @bind-Value="@searchText" 
                           Style="width: 100%;" @oninput="@OnSearchChanged" />
            
            <RadzenStack Orientation="Orientation.Horizontal" Gap="1rem">
                <RadzenDropDown @bind-Value="@selectedDepartmentId" Data="@departments" 
                                TextProperty="Name" ValueProperty="Id" Placeholder="All Departments"
                                AllowClear="true" Change="@OnFilterChanged" />
                
                <RadzenDropDown @bind-Value="@selectedStatus" Data="@statuses" 
                                Placeholder="All Statuses" AllowClear="true" Change="@OnFilterChanged" />
            </RadzenStack>
        </RadzenStack>
    </RadzenCard>
    
    <RadzenCard>
        <RadzenDataGrid @ref="grid" Data="@workflows" TItem="WorkflowDto" 
                        AllowPaging="true" PageSize="10" AllowSorting="true"
                        IsLoading="@isLoading">
            <Columns>
                <RadzenDataGridColumn TItem="WorkflowDto" Property="Name" Title="Name" />
                <RadzenDataGridColumn TItem="WorkflowDto" Property="Description" Title="Description" />
                <RadzenDataGridColumn TItem="WorkflowDto" Property="DepartmentName" Title="Department" />
                <RadzenDataGridColumn TItem="WorkflowDto" Property="OwnerName" Title="Owner" />
                <RadzenDataGridColumn TItem="WorkflowDto" Property="UpdatedAt" Title="Last Modified" 
                                      FormatString="{0:yyyy-MM-dd HH:mm}" />
                <RadzenDataGridColumn TItem="WorkflowDto" Title="Actions" Sortable="false">
                    <Template Context="workflow">
                        <RadzenStack Orientation="Orientation.Horizontal" Gap="0.5rem">
                            <RadzenButton Icon="visibility" ButtonStyle="ButtonStyle.Light" Size="ButtonSize.Small"
                                          Click="@(() => ViewWorkflow(workflow.Id))" />
                            <AuthorizeView Policy="CanEdit">
                                <RadzenButton Icon="edit" ButtonStyle="ButtonStyle.Light" Size="ButtonSize.Small"
                                              Click="@(() => EditWorkflow(workflow.Id))" />
                            </AuthorizeView>
                            <AuthorizeView Policy="CanDelete">
                                <RadzenButton Icon="delete" ButtonStyle="ButtonStyle.Danger" Size="ButtonSize.Small"
                                              Click="@(() => DeleteWorkflow(workflow.Id))" />
                            </AuthorizeView>
                        </RadzenStack>
                    </Template>
                </RadzenDataGridColumn>
            </Columns>
        </RadzenDataGrid>
    </RadzenCard>
</RadzenStack>

@code {
    private RadzenDataGrid<WorkflowDto> grid;
    private List<WorkflowDto> workflows = new();
    private List<DepartmentDto> departments = new();
    private List<string> statuses = new() { "Draft", "Published" };
    
    private string searchText = "";
    private Guid? selectedDepartmentId;
    private string selectedStatus;
    private bool isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        isLoading = true;
        workflows = await WorkflowService.GetWorkflowsAsync(searchText, selectedDepartmentId);
        departments = await WorkflowService.GetDepartmentsAsync();
        isLoading = false;
    }
    
    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        searchText = e.Value?.ToString() ?? "";
        await LoadData();
    }
    
    private async Task OnFilterChanged()
    {
        await LoadData();
    }
    
    private void CreateNewWorkflow()
    {
        NavigationManager.NavigateTo("/workflow/new");
    }
    
    private void ViewWorkflow(Guid id)
    {
        NavigationManager.NavigateTo($"/workflow/{id}/view");
    }
    
    private void EditWorkflow(Guid id)
    {
        NavigationManager.NavigateTo($"/workflow/{id}");
    }
    
    private async Task DeleteWorkflow(Guid id)
    {
        var confirmed = await DialogService.Confirm("Are you sure you want to delete this workflow?", 
                                                     "Delete Workflow", 
                                                     new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmed == true)
        {
            await WorkflowService.DeleteWorkflowAsync(id);
            await LoadData();
            NotificationService.Notify(NotificationSeverity.Success, "Workflow deleted successfully");
        }
    }
}
```

#### 6.3 Workflow Card Component

**Tasks:**
- [ ] Create reusable workflow card
- [ ] Show SVG preview
- [ ] Display metadata
- [ ] Add action buttons

---

### Phase 7: Admin Features (Week 7-8)

#### 7.1 User Management

**Tasks:**
- [ ] Create user list page
- [ ] Add user creation form
- [ ] Implement user editing
- [ ] Add role assignment
- [ ] Implement user deactivation

#### 7.2 Department Management

**Tasks:**
- [ ] Create department list
- [ ] Add department CRUD operations
- [ ] Show department statistics

#### 7.3 Audit Log Viewer

**Tasks:**
- [ ] Create audit log page
- [ ] Implement filtering by user/action/date
- [ ] Add export functionality

---

### Phase 8: Testing & Quality Assurance (Week 8-9)

#### 8.1 Unit Tests

**Tasks:**
- [ ] Write service layer tests
- [ ] Write repository tests
- [ ] Write validator tests
- [ ] Achieve 80%+ code coverage

**Example Test:**
```csharp
public class WorkflowServiceTests
{
    private readonly Mock<IWorkflowRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly WorkflowService _service;
    
    public WorkflowServiceTests()
    {
        _mockRepo = new Mock<IWorkflowRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new WorkflowService(_mockRepo.Object, _mockMapper.Object);
    }
    
    [Fact]
    public async Task CreateWorkflow_ValidRequest_ReturnsWorkflowDto()
    {
        // Arrange
        var request = new CreateWorkflowRequest 
        { 
            Name = "Test Workflow",
            BpmnXml = "<xml>...</xml>"
        };
        var userId = Guid.NewGuid();
        
        // Act
        var result = await _service.CreateWorkflowAsync(request, userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Workflow", result.Name);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Workflow>()), Times.Once);
    }
}
```

#### 8.2 Integration Tests

**Tasks:**
- [ ] Test API endpoints
- [ ] Test authentication flow
- [ ] Test authorization rules
- [ ] Test database operations

#### 8.3 End-to-End Tests

**Tasks:**
- [ ] Setup Playwright/Selenium
- [ ] Test complete user workflows
- [ ] Test BPMN editor functionality
- [ ] Test cross-browser compatibility

---

### Phase 9: Performance & Security (Week 9)

#### 9.1 Performance Optimization

**Tasks:**
- [ ] Implement response caching
- [ ] Add Redis caching
- [ ] Optimize database queries
- [ ] Minimize bundle size
- [ ] Lazy load components
- [ ] Compress responses

#### 9.2 Security Hardening

**Tasks:**
- [ ] Security audit
- [ ] Implement rate limiting
- [ ] Add CORS configuration
- [ ] Enable HTTPS
- [ ] Add security headers
- [ ] Implement CSP
- [ ] Sanitize inputs
- [ ] Validate BPMN XML

---

### Phase 10: Documentation & Deployment (Week 10)

#### 10.1 Documentation

**Tasks:**
- [ ] Write user manual
- [ ] Create API documentation (Swagger)
- [ ] Write developer guide
- [ ] Create deployment guide
- [ ] Document architecture decisions

#### 10.2 Deployment

**Tasks:**
- [ ] Setup production database
- [ ] Configure production environment
- [ ] Setup CI/CD pipeline
- [ ] Deploy to production
- [ ] Configure monitoring
- [ ] Setup backup strategy

**Deployment Checklist:**
- [ ] Environment variables configured
- [ ] Database migrations applied
- [ ] SSL certificates installed
- [ ] Firewall rules configured
- [ ] Monitoring enabled
- [ ] Backup scheduled
- [ ] Health checks configured

---

## 📊 Progress Tracking

### Milestones

| Milestone | Target Date | Status |
|-----------|-------------|--------|
| Phase 1: Foundation Complete | Week 2 | ⏳ Pending |
| Phase 2: Auth Complete | Week 3 | ⏳ Pending |
| Phase 3: API Complete | Week 4 | ⏳ Pending |
| Phase 4: Frontend Foundation | Week 5 | ⏳ Pending |
| Phase 5: BPMN Editor Working | Week 6 | ⏳ Pending |
| Phase 6: Workflow Management | Week 7 | ⏳ Pending |
| Phase 7: Admin Features | Week 8 | ⏳ Pending |
| Phase 8: Testing Complete | Week 9 | ⏳ Pending |
| Phase 9: Production Ready | Week 10 | ⏳ Pending |
| Phase 10: Deployed | Week 10 | ⏳ Pending |

---

## 🎯 Success Criteria

### Functional Requirements
- ✅ Users can register and login
- ✅ Users can create BPMN diagrams
- ✅ Users can save and load diagrams
- ✅ Users can export to XML and SVG
- ✅ Role-based permissions work correctly
- ✅ Admin can manage users and departments

### Non-Functional Requirements
- ✅ Page load time < 2 seconds
- ✅ API response time < 200ms (p95)
- ✅ Support 100+ concurrent users
- ✅ 99.9% uptime
- ✅ BPMN 2.0 compliant
- ✅ Mobile responsive (tablet+)

---

## 🚨 Risk Management

### Technical Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| bpmn-js integration issues | High | Prototype early, test thoroughly |
| Performance with large diagrams | Medium | Implement lazy loading, optimize rendering |
| Browser compatibility | Medium | Test on major browsers, use polyfills |
| Security vulnerabilities | High | Regular security audits, penetration testing |

### Project Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Scope creep | High | Strict change management, MVP focus |
| Timeline delays | Medium | Buffer time in schedule, agile approach |
| Resource availability | Medium | Cross-training, documentation |

---

## 📚 Resources

### Required Skills
- C# / .NET Core
- Blazor WebAssembly
- Entity Framework Core
- SQL Server / PostgreSQL
- JavaScript (for bpmn-js integration)
- HTML/CSS
- Git

### Tools
- Visual Studio 2022 / VS Code / Rider
- SQL Server Management Studio / pgAdmin
- Postman (API testing)
- Docker Desktop
- Git

### Libraries
- Radzen Blazor Components
- bpmn-js
- AutoMapper
- FluentValidation
- Serilog
- xUnit / NUnit

---

**Document Version**: 1.0  
**Last Updated**: 2025-12-18  
**Status**: Ready for Implementation

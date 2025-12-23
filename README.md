# BPMN Workflow Designer

> A modern, enterprise-grade web application for designing, managing, and documenting BPMN 2.0 business process workflows.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Blazor](https://img.shields.io/badge/Blazor-WASM-blue.svg)

## 📖 Overview

**BPMN Workflow Designer** is a comprehensive web application that enables organizations to visually design, edit, store, and manage BPMN 2.0 business process diagrams directly in the browser. Built with ASP.NET Blazor and Radzen UI, it provides an intuitive interface for both technical and non-technical users to document and optimize business processes.

### Key Features

✨ **Visual BPMN 2.0 Editor**
- Drag-and-drop interface powered by bpmn-js
- Full support for BPMN 2.0 elements (events, tasks, gateways, flows)
- Real-time diagram editing with zoom and pan controls
- Undo/Redo functionality

💾 **Workflow Management**
- Save and load workflows from database
- Export diagrams as BPMN XML or SVG
- Version control and history tracking
- Search and filter capabilities

🔐 **Security & Access Control**
- JWT-based authentication
- Role-based authorization (Designer, Viewer, Admin)
- Department-based access control
- Comprehensive audit logging

🎨 **Modern UI/UX**
- Built with Radzen Blazor components
- Responsive design for desktop and tablet
- Dark mode support (optional)
- Intuitive navigation and workflows

🚀 **Camunda Integration** (NEW!)
- Deploy workflows to Camunda Platform 7 engine
- Execute BPMN processes with real workflow engine
- Manage user tasks and external tasks
- Monitor running process instances
- Process variables and business data management
- Integration with existing systems via external tasks

## 🏗️ Architecture

### Technology Stack

**Frontend:**
- Blazor WebAssembly
- Radzen Blazor Components
- bpmn-js (BPMN modeler)
- JavaScript Interop

**Backend:**
- ASP.NET Core 8.0 Web API
- Entity Framework Core 8.0
- JWT Authentication
- FluentValidation

**Database:**
- SQL Server 2019+ or PostgreSQL 14+
- Redis (optional, for caching)

**Workflow Engine:**
- Camunda Platform 7 (Community Edition)
- BPMN 2.0 execution engine
- DMN decision engine
- External task workers

**DevOps:**
- Docker & Docker Compose
- GitHub Actions / Azure DevOps
- Azure App Service / IIS

### Project Structure

```
BPMN Workflow Designer/
├── Client/                          # Blazor WebAssembly frontend
│   └── BpmnWorkflow.Client/
│       ├── Pages/                   # Razor pages
│       ├── Components/              # Reusable components
│       ├── Services/                # API client services
│       └── wwwroot/                 # Static assets
│
├── Server/                          # ASP.NET Core backend
│   ├── BpmnWorkflow.API/            # Web API project
│   ├── BpmnWorkflow.Application/    # Business logic
│   ├── BpmnWorkflow.Domain/         # Domain entities
│   └── BpmnWorkflow.Infrastructure/ # Data access
│
├── Tests/                           # Test projects
│   ├── BpmnWorkflow.UnitTests/
│   └── BpmnWorkflow.IntegrationTests/
│
├── docs/                            # Documentation
├── docker-compose.yml               # Docker configuration
├── ARCHITECTURE.md                  # Architecture documentation
├── IMPLEMENTATION_PLAN.md           # Implementation guide
└── README.md                        # This file
```

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (for npm packages)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) or [PostgreSQL 14+](https://www.postgresql.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for containerized development)
- [Git](https://git-scm.com/)

### Installation

#### Option 1: Local Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/bpmn-workflow-designer.git
   cd bpmn-workflow-designer
   ```

2. **Setup the database**
   
   For SQL Server:
   ```bash
   # Update connection string in Server/BpmnWorkflow.API/appsettings.json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=BpmnWorkflow;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
   
   For PostgreSQL:
   ```bash
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=BpmnWorkflow;Username=postgres;Password=yourpassword"
   }
   ```

3. **Apply database migrations**
   ```bash
   cd Server/BpmnWorkflow.API
   dotnet ef database update
   ```

4. **Configure JWT settings**
   
   Update `appsettings.json`:
   ```json
   "JwtSettings": {
     "SecretKey": "your-super-secret-key-minimum-32-characters-long",
     "Issuer": "BpmnWorkflowDesigner",
     "Audience": "BpmnWorkflowDesigner",
     "ExpirationMinutes": 60
   }
   ```

5. **Install frontend dependencies**
   ```bash
   cd ../../Client/BpmnWorkflow.Client
   dotnet restore
   ```

6. **Download bpmn-js library**
   
   Download from [bpmn.io](https://bpmn.io/toolkit/bpmn-js/walkthrough/) and place in:
   ```
   Client/BpmnWorkflow.Client/wwwroot/lib/bpmn-js/
   ```

7. **Run the application**
   
   Terminal 1 (Backend):
   ```bash
   cd Server/BpmnWorkflow.API
   dotnet run
   ```
   
   Terminal 2 (Frontend):
   ```bash
   cd Client/BpmnWorkflow.Client
   dotnet run
   ```

8. **Access the application**
   - Frontend: https://localhost:5001
   - API: https://localhost:7001
   - Swagger: https://localhost:7001/swagger

#### Option 2: Docker Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/bpmn-workflow-designer.git
   cd bpmn-workflow-designer
   ```

2. **Start with Docker Compose**
   ```bash
   docker-compose up -d
   ```

3. **Access the application**
   - Frontend: http://localhost:5001
   - API: http://localhost:5000
   - SQL Server: localhost:1433

4. **Stop the application**
   ```bash
   docker-compose down
   ```

### Default Credentials

After initial setup, use these credentials to login:

- **Admin User**
  - Email: `admin@bpmn.local`
  - Password: `Admin@123`

- **Designer User**
  - Email: `designer@bpmn.local`
  - Password: `Designer@123`

- **Viewer User**
  - Email: `viewer@bpmn.local`
  - Password: `Viewer@123`

> ⚠️ **Important:** Change these passwords immediately in production!

## 📚 Usage Guide

### Creating Your First Workflow

1. **Login** with your credentials
2. Navigate to **Workflows** from the menu
3. Click **New Workflow** button
4. Enter workflow details:
   - Name: "My First Process"
   - Description: "A simple approval process"
   - Department: Select from dropdown
5. Use the **BPMN Editor** to design your process:
   - Drag elements from the palette
   - Connect elements with sequence flows
   - Double-click to edit element properties
6. Click **Save** to store your workflow
7. Use **Export XML** or **Export SVG** to download

### Managing Workflows

- **View All Workflows**: Navigate to Workflows page
- **Search**: Use the search bar to find workflows by name
- **Filter**: Filter by department or status
- **Edit**: Click the edit icon on any workflow you own
- **Delete**: Click the delete icon (requires permission)
- **Export**: Download workflows as BPMN XML or SVG

### User Roles

**Viewer**
- View workflows
- Export workflows
- Cannot create or modify

**Designer**
- All Viewer permissions
- Create new workflows
- Edit own workflows
- Delete own workflows

**Admin**
- All Designer permissions
- Edit any workflow
- Delete any workflow
- Manage users and departments
- View audit logs

## 🔧 Configuration

### Environment Variables

Create `appsettings.Production.json` for production settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=BpmnWorkflow;User Id=sa;Password=***;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "production-secret-key-very-long-and-secure",
    "Issuer": "BpmnWorkflowDesigner",
    "Audience": "BpmnWorkflowDesigner",
    "ExpirationMinutes": 60
  },
  "Redis": {
    "ConnectionString": "redis-server:6379",
    "InstanceName": "BpmnWorkflow:"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  }
}
```

### Database Configuration

**SQL Server:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BpmnWorkflow;Trusted_Connection=True;TrustServerCertificate=True"
}
```

**PostgreSQL:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=BpmnWorkflow;Username=postgres;Password=yourpassword"
}
```

Change the database provider in `BpmnWorkflow.Infrastructure`:

```csharp
// For SQL Server (default)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// For PostgreSQL
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
```

## 🧪 Testing

### Running Tests

**Unit Tests:**
```bash
dotnet test Tests/BpmnWorkflow.UnitTests/
```

**Integration Tests:**
```bash
dotnet test Tests/BpmnWorkflow.IntegrationTests/
```

**All Tests:**
```bash
dotnet test
```

**With Coverage:**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Structure

```
Tests/
├── BpmnWorkflow.UnitTests/
│   ├── Services/
│   │   ├── WorkflowServiceTests.cs
│   │   ├── AuthServiceTests.cs
│   │   └── UserServiceTests.cs
│   ├── Validators/
│   │   └── CreateWorkflowValidatorTests.cs
│   └── Repositories/
│       └── WorkflowRepositoryTests.cs
│
└── BpmnWorkflow.IntegrationTests/
    ├── Controllers/
    │   ├── WorkflowsControllerTests.cs
    │   └── AuthControllerTests.cs
    └── Infrastructure/
        └── DatabaseTests.cs
```

## 📦 Deployment

### Production Deployment Checklist

- [ ] Update `appsettings.Production.json` with production values
- [ ] Change default passwords
- [ ] Generate strong JWT secret key (minimum 32 characters)
- [ ] Configure HTTPS/SSL certificates
- [ ] Setup database backups
- [ ] Configure logging (Serilog to file/cloud)
- [ ] Enable CORS for production domain only
- [ ] Setup monitoring (Application Insights, etc.)
- [ ] Configure rate limiting
- [ ] Review security headers
- [ ] Test all functionality in staging environment

### Azure Deployment

1. **Create Azure Resources**
   ```bash
   az group create --name bpmn-workflow-rg --location eastus
   az sql server create --name bpmn-sql-server --resource-group bpmn-workflow-rg --admin-user sqladmin --admin-password YourPassword123!
   az sql db create --resource-group bpmn-workflow-rg --server bpmn-sql-server --name BpmnWorkflow --service-objective S0
   az appservice plan create --name bpmn-app-plan --resource-group bpmn-workflow-rg --sku B1
   az webapp create --name bpmn-api --resource-group bpmn-workflow-rg --plan bpmn-app-plan
   az staticwebapp create --name bpmn-client --resource-group bpmn-workflow-rg --location eastus
   ```

2. **Configure App Settings**
   ```bash
   az webapp config appsettings set --name bpmn-api --resource-group bpmn-workflow-rg --settings \
     ConnectionStrings__DefaultConnection="Server=tcp:bpmn-sql-server.database.windows.net,1433;Database=BpmnWorkflow;User ID=sqladmin;Password=YourPassword123!;" \
     JwtSettings__SecretKey="your-production-secret-key"
   ```

3. **Deploy API**
   ```bash
   cd Server/BpmnWorkflow.API
   dotnet publish -c Release
   az webapp deployment source config-zip --resource-group bpmn-workflow-rg --name bpmn-api --src publish.zip
   ```

4. **Deploy Client**
   ```bash
   cd Client/BpmnWorkflow.Client
   dotnet publish -c Release
   # Deploy to Azure Static Web Apps via GitHub Actions
   ```

### Docker Deployment

1. **Build Images**
   ```bash
   docker build -t bpmn-api:latest -f Server/Dockerfile .
   docker build -t bpmn-client:latest -f Client/Dockerfile .
   ```

2. **Run Containers**
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

3. **Check Status**
   ```bash
   docker-compose ps
   docker-compose logs -f
   ```

## 🔒 Security

### Security Features

- **Authentication**: JWT Bearer tokens with configurable expiration
- **Authorization**: Role-based access control (RBAC)
- **Password Security**: BCrypt hashing with salt
- **Input Validation**: FluentValidation on all inputs
- **SQL Injection Prevention**: EF Core parameterized queries
- **XSS Prevention**: Blazor automatic encoding
- **CSRF Protection**: Built-in Blazor CSRF tokens
- **Audit Logging**: All actions logged with user, timestamp, IP

### Security Best Practices

1. **Always use HTTPS in production**
2. **Keep JWT secret key secure** (use Azure Key Vault or similar)
3. **Regularly update dependencies** (`dotnet list package --outdated`)
4. **Implement rate limiting** to prevent abuse
5. **Enable CORS only for trusted domains**
6. **Regular security audits** and penetration testing
7. **Monitor audit logs** for suspicious activity

## 📊 API Documentation

### Swagger UI

Access interactive API documentation at:
- Development: https://localhost:7001/swagger
- Production: https://yourdomain.com/api/swagger

### Key Endpoints

**Authentication:**
```
POST   /api/auth/login          # User login
POST   /api/auth/register       # User registration
POST   /api/auth/refresh-token  # Refresh JWT token
GET    /api/auth/me             # Get current user info
```

**Workflows:**
```
GET    /api/workflows           # List all workflows
GET    /api/workflows/{id}      # Get workflow by ID
POST   /api/workflows           # Create new workflow
PUT    /api/workflows/{id}      # Update workflow
DELETE /api/workflows/{id}      # Delete workflow
GET    /api/workflows/{id}/xml  # Get BPMN XML
GET    /api/workflows/{id}/svg  # Get SVG preview
```

**Users (Admin only):**
```
GET    /api/users               # List all users
GET    /api/users/{id}          # Get user details
POST   /api/users               # Create user
PUT    /api/users/{id}          # Update user
DELETE /api/users/{id}          # Deactivate user
```

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting PR
- Keep commits atomic and well-described

## 🐛 Troubleshooting

### Common Issues

**Issue: Database connection fails**
```
Solution: Check connection string in appsettings.json
Verify SQL Server is running: services.msc
Test connection: dotnet ef database update
```

**Issue: JWT authentication fails**
```
Solution: Ensure JWT secret key is at least 32 characters
Check token expiration time
Verify issuer and audience match in frontend and backend
```

**Issue: BPMN editor doesn't load**
```
Solution: Verify bpmn-js files are in wwwroot/lib/bpmn-js/
Check browser console for JavaScript errors
Ensure JS interop is properly initialized
```

**Issue: CORS errors**
```
Solution: Add frontend URL to CORS allowed origins in Program.cs
builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Your Name** - *Initial work* - [YourGitHub](https://github.com/yourusername)

## 🙏 Acknowledgments

- [bpmn.io](https://bpmn.io/) - For the excellent bpmn-js library
- [Radzen](https://www.radzen.com/) - For the beautiful Blazor components
- [Microsoft](https://dotnet.microsoft.com/) - For .NET and Blazor
- [BPMN.org](https://www.bpmn.org/) - For BPMN 2.0 specification

## 📞 Support

For support, please:
- Open an issue on GitHub
- Email: support@yourdomain.com
- Documentation: [docs/](docs/)

## 🗺️ Roadmap

### Version 1.0 (Current)
- ✅ BPMN 2.0 visual editor
- ✅ Workflow CRUD operations
- ✅ Role-based access control
- ✅ Export to XML/SVG

### Version 1.1 (Planned)
- [ ] Workflow versioning with diff view
- [ ] Collaboration features (comments, sharing)
- [ ] Advanced BPMN elements (sub-processes, events)
- [ ] Workflow templates library

### Version 2.0 (Future)
- [ ] Workflow execution engine
- [ ] Real-time collaboration
- [ ] Mobile app (Xamarin/MAUI)
- [ ] AI-powered process optimization suggestions
- [ ] Integration with external systems (REST APIs, webhooks)

---

**Built with ❤️ using Blazor, ASP.NET Core, and bpmn-js**

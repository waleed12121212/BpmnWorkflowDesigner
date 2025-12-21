# BPMN Workflow Designer - Quick Start Guide

## 🚀 5-Minute Quick Start

This guide will get you up and running with the BPMN Workflow Designer in just 5 minutes.

---

## Prerequisites Check

Before starting, verify you have:
- [ ] .NET 8.0 SDK installed (`dotnet --version`)
- [ ] SQL Server or PostgreSQL running
- [ ] Git installed

---

## Step 1: Create the Project Structure (2 minutes)

Open PowerShell/Terminal and run:

```powershell
# Navigate to your workspace
cd "c:\Users\user\Desktop\New folder\BPMN Workflow Designer"

# Create solution
dotnet new sln -n BpmnWorkflowDesigner

# Create Server projects
dotnet new webapi -n BpmnWorkflow.API -o Server/BpmnWorkflow.API
dotnet new classlib -n BpmnWorkflow.Application -o Server/BpmnWorkflow.Application
dotnet new classlib -n BpmnWorkflow.Domain -o Server/BpmnWorkflow.Domain
dotnet new classlib -n BpmnWorkflow.Infrastructure -o Server/BpmnWorkflow.Infrastructure

# Create Client project
dotnet new blazorwasm -n BpmnWorkflow.Client -o Client/BpmnWorkflow.Client

# Add projects to solution
dotnet sln add Server/BpmnWorkflow.API/BpmnWorkflow.API.csproj
dotnet sln add Server/BpmnWorkflow.Application/BpmnWorkflow.Application.csproj
dotnet sln add Server/BpmnWorkflow.Domain/BpmnWorkflow.Domain.csproj
dotnet sln add Server/BpmnWorkflow.Infrastructure/BpmnWorkflow.Infrastructure.csproj
dotnet sln add Client/BpmnWorkflow.Client/BpmnWorkflow.Client.csproj

# Add project references
dotnet add Server/BpmnWorkflow.API/BpmnWorkflow.API.csproj reference Server/BpmnWorkflow.Application/BpmnWorkflow.Application.csproj
dotnet add Server/BpmnWorkflow.API/BpmnWorkflow.API.csproj reference Server/BpmnWorkflow.Infrastructure/BpmnWorkflow.Infrastructure.csproj
dotnet add Server/BpmnWorkflow.Application/BpmnWorkflow.Application.csproj reference Server/BpmnWorkflow.Domain/BpmnWorkflow.Domain.csproj
dotnet add Server/BpmnWorkflow.Infrastructure/BpmnWorkflow.Infrastructure.csproj reference Server/BpmnWorkflow.Domain/BpmnWorkflow.Domain.csproj
```

---

## Step 2: Install Required Packages (1 minute)

```powershell
# API Project packages
cd Server/BpmnWorkflow.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Application Project packages
cd ../BpmnWorkflow.Application
dotnet add package AutoMapper
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

# Infrastructure Project packages
cd ../BpmnWorkflow.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package BCrypt.Net-Next

# Client Project packages
cd ../../Client/BpmnWorkflow.Client
dotnet add package Radzen.Blazor
dotnet add package Blazored.LocalStorage

# Return to root
cd ../..
```

---

## Step 3: Setup Database (1 minute)

### Option A: SQL Server (LocalDB)

```powershell
# Create database using SqlCmd
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE BpmnWorkflow"
```

### Option B: SQL Server (Full Instance)

```powershell
# Update connection string in Server/BpmnWorkflow.API/appsettings.json
# Then create database
sqlcmd -S "localhost" -U sa -P "YourPassword" -Q "CREATE DATABASE BpmnWorkflow"
```

### Option C: PostgreSQL

```powershell
# Create database
psql -U postgres -c "CREATE DATABASE bpmn_workflow;"

# Update connection string in appsettings.json to use PostgreSQL
# Change Infrastructure project to use Npgsql:
cd Server/BpmnWorkflow.Infrastructure
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

---

## Step 4: Verify Setup (1 minute)

```powershell
# Build the solution
dotnet build

# Check for errors
# If successful, you should see "Build succeeded"
```

---

## Next Steps

Now that your project structure is ready, follow the **IMPLEMENTATION_PLAN.md** to:

1. **Phase 1**: Create domain entities and database context
2. **Phase 2**: Implement authentication and authorization
3. **Phase 3**: Build API endpoints
4. **Phase 4**: Create Blazor frontend
5. **Phase 5**: Integrate bpmn-js editor

---

## Quick Reference Commands

### Build & Run

```powershell
# Build entire solution
dotnet build

# Run API (from Server/BpmnWorkflow.API)
dotnet run

# Run Client (from Client/BpmnWorkflow.Client)
dotnet run

# Run both with hot reload
dotnet watch run
```

### Database Migrations

```powershell
# Create migration (from Server/BpmnWorkflow.API)
dotnet ef migrations add MigrationName --project ../BpmnWorkflow.Infrastructure

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove --project ../BpmnWorkflow.Infrastructure

# List migrations
dotnet ef migrations list --project ../BpmnWorkflow.Infrastructure
```

### Testing

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test Tests/BpmnWorkflow.UnitTests/
```

### Package Management

```powershell
# List installed packages
dotnet list package

# Check for outdated packages
dotnet list package --outdated

# Update package
dotnet add package PackageName --version x.x.x

# Remove package
dotnet remove package PackageName
```

---

## Troubleshooting Quick Fixes

### "dotnet command not found"
```powershell
# Verify .NET installation
dotnet --version

# If not found, download from: https://dotnet.microsoft.com/download
```

### "Database connection failed"
```powershell
# Test SQL Server connection
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"

# Or for PostgreSQL
psql -U postgres -c "SELECT version();"
```

### "Port already in use"
```powershell
# Change port in Properties/launchSettings.json
# Or kill process using port
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### "Package restore failed"
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

---

## Project Structure Overview

```
BPMN Workflow Designer/
├── Server/
│   ├── BpmnWorkflow.API/            # ✅ Created - Web API entry point
│   ├── BpmnWorkflow.Application/    # ✅ Created - Business logic
│   ├── BpmnWorkflow.Domain/         # ✅ Created - Domain entities
│   └── BpmnWorkflow.Infrastructure/ # ✅ Created - Data access
│
├── Client/
│   └── BpmnWorkflow.Client/         # ✅ Created - Blazor WASM app
│
├── Tests/                           # ⏳ To be created
│   ├── BpmnWorkflow.UnitTests/
│   └── BpmnWorkflow.IntegrationTests/
│
├── docs/                            # ⏳ To be created
├── ARCHITECTURE.md                  # ✅ Created
├── IMPLEMENTATION_PLAN.md           # ✅ Created
├── README.md                        # ✅ Created
└── BpmnWorkflowDesigner.sln        # ✅ Created
```

---

## What's Next?

### Immediate Next Steps (Phase 1 - Week 1)

1. **Create Domain Entities** (30 minutes)
   - Create `Workflow.cs`, `User.cs`, `Role.cs`, `Department.cs` in Domain project
   - See IMPLEMENTATION_PLAN.md Phase 1.3 for details

2. **Setup DbContext** (30 minutes)
   - Create `ApplicationDbContext.cs` in Infrastructure project
   - Configure entity relationships
   - See ARCHITECTURE.md Section 3.2 for entity definitions

3. **Create Initial Migration** (15 minutes)
   ```powershell
   cd Server/BpmnWorkflow.API
   dotnet ef migrations add InitialCreate --project ../BpmnWorkflow.Infrastructure
   dotnet ef database update
   ```

4. **Seed Initial Data** (15 minutes)
   - Create `DbInitializer.cs` to seed roles and admin user
   - Run seeding on application startup

### This Week's Goals

- [ ] Complete domain model
- [ ] Setup database with migrations
- [ ] Create repository interfaces
- [ ] Implement basic API structure
- [ ] Setup JWT authentication

### Resources

- **Architecture Details**: See `ARCHITECTURE.md`
- **Full Implementation Guide**: See `IMPLEMENTATION_PLAN.md`
- **API Documentation**: Will be available at `/swagger` after API is running
- **BPMN 2.0 Spec**: https://www.omg.org/spec/BPMN/2.0/
- **bpmn-js Docs**: https://bpmn.io/toolkit/bpmn-js/

---

## Getting Help

If you encounter issues:

1. **Check the documentation**
   - ARCHITECTURE.md for system design
   - IMPLEMENTATION_PLAN.md for step-by-step guide
   - README.md for general information

2. **Common issues**
   - See "Troubleshooting Quick Fixes" section above
   - Check README.md "Troubleshooting" section

3. **Still stuck?**
   - Review error messages carefully
   - Check package versions compatibility
   - Ensure all prerequisites are installed

---

## Success Indicators

You're on the right track if:
- ✅ Solution builds without errors (`dotnet build`)
- ✅ All projects are referenced correctly
- ✅ Database connection works
- ✅ NuGet packages restored successfully

---

**Ready to start building? Open IMPLEMENTATION_PLAN.md and begin with Phase 1!**

---

*Last Updated: 2025-12-18*
*Version: 1.0*

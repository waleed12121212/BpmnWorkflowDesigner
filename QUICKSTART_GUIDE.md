# Quick Start Guide - BPMN Workflow Designer

## Prerequisites
- ✅ .NET 8 SDK
- ✅ SQL Server (LocalDB or full instance)
- ⚠️ Camunda 8 (optional - worker is disabled)

---

## Step 1: Start the API Server

### Option A: Using Visual Studio
1. Open `Server/BpmnWorkflow.API/BpmnWorkflow.API.csproj`
2. Press F5 or click "Run"

### Option B: Using Command Line
```powershell
cd "Server\BpmnWorkflow.API"
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7225
      Now listening on: http://localhost:5289
```

⚠️ **Important**: Keep this terminal window open!

---

## Step 2: Start the Blazor Client

### Open a NEW terminal window:

```powershell
cd "Client\BpmnWorkflow.Client"
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
```

---

## Step 3: Open the Application

1. Open your browser
2. Navigate to: **https://localhost:7001**
3. You should see the login page

---

## Step 4: Login

Use the default admin credentials:

- **Username**: `admin`
- **Password**: `admin123`

Click "Login" → You should be redirected to the Dashboard

---

## Verification Checklist

### ✅ API Server is Running
- Open: https://localhost:7225/swagger
- You should see the Swagger UI

### ✅ Database is Seeded
- Check API logs for: `"User admin logged in successfully"`
- If you see database errors, run migrations:
  ```powershell
  cd Server\BpmnWorkflow.API
  dotnet ef database update
  ```

### ✅ No 401 Errors
- Open browser DevTools (F12)
- Go to Console tab
- Navigate around the app
- **Should NOT see**: `401 (Unauthorized)` errors

### ✅ No 500 Errors on Login
- If login fails, check API logs
- Look for BCrypt or password-related errors

---

## Common Issues

### Issue: "Failed to bind to address... address already in use"
**Solution**: Another instance is already running
```powershell
# Find and kill the process
netstat -ano | findstr :7225
taskkill /PID <PID> /F
```

### Issue: Database connection errors
**Solution**: Update connection string in `Server/BpmnWorkflow.API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=BpmnWorkflow;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Issue: 401 errors after login
**Solution**: Clear browser local storage
1. F12 → Application → Local Storage
2. Right-click → Clear
3. Refresh page and login again

---

## Project Structure

```
BPMN Workflow Designer/
├── Client/
│   └── BpmnWorkflow.Client/          # Blazor WebAssembly app
│       ├── Pages/                     # Razor pages
│       ├── Services/                  # API client services
│       └── Program.cs                 # Client startup
│
├── Server/
│   ├── BpmnWorkflow.API/              # ASP.NET Core Web API
│   │   ├── Controllers/               # API endpoints
│   │   └── Program.cs                 # API startup
│   │
│   ├── BpmnWorkflow.Application/      # Business logic
│   │   └── Services/                  # Domain services
│   │
│   ├── BpmnWorkflow.Domain/           # Entities & interfaces
│   │
│   └── BpmnWorkflow.Infrastructure/   # Data access & external services
│       ├── Data/                      # EF Core DbContext
│       └── Background/                # Background workers
│
└── AUTHENTICATION_FIX_SUMMARY.md      # Recent fixes documentation
```

---

## Development Workflow

### Making Changes to the Client
1. Edit files in `Client/BpmnWorkflow.Client/`
2. Save → Hot reload should apply changes automatically
3. If not, restart the client: `Ctrl+C` → `dotnet run`

### Making Changes to the API
1. Edit files in `Server/BpmnWorkflow.API/` or `Server/BpmnWorkflow.Application/`
2. Save → Hot reload should apply changes automatically
3. If not, restart the API: `Ctrl+C` → `dotnet run`

### Database Changes
1. Edit entities in `Server/BpmnWorkflow.Domain/Entities/`
2. Create migration:
   ```powershell
   cd Server\BpmnWorkflow.API
   dotnet ef migrations add YourMigrationName
   ```
3. Apply migration:
   ```powershell
   dotnet ef database update
   ```

---

## Stopping the Application

1. Go to each terminal window
2. Press `Ctrl+C`
3. Wait for graceful shutdown

---

## Next Steps

### Explore Features
- ✅ Dashboard - View all workflows
- ✅ Workflow Editor - Create/edit BPMN diagrams
- ✅ Form Builder - Design forms
- ✅ DMN Editor - Create decision tables
- ⚠️ Camunda Integration - Requires Camunda to be running

### Enable Camunda (Optional)
1. Download Camunda 8 or use Docker
2. Start Camunda on port 8081
3. Uncomment worker in `CamundaWorker.cs`
4. Restart API server

---

## Support

### Check Logs
- **API Logs**: Terminal where you ran `dotnet run` for the API
- **Client Logs**: Browser DevTools → Console
- **Database Logs**: Check SQL Server logs

### Debug Mode
Run with debugger attached:
```powershell
# API
cd Server\BpmnWorkflow.API
dotnet run --launch-profile "https"

# Client  
cd Client\BpmnWorkflow.Client
dotnet run
```

---

**Last Updated**: 2025-12-23  
**Status**: ✅ Ready for development

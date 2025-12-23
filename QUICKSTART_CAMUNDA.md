# 🚀 Quick Start - Camunda Integration

## ⚡ Fast Track (5 Minutes)

### 1. Start Camunda (1 minute)
```powershell
cd "c:\Users\user\Desktop\New folder\BPMN Workflow Designer"
docker-compose up -d
```

### 2. Wait for Camunda to Start (30 seconds)
```powershell
# Check status
docker-compose ps

# Should show camunda as "Up"
```

### 3. Verify Camunda (30 seconds)
Open browser: http://localhost:8080/camunda
- Login: `demo` / `demo`
- You should see Camunda Cockpit

### 4. Run Database Migration (1 minute)
```powershell
cd "Server\BpmnWorkflow.API"
dotnet ef migrations add AddCamundaIntegration --project ..\BpmnWorkflow.Infrastructure
dotnet ef database update
```

### 5. Restore & Build (2 minutes)
```powershell
cd "c:\Users\user\Desktop\New folder\BPMN Workflow Designer"
dotnet restore
dotnet build
```

### 6. Run Application
```powershell
# Terminal 1: API
cd "Server\BpmnWorkflow.API"
dotnet run

# Terminal 2: Client
cd "Client\BpmnWorkflow.Client"
dotnet run
```

### 7. Test Integration
```powershell
# Test Camunda health
curl http://localhost:7225/api/camunda/health
```

## ✅ Success!

If all steps completed without errors, you're ready to:
- Deploy BPMN workflows to Camunda
- Start process instances
- Manage user tasks
- Monitor running processes

## 📖 Next Steps

1. Read `CAMUNDA_SETUP.md` for detailed usage
2. Check `CAMUNDA_INTEGRATION.md` for architecture
3. Review API endpoints in `CAMUNDA_INSTALLATION_SUMMARY.md`

## 🆘 Problems?

- **Docker not starting?** Make sure Docker Desktop is running
- **Port conflicts?** Check if port 8080 or 5432 are in use
- **Migration fails?** Check SQL Server connection string
- **Build errors?** Run `dotnet restore` again

See `CAMUNDA_SETUP.md` for detailed troubleshooting.

---

**Happy Process Automation! 🎉**

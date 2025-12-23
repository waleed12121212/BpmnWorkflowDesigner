# Quick Start Guide - BPMN Workflow Designer

## 🚀 Quick Start (Recommended)

**Double-click** the `start-dev.bat` file in the root folder. This will automatically start both servers.

## 📋 Manual Start

### Step 1: Start API Server
```powershell
cd "Server\BpmnWorkflow.API"
dotnet run
```
Wait for: `Now listening on: https://localhost:7225`

### Step 2: Start Client
```powershell
cd "Client\BpmnWorkflow.Client"
dotnet run
```
Wait for: `Now listening on: https://localhost:7096`

### Step 3: Open Browser
Navigate to: `https://localhost:7096`

## ✅ Testing Camunda Deployment

1. **Login** to the application
2. **Create or open** a workflow
3. **Save** the workflow first (important!)
4. Click **"Deploy to Camunda"** button
5. Should see success message ✓

## 🔧 What Was Fixed

The `CamundaClientService` was not configured with the correct API base URL. This has been fixed in `Client/BpmnWorkflow.Client/Program.cs`.

## ⚠️ Important Notes

- ✅ API Server **must be running** before deploying
- ✅ Workflow **must be saved** before deploying
- ✅ Camunda Engine should be running (default: `http://localhost:8080/engine-rest/`)

## 🐛 Troubleshooting

### Still getting 404?
- Check API server is running on port 7225
- Rebuild the client: `dotnet build` in Client folder
- Clear browser cache and reload

### Camunda connection error?
- Verify Camunda Engine is running
- Check `appsettings.json` for correct Camunda URL

## 📁 Modified Files
- `Client/BpmnWorkflow.Client/Program.cs` ✓
- `start-dev.bat` (new) ✓

# 🔧 Camunda Integration - Build & Run Guide

## ✅ الإصلاحات المطبقة

### Fix 1: Using Directives
تم إضافة `@using` directives في `_Imports.razor`:
```razor
@using BpmnWorkflow.Client.Models
@using BpmnWorkflow.Client.Services
@using BpmnWorkflow.Client.Components.Camunda
@using BpmnWorkflow.Client.Pages.Camunda
```

### Fix 2: RadzenTree Generic Type
تم إصلاح `ProcessDetailsDialog.razor`:
```razor
<RadzenTree Data="@GetTreeData()" TItem="TreeNode" Style="width: 100%;">
    <RadzenTreeLevel TItem="TreeNode" TextProperty="Text" ChildrenProperty="Children" HasChildren="@(e => e.Children?.Any() == true)" />
</RadzenTree>
```

### Fix 3: CSS Reference
تم إضافة في `index.html`:
```html
<link rel="stylesheet" href="css/camunda.css" />
```

## 🚀 خطوات التشغيل

### 1. Clean & Restore
```powershell
cd "c:\Users\user\Desktop\New folder\BPMN Workflow Designer"

# Clean solution
dotnet clean

# Restore packages
dotnet restore
```

### 2. Build Solution
```powershell
# Build entire solution
dotnet build

# أو Build كل مشروع على حدة
cd "Server\BpmnWorkflow.API"
dotnet build

cd "..\..\Client\BpmnWorkflow.Client"
dotnet build
```

### 3. Start Camunda
```powershell
cd "c:\Users\user\Desktop\New folder\BPMN Workflow Designer"
docker-compose up -d

# تحقق من الحالة
docker-compose ps

# انتظر حتى يصبح Camunda جاهزاً (30-60 ثانية)
```

### 4. Database Migration
```powershell
cd "Server\BpmnWorkflow.API"

# إنشاء Migration
dotnet ef migrations add AddCamundaIntegration --project ..\BpmnWorkflow.Infrastructure

# تطبيق Migration
dotnet ef database update
```

### 5. Run Backend
```powershell
cd "Server\BpmnWorkflow.API"
dotnet run
```

**المتوقع:**
```
Now listening on: https://localhost:7225
Now listening on: http://localhost:5225
```

### 6. Run Frontend (Terminal جديد)
```powershell
cd "Client\BpmnWorkflow.Client"
dotnet run
```

**المتوقع:**
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

### 7. Test في المتصفح
افتح:
```
https://localhost:5001/camunda/dashboard
```

## 🐛 استكشاف الأخطاء

### خطأ: "Type or namespace name not found"

**السبب:** الـ build لم يتم بعد أو هناك مشكلة في الـ references

**الحل:**
```powershell
# 1. Clean
dotnet clean

# 2. Restore
dotnet restore

# 3. Build
dotnet build

# 4. إذا استمرت المشكلة، أعد تشغيل VS Code/Visual Studio
```

### خطأ: "Children property not found"

**السبب:** RadzenTree يحتاج generic type

**الحل:** تم الإصلاح في `ProcessDetailsDialog.razor` - تأكد من الـ build

### خطأ: "Camunda service not registered"

**السبب:** لم يتم تسجيل الخدمة في `Program.cs`

**الحل:** تم الإصلاح - تأكد من:
```csharp
builder.Services.AddScoped<CamundaClientService>();
```

### خطأ: "Cannot connect to Camunda"

**السبب:** Camunda غير مشغل

**الحل:**
```powershell
# تحقق من Docker
docker-compose ps

# إذا لم يكن مشغلاً
docker-compose up -d

# تحقق من logs
docker-compose logs camunda
```

## ✅ Verification Checklist

بعد التشغيل، تحقق من:

- [ ] Backend يعمل على https://localhost:7225
- [ ] Frontend يعمل على https://localhost:5001
- [ ] Camunda يعمل على http://localhost:8080/camunda
- [ ] لا توجد أخطاء في Console
- [ ] Dashboard يظهر بشكل صحيح
- [ ] يمكن الوصول إلى Process Instances page
- [ ] يمكن الوصول إلى Task List page

## 📝 ملاحظات مهمة

### 1. Build Order
يجب build المشاريع بالترتيب:
1. Domain
2. Application
3. Infrastructure
4. API
5. Client

`dotnet build` في الـ root يفعل ذلك تلقائياً.

### 2. Hot Reload
عند تعديل ملفات Razor:
- Frontend: Hot reload يعمل تلقائياً
- Backend: قد تحتاج إعادة تشغيل

### 3. Database
تأكد من:
- SQL Server يعمل
- Connection string صحيح في `appsettings.json`
- Migration تم تطبيقه

### 4. Camunda
تأكد من:
- Docker Desktop يعمل
- Port 8080 غير مستخدم
- PostgreSQL container يعمل

## 🎯 Quick Test

بعد التشغيل، جرب:

```powershell
# Test 1: Backend Health
curl https://localhost:7225/api/camunda/health

# Test 2: Camunda Direct
curl http://localhost:8080/engine-rest/engine

# Test 3: Frontend
# افتح المتصفح على https://localhost:5001/camunda/dashboard
```

## 📚 الملفات المهمة

- `_Imports.razor` - Using directives
- `Program.cs` (Client) - Service registration
- `Program.cs` (Server) - Camunda configuration
- `appsettings.json` - Camunda settings
- `docker-compose.yml` - Infrastructure

## 🆘 لا يزال لديك مشاكل؟

1. **تحقق من الـ Output في VS Code/Visual Studio**
2. **راجع Browser Console للأخطاء**
3. **تحقق من Terminal logs**
4. **جرب Clean & Rebuild**
5. **أعد تشغيل Docker containers**

---

**بعد اتباع هذه الخطوات، يجب أن يعمل كل شيء بشكل صحيح!** ✅

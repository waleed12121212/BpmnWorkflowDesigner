# كيفية تشغيل التطبيق | How to Run the Application

## المشكلة التي تم حلها | Problem Solved
كانت المشكلة "TypeError: Failed to fetch" تحدث بسبب عدم تشغيل الـ API Server على المنفذ الصحيح.

The "TypeError: Failed to fetch" error was occurring because the API Server was not running on the correct port.

## الحل | Solution

### 1. تشغيل الـ API Server (الخادم الخلفي)
يجب تشغيل الـ API Server على المنفذ `https://localhost:7225` باستخدام الأمر التالي:

**PowerShell:**
```powershell
dotnet run --project "Server\BpmnWorkflow.API\BpmnWorkflow.API.csproj" --launch-profile https
```

**أو من داخل مجلد الـ API:**
```powershell
cd Server\BpmnWorkflow.API
dotnet run --launch-profile https
```

### 2. تشغيل الـ Blazor Client (التطبيق الأمامي)
في نافذة PowerShell أخرى، قم بتشغيل:

```powershell
dotnet run --project "Client\BpmnWorkflow.Client\BpmnWorkflow.Client.csproj"
```

**أو من داخل مجلد الـ Client:**
```powershell
cd Client\BpmnWorkflow.Client
dotnet run
```

## التحقق من أن كل شيء يعمل | Verification

### 1. التحقق من الـ API Server
يجب أن ترى في الـ terminal:
```
Now listening on: https://localhost:7225
Now listening on: http://localhost:5289
```

### 2. اختبار الـ API
```powershell
curl -k https://localhost:7225/api/workflows
```

يجب أن تحصل على قائمة بالـ workflows بصيغة JSON.

### 3. فتح التطبيق
افتح المتصفح على العنوان الذي يظهر في terminal الـ Blazor Client (عادة `https://localhost:7096`)

## ملاحظات مهمة | Important Notes

1. **يجب تشغيل الـ API Server أولاً** قبل تشغيل الـ Blazor Client
2. **استخدم `--launch-profile https`** للـ API Server للتأكد من تشغيله على المنفذ الصحيح
3. إذا واجهت مشكلة "address already in use"، تحقق من العمليات التي تستخدم المنفذ:
   ```powershell
   Get-NetTCPConnection -LocalPort 7225,5289 | Select-Object LocalPort, State, OwningProcess
   ```

## إعدادات المنافذ | Port Configuration

- **API Server (HTTPS):** `https://localhost:7225`
- **API Server (HTTP):** `http://localhost:5289`
- **Blazor Client (HTTPS):** `https://localhost:7096`
- **Blazor Client (HTTP):** `http://localhost:5028`

## الملفات المهمة | Important Files

- **API Launch Settings:** `Server\BpmnWorkflow.API\Properties\launchSettings.json`
- **Client Launch Settings:** `Client\BpmnWorkflow.Client\Properties\launchSettings.json`
- **Client API Configuration:** `Client\BpmnWorkflow.Client\wwwroot\appsettings.json`

---

## الحالة الحالية | Current Status

✅ **API Server يعمل على:** `https://localhost:7225`  
✅ **تم اختبار إنشاء workflow جديد بنجاح**  
✅ **تم اختبار قراءة workflows بنجاح**  

**التطبيق جاهز للاستخدام!** | **The application is ready to use!**

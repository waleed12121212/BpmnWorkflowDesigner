# حل مشكلة 404 - Camunda Deployment

## المشكلة
كانت المشكلة أن خدمة `CamundaClientService` لم تكن مُهيأة بشكل صحيح للاتصال بـ API Server. بالإضافة إلى ذلك، يجب التأكد من أن API Server يعمل.

## الحلول المطبقة

### 1. إصلاح تسجيل CamundaClientService
تم تحديث ملف `Client/BpmnWorkflow.Client/Program.cs` لضمان أن `CamundaClientService` يستخدم HttpClient المُهيأ بشكل صحيح مع عنوان API الصحيح.

**قبل:**
```csharp
builder.Services.AddScoped<CamundaClientService>();
```

**بعد:**
```csharp
builder.Services.AddScoped<CamundaClientService>(sp => 
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("BpmnWorkflow.API");
    return new CamundaClientService(httpClient);
});
```

### 2. التأكد من تشغيل API Server

## كيفية تشغيل التطبيق

### الطريقة السهلة (مُوصى بها)
قم بتشغيل ملف `start-dev.bat` الذي تم إنشاؤه في المجلد الرئيسي. هذا الملف سيقوم بـ:
1. تشغيل API Server على المنفذ 7225
2. تشغيل Client Application على المنفذ 7096
3. فتح نافذتين منفصلتين لكل خادم

```batch
start-dev.bat
```

### الطريقة اليدوية

#### تشغيل API Server (Backend)
افتح نافذة PowerShell أو Command Prompt:
```bash
cd "Server\BpmnWorkflow.API"
dotnet run
```

انتظر حتى ترى رسالة مثل:
```
Now listening on: https://localhost:7225
```

#### تشغيل Client (Frontend)
افتح نافذة PowerShell أو Command Prompt أخرى:
```bash
cd "Client\BpmnWorkflow.Client"
dotnet run
```

انتظر حتى ترى رسالة مثل:
```
Now listening on: https://localhost:7096
```

## التحقق من الحل

1. افتح المتصفح على `https://localhost:7096`
2. سجل الدخول إلى التطبيق
3. افتح أو أنشئ Workflow
4. احفظ الـ Workflow أولاً
5. اضغط على زر "Deploy to Camunda"
6. يجب أن يعمل الآن بدون خطأ 404

## ملاحظات مهمة

- تأكد من أن **API Server يعمل** قبل محاولة Deploy
- تأكد من **حفظ الـ Workflow** قبل محاولة Deploy
- تأكد من أن **Camunda Engine** يعمل (إذا كنت تستخدم Camunda حقيقي)
- العنوان الافتراضي لـ Camunda Engine هو: `http://localhost:8080/engine-rest/`

## استكشاف الأخطاء

### إذا استمر خطأ 404:
1. تحقق من أن API Server يعمل على المنفذ 7225
2. افتح Developer Tools في المتصفح (F12)
3. تحقق من علامة التبويب Network
4. ابحث عن الطلب الفاشل وتحقق من الـ URL الكامل

### إذا ظهر خطأ اتصال بـ Camunda:
1. تأكد من أن Camunda Engine يعمل
2. تحقق من إعدادات Camunda في `appsettings.json`:
   ```json
   "Camunda": {
     "BaseUrl": "http://localhost:8080/engine-rest/",
     "Username": "",
     "Password": ""
   }
   ```

## الملفات المعدلة
- ✅ `Client/BpmnWorkflow.Client/Program.cs` - إصلاح تسجيل CamundaClientService
- ✅ `start-dev.bat` - ملف جديد لتشغيل كلا الخادمين

## الخطوات التالية
بعد تشغيل التطبيق بنجاح، يمكنك:
1. إنشاء Workflows جديدة
2. Deploy إلى Camunda
3. بدء Process Instances
4. إدارة User Tasks

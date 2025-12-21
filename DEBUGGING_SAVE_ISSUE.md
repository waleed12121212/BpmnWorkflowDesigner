# تشخيص مشكلة حفظ الرسمة | Diagnosing the Save Issue

## التغييرات التي تم إجراؤها | Changes Made

### 1. إضافة Logging في JavaScript
تم إضافة console.log في ملف `bpmn-modeler.js` لتتبع عملية تصدير XML و SVG:
- عند تصدير XML، سيظهر طول الـ XML ومعاينة للمحتوى
- عند تصدير SVG، سيظهر طول الـ SVG

### 2. إضافة معالجة أفضل للأخطاء في WorkflowEditor
تم تحسين دالة `SaveAsync` في `WorkflowEditor.razor`:
- إضافة try-catch شامل
- إضافة رسائل console.log لتتبع كل خطوة
- إضافة رسائل خطأ أوضح للمستخدم

### 3. إضافة صفحة Debug
تم إنشاء صفحة جديدة `WorkflowDebug.razor` لعرض:
- تفاصيل الـ Workflow المحفوظ
- الـ BPMN XML الكامل
- الـ SVG Preview (إن وُجد)
- طول كل من XML و SVG

### 4. إضافة زر Debug
تم إضافة زر "View Debug" في صفحة WorkflowEditor للانتقال إلى صفحة Debug

## كيفية اختبار المشكلة | How to Test

### الخطوة 1: افتح Console في المتصفح
1. افتح المتصفح (Chrome/Edge)
2. اضغط F12 لفتح Developer Tools
3. انتقل إلى تبويب "Console"

### الخطوة 2: أنشئ Workflow جديد
1. انتقل إلى `/workflow-editor`
2. ارسم workflow (أضف عناصر BPMN مثل Tasks, Gateways, Events)
3. اضغط على زر "Save"

### الخطوة 3: راقب Console
يجب أن ترى الرسائل التالية:
```
Starting save operation...
Exporting XML...
XML exported successfully, length: [number]
XML preview: [first 200 characters]
XML Export - Success: true, XML Length: [number]
Exporting SVG...
SVG exported successfully, length: [number]
SVG Export - Success: true, SVG Length: [number]
Saving workflow: [name], WorkflowId: [guid or null]
Workflow created successfully: [guid]
```

### الخطوة 4: افتح صفحة Debug
1. بعد الحفظ، اضغط على زر "View Debug"
2. تحقق من:
   - هل الـ BPMN XML موجود؟
   - هل طول الـ XML معقول (أكثر من 100 حرف)؟
   - هل الـ SVG Preview موجود؟

### الخطوة 5: تحقق من قائمة Workflows
1. ارجع إلى `/workflows`
2. تحقق من أن الـ workflow الجديد موجود في القائمة
3. اضغط على زر "Edit" للـ workflow
4. تحقق من أن الرسمة تظهر بشكل صحيح

## الأخطاء المحتملة | Possible Errors

### 1. "Modeler not initialized"
**السبب:** لم يتم تحميل bpmn-js بشكل صحيح
**الحل:** تحقق من أن ملف `index.html` يحتوي على:
```html
<script src="https://unpkg.com/bpmn-js@17.11.1/dist/bpmn-navigated-viewer.production.min.js"></script>
```

### 2. "Could not export XML from diagram"
**السبب:** فشل تصدير XML من المحرر
**الحل:** افحص console للحصول على تفاصيل الخطأ

### 3. "Failed to create workflow"
**السبب:** الـ API لم يستجب أو رفض الطلب
**الحل:** 
- تحقق من أن الـ API Server يعمل على `https://localhost:7225`
- افحص Network tab في Developer Tools
- تحقق من logs الـ API Server

## ملاحظات مهمة | Important Notes

1. **الـ XML يجب أن يحتوي على جميع العناصر:** عندما ترسم workflow معقد، يجب أن يكون الـ XML أطول من الـ XML الافتراضي (الذي يحتوي فقط على StartEvent)

2. **الـ SVG اختياري:** إذا فشل تصدير SVG، سيتم حفظ الـ workflow بدون SVG Preview

3. **التحديثات التلقائية:** بعد إنشاء workflow جديد، سيتم إعادة توجيهك إلى صفحة التحرير مع الـ ID الجديد

## الخطوات التالية | Next Steps

إذا كانت المشكلة لا تزال موجودة:
1. شارك محتوى Console بالكامل
2. شارك محتوى Network tab (طلب POST إلى `/api/workflows`)
3. شارك محتوى صفحة Debug
4. شارك لقطة شاشة للرسمة التي تحاول حفظها

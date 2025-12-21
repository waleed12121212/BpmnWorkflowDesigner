with open(r'c:\Users\user\Desktop\New folder\BPMN Workflow Designer\Client\BpmnWorkflow.Client\Pages\WorkflowEditor.razor', 'r', encoding='utf-8') as f:
    lines = f.readlines()

brace_count = 0
for line_num, line in enumerate(lines, 1):
    for char in line:
        if char == '{':
            brace_count += 1
        elif char == '}':
            brace_count -= 1
    if brace_count != 0:
        # print(f"Line {line_num}: count {brace_count}")
        pass

print(f"Total count: {brace_count}")

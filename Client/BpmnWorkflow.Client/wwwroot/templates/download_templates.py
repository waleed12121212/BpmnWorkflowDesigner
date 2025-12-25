import json
import urllib.request
import os

urls = [
    "https://raw.githubusercontent.com/camunda/connectors/main/connectors/http/rest/element-templates/http-json-connector.json",
    "https://raw.githubusercontent.com/camunda/connectors/main/connectors/slack/element-templates/slack-outbound-connector.json",
    "https://raw.githubusercontent.com/camunda/connectors/main/connectors/sendgrid/element-templates/sendgrid-outbound-connector.json",
    "https://raw.githubusercontent.com/camunda/connectors/main/connectors/aws/aws-lambda/element-templates/aws-lambda-outbound-connector.json"
]

all_templates = []

for url in urls:
    try:
        print(f"Downloading {url}...")
        with urllib.request.urlopen(url) as response:
            data = json.loads(response.read().decode('utf-8'))
            if isinstance(data, list):
                all_templates.extend(data)
            else:
                all_templates.append(data)
    except Exception as e:
        print(f"Failed to download {url}: {e}")

output_path = r"c:\Users\user\Desktop\New folder\BPMN Workflow Designer\Client\BpmnWorkflow.Client\wwwroot\templates\all-connectors.json"
with open(output_path, "w", encoding="utf-8") as f:
    json.dump(all_templates, f, indent=2)

print(f"Saved {len(all_templates)} templates to {output_path}")

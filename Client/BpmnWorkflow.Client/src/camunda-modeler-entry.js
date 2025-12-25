import BpmnModeler from 'camunda-bpmn-js/lib/camunda-cloud/Modeler';

// Import necessary styles from camunda-bpmn-js which bundles many of these
import 'camunda-bpmn-js/dist/assets/camunda-cloud-modeler.css';
import 'camunda-bpmn-js/dist/assets/properties-panel.css';
import 'camunda-bpmn-js/dist/assets/element-templates.css';

console.log('Camunda Modeler Bundle Loading...');

let modelerInstance = null;
let currentTemplates = [];

export function init(containerId, propertiesPanelId, templates) {
    console.log('Initializing Camunda Modeler in', containerId);
    if (modelerInstance) {
        modelerInstance.destroy();
    }

    if (templates) {
        currentTemplates = templates;
        console.log('Templates provided:', templates.length);
    } else {
        console.warn('No templates provided during init');
    }

    modelerInstance = new BpmnModeler({
        container: '#' + containerId,
        propertiesPanel: {
            parent: '#' + propertiesPanelId
        },
        keyboard: {
            bindTo: window
        },
        elementTemplates: currentTemplates
    });

    console.log('Modeler initialized with', currentTemplates.length, 'templates');

    // Create a default diagram
    modelerInstance.createDiagram();
}

export function setElementTemplates(templates) {
    currentTemplates = templates;
    if (modelerInstance) {
        const elementTemplates = modelerInstance.get('elementTemplates');
        elementTemplates.set(templates);
    }
}

export async function importXML(xml) {
    if (modelerInstance) {
        try {
            await modelerInstance.importXML(xml);
        } catch (err) {
            console.error('Error importing XML:', err);
        }
    }
}

export async function getXML() {
    if (modelerInstance) {
        try {
            const result = await modelerInstance.saveXML({ format: true });
            return result.xml;
        } catch (err) {
            console.error('Error saving XML:', err);
            return null;
        }
    }
    return null;
}

export async function loadTemplates(url) {
    try {
        const response = await fetch(url);
        const templates = await response.json();
        setElementTemplates(templates);
        console.log('Templates loaded from', url, ':', templates.length);
        return templates;
    } catch (err) {
        console.error('Error loading templates:', err);
        return null;
    }
}

export function triggerOpenFile() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.bpmn, .xml';
    input.onchange = e => {
        const file = e.target.files[0];
        const reader = new FileReader();
        reader.onload = async (readerEvent) => {
            const content = readerEvent.target.result;
            await importXML(content);
        };
        reader.readAsText(file, 'UTF-8');
    };
    input.click();
}

// Global utility for downloading
window.downloadFileFromStream = function (filename, content) {
    const blob = new Blob([content], { type: 'application/xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    setTimeout(() => {
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    }, 0);
};

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

export async function exportXML() {
    if (!modelerInstance) return null;
    const { xml } = await modelerInstance.saveXML({ format: true });
    return xml;
}

export function triggerOpenFile() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.bpmn, .xml';
    input.onchange = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = async (event) => {
            const xml = event.target.result;
            await importXML(xml);
        };
        reader.readAsText(file);
    };
    input.click();
}

export function downloadFileFromStream(fileName, content) {
    const blob = new Blob([content], { type: 'application/xml' });
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? 'diagram.bpmn';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

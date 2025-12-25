window.BpmnModeler = (function () {
    let modeler = null;

    function createEmptyDiagram() {
        const processId = 'Process_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();
        return `<?xml version="1.0" encoding="UTF-8"?>
    <bpmn:definitions xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
        xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"
        xmlns:dc="http://www.omg.org/spec/DD/20100524/DC"
        xmlns:camunda="http://camunda.org/schema/1.0/bpmn"
        id="Definitions_1"
        targetNamespace="http://bpmn.io/schema/bpmn">
        <bpmn:process id="${processId}" isExecutable="true" camunda:historyTimeToLive="180">
            <bpmn:startEvent id="StartEvent_1" />
        </bpmn:process>
        <bpmndi:BPMNDiagram id="BPMNDiagram_1">
            <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="${processId}">
                <bpmndi:BPMNShape id="_BPMNShape_StartEvent_2" bpmnElement="StartEvent_1">
                    <dc:Bounds x="173" y="102" width="36" height="36" />
                </bpmndi:BPMNShape>
            </bpmndi:BPMNPlane>
        </bpmndi:BPMNDiagram>
    </bpmn:definitions>`;
    }

    return {
        getEmptyDiagramXML: function () {
            return createEmptyDiagram();
        },
        initialize: function (containerId) {
            const container = document.getElementById(containerId);

            // Check for BPMN library
            const BpmnConstructor = window.BpmnJS || window.BpmnModeler || window.Modeler;

            if (!container) {
                console.error("Container not found:", containerId);
                return;
            }

            if (!BpmnConstructor) {
                console.error("BPMN library not loaded. Available globals:", Object.keys(window).filter(k => k.toLowerCase().includes('bpmn')));
                return;
            }

            console.log("Initializing BPMN modeler with constructor:", BpmnConstructor.name);

            try {
                modeler = new BpmnConstructor({
                    container: container,
                    additionalModules: [
                        window.BpmnJSMinimap || {},
                        // Grid snapping is often a separate module or built in
                    ],
                    gridSnapping: { active: true }
                });

                // Show minimap by default if module is available
                if (modeler.get('minimap', false)) {
                    modeler.get('minimap').open();
                }

                // Add real-time linting
                modeler.on('commandStack.changed', () => {
                    this.validateAndMark();
                });

                console.log("BPMN modeler initialized successfully");
            } catch (e) {
                console.error("Error creating BPMN modeler:", e);
            }
        },
        createNewDiagram: async function () {
            if (!modeler) {
                console.error("Modeler not initialized for createNewDiagram");
                return { success: false, error: "Modeler not initialized" };
            }
            try {
                console.log("Creating new diagram...");
                const emptyDiagram = createEmptyDiagram();
                console.log("Empty diagram XML:", emptyDiagram.substring(0, 200));
                await modeler.importXML(emptyDiagram);
                console.log("New diagram created successfully");
                return { success: true };
            } catch (e) {
                console.error("Error creating new diagram:", e);
                return { success: false, error: e.message };
            }
        },
        importXML: async function (xml) {
            if (!modeler) {
                console.error("Modeler not initialized for importXML");
                return { success: false, error: "Modeler not initialized" };
            }
            try {
                console.log("Importing XML, length:", xml?.length);
                console.log("XML preview:", xml?.substring(0, 200));
                const result = await modeler.importXML(xml);
                console.log("XML imported successfully");
                console.log("Import warnings:", result.warnings);
                return { success: true };
            } catch (e) {
                console.error("Error importing XML:", e);
                console.error("Error details:", e.message);
                return { success: false, error: e.message };
            }
        },
        exportXML: async function () {
            if (!modeler) {
                console.error("Modeler not initialized for exportXML");
                return { success: false, error: "Modeler not initialized" };
            }
            try {
                if (!modeler.getDefinitions()) {
                    return { success: false, error: "No diagram loaded" };
                }
                console.log("Exporting XML...");
                const { xml } = await modeler.saveXML({ format: true });
                console.log("XML exported successfully, length:", xml?.length);
                console.log("XML preview:", xml?.substring(0, 200));
                return { success: true, xml: xml };
            } catch (e) {
                console.error("Error exporting XML:", e);
                return { success: false, error: e.message };
            }
        },
        exportSVG: async function () {
            if (!modeler) {
                console.error("Modeler not initialized for exportSVG");
                return { success: false, error: "Modeler not initialized" };
            }
            try {
                if (!modeler.getDefinitions()) {
                    return { success: false, error: "No diagram loaded" };
                }
                console.log("Exporting SVG...");
                const { svg } = await modeler.saveSVG();
                console.log("SVG exported successfully, length:", svg?.length);
                return { success: true, svg: svg };
            } catch (e) {
                console.error("Error exporting SVG:", e);
                return { success: false, error: e.message };
            }
        },
        exportPNG: async function () {
            if (!modeler) return { success: false };
            try {
                const { svg } = await modeler.saveSVG();
                const canvas = document.createElement('canvas');
                const svgBlob = new Blob([svg], { type: 'image/svg+xml;charset=utf-8' });
                const url = URL.createObjectURL(svgBlob);

                const img = new Image();
                return new Promise((resolve) => {
                    img.onload = function () {
                        canvas.width = img.width * 2; // High DPI
                        canvas.height = img.height * 2;
                        const ctx = canvas.getContext('2d');
                        ctx.fillStyle = 'white';
                        ctx.fillRect(0, 0, canvas.width, canvas.height);
                        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
                        URL.revokeObjectURL(url);
                        resolve({ success: true, dataUrl: canvas.toDataURL('image/png') });
                    };
                    img.onerror = () => resolve({ success: false });
                    img.src = url;
                });
            } catch (e) {
                return { success: false, error: e.message };
            }
        },
        exportPDF: async function () {
            if (!modeler) {
                return { success: false, error: "Modeler not initialized" };
            }
            try {
                const { svg } = await modeler.saveSVG();
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');

                // Use canvg to parse SVG and draw to canvas
                const v = canvg.Canvg.fromString(ctx, svg);
                await v.render();

                const imgData = canvas.toDataURL('image/png');
                const { jsPDF } = window.jspdf;
                const pdf = new jsPDF({
                    orientation: canvas.width > canvas.height ? 'l' : 'p',
                    unit: 'px',
                    format: [canvas.width, canvas.height]
                });

                pdf.addImage(imgData, 'PNG', 0, 0, canvas.width, canvas.height);
                const pdfBase64 = pdf.output('datauristring').split(',')[1];

                return { success: true, pdfBase64: pdfBase64 };
            } catch (e) {
                console.error("Error exporting PDF:", e);
                return { success: false, error: e.message };
            }
        },
        zoomIn: function () {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const canvas = modeler.get('canvas');
                canvas.zoom(canvas.zoom() + 0.1);
                return { success: true };
            } catch (e) {
                console.error("Error zooming in:", e);
                return { success: false, error: e.message };
            }
        },
        zoomOut: function () {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const canvas = modeler.get('canvas');
                canvas.zoom(canvas.zoom() - 0.1);
                return { success: true };
            } catch (e) {
                console.error("Error zooming out:", e);
                return { success: false, error: e.message };
            }
        },
        resetZoom: function () {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const canvas = modeler.get('canvas');
                canvas.zoom('fit-viewport');
                return { success: true };
            } catch (e) {
                console.error("Error resetting zoom:", e);
                return { success: false, error: e.message };
            }
        },
        zoomToActual: function () {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const canvas = modeler.get('canvas');
                canvas.zoom(1.0);
                return { success: true };
            } catch (e) {
                console.error("Error setting actual zoom:", e);
                return { success: false, error: e.message };
            }
        },
        undo: function () {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const commandStack = modeler.get('commandStack');
                commandStack.undo();
                return { success: true };
            } catch (e) {
                console.error("Error performing undo:", e);
                return { success: false, error: e.message };
            }
        },
        redo: function () {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const commandStack = modeler.get('commandStack');
                commandStack.redo();
                return { success: true };
            } catch (e) {
                console.error("Error performing redo:", e);
                return { success: false, error: e.message };
            }
        },
        downloadFile: function (filename, content, contentType) {
            try {
                let blob;
                if (contentType === 'application/pdf' && typeof content === 'string' && !content.startsWith('<?xml')) {
                    // Assume base64 for PDF
                    const byteCharacters = atob(content);
                    const byteNumbers = new Array(byteCharacters.length);
                    for (let i = 0; i < byteCharacters.length; i++) {
                        byteNumbers[i] = byteCharacters.charCodeAt(i);
                    }
                    const byteArray = new Uint8Array(byteNumbers);
                    blob = new Blob([byteArray], { type: contentType });
                } else {
                    blob = new Blob([content], { type: contentType });
                }

                const element = document.createElement('a');
                element.href = URL.createObjectURL(blob);
                element.download = filename;
                document.body.appendChild(element);
                element.click();
                document.body.removeChild(element);
                return { success: true };
            } catch (e) {
                console.error("Error downloading file:", e);
                return { success: false, error: e.message };
            }
        },
        validateDiagram: function () {
            if (!modeler) return { success: false, error: "Modeler not initialized" };

            try {
                const elementRegistry = modeler.get('elementRegistry');
                // Get all flow elements (excluding labels and sequence flows themselves which are connections)
                const elements = elementRegistry.filter(function (element) {
                    if (!element.type) return false;
                    return element.type !== 'label' &&
                        element.type !== 'bpmn:Process' &&
                        element.type !== 'bpmn:Collaboration' &&
                        element.type !== 'bpmn:Participant' &&
                        element.type !== 'bpmn:SequenceFlow' &&
                        element.type !== 'bpmn:Association' &&
                        element.type !== 'bpmn:TextAnnotation' &&
                        !element.type.includes('Definitions');
                });

                const errors = [];
                const warnings = [];

                let hasStart = false;
                let hasEnd = false;

                elements.forEach(element => {
                    if (element.type === 'bpmn:StartEvent') {
                        if (hasStart) {
                            errors.push("Process cannot have multiple Start Events. Please use just one.");
                        }
                        hasStart = true;
                    }
                    if (element.type === 'bpmn:EndEvent') hasEnd = true;

                    // Check for disconnected elements
                    // Ignore StartEvent for incoming check
                    if (element.type !== 'bpmn:StartEvent' && (!element.incoming || element.incoming.length === 0)) {
                        const name = element.businessObject.name || element.id;
                        warnings.push(`Element '${name}'(${element.type.replace('bpmn:', '')}) has no incoming flow.`);
                    }

                    // Ignore EndEvent for outgoing check
                    if (element.type !== 'bpmn:EndEvent' && element.type !== 'bpmn:TerminateEventDefinition' && (!element.outgoing || element.outgoing.length === 0)) {
                        const name = element.businessObject.name || element.id;
                        warnings.push(`Element '${name}'(${element.type.replace('bpmn:', '')}) has no outgoing flow.`);
                    }
                });

                if (!hasStart) errors.push("Diagram must have at least one Start Event.");
                if (!hasEnd) errors.push("Diagram must have at least one End Event.");

                // Check if process is executable (CRITICAL for Camunda)
                const proceses = elementRegistry.filter(e => e.type === 'bpmn:Process');
                proceses.forEach(p => {
                    const bo = p.businessObject;
                    if (bo.isExecutable === false || bo.isExecutable === undefined) {
                        errors.push(`Process '${bo.name || bo.id}' is not marked as executable.It will not appear in Camunda cockpit.`);
                    }
                });

                return { success: true, errors: errors, warnings: warnings };
            } catch (e) {
                console.error("Error validating diagram:", e);
                return { success: false, error: e.message };
            }
        },
        validateAndMark: function () {
            if (!modeler) return;
            try {
                const elementRegistry = modeler.get('elementRegistry');
                const overlays = modeler.get('overlays');
                const canvas = modeler.get('canvas');

                // Clear old markers
                overlays.remove({ type: 'lint-overlay' });
                elementRegistry.forEach(e => canvas.removeMarker(e.id, 'lint-error'));
                elementRegistry.forEach(e => canvas.removeMarker(e.id, 'lint-warning'));

                const results = this.validateDiagram();
                if (!results.success) return;

                // Mark elements with warnings
                const elements = elementRegistry.getAll();
                elements.forEach(element => {
                    if (!element.type) return;

                    const isStart = element.type === 'bpmn:StartEvent';
                    const isEnd = element.type === 'bpmn:EndEvent';
                    const isGateway = element.type.includes('Gateway');
                    const isTask = element.type.includes('Task') || element.type === 'bpmn:SubProcess' || element.type === 'bpmn:CallActivity';

                    let msg = null;
                    let type = 'warning';

                    if (element.type.startsWith('bpmn:') && element.type !== 'bpmn:SequenceFlow' && element.type !== 'bpmn:Process') {
                        if (!isStart && (!element.incoming || element.incoming.length === 0)) {
                            msg = "Missing incoming flow";
                        } else if (!isEnd && (!element.outgoing || element.outgoing.length === 0)) {
                            msg = "Missing outgoing flow";
                        }
                    }

                    if (msg) {
                        canvas.addMarker(element.id, type === 'error' ? 'lint-error' : 'lint-warning');
                        overlays.add(element.id, 'lint-overlay', {
                            position: { top: -10, left: -10 },
                            html: `< div class="lint-badge lint-badge-${type}" title = "${msg}" > <i class="fa fa-exclamation-triangle"></i></div > `,
                            type: 'lint-overlay'
                        });
                    }
                });

            } catch (e) {
                console.error("Linting error:", e);
            }
        },
        registerSelectionListener: function (dotNetReference) {
            if (!modeler) return;
            try {
                modeler.on('selection.changed', function (e) {
                    const selection = e.newSelection;
                    let elementData = null;

                    if (selection.length === 1) {
                        const element = selection[0];
                        const bo = element.businessObject;

                        // Get documentation
                        let doc = '';
                        if (bo.documentation && bo.documentation.length > 0) {
                            doc = bo.documentation[0].text;
                        }

                        // Get condition
                        let condition = '';
                        if (bo.conditionExpression) {
                            condition = bo.conditionExpression.body;
                        }

                        elementData = {
                            id: element.id,
                            type: element.type,
                            name: bo.name || '',
                            documentation: doc,
                            condition: condition,
                            color: element.di.get('stroke') || '#000000',
                            backgroundColor: element.di.get('fill') || '#ffffff',
                            formId: bo.get('formId') || ''
                        };
                    } else if (selection.length === 0) {
                        // If nothing selected, return process info
                        const elementRegistry = modeler.get('elementRegistry');
                        const processElement = elementRegistry.filter(e => e.type === 'bpmn:Process')[0];
                        if (processElement) {
                            const bo = processElement.businessObject;
                            elementData = {
                                id: processElement.id,
                                type: 'bpmn:Process',
                                name: bo.name || '',
                                isExecutable: bo.isExecutable !== false,
                                camundaKey: bo.id
                            };
                        }
                    }

                    dotNetReference.invokeMethodAsync('OnExternalSelectionChanged', elementData);
                });
            } catch (e) {
                console.error("Error registering selection listener:", e);
            }
        },
        updateElementProperty: function (elementId, key, value) {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const elementRegistry = modeler.get('elementRegistry');
                const modeling = modeler.get('modeling');
                const moddle = modeler.get('moddle');

                const element = elementRegistry.get(elementId);

                if (!element) return { success: false, error: "Element not found" };

                const props = {};

                if (key === 'documentation') {
                    const newDocumentation = moddle.create('bpmn:Documentation', { text: value });
                    props.documentation = [newDocumentation];
                    modeling.updateProperties(element, props);
                } else if (key === 'condition') {
                    const newCondition = moddle.create('bpmn:FormalExpression', { body: value });
                    props.conditionExpression = newCondition;
                    modeling.updateProperties(element, props);
                } else if (key === 'color') {
                    modeling.setColor(element, { stroke: value });
                } else if (key === 'backgroundColor') {
                    modeling.setColor(element, { fill: value });
                } else if (key === 'formId') {
                    modeling.updateProperties(element, { formId: value });
                } else {
                    props[key] = value;
                    modeling.updateProperties(element, props);
                }

                return { success: true };
            } catch (e) {
                console.error("Error updating element property:", e);
                return { success: false, error: e.message };
            }
        },
        updateProcessProperty: function (key, value) {
            if (!modeler) return { success: false, error: "Modeler not initialized" };
            try {
                const elementRegistry = modeler.get('elementRegistry');
                const modeling = modeler.get('modeling');

                // Find the process element
                const processElement = elementRegistry.filter(e => e.type === 'bpmn:Process')[0];
                if (!processElement) return { success: false, error: "Process element not found" };

                const props = {};
                props[key] = value;

                if (key === 'id') {
                    // Updating ID is tricky in bpmn-js as it affects plane references
                    // But modeling.updateProperties(processElement, { id: value }) should handle it if recent version
                    modeling.updateProperties(processElement, { id: value });

                    // Also need to update the BPMNPlane reference
                    const definitions = modeler.getDefinitions();
                    const diagrams = definitions.diagrams;
                    if (diagrams && diagrams.length > 0) {
                        diagrams[0].plane.bpmnElement = processElement.businessObject;
                    }
                } else {
                    modeling.updateProperties(processElement, props);
                }

                return { success: true };
            } catch (e) {
                console.error("Error updating process property:", e);
                return { success: false, error: e.message };
            }
        },
        getDiagramStatistics: function () {
            if (!modeler) return null;
            const elementRegistry = modeler.get('elementRegistry');
            const elements = elementRegistry.getAll();

            const stats = {
                total: 0,
                tasks: 0,
                gateways: 0,
                events: 0,
                flows: 0
            };

            elements.forEach(e => {
                if (!e.type || e.type === 'bpmn:Process' || e.type === 'bpmn:Collaboration') return;
                stats.total++;
                if (e.type.includes('Task')) stats.tasks++;
                else if (e.type.includes('Gateway')) stats.gateways++;
                else if (e.type.includes('Event')) stats.events++;
                else if (e.type === 'bpmn:SequenceFlow') stats.flows++;
            });

            return stats;
        },
        startSimulation: function (dotNetReference) {
            if (!modeler) return false;
            try {
                const elementRegistry = modeler.get('elementRegistry');
                const startEvent = elementRegistry.filter(e => e.type === 'bpmn:StartEvent')[0];

                if (!startEvent) {
                    alert("Please add a Start Event to begin simulation.");
                    return false;
                }

                this.isSimulating = true;
                this.simRef = dotNetReference;
                this.highlightElement(startEvent.id);
                this.showSimulationOverlay(startEvent.id);

                return true;
            } catch (e) {
                console.error("Simulation error:", e);
                return false;
            }
        },
        stopSimulation: function () {
            this.isSimulating = false;
            this.clearSimulationHighlights();
        },
        advanceSimulation: function (elementId) {
            if (!this.isSimulating) return;
            try {
                const elementRegistry = modeler.get('elementRegistry');
                const currentElement = elementRegistry.get(elementId);

                if (!currentElement) return;

                this.clearSimulationHighlights();

                // Find outgoing flows
                const outgoing = currentElement.outgoing || [];
                if (outgoing.length === 0) {
                    if (currentElement.type === 'bpmn:EndEvent') {
                        alert("Workflow execution finished!");
                    } else {
                        alert("Execution stuck: No outgoing flows.");
                    }
                    this.stopSimulation();
                    return;
                }

                // Show choices for all outgoing flows
                outgoing.forEach(flow => {
                    const nextElement = flow.target;
                    this.highlightElement(nextElement.id);
                    this.showSimulationOverlay(nextElement.id);
                });

            } catch (e) {
                console.error("Error advancing simulation:", e);
            }
        },
        highlightElement: function (elementId) {
            const canvas = modeler.get('canvas');
            canvas.addMarker(elementId, 'sim-highlight');
        },
        clearSimulationHighlights: function () {
            const elementRegistry = modeler.get('elementRegistry');
            const canvas = modeler.get('canvas');
            const overlays = modeler.get('overlays');

            elementRegistry.forEach(e => {
                canvas.removeMarker(e.id, 'sim-highlight');
            });
            overlays.remove({ type: 'sim-overlay' });
        },
        showSimulationOverlay: function (elementId) {
            const overlays = modeler.get('overlays');
            const html = `
    < div class="sim-overlay-content" style = "background: #4facfe; color: white; padding: 2px 8px; border-radius: 4px; font-size: 10px; pointer-events: auto; cursor: pointer;" >
        Executing... <i class="fa fa-play"></i>
                </div > `;

            overlays.add(elementId, 'sim-overlay', {
                position: { bottom: 0, right: 0 },
                html: html,
                type: 'sim-overlay'
            });

            // Add click listener to advance
            setTimeout(() => {
                const el = document.querySelector('.sim-overlay-content');
                if (el) {
                    el.onclick = () => {
                        this.advanceSimulation(elementId);
                    };
                }
            }, 100);
        },
        showDiff: async function (xmlA, xmlB) {
            this.clearDiff();
            const canvas = modeler.get('canvas');
            const elementRegistry = modeler.get('elementRegistry');

            // Parse XML A to extract elements
            const parser = new DOMParser();
            const docA = parser.parseFromString(xmlA, "text/xml");
            const elementsA = Array.from(docA.querySelectorAll('[id]')).reduce((acc, el) => {
                acc[el.getAttribute('id')] = el.outerHTML;
                return acc;
            }, {});

            // Current state is XML B (effectively)
            elementRegistry.forEach(element => {
                if (element.type === 'label' || !element.id) return;

                const id = element.id;
                if (!elementsA[id]) {
                    // Added
                    canvas.addMarker(id, 'diff-added');
                } else if (elementsA[id] && elementsA[id] !== element.businessObject?.$type) {
                    // This is a simple check, for real diffing we'd need more complex property matching
                    // But for now let's just use businessObject properties comparison if possible
                }
            });

            // Re-import XML B into a temporary bpmn-js instance if we want full structural diff
            // For now, let's keep it simple: just mark New ones.
        },
        clearDiff: function () {
            const canvas = modeler.get('canvas');
            const elementRegistry = modeler.get('elementRegistry');
            elementRegistry.forEach(e => {
                canvas.removeMarker(e.id, 'diff-added');
                canvas.removeMarker(e.id, 'diff-modified');
            });
        },
        getDiagramElements: function () {
            if (!modeler) return [];
            const elementRegistry = modeler.get('elementRegistry');
            return elementRegistry.filter(e => e.type !== 'label' && e.businessObject).map(e => {
                const bo = e.businessObject;
                let doc = '';
                if (bo.documentation && bo.documentation.length > 0) {
                    doc = bo.documentation[0].text;
                }
                return {
                    id: e.id,
                    type: e.type,
                    name: bo.name || '',
                    documentation: doc
                };
            });
        },
        registerModelChangeListener: function (dotnetRef) {
            if (!modeler) return;
            let timeout;
            modeler.on('commandStack.changed', async function () {
                if (!modeler.getDefinitions()) return;

                clearTimeout(timeout);
                timeout = setTimeout(async () => {
                    try {
                        const { xml } = await modeler.saveXML({ format: true });
                        await dotnetRef.invokeMethodAsync('OnModelChanged', xml);
                    } catch (e) {
                        console.error('Error saving XML on change', e);
                    }
                }, 500); // Debounce 500ms
            });
        },
        getCommandStack: function () {
            if (!modeler) return [];
            const commandStack = modeler.get('commandStack');
            const stack = commandStack._stack;
            const idx = commandStack._stackIdx;

            const labels = {
                'shape.create': 'Create Element',
                'shape.move': 'Move Element',
                'shape.delete': 'Delete Element',
                'connection.create': 'Connect Elements',
                'connection.move': 'Move Connection',
                'connection.delete': 'Remove Connection',
                'element.updateProperties': 'Update Properties',
                'element.updateLabel': 'Rename Element',
                'shape.resize': 'Resize Element',
                'label.create': 'Create Label'
            };

            return stack.map((entry, i) => ({
                command: entry.command,
                label: labels[entry.command] || entry.command,
                isUndo: i > idx
            }));
        },
        registerCommandStackListener: function (dotNetReference) {
            if (!modeler) return;
            modeler.on('commandStack.changed', () => {
                dotNetReference.invokeMethodAsync('OnCommandStackChanged');
            });
        },

        applyHeatmap: function (stats) {
            if (!modeler) return;
            const overlays = modeler.get('overlays');
            this.clearHeatmap();

            stats.forEach(stat => {
                const count = stat.instances;
                if (count === 0) return;

                // Determine color based on count (simple gradient)
                let color = '#4facfe'; // Blue
                if (count > 100) color = '#f59e0b'; // Amber
                if (count > 500) color = '#ef4444'; // Red

                const html = `
    < div class="heatmap-badge" style = "
background: ${color};
color: white;
padding: 2px 8px;
border - radius: 10px;
font - size: 11px;
font - weight: bold;
box - shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
pointer - events: none;
white - space: nowrap;
">
                        ${count}
                    </div >
    `;

                overlays.add(stat.activityId, 'heatmap', {
                    position: {
                        bottom: 0,
                        right: 0
                    },
                    html: html
                });
            });
        },

        clearHeatmap: function () {
            if (!modeler) return;
            const overlays = modeler.get('overlays');
            overlays.remove({ type: 'heatmap' });
        }
    };
})();

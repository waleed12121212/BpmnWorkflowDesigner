window.BpmnViewer = (function () {
    let viewer = null;

    return {
        initialize: function (containerId) {
            const container = document.getElementById(containerId);
            if (!container) return;

            // Use window.BpmnJS if available (UMD bundle often provides it)
            // For viewing, we can use BpmnJS which acts as a Viewer by default if no additional modules are added
            const BpmnConstructor = window.BpmnJS || window.Modeler;

            if (viewer) {
                viewer.destroy();
            }

            try {
                viewer = new BpmnConstructor({
                    container: container
                });
                console.log("BPMN viewer initialized");
            } catch (err) {
                console.error("Failed to initialize BPMN viewer", err);
            }
        },
        importXML: async function (xml) {
            if (!viewer) return { success: false, error: "Viewer not initialized" };
            try {
                await viewer.importXML(xml);
                const canvas = viewer.get('canvas');
                canvas.zoom('fit-viewport');
                return { success: true };
            } catch (err) {
                console.error("Error importing XML to viewer", err);
                return { success: false, error: err.message };
            }
        },
        highlightActivities: function (activeIds, completedIds) {
            if (!viewer) return;
            const canvas = viewer.get('canvas');
            const elementRegistry = viewer.get('elementRegistry');

            // Clear markers
            elementRegistry.forEach(element => {
                canvas.removeMarker(element.id, 'activity-active');
                canvas.removeMarker(element.id, 'activity-completed');
            });

            // Add completed markers
            if (completedIds) {
                completedIds.forEach(id => {
                    canvas.addMarker(id, 'activity-completed');
                });
            }

            // Add active markers
            if (activeIds) {
                activeIds.forEach(id => {
                    canvas.addMarker(id, 'activity-active');
                });
            }
        },
        destroy: function () {
            if (viewer) {
                viewer.destroy();
                viewer = null;
            }
        }
    };
})();

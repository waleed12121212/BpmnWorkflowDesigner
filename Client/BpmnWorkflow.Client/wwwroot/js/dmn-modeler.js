window.DmnModeler = (function () {
    let modeler = null;

    return {
        initialize: async function (containerId, xml) {
            const container = document.getElementById(containerId);
            if (!container) {
                console.error('DMN container not found:', containerId);
                return { success: false, error: 'Container not found' };
            }

            try {
                if (modeler) {
                    modeler.destroy();
                }

                modeler = new DmnJS({
                    container: container,
                    drd: {
                        propertiesPanel: {
                            parent: '#dmn-properties-panel'
                        }
                    }
                });

                if (xml) {
                    await modeler.importXML(xml);
                }

                console.log('DMN Modeler initialized successfully');
                return { success: true };
            } catch (err) {
                console.error('Failed to initialize DMN modeler:', err);
                return { success: false, error: err.message };
            }
        },

        getXML: async function () {
            if (!modeler) return null;
            try {
                const result = await modeler.saveXML({ format: true });
                return result.xml;
            } catch (err) {
                console.error('Failed to get XML:', err);
                return null;
            }
        },

        importXML: async function (xml) {
            if (!modeler) return false;
            try {
                await modeler.importXML(xml);
                return true;
            } catch (err) {
                console.error('Failed to import XML:', err);
                return false;
            }
        },

        getActiveView: function () {
            if (!modeler) return null;
            return modeler.getActiveView();
        },

        getActiveViewer: function () {
            if (!modeler) return null;
            return modeler.getActiveViewer();
        },

        undo: function () {
            if (!modeler) return;
            const activeViewer = modeler.getActiveViewer();
            if (activeViewer && activeViewer.get) {
                const commandStack = activeViewer.get('commandStack');
                if (commandStack && commandStack.canUndo()) {
                    commandStack.undo();
                }
            }
        },

        redo: function () {
            if (!modeler) return;
            const activeViewer = modeler.getActiveViewer();
            if (activeViewer && activeViewer.get) {
                const commandStack = activeViewer.get('commandStack');
                if (commandStack && commandStack.canRedo()) {
                    commandStack.redo();
                }
            }
        },

        zoomIn: function () {
            if (!modeler) return;
            const activeViewer = modeler.getActiveViewer();
            if (activeViewer && activeViewer.get) {
                const zoomScroll = activeViewer.get('zoomScroll');
                if (zoomScroll) {
                    zoomScroll.stepZoom(1);
                }
            }
        },

        zoomOut: function () {
            if (!modeler) return;
            const activeViewer = modeler.getActiveViewer();
            if (activeViewer && activeViewer.get) {
                const zoomScroll = activeViewer.get('zoomScroll');
                if (zoomScroll) {
                    zoomScroll.stepZoom(-1);
                }
            }
        },

        zoomReset: function () {
            if (!modeler) return;
            const activeViewer = modeler.getActiveViewer();
            if (activeViewer && activeViewer.get) {
                const canvas = activeViewer.get('canvas');
                if (canvas) {
                    canvas.zoom('fit-viewport');
                }
            }
        },

        downloadXML: function (filename, xml) {
            const blob = new Blob([xml], { type: 'application/xml' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            a.click();
            window.URL.revokeObjectURL(url);
        },

        toggleFullScreen: function (elementId) {
            const el = document.getElementById(elementId) || document.documentElement;
            if (!document.fullscreenElement) {
                el.requestFullscreen().catch(err => {
                    console.error(`Error attempting to enable full-screen mode: ${err.message}`);
                });
            } else {
                document.exitFullscreen();
            }
        },

        destroy: function () {
            if (modeler) {
                modeler.destroy();
                modeler = null;
            }
        }
    };
})();

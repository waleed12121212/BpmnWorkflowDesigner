window.FormBuilder = (function () {
    let editor = null;
    let viewer = null;

    return {
        initialize: async function (containerId, schema) {
            const container = document.getElementById(containerId);
            if (!container) return;

            const FormEditor = window.FormEditor.FormEditor;

            if (editor) {
                editor.destroy();
            }

            try {
                editor = new FormEditor({
                    container: container
                });

                if (schema) {
                    const parsedSchema = typeof schema === 'string' ? JSON.parse(schema) : schema;
                    await editor.importSchema(parsedSchema);
                }

                console.log("Form editor initialized");

                // Add event listener for changes if needed
                editor.on('changed', () => {
                    // console.log('Form changed');
                });

                return { success: true };
            } catch (err) {
                console.error("Failed to initialize form editor", err);
                return { success: false, error: err.message };
            }
        },
        getSchema: function () {
            if (!editor) return null;
            return JSON.stringify(editor.getSchema());
        },
        undo: function () {
            if (editor && editor.get('commandStack')) {
                editor.get('commandStack').undo();
            }
        },
        redo: function () {
            if (editor && editor.get('commandStack')) {
                editor.get('commandStack').redo();
            }
        },
        // form-js doesn't have native zoom, using CSS transform for better result
        zoom: function (scale) {
            const el = document.querySelector('.fjs-container');
            if (el) {
                el.style.transform = `scale(${scale})`;
                el.style.transformOrigin = 'top left';
                // Adjust container height/width if needed to avoid clipping
                const parent = el.parentElement;
                if (parent) {
                    parent.style.height = (parent.offsetHeight * scale) + 'px';
                }
            }
        },
        // Viewer for Preview
        initializeViewer: async function (containerId, schema, data = {}) {
            const container = document.getElementById(containerId);
            if (!container) return;

            // UMD bundle global for viewer is usually 'FormViewer.Form' or just 'Form'
            const FormViewerConstructor = (window.FormViewer && window.FormViewer.Form) || window.Form;

            if (viewer) {
                viewer.destroy();
            }

            try {
                if (!FormViewerConstructor) {
                    throw new Error("FormViewer constructor not found. Check if form-viewer.umd.js is loaded.");
                }

                viewer = new FormViewerConstructor({
                    container: container
                });

                const parsedSchema = typeof schema === 'string' ? JSON.parse(schema) : schema;
                await viewer.importSchema(parsedSchema, data);
                console.log("Form viewer initialized");
            } catch (err) {
                console.error("Failed to initialize form viewer", err);
            }
        },
        downloadSchema: function (filename, schema) {
            const blob = new Blob([schema], { type: 'application/json' });
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
            if (editor) {
                editor.destroy();
                editor = null;
            }
            if (viewer) {
                viewer.destroy();
                viewer = null;
            }
        }
    };
})();

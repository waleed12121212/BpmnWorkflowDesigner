window.SvgExporter = {
    getSvgFromXml: async function (xml) {
        return new Promise(async (resolve, reject) => {
            try {
                // Ensure BpmnJS is available
                const BpmnConstructor = window.BpmnJS || window.BpmnModeler;
                if (!BpmnConstructor) {
                    console.error("BpmnJS library not found.");
                    resolve(null); // Return null instead of rejecting to avoid crashing the save
                    return;
                }

                // Create a temporary container
                const container = document.createElement('div');
                // Use fixed dimensions to ensure layout happens correctly, but hide it
                container.style.position = 'absolute';
                container.style.left = '-10000px';
                container.style.top = '-10000px';
                container.style.width = '1000px';
                container.style.height = '1000px';
                container.style.visibility = 'hidden';
                document.body.appendChild(container);

                // Initialize viewer (lighter than modeler)
                const viewer = new BpmnConstructor({ container: container });

                // Import XML
                await viewer.importXML(xml);

                // Get SVG
                const { svg } = await viewer.saveSVG();

                // Cleanup
                viewer.destroy();
                document.body.removeChild(container);

                resolve(svg);
            } catch (e) {
                console.error("Error generating SVG from XML:", e);
                // Try to cleanup if possible
                try {
                    if (container && container.parentNode) document.body.removeChild(container);
                } catch (ex) { }
                resolve(null);
            }
        });
    }
};

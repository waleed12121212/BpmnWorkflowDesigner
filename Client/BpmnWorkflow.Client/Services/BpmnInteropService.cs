using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BpmnWorkflow.Client.Services
{
    public class BpmnInteropService : IBpmnInteropService
    {
        private readonly IJSRuntime _jsRuntime;

        public BpmnInteropService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task InitializeAsync(string containerId) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.initialize", containerId).AsTask();

        public async Task<bool> CreateNewDiagramAsync()
        {
            var result = await _jsRuntime.InvokeAsync<InteropResult>("BpmnModeler.createNewDiagram");
            return result.Success;
        }

        public async Task<bool> ImportXmlAsync(string xml)
        {
            var result = await _jsRuntime.InvokeAsync<InteropResult>("BpmnModeler.importXML", xml);
            return result.Success;
        }

        public async Task<(bool success, string? xml)> ExportXmlAsync()
        {
            var result = await _jsRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportXML");
            return (result.Success, result.Xml);
        }

        public async Task<(bool success, string? svg)> ExportSvgAsync()
        {
            var result = await _jsRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportSVG");
            return (result.Success, result.Svg);
        }

        public async Task<(bool success, string? dataUrl)> ExportPngAsync()
        {
            var result = await _jsRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportPNG");
            return (result.Success, result.DataUrl);
        }

        public async Task<(bool success, string? pdfBase64)> ExportPdfAsync()
        {
            var result = await _jsRuntime.InvokeAsync<ExportResult>("BpmnModeler.exportPDF");
            return (result.Success, result.PdfBase64);
        }

        private sealed class InteropResult
        {
            public bool Success { get; set; }
            public string? Error { get; set; }
        }

        private sealed class ExportResult
        {
            public bool Success { get; set; }
            public string? Xml { get; set; }
            public string? Svg { get; set; }
            public string? DataUrl { get; set; }
            public string? PdfBase64 { get; set; }
            public string? Error { get; set; }
        }

        public Task ZoomInAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.zoomIn").AsTask();

        public Task ZoomOutAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.zoomOut").AsTask();

        public Task ResetZoomAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.resetZoom").AsTask();

        public Task ZoomToActualAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.zoomToActual").AsTask();

        public Task UndoAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.undo").AsTask();

        public Task RedoAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.redo").AsTask();

        public Task DownloadFileAsync(string filename, string content, string contentType) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.downloadFile", filename, content, contentType).AsTask();

        public Task DownloadBinaryFileAsync(string filename, string base64Content, string contentType) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.downloadFile", filename, base64Content, contentType).AsTask();

        public async Task<ValidationResult> ValidateDiagramAsync()
        {
            return await _jsRuntime.InvokeAsync<ValidationResult>("BpmnModeler.validateDiagram");
        }

        public Task RegisterSelectionListenerAsync(object dotnetRef) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.registerSelectionListener", dotnetRef).AsTask();

        public Task UpdateElementPropertyAsync(string elementId, string key, string value) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.updateElementProperty", elementId, key, value).AsTask();

        public async Task<bool> StartSimulationAsync(object dotnetRef)
        {
            return await _jsRuntime.InvokeAsync<bool>("BpmnModeler.startSimulation", dotnetRef);
        }

        public Task StopSimulationAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.stopSimulation").AsTask();

        public Task AdvanceSimulationAsync(string elementId) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.advanceSimulation", elementId).AsTask();

        public Task TriggerLintingAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.validateAndMark").AsTask();

        public Task<DiagramStatsDto?> GetDiagramStatisticsAsync() =>
            _jsRuntime.InvokeAsync<DiagramStatsDto?>("BpmnModeler.getDiagramStatistics").AsTask();

        public Task ShowDiffAsync(string xmlA, string xmlB) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.showDiff", xmlA, xmlB).AsTask();

        public Task ClearDiffAsync() =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.clearDiff").AsTask();

        public async Task<List<CommandStackEntryDto>> GetCommandStackAsync() =>
            await _jsRuntime.InvokeAsync<List<CommandStackEntryDto>>("BpmnModeler.getCommandStack");

        public Task RegisterCommandStackListenerAsync(object dotnetRef) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.registerCommandStackListener", dotnetRef).AsTask();

        public async Task<List<BpmnElementData>> GetDiagramElementsAsync() =>
            await _jsRuntime.InvokeAsync<List<BpmnElementData>>("BpmnModeler.getDiagramElements");

        public Task RegisterModelChangeListenerAsync(object dotnetRef) =>
            _jsRuntime.InvokeVoidAsync("BpmnModeler.registerModelChangeListener", dotnetRef).AsTask();
    }
}



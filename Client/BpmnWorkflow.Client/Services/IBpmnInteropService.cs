using System.Threading.Tasks;
using System.Collections.Generic;

namespace BpmnWorkflow.Client.Services
{
    public interface IBpmnInteropService
    {
        Task InitializeAsync(string containerId);
        Task<bool> CreateNewDiagramAsync();
        Task<bool> ImportXmlAsync(string xml);
        Task<(bool success, string? xml)> ExportXmlAsync();
        Task<(bool success, string? svg)> ExportSvgAsync();
        Task<(bool success, string? dataUrl)> ExportPngAsync();
        Task<(bool success, string? pdfBase64)> ExportPdfAsync();
        Task ZoomInAsync();
        Task ZoomOutAsync();
        Task ResetZoomAsync();
        Task ZoomToActualAsync();
        Task UndoAsync();
        Task RedoAsync();
        Task DownloadFileAsync(string filename, string content, string contentType);
        Task DownloadBinaryFileAsync(string filename, string base64Content, string contentType);
        Task<ValidationResult> ValidateDiagramAsync();
        Task RegisterSelectionListenerAsync(object dotnetRef);
        Task UpdateElementPropertyAsync(string elementId, string key, string value);
        Task<bool> StartSimulationAsync(object dotnetRef);
        Task StopSimulationAsync();
        Task AdvanceSimulationAsync(string elementId);
        Task TriggerLintingAsync();
        Task<DiagramStatsDto?> GetDiagramStatisticsAsync();
        Task ShowDiffAsync(string xmlA, string xmlB);
        Task ClearDiffAsync();
        Task<List<CommandStackEntryDto>> GetCommandStackAsync();
        Task RegisterCommandStackListenerAsync(object dotnetRef);
        Task<List<BpmnElementData>> GetDiagramElementsAsync();
        Task RegisterModelChangeListenerAsync(object dotnetRef);
        Task UpdateProcessPropertyAsync(string key, string value);
        Task ApplyHeatmapAsync(List<Models.ActivityStatsDto> stats);
        Task ClearHeatmapAsync();
    }

    public class CommandStackEntryDto
    {
        public string Command { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsUndo { get; set; }
    }

    public class DiagramStatsDto
    {
        public int Total { get; set; }
        public int Tasks { get; set; }
        public int Gateways { get; set; }
        public int Events { get; set; }
        public int Flows { get; set; }
    }

    public delegate void SimulationStepHandler(string elementId);

    public class ValidationResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? Error { get; set; } // For JS errors
    }

    public class BpmnElementData
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Documentation { get; set; }
        public string Condition { get; set; }
        public string Color { get; set; } // Stroke color
        public string BackgroundColor { get; set; } // Fill color
        public string? FormId { get; set; }
        public bool IsExecutable { get; set; }
        public string? CamundaKey { get; set; }
    }
}



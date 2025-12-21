namespace BpmnWorkflow.Client.Models
{
    public class UpsertWorkflowRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BpmnXml { get; set; } = string.Empty;
        public string? SvgPreview { get; set; }
    }
}



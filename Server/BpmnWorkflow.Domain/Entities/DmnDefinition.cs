using System;

namespace BpmnWorkflow.Domain.Entities
{
    public class DmnDefinition
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DmnXml { get; set; } = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<definitions xmlns=\"https://www.omg.org/spec/DMN/20191111/MODEL/\" xmlns:dmndi=\"https://www.omg.org/spec/DMN/20191111/DMNDI/\" id=\"Definitions_1\" name=\"Definitions\" namespace=\"http://camunda.org/schema/1.0/dmn\">\r\n  <decision id=\"Decision_1\" name=\"Decision 1\">\r\n    <decisionTable id=\"DecisionTable_1\">\r\n      <input id=\"Input_1\">\r\n        <inputExpression id=\"InputExpression_1\" typeRef=\"string\">\r\n          <text></text>\r\n        </inputExpression>\r\n      </input>\r\n      <output id=\"Output_1\" typeRef=\"string\" />\r\n    </decisionTable>\r\n  </decision>\r\n  <dmndi:DMNDI>\r\n    <dmndi:DMNDiagram>\r\n      <dmndi:DMNShape dmnElementRef=\"Decision_1\">\r\n        <dc:Bounds xmlns:dc=\"http://www.omg.org/spec/DMN/20180521/DC/\" height=\"80\" width=\"180\" x=\"160\" y=\"80\" />\r\n      </dmndi:DMNShape>\r\n    </dmndi:DMNDiagram>\r\n  </dmndi:DMNDI>\r\n</definitions>";
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DmnVersion
    {
        public Guid Id { get; set; }
        public Guid DmnId { get; set; }
        public DmnDefinition? Dmn { get; set; }
        public string DmnXml { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Comment { get; set; }
    }
}

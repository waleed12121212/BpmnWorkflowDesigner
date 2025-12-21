namespace BpmnWorkflow.Client.Constants
{
    public static class BpmnTemplates
    {
        public const string SimpleApproval = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<bpmn:definitions xmlns:bpmn=""http://www.omg.org/spec/BPMN/20100524/MODEL"" xmlns:bpmndi=""http://www.omg.org/spec/BPMN/20100524/DI"" xmlns:dc=""http://www.omg.org/spec/DD/20100524/DC"" xmlns:di=""http://www.omg.org/spec/DD/20100524/DI"" id=""Definitions_1"" targetNamespace=""http://bpmn.io/schema/bpmn"">
  <bpmn:process id=""Process_1"" isExecutable=""false"">
    <bpmn:startEvent id=""StartEvent_1"" name=""Request Submitted"">
      <bpmn:outgoing>Flow_1</bpmn:outgoing>
    </bpmn:startEvent>
    <bpmn:task id=""Task_1"" name=""Manager Approval"">
      <bpmn:incoming>Flow_1</bpmn:incoming>
      <bpmn:outgoing>Flow_2</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id=""Flow_1"" sourceRef=""StartEvent_1"" targetRef=""Task_1"" />
    <bpmn:endEvent id=""EndEvent_1"" name=""Approved"">
      <bpmn:incoming>Flow_2</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:sequenceFlow id=""Flow_2"" sourceRef=""Task_1"" targetRef=""EndEvent_1"" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id=""BPMNDiagram_1"">
    <bpmndi:BPMNPlane id=""BPMNPlane_1"" bpmnElement=""Process_1"">
      <bpmndi:BPMNShape id=""_BPMNShape_StartEvent_2"" bpmnElement=""StartEvent_1"">
        <dc:Bounds x=""173"" y=""102"" width=""36"" height=""36"" />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id=""Task_1_di"" bpmnElement=""Task_1"">
        <dc:Bounds x=""260"" y=""80"" width=""100"" height=""80"" />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id=""EndEvent_1_di"" bpmnElement=""EndEvent_1"">
        <dc:Bounds x=""412"" y=""102"" width=""36"" height=""36"" />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id=""Flow_1_di"" bpmnElement=""Flow_1"">
        <di:waypoint x=""209"" y=""120"" />
        <di:waypoint x=""260"" y=""120"" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id=""Flow_2_di"" bpmnElement=""Flow_2"">
        <di:waypoint x=""360"" y=""120"" />
        <di:waypoint x=""412"" y=""120"" />
      </bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>";

        public const string CustomerOnboarding = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<bpmn:definitions xmlns:bpmn=""http://www.omg.org/spec/BPMN/20100524/MODEL"" xmlns:bpmndi=""http://www.omg.org/spec/BPMN/20100524/DI"" xmlns:dc=""http://www.omg.org/spec/DD/20100524/DC"" xmlns:di=""http://www.omg.org/spec/DD/20100524/DI"" id=""Definitions_2"" targetNamespace=""http://bpmn.io/schema/bpmn"">
  <bpmn:process id=""Process_CustomerOnboarding"" isExecutable=""false"">
    <bpmn:startEvent id=""Start_Onboarding"" name=""Register"">
      <bpmn:outgoing>Flow_A</bpmn:outgoing>
    </bpmn:startEvent>
    <bpmn:userTask id=""Task_KYC"" name=""KYC Verification"">
      <bpmn:incoming>Flow_A</bpmn:incoming>
      <bpmn:outgoing>Flow_B</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:exclusiveGateway id=""Gateway_Verify"" name=""Valid?"">
      <bpmn:incoming>Flow_B</bpmn:incoming>
      <bpmn:outgoing>Flow_Valid</bpmn:outgoing>
      <bpmn:outgoing>Flow_Invalid</bpmn:outgoing>
    </bpmn:exclusiveGateway>
    <bpmn:sequenceFlow id=""Flow_A"" sourceRef=""Start_Onboarding"" targetRef=""Task_KYC"" />
    <bpmn:sequenceFlow id=""Flow_B"" sourceRef=""Task_KYC"" targetRef=""Gateway_Verify"" />
    <bpmn:task id=""Task_Welcome"" name=""Send Welcome Kit"">
      <bpmn:incoming>Flow_Valid</bpmn:incoming>
      <bpmn:outgoing>Flow_Done</bpmn:outgoing>
    </bpmn:task>
    <bpmn:endEvent id=""End_Onboarded"" name=""Complete"">
      <bpmn:incoming>Flow_Done</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:endEvent id=""End_Rejected"" name=""Rejected"">
      <bpmn:incoming>Flow_Invalid</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:sequenceFlow id=""Flow_Valid"" name=""Yes"" sourceRef=""Gateway_Verify"" targetRef=""Task_Welcome"" />
    <bpmn:sequenceFlow id=""Flow_Invalid"" name=""No"" sourceRef=""Gateway_Verify"" targetRef=""End_Rejected"" />
    <bpmn:sequenceFlow id=""Flow_Done"" sourceRef=""Task_Welcome"" targetRef=""End_Onboarded"" />
  </bpmn:process>
</bpmn:definitions>";
    }
}

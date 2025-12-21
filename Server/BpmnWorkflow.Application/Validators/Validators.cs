using BpmnWorkflow.Client.Models;
using FluentValidation;

namespace BpmnWorkflow.Application.Validators
{
    public class UpsertWorkflowRequestValidator : AbstractValidator<UpsertWorkflowRequest>
    {
        public UpsertWorkflowRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Workflow name is required.")
                               .MaximumLength(100).WithMessage("Workflow name must not exceed 100 characters.");
            
            RuleFor(x => x.BpmnXml).NotEmpty().WithMessage("BPMN XML content is required.");
        }
    }

    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
        }
    }
}

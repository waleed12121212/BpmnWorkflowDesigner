using BpmnWorkflow.Domain.Entities;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}

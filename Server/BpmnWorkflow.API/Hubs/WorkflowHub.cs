using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BpmnWorkflow.API.Hubs
{
    public class WorkflowHub : Hub
    {
        // Map ConnectionId -> WorkflowId
        private static readonly ConcurrentDictionary<string, string> _connectionWorkflowMap = new();
        
        // Map ConnectionId -> UserDisplayName
        private static readonly ConcurrentDictionary<string, string> _connectionUserMap = new();

        public async Task JoinWorkflow(string workflowId, string userName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, workflowId);
            _connectionWorkflowMap[Context.ConnectionId] = workflowId;
            _connectionUserMap[Context.ConnectionId] = userName;

            var usersInGroup = _connectionWorkflowMap
                .Where(kvp => kvp.Value == workflowId)
                .Select(kvp => _connectionUserMap.TryGetValue(kvp.Key, out var name) ? name : "Unknown")
                .Distinct()
                .ToList();

            await Clients.Group(workflowId).SendAsync("UpdateUserList", usersInGroup);
            await Clients.Group(workflowId).SendAsync("UserJoined", userName);
        }

        public async Task LeaveWorkflow(string workflowId)
        {
             await Groups.RemoveFromGroupAsync(Context.ConnectionId, workflowId);
             if (_connectionWorkflowMap.TryRemove(Context.ConnectionId, out _))
             {
                 _connectionUserMap.TryRemove(Context.ConnectionId, out var userName);
                 
                 var usersInGroup = _connectionWorkflowMap
                    .Where(kvp => kvp.Value == workflowId)
                    .Select(kvp => _connectionUserMap.TryGetValue(kvp.Key, out var name) ? name : "Unknown")
                    .Distinct()
                    .ToList();

                 await Clients.Group(workflowId).SendAsync("UpdateUserList", usersInGroup);
                 if (userName != null)
                 {
                     await Clients.Group(workflowId).SendAsync("UserLeft", userName);
                 }
             }
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            if (_connectionWorkflowMap.TryGetValue(Context.ConnectionId, out var workflowId))
            {
                await LeaveWorkflow(workflowId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendDiagramUpdate(string workflowId, string xml)
        {
            await Clients.OthersInGroup(workflowId).SendAsync("ReceiveDiagramUpdate", xml);
        }
    }
}

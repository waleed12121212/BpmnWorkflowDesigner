using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BpmnWorkflow.API.Hubs
{
    public class FormHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connectionFormMap = new();
        private static readonly ConcurrentDictionary<string, string> _connectionUserMap = new();

        public async Task JoinForm(string formId, string userName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, formId);
            _connectionFormMap[Context.ConnectionId] = formId;
            _connectionUserMap[Context.ConnectionId] = userName;

            var usersInGroup = _connectionFormMap
                .Where(kvp => kvp.Value == formId)
                .Select(kvp => _connectionUserMap.TryGetValue(kvp.Key, out var name) ? name : "Unknown")
                .Distinct()
                .ToList();

            await Clients.Group(formId).SendAsync("UpdateUserList", usersInGroup);
            await Clients.Group(formId).SendAsync("UserJoined", userName);
        }

        public async Task LeaveForm(string formId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, formId);
            if (_connectionFormMap.TryRemove(Context.ConnectionId, out _))
            {
                _connectionUserMap.TryRemove(Context.ConnectionId, out var userName);
                
                var usersInGroup = _connectionFormMap
                    .Where(kvp => kvp.Value == formId)
                    .Select(kvp => _connectionUserMap.TryGetValue(kvp.Key, out var name) ? name : "Unknown")
                    .Distinct()
                    .ToList();

                await Clients.Group(formId).SendAsync("UpdateUserList", usersInGroup);
                if (userName != null)
                {
                    await Clients.Group(formId).SendAsync("UserLeft", userName);
                }
            }
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            if (_connectionFormMap.TryGetValue(Context.ConnectionId, out var formId))
            {
                await LeaveForm(formId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendFormUpdate(string formId, string schema)
        {
            await Clients.OthersInGroup(formId).SendAsync("ReceiveFormUpdate", schema);
        }
    }
}

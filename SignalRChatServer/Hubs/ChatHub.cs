using Microsoft.AspNetCore.SignalR;
using SignalRChatServer.Data;
using SignalRChatServer.Models;
using System.Security.Cryptography.X509Certificates;

namespace SignalRChatServer.Hubs
{
    public class ChatHub : Hub
    {
        public async Task GetNickName(string nickName)
        {
            var clientName = ClientSource.Clients.Any(c => c.NickName == nickName);

            if (!clientName)
            {


                Client client = new Client
                {
                    ConnectionId = Context.ConnectionId,
                    NickName = nickName
                };

                ClientSource.Clients.Add(client);

                await Clients.Others.SendAsync("clientJoined", nickName);

                await Clients.All.SendAsync("clients", ClientSource.Clients);
            }
            else
            {
                await Clients.Caller.SendAsync("existClientName", nickName);
            }
        }

        public async Task SendMessageAsync(string message, string clientName)
        {
            clientName = clientName.Trim();

            var senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            if (clientName == "Tümü")
            {
                await Clients.Others.SendAsync("receiveMessage", message, senderClient.NickName);
            }
            else
            {
                Client client = ClientSource.Clients.FirstOrDefault(c => c.NickName == clientName);
                await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message, senderClient.NickName);
            }
        }

        public async Task AddGroup(string groupName)
        {
            var result = GroupSource.Groups.Any(g => g.GroupName == groupName);

            if (!result)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                Group group = new Group
                {
                    GroupName = groupName
                };

                group.Clients.Add(ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId));

                GroupSource.Groups.Add(group);

                await Clients.All.SendAsync("groups", GroupSource.Groups);
            }
            else
            {
                await Clients.Caller.SendAsync("existGroup", groupName);
            }
        }

        public async Task AddClientToGroup(IEnumerable<string> groupNames)
        {
            Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            foreach (var group in groupNames)
            {
                Group _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == group);

                var result = _group.Clients.Any(c => c.ConnectionId == Context.ConnectionId);

                if (!result)
                {
                    _group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, group);
                }
            }
        }

        public async Task GetClientInGroup(string groupName)
        {
            Group group = GroupSource.Groups.FirstOrDefault(c => c.GroupName == groupName);

            await Clients.Caller.SendAsync("clients", groupName == "-1" ? ClientSource.Clients : group.Clients);
        }

        public async Task SendMessageToGroupAsync(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("receiveMessage", message, ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId).NickName);
        }
    }
}

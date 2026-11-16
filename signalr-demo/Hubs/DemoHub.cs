using Microsoft.AspNetCore.SignalR;

namespace SignalRDemo.Hubs;

public sealed class DemoHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync(
            "ReceiveSystemMessage",
            $"{Context.ConnectionId} joined {roomName}");
    }

    public async Task SendRoomMessage(string roomName, string user, string message)
    {
        await Clients.Group(roomName).SendAsync("ReceiveMessage", user, message);
    }
}

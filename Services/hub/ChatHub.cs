using Microsoft.AspNetCore.SignalR;

namespace Nash_Manassas.Hub;

public class ChatHub: Microsoft.AspNetCore.SignalR.Hub
{
    public async Task SendChatMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveChatMessage", user, message);
    }
}
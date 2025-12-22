using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using NashAI_app.Model;
using NashAI_app.Services;

namespace Nash_Manassas.Hub;

public class ChatHub: Microsoft.AspNetCore.SignalR.Hub
{
    private readonly IChatClient _chatClient;
    private readonly SemanticSearchService _semanticSearchService;
    // public async Task SendChatMessage(string user, string message)
    // {
    //     await Clients.All.SendAsync("ReceiveChatMessage", user, message);
    // }

    public ChatHub(IChatClient chatClient, SemanticSearchService semanticSearchService)
    {
        _chatClient = chatClient;
        _semanticSearchService = semanticSearchService;
    }

    public Task Ping()
    {
        return Clients.Caller.SendAsync("Pong", "Chat is Alive man");
    }

    public async IAsyncEnumerable<string> StreamChatAsync(ChatSessionVBModel sessionVb,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Console.WriteLine("🔥 StreamChat invoked");
        
        var userMessage = sessionVb.Messages.LastOrDefault(m => m.Role == ChatRole.User);
        if (userMessage == null)
        {
            Console.WriteLine("❌ No user message");
            yield break;
        }
        
        var chatMessage = sessionVb.Messages
            .Select(m => new ChatMessage(m.Role, m.MessageContent))
            .ToList();
        
        Console.WriteLine($"Sending {chatMessage.Count} messages to AI");

        await foreach (var update in _chatClient.GetStreamingResponseAsync(chatMessage,
                           cancellationToken: cancellationToken))
        {
            Console.WriteLine($"➡️ Token: {update.Text}");
            
            if (!string.IsNullOrWhiteSpace(update.Text))
                yield return update.Text;
        }
        Console.WriteLine("✅ Stream completed");
        await Clients.Caller.SendAsync("StreamCompleted");
    }
    
    public async Task StreamChat(ChatSessionVBModel sessionVb, CancellationToken cancellationToken)
    {
        Console.WriteLine("🔥 StreamChat invoked");

        var userMessage = sessionVb.Messages.LastOrDefault(m => m.Role == ChatRole.User);
        if (userMessage == null)
        {
            Console.WriteLine("❌ No user message");
            return;
        }

        var chatMessages = sessionVb.Messages
            .Select(m => new ChatMessage(m.Role, m.MessageContent))
            .ToList();

        Console.WriteLine($"Sending {chatMessages.Count} messages to AI");

        await foreach (var update in _chatClient.GetStreamingResponseAsync(
                           chatMessages,
                           cancellationToken: cancellationToken))
        {
            Console.WriteLine($"➡️ Token: {update.Text}");

            if (!string.IsNullOrWhiteSpace(update.Text))
            {
                // 🔥 THIS is streaming in SignalR
                await Clients.Caller.SendAsync("StreamToken", update.Text, cancellationToken);
            }
        }

        Console.WriteLine("✅ Stream completed");
        await Clients.Caller.SendAsync("StreamCompleted", cancellationToken);
    }

}
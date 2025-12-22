using NashAI_app.Model;

namespace NashAI_app.Services;

public interface IProjectChatService
{
    Task<ChatResponseDto> HandleProjectChatAsync(ChatSessionVBModel session);
}
using NashAI_app.Model;

namespace NashAI_app.Services;

public interface IEquipmentChatService
{
    Task<ChatResponseDto> HandleEquipmentChatAsync(ChatSessionVBModel session);
}
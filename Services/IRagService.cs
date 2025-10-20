using Microsoft.AspNetCore.Mvc;
using NashAI_app.Model;

namespace NashAI_app.Services;

public interface IRagService
{
    Task<string> GetRagResponseAsync(ChatSessionModel session, [FromQuery] string filesystem);

    Task<string> GenerateResponseAsync(string query, string sessionId);
}
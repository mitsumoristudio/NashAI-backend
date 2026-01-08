using Microsoft.AspNetCore.Mvc;
using NashAI_app.Services;
using NashAI_app.utils;

namespace NashAI_app.Controller;

[ApiController]
[Route(ApiEndPoints.Speech.speechBase)]
public class SpeechController: ControllerBase
{
    public record TtsRequest(string text);
    private readonly AzureTexttoSpeechService _azureSpeechService;

    public SpeechController(AzureTexttoSpeechService azureSpeechService)
    {
        _azureSpeechService = azureSpeechService;
    }

    [HttpPost(ApiEndPoints.Speech.TEXT_TO_SPEECH)]
    public async Task<IActionResult> TexttoSpeechAsync([FromBody] string ttsRequest)
    {
        if (string.IsNullOrWhiteSpace(ttsRequest))
        {
            return BadRequest("Please provide a text as it is empty");
        }
        
        var audioSpeech = await _azureSpeechService.TextToSpeechAsync(ttsRequest);
        
        return File(
            fileContents:audioSpeech,
            contentType:"audio/wave",
            fileDownloadName:"speech.wav",
            enableRangeProcessing: false);
    }
}
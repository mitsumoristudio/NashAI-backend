using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace NashAI_app.Services;

public class AzureSpeechService : IDisposable
{
    private readonly SpeechRecognizer _recognizer;
    private readonly PushAudioInputStream _pushStream;
    private readonly StringBuilder _text = new();

    public AzureSpeechService(IConfiguration config)
    {
        var speechConfig = SpeechConfig.FromSubscription(
            config["AZURE_SPEECH_KEY"],
            config["AZURE_SPEECH_REGION"]);

        speechConfig.SpeechRecognitionLanguage = "en-US";

        _pushStream = AudioInputStream.CreatePushStream(
            AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1));

        var audioConfig = AudioConfig.FromStreamInput(_pushStream);
        
        _recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        _recognizer.SessionStarted += (_, e) =>
        {
            Console.WriteLine("🟢 Azure Speech session started");
        };

        _recognizer.SessionStopped += (_, e) =>
        {
            Console.WriteLine("🔵 Azure Speech session stopped");
        };

        _recognizer.Canceled += (_, e) =>
        {
            Console.WriteLine($"❌ Azure Speech canceled: {e.ErrorDetails}");
        };
        
        _recognizer.Recognizing += (_, e) =>
        {
            Console.WriteLine($"🟡 Partial: {e.Result.Text}");
        };

        _recognizer.Recognized += (_, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"🟢 Final: {e.Result.Text}");
                _text.Append(e.Result.Text).Append(' ');
            }
        };
        
        _recognizer.StartContinuousRecognitionAsync();
    }
    
    public Task StartAsync() => _recognizer.StartContinuousRecognitionAsync();

    public void PushAudio(byte[] pcm16)
    {
        Console.WriteLine($"🎧 Received audio: {pcm16.Length} bytes");
        
        _pushStream.Write(pcm16);
    }

    public async Task<string> StopAndRecognizeAsync()
    {
        // Give Azure time to process last audio frames
        await Task.Delay(400);
        
        _pushStream.Close();

        // RecognizeOnceAsync is not used for streaming, short audio, entire audio already available, 
        await _recognizer.StopContinuousRecognitionAsync();
        return _text.ToString().Trim();
    }

    public void Dispose()
    {
        _recognizer.Dispose();
        _pushStream.Dispose();
    }
    
}

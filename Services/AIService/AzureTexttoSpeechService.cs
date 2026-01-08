using System.Text;
using Microsoft.CognitiveServices.Speech;

namespace NashAI_app.Services;

public class AzureTexttoSpeechService(IConfiguration config)
{
    public async Task<byte[]> TextToSpeechAsync(string text)
    {
        var speechConfig = SpeechConfig.FromSubscription(
            config["AZURE_SPEECH_KEY"],
            config["AZURE_SPEECH_REGION"]);
        
        // Adding Neutral Voice
        speechConfig.SpeechSynthesisVoiceName = "en-GB-SoniaNeural";
        // Language support https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=tts#supported-languages
        
        speechConfig.SetSpeechSynthesisOutputFormat(
            SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);

        using var synthesizer = new SpeechSynthesizer(speechConfig, null);

        var result = await synthesizer.SpeakTextAsync(text);

        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new Exception(
                $"TTS failed: {result.Reason}");
        }
        
     //   return result.AudioData;
     var wav = WrapPcmAsWav(result.AudioData);
     return wav;

    }
    
    public static byte[] WrapPcmAsWav(byte[] pcm, int sampleRate = 16000, short bitsPerSample = 16, short channels = 1)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        int byteRate = sampleRate * channels * bitsPerSample / 8;
        short blockAlign = (short)(channels * bitsPerSample / 8);

        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + pcm.Length);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1); // PCM
        bw.Write(channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write(blockAlign);
        bw.Write(bitsPerSample);

        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(pcm.Length);
        bw.Write(pcm);

        return ms.ToArray();
    }

    
    
}
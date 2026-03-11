using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UgnayDesktop.Services;

public sealed class GloveSpeechService : IDisposable
{
    private const int AudioPort = 5006;
    private const int ChunkSize = 512;

    private readonly SemaphoreSlim _speakGate = new(1, 1);
    private readonly UdpClient _udpClient = new();

    public async Task SpeakAsync(string text, string ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(ipAddress))
        {
            return;
        }

        if (!IPAddress.TryParse(ipAddress, out var targetAddress))
        {
            throw new InvalidOperationException("Invalid glove IP address.");
        }

        await _speakGate.WaitAsync(cancellationToken);
        try
        {
            var audio = await GenerateSpeechAsync(text.Replace('_', ' '), cancellationToken);
            var endpoint = new IPEndPoint(targetAddress, AudioPort);

            for (var offset = 0; offset < audio.Length; offset += ChunkSize)
            {
                var count = Math.Min(ChunkSize, audio.Length - offset);
                await _udpClient.SendAsync(audio.AsMemory(offset, count), endpoint, cancellationToken);
                await Task.Delay(18, cancellationToken);
            }
        }
        finally
        {
            _speakGate.Release();
        }
    }

    private static async Task<byte[]> GenerateSpeechAsync(string text, CancellationToken cancellationToken)
    {
        var escapedText = text.Replace("'", "''");
        var script = string.Join(';',
            "$ErrorActionPreference='Stop'",
            "Add-Type -AssemblyName System.Speech",
            "$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer",
            "$format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(16000,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono)",
            "$stream = New-Object System.IO.MemoryStream",
            "$synth.SetOutputToAudioStream($stream,$format)",
            $"$synth.Speak('{escapedText}')",
            "[Convert]::ToBase64String($stream.ToArray())");

        var psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -Command \"{script}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Unable to start speech process.");
        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr) ? "Speech synthesis failed." : stderr.Trim());
        }

        var base64 = stdout.Trim();
        if (string.IsNullOrWhiteSpace(base64))
        {
            throw new InvalidOperationException("Speech synthesis returned no audio.");
        }

        return Convert.FromBase64String(base64);
    }

    public void Dispose()
    {
        _udpClient.Dispose();
        _speakGate.Dispose();
    }
}

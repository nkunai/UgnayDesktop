using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestureCollector;

internal static class Program
{
    private const int LandmarkValueCount = 42; // 21 points x (x,y)
    private const int CaptureDelaySeconds = 3;

    public static async Task<int> Main(string[] args)
    {
        var options = CollectorOptions.FromArgs(args);

        Console.WriteLine("=== Gesture Collector (C#) ===");
        Console.WriteLine($"UDP Port: {options.UdpPort}");
        Console.WriteLine($"Dataset: {Path.GetFullPath(options.DatasetPath)}");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        using var handSource = new MediaPipeWorker(options.CameraIndex);
        handSource.Start();

        using var telemetry = new DualGloveUdpReceiver(options.UdpPort, options.LeftGloveIp, options.RightGloveIp);
        telemetry.Start(cts.Token);

        var calibrator = new TelemetryCalibrator();
        Console.WriteLine("Sending CALIBRATE command to gloves...");
        await SendCalibrationCommandAsync(options, cts.Token);
        Console.WriteLine("Initial calibration started (MPU left/right). Keep both gloves still.");
        await calibrator.CalibrateAsync(telemetry, sampleCount: 100, cts.Token);
        Console.WriteLine("Calibration complete.");
        Console.WriteLine("Press ENTER when you are ready to set gesture label...");
        Console.ReadLine();

        var gestureLabel = ResolveGestureLabel(options.GestureLabel);
        Console.WriteLine($"Gesture label: {gestureLabel}");
        Console.WriteLine("Press SPACE to record sample, G to change gesture label, R to recalibrate, ESC to quit.");

        await RunLoopAsync(options, gestureLabel, handSource, telemetry, calibrator, cts.Token);
        return 0;
    }

    private static async Task RunLoopAsync(
        CollectorOptions options,
        string initialGestureLabel,
        MediaPipeWorker handSource,
        DualGloveUdpReceiver telemetry,
        TelemetryCalibrator calibrator,
        CancellationToken token)
    {
        var writeHeader = !File.Exists(options.DatasetPath) || new FileInfo(options.DatasetPath).Length == 0;
        await using var file = new StreamWriter(options.DatasetPath, append: true, Encoding.UTF8);

        if (writeHeader)
        {
            await file.WriteLineAsync(BuildHeader());
            await file.FlushAsync();
        }

        var gestureLabel = initialGestureLabel;

        while (!token.IsCancellationRequested)
        {
            if (!Console.KeyAvailable)
            {
                await Task.Delay(20, token);
                continue;
            }

            var key = Console.ReadKey(intercept: true).Key;

            if (key == ConsoleKey.Escape)
            {
                Console.WriteLine("Exit requested.");
                break;
            }

            if (key == ConsoleKey.G)
            {
                gestureLabel = ResolveGestureLabel(null);
                Console.WriteLine($"Gesture label: {gestureLabel}");
                continue;
            }

            if (key == ConsoleKey.R)
            {
                Console.WriteLine("Sending CALIBRATE command to gloves...");
                await SendCalibrationCommandAsync(options, token);
                Console.WriteLine("Calibrating... keep gloves still.");
                await calibrator.CalibrateAsync(telemetry, sampleCount: 100, token);
                Console.WriteLine("Calibration complete.");
                continue;
            }

            if (key != ConsoleKey.Spacebar)
            {
                continue;
            }

            await WaitForCaptureCountdownAsync(CaptureDelaySeconds, token);

            var hands = handSource.Latest;
            if (hands == null)
            {
                Console.WriteLine("No MediaPipe hand frame yet. Try again.");
                continue;
            }

            var snapshot = telemetry.LatestSnapshot;
            if (!snapshot.HasBothMpu)
            {
                Console.WriteLine("Need MPU data from both gloves. Try again.");
                continue;
            }

            var leftLandmarks = Ensure42(hands.Left);
            var rightLandmarks = Ensure42(hands.Right);
            if (!HasAnyHandData(leftLandmarks, rightLandmarks))
            {
                Console.WriteLine("No hand detected in camera. Try again.");
                continue;
            }

            var left = calibrator.ApplyLeft(snapshot.Left);
            var right = calibrator.ApplyRight(snapshot.Right);

            var row = new List<string>(1 + 42 + 42 + 3 + 3 + 1)
            {
                gestureLabel
            };

            AddFloats(row, leftLandmarks);
            AddFloats(row, rightLandmarks);

            row.Add(left.Roll.ToString("F6", CultureInfo.InvariantCulture));
            row.Add(left.Pitch.ToString("F6", CultureInfo.InvariantCulture));
            row.Add(left.Yaw.ToString("F6", CultureInfo.InvariantCulture));

            row.Add(right.Roll.ToString("F6", CultureInfo.InvariantCulture));
            row.Add(right.Pitch.ToString("F6", CultureInfo.InvariantCulture));
            row.Add(right.Yaw.ToString("F6", CultureInfo.InvariantCulture));

            row.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture));

            await file.WriteLineAsync(string.Join(',', row));
            await file.FlushAsync();

            Console.WriteLine($"sample recorded ({gestureLabel})");
        }
    }

    private static async Task SendCalibrationCommandAsync(CollectorOptions options, CancellationToken token)
    {
        var targets = new List<IPEndPoint>();
        IPAddress? leftIp = null;

        if (IPAddress.TryParse(options.LeftGloveIp, out leftIp))
        {
            targets.Add(new IPEndPoint(leftIp, options.UdpPort));
        }

        if (IPAddress.TryParse(options.RightGloveIp, out var rightIp) &&
            (leftIp == null || !rightIp.Equals(leftIp)))
        {
            targets.Add(new IPEndPoint(rightIp, options.UdpPort));
        }

        if (targets.Count == 0)
        {
            targets.Add(new IPEndPoint(IPAddress.Broadcast, options.UdpPort));
        }

        var payload = Encoding.UTF8.GetBytes("CALIBRATE\n");

        using var client = new UdpClient();
        client.EnableBroadcast = true;

        foreach (var target in targets)
        {
            token.ThrowIfCancellationRequested();
            await client.SendAsync(payload, target, token);
        }
    }

    private static string ResolveGestureLabel(string? initial)
    {
        if (!string.IsNullOrWhiteSpace(initial))
        {
            return initial.Trim();
        }

        while (true)
        {
            Console.Write("Gesture label: ");
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("Gesture label is required.");
        }
    }


    private static async Task WaitForCaptureCountdownAsync(int seconds, CancellationToken token)
    {
        for (var remaining = seconds; remaining >= 1; remaining--)
        {
            Console.Write($"\rCapturing in {remaining}...");
            await Task.Delay(1000, token);
        }

        Console.WriteLine("\rCapturing now...       ");
    }

    private static bool HasAnyHandData(IReadOnlyList<float> left, IReadOnlyList<float> right) =>
        left.Any(x => !float.IsNaN(x)) || right.Any(x => !float.IsNaN(x));
    private static string BuildHeader()
    {
        var cols = new List<string> { "label" };

        for (var i = 0; i < 21; i++)
        {
            cols.Add($"left_{i}_x");
            cols.Add($"left_{i}_y");
        }

        for (var i = 0; i < 21; i++)
        {
            cols.Add($"right_{i}_x");
            cols.Add($"right_{i}_y");
        }

        cols.Add("left_roll");
        cols.Add("left_pitch");
        cols.Add("left_yaw");

        cols.Add("right_roll");
        cols.Add("right_pitch");
        cols.Add("right_yaw");

        cols.Add("timestamp_unix_ms");

        return string.Join(',', cols);
    }

    private static IReadOnlyList<float> Ensure42(IReadOnlyList<float>? values)
    {
        if (values is { Count: LandmarkValueCount })
        {
            return values;
        }

        return Enumerable.Repeat(float.NaN, LandmarkValueCount).ToArray();
    }

    private static void AddFloats(List<string> row, IReadOnlyList<float> values)
    {
        foreach (var value in values)
        {
            row.Add(float.IsNaN(value) ? "" : value.ToString("F6", CultureInfo.InvariantCulture));
        }
    }
}

internal sealed class CollectorOptions
{
    public string? GestureLabel { get; init; }
    public required string DatasetPath { get; init; }
    public required int UdpPort { get; init; }
    public required int CameraIndex { get; init; }
    public string? LeftGloveIp { get; init; }
    public string? RightGloveIp { get; init; }

    public static CollectorOptions FromArgs(string[] args)
    {
        var map = args
            .Select(x => x.Split('=', 2))
            .Where(x => x.Length == 2)
            .ToDictionary(x => x[0].TrimStart('-').ToLowerInvariant(), x => x[1]);

        var label = map.TryGetValue("label", out var tmpLabel) ? tmpLabel : null;
        var dataset = map.TryGetValue("out", out var tmpOut) ? tmpOut : "dataset.csv";
        var udpPort = map.TryGetValue("port", out var tmpPort) && int.TryParse(tmpPort, out var p) ? p : 5005;
        var camera = map.TryGetValue("camera", out var tmpCam) && int.TryParse(tmpCam, out var c) ? c : 0;

        map.TryGetValue("leftip", out var leftIp);
        map.TryGetValue("rightip", out var rightIp);

        return new CollectorOptions
        {
            GestureLabel = label,
            DatasetPath = dataset,
            UdpPort = udpPort,
            CameraIndex = camera,
            LeftGloveIp = string.IsNullOrWhiteSpace(leftIp) ? null : leftIp,
            RightGloveIp = string.IsNullOrWhiteSpace(rightIp) ? null : rightIp
        };
    }
}

internal sealed class MediaPipeWorker : IDisposable
{
    private readonly int _cameraIndex;
    private Process? _process;
    private readonly object _gate = new();

    public HandFrame? Latest { get; private set; }

    public MediaPipeWorker(int cameraIndex)
    {
        _cameraIndex = cameraIndex;
    }

    public void Start()
    {
        var scriptName = "mediapipe_hands_worker.py";
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, scriptName),
            Path.Combine(Directory.GetCurrentDirectory(), scriptName),
            Path.Combine(Directory.GetCurrentDirectory(), "GestureCollector", scriptName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", scriptName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GestureCollector", scriptName)
        };

        var scriptPath = candidates
            .Select(Path.GetFullPath)
            .FirstOrDefault(File.Exists);

        if (scriptPath == null)
        {
            throw new FileNotFoundException("Missing mediapipe_hands_worker.py.");
        }

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\" --camera {_cameraIndex}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        _process = Process.Start(psi) ?? throw new InvalidOperationException("Unable to start MediaPipe worker.");

        _ = Task.Run(async () =>
        {
            while (!_process.HasExited)
            {
                var line = await _process.StandardOutput.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    var frame = JsonSerializer.Deserialize<HandFrame>(line);
                    if (frame != null)
                    {
                        lock (_gate)
                        {
                            Latest = frame;
                        }
                    }
                }
                catch
                {
                    // Ignore malformed lines.
                }
            }
        });

        _ = Task.Run(async () =>
        {
            while (!_process.HasExited)
            {
                var err = await _process.StandardError.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Console.WriteLine($"[mediapipe] {err}");
                }
            }
        });
    }

    public void Dispose()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(1000);
            }
        }
        catch
        {
            // ignored
        }

        _process?.Dispose();
    }
}

internal sealed class DualGloveUdpReceiver : IDisposable
{
    private readonly UdpClient _udp;
    private readonly string? _leftIp;
    private readonly string? _rightIp;
    private readonly object _gate = new();

    private string? _autoLeftIp;
    private string? _autoRightIp;

    public TelemetrySnapshot LatestSnapshot { get; private set; } = new();

    public DualGloveUdpReceiver(int port, string? leftIp, string? rightIp)
    {
        _udp = new UdpClient(port);
        _leftIp = leftIp;
        _rightIp = rightIp;
    }

    public void Start(CancellationToken token)
    {
        _ = Task.Run(async () =>
        {
        while (!token.IsCancellationRequested)
            {
                UdpReceiveResult packet;
                try
                {
                    packet = await _udp.ReceiveAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    continue;
                }

                var senderIp = packet.RemoteEndPoint.Address.ToString();
                var text = Encoding.UTF8.GetString(packet.Buffer).Trim();

                if (!TryParseCsv(text, out var values))
                {
                    LogStatusPacket(senderIp, text);
                    continue;
                }

                lock (_gate)
                {
                    var snap = LatestSnapshot;

                    var target = ResolveTarget(senderIp, values.Length);
                    if (target == HandSide.Left && values.Length >= 3)
                    {
                        snap.Left = new LeftGloveData(values[0], values[1], values[2], true);
                    }
                    else if (target == HandSide.Right && values.Length >= 5)
                    {
                        snap.Right = new RightGloveData(values[0], values[1], values[2], values[3], values[4], true);
                    }

                    LatestSnapshot = snap;
                }
            }
        }, token);
    }

    private HandSide ResolveTarget(string senderIp, int valueCount)
    {
        if (!string.IsNullOrWhiteSpace(_leftIp) && senderIp == _leftIp)
        {
            return HandSide.Left;
        }

        if (!string.IsNullOrWhiteSpace(_rightIp) && senderIp == _rightIp)
        {
            return HandSide.Right;
        }

        if (valueCount == 3)
        {
            _autoLeftIp ??= senderIp;
            return HandSide.Left;
        }

        if (valueCount >= 5)
        {
            _autoRightIp ??= senderIp;
            return HandSide.Right;
        }

        return HandSide.Unknown;
    }

    private HandSide ResolveSideByIp(string senderIp)
    {
        if (!string.IsNullOrWhiteSpace(_leftIp) && senderIp == _leftIp)
        {
            return HandSide.Left;
        }

        if (!string.IsNullOrWhiteSpace(_rightIp) && senderIp == _rightIp)
        {
            return HandSide.Right;
        }

        if (!string.IsNullOrWhiteSpace(_autoLeftIp) && senderIp == _autoLeftIp)
        {
            return HandSide.Left;
        }

        if (!string.IsNullOrWhiteSpace(_autoRightIp) && senderIp == _autoRightIp)
        {
            return HandSide.Right;
        }

        return HandSide.Unknown;
    }

    private void LogStatusPacket(string senderIp, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var side = ResolveSideByIp(senderIp);
        var source = side switch
        {
            HandSide.Left => "left glove",
            HandSide.Right => "right glove",
            _ => $"glove {senderIp}"
        };

        Console.WriteLine($"[{source}] {text}");
    }

    private static bool TryParseCsv(string line, out float[] values)
    {
        var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        values = Array.Empty<float>();

        if (parts.Length < 3)
        {
            return false;
        }

        var tmp = new float[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp[i]))
            {
                return false;
            }
        }

        values = tmp;
        return true;
    }

    public void Dispose()
    {
        _udp.Dispose();
    }
}

internal sealed class TelemetryCalibrator
{
    private (float Roll, float Pitch, float Yaw) _leftOffset;
    private (float Roll, float Pitch, float Yaw) _rightOffset;

    public async Task CalibrateAsync(DualGloveUdpReceiver receiver, int sampleCount, CancellationToken token)
    {
        var leftSamples = new List<LeftGloveData>(sampleCount);
        var rightSamples = new List<RightGloveData>(sampleCount);

        var lastLeft = 0;
        var lastRight = 0;
        var lastProgressAt = Stopwatch.StartNew();

        while ((leftSamples.Count < sampleCount || rightSamples.Count < sampleCount) && !token.IsCancellationRequested)
        {
            var snap = receiver.LatestSnapshot;

            if (leftSamples.Count < sampleCount && snap.Left.Valid)
            {
                leftSamples.Add(snap.Left);
            }

            if (rightSamples.Count < sampleCount && snap.Right.Valid)
            {
                rightSamples.Add(snap.Right);
            }

            Console.Write($"\rLeft: {leftSamples.Count}/{sampleCount}  Right: {rightSamples.Count}/{sampleCount}");

            if (leftSamples.Count != lastLeft || rightSamples.Count != lastRight)
            {
                lastLeft = leftSamples.Count;
                lastRight = rightSamples.Count;
                lastProgressAt.Restart();
            }
            else if (lastProgressAt.Elapsed > TimeSpan.FromSeconds(25))
            {
                Console.WriteLine();
                throw new InvalidOperationException(
                    "No MPU telemetry received after CALIBRATE command. " +
                    "Check glove IPs/port and confirm both boards are connected to Wi-Fi.");
            }

            await Task.Delay(10, token);
        }

        Console.WriteLine();

        _leftOffset = (
            leftSamples.Average(x => x.Roll),
            leftSamples.Average(x => x.Pitch),
            leftSamples.Average(x => x.Yaw)
        );

        _rightOffset = (
            rightSamples.Average(x => x.Roll),
            rightSamples.Average(x => x.Pitch),
            rightSamples.Average(x => x.Yaw)
        );
    }

    public LeftGloveData ApplyLeft(LeftGloveData src) =>
        src.Valid
            ? new LeftGloveData(src.Roll - _leftOffset.Roll, src.Pitch - _leftOffset.Pitch, src.Yaw - _leftOffset.Yaw, true)
            : src;

    public RightGloveData ApplyRight(RightGloveData src) =>
        src.Valid
            ? new RightGloveData(
                src.Roll - _rightOffset.Roll,
                src.Pitch - _rightOffset.Pitch,
                src.Yaw - _rightOffset.Yaw,
                src.HeartRate,
                src.HandTemp,
                true)
            : src;
}

internal enum HandSide
{
    Unknown,
    Left,
    Right
}

internal sealed class HandFrame
{
    [JsonPropertyName("left")]
    public List<float>? Left { get; set; }

    [JsonPropertyName("right")]
    public List<float>? Right { get; set; }
}

internal readonly record struct LeftGloveData(float Roll, float Pitch, float Yaw, bool Valid);
internal readonly record struct RightGloveData(float Roll, float Pitch, float Yaw, float HeartRate, float HandTemp, bool Valid);

internal sealed class TelemetrySnapshot
{
    public LeftGloveData Left { get; set; } = new(0, 0, 0, false);
    public RightGloveData Right { get; set; } = new(0, 0, 0, float.NaN, float.NaN, false);

    public bool HasBothMpu => Left.Valid && Right.Valid;
}










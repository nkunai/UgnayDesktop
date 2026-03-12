using System.Diagnostics;
using System.Text.Json;

namespace UgnayDesktop.Services;

public sealed class GestureRuntimeService : IDisposable
{
    private const int MotionWindowSize = 18;
    private const int MotionMinFrames = 8;
    private const double MotionThreshold = 0.010;

    private readonly object _gate = new();
    private readonly Queue<Dictionary<string, double>> _recentFrames = new();
    private readonly NearestCentroidRuntimeModel _staticModel;
    private readonly NearestCentroidRuntimeModel _motionModel;

    private Process? _process;
    private string? _lastWorkerError;
    private GestureRuntimeSnapshot _latestSnapshot;

    public GestureRuntimeService()
    {
        _staticModel = NearestCentroidRuntimeModel.Load(ResolveModelPath(
            "gesture_static_separated_model.json",
            "gesture_static_model.json"));
        _motionModel = NearestCentroidRuntimeModel.Load(ResolveModelPath(
            "gesture_motion_separated_model.json",
            "gesture_motion_model.json"));
        _latestSnapshot = new GestureRuntimeSnapshot
        {
            Status = "Idle",
            ActiveModel = "None",
            PredictedLabel = "Waiting to start camera"
        };
    }


    private static void LogWorkerMessage(string message)
    {
        var line = $"[Stage3 Camera] {message}";
        Console.Error.WriteLine(line);
        Debug.WriteLine(line);
        Trace.WriteLine(line);
    }
    public GestureRuntimeSnapshot LatestSnapshot
    {
        get
        {
            lock (_gate)
            {
                return _latestSnapshot;
            }
        }
    }

    public void Start(int cameraIndex, Stage3DisplayMode displayMode = Stage3DisplayMode.InAppStream)
    {
        Stop();
        _lastWorkerError = null;

        var scriptPath = ResolveSupportFile("mediapipe_hands_worker.py");

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\" --camera {cameraIndex} --display-mode {ToWorkerArg(displayMode)}{(displayMode == Stage3DisplayMode.InAppStream ? " --no-window" : string.Empty)}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? Directory.GetCurrentDirectory()
        };

        _process = Process.Start(psi) ?? throw new InvalidOperationException("Unable to start camera worker.");
        UpdateSnapshot(snapshot => snapshot with
        {
            Status = "Camera starting...",
            PredictedLabel = "Waiting for hand landmarks",
            ActiveModel = $"Static: {_staticModel.Labels.Count} labels | Motion: {_motionModel.Labels.Count} labels"
        });

        _ = Task.Run(ReadOutputLoopAsync);
        _ = Task.Run(ReadErrorLoopAsync);
        _ = Task.Run(WatchProcessAsync);
    }

    public void Stop()
    {
        lock (_gate)
        {
            _recentFrames.Clear();
        }

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
        finally
        {
            _process?.Dispose();
            _process = null;
        }

        UpdateSnapshot(snapshot => snapshot with
        {
            Status = "Camera stopped",
            PredictedLabel = "Camera stopped",
            ActiveModel = "None",
            MovementScore = 0
        });
    }

    private async Task ReadOutputLoopAsync()
    {
        if (_process == null)
        {
            return;
        }

        while (!_process.HasExited)
        {
            var line = await _process.StandardOutput.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var frame = JsonSerializer.Deserialize<HandFramePayload>(line);
                if (frame == null)
                {
                    continue;
                }

                EvaluateFrame(frame);
            }
            catch
            {
                // ignore malformed worker lines
            }
        }
    }

    private async Task ReadErrorLoopAsync()
    {
        if (_process == null)
        {
            return;
        }

        while (!_process.HasExited)
        {
            var line = await _process.StandardError.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            _lastWorkerError = line.Trim();
            LogWorkerMessage(_lastWorkerError);
            UpdateSnapshot(snapshot => snapshot with { Status = _lastWorkerError });
        }
    }

    private async Task WatchProcessAsync()
    {
        if (_process == null)
        {
            return;
        }

        await _process.WaitForExitAsync();

        var exitCode = _process.ExitCode;
        var error = string.IsNullOrWhiteSpace(_lastWorkerError)
            ? $"Camera worker stopped (exit code {exitCode})."
            : $"{_lastWorkerError} (exit code {exitCode})";

        LogWorkerMessage(error);

        UpdateSnapshot(snapshot => snapshot with
        {
            Status = error,
            PredictedLabel = "Camera stopped",
            ActiveModel = "Worker exited",
            MovementScore = 0
        });
    }

    private void EvaluateFrame(HandFramePayload frame)
    {
        if (frame.Left == null && frame.Right == null)
        {
            lock (_gate)
            {
                _recentFrames.Clear();
            }

            UpdateSnapshot(snapshot => snapshot with
            {
                Status = "Camera running",
                PredictedLabel = "No hand detected",
                ActiveModel = "Waiting for input",
                MovementScore = 0
            });
            return;
        }

        var rawFeatureMap = FeatureMapper.BuildRawFeatureMap(frame);
        var movementScore = 0d;
        bool useMotionModel;

        lock (_gate)
        {
            _recentFrames.Enqueue(rawFeatureMap);
            while (_recentFrames.Count > MotionWindowSize)
            {
                _recentFrames.Dequeue();
            }

            movementScore = ComputeMovementScoreLocked();
            useMotionModel = _recentFrames.Count >= MotionMinFrames && movementScore >= MotionThreshold;
        }

        var prediction = useMotionModel
            ? _motionModel.Predict(BuildMotionFeatures())
            : _staticModel.Predict(BuildStaticFeatures(rawFeatureMap));

        UpdateSnapshot(snapshot => snapshot with
        {
            Status = "Camera running",
            PredictedLabel = prediction.Label,
            ActiveModel = useMotionModel ? "Motion model" : "Static model",
            MovementScore = movementScore,
            Distance = prediction.Distance,
            Confidence = prediction.Confidence,
            PreviewImageBytes = ParsePreviewBytes(frame.PreviewJpegBase64)
        });
    }

    private double[] BuildStaticFeatures(Dictionary<string, double> rawFeatureMap)
        => _staticModel.FeatureNames
            .Select(name => rawFeatureMap.TryGetValue(name, out var value) ? value : 0d)
            .ToArray();

    private double[] BuildMotionFeatures()
    {
        lock (_gate)
        {
            var frames = _recentFrames.ToArray();
            return _motionModel.FeatureNames
                .Select(name => AggregateFeature(name, frames))
                .ToArray();
        }
    }

    private static double AggregateFeature(string aggregateName, IReadOnlyList<Dictionary<string, double>> frames)
    {
        if (frames.Count == 0)
        {
            return 0d;
        }

        var suffixes = new[] { "_start", "_end", "_mean", "_min", "_max", "_delta", "_std" };
        var suffix = suffixes.FirstOrDefault(aggregateName.EndsWith);
        if (suffix == null)
        {
            return 0d;
        }

        var baseName = aggregateName[..^suffix.Length];
        var values = frames.Select(frame => frame.TryGetValue(baseName, out var value) ? value : 0d).ToArray();
        var mean = values.Average();

        return suffix switch
        {
            "_start" => values[0],
            "_end" => values[^1],
            "_mean" => mean,
            "_min" => values.Min(),
            "_max" => values.Max(),
            "_delta" => values[^1] - values[0],
            "_std" => Math.Sqrt(values.Select(value => Math.Pow(value - mean, 2)).Average()),
            _ => 0d
        };
    }

    private double ComputeMovementScoreLocked()
    {
        if (_recentFrames.Count < 2)
        {
            return 0d;
        }

        var frames = _recentFrames.ToArray();
        double total = 0d;
        int count = 0;

        for (var i = 1; i < frames.Length; i++)
        {
            total += PointDelta(frames[i - 1], frames[i], "left_0_x", "left_0_y");
            total += PointDelta(frames[i - 1], frames[i], "right_0_x", "right_0_y");
            count += 2;
        }

        return count == 0 ? 0d : total / count;
    }

    private static double PointDelta(Dictionary<string, double> previous, Dictionary<string, double> current, string xName, string yName)
    {
        previous.TryGetValue(xName, out var px);
        previous.TryGetValue(yName, out var py);
        current.TryGetValue(xName, out var cx);
        current.TryGetValue(yName, out var cy);

        if (px == 0d && py == 0d && cx == 0d && cy == 0d)
        {
            return 0d;
        }

        var dx = cx - px;
        var dy = cy - py;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private void UpdateSnapshot(Func<GestureRuntimeSnapshot, GestureRuntimeSnapshot> update)
    {
        lock (_gate)
        {
            _latestSnapshot = update(_latestSnapshot);
        }
    }


    private static string ToWorkerArg(Stage3DisplayMode displayMode)
        => displayMode == Stage3DisplayMode.ExternalWindow ? "external" : "inapp";

    private static byte[]? ParsePreviewBytes(string? previewJpegBase64)
    {
        if (string.IsNullOrWhiteSpace(previewJpegBase64))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(previewJpegBase64);
        }
        catch
        {
            return null;
        }
    }
    private string ResolveSupportFile(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, fileName),
            Path.Combine(Directory.GetCurrentDirectory(), fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "GestureCollector", fileName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", fileName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GestureCollector", fileName)
        };

        var match = candidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
        return match ?? throw new FileNotFoundException($"Missing support file: {fileName}");
    }    private static string ResolveModelPath(string preferred, string fallback)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, preferred),
            Path.Combine(AppContext.BaseDirectory, fallback),
            Path.Combine(Directory.GetCurrentDirectory(), "GestureTrainer", "artifacts", preferred),
            Path.Combine(Directory.GetCurrentDirectory(), "GestureTrainer", "artifacts", fallback)
        };

        var match = candidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
        return match ?? throw new FileNotFoundException($"Missing model file: {preferred}");
    }

    public void Dispose()
    {
        Stop();
    }
}

public sealed record GestureRuntimeSnapshot
{
    public byte[]? PreviewImageBytes { get; init; }
    public string Status { get; init; } = "Idle";
    public string ActiveModel { get; init; } = "None";
    public string PredictedLabel { get; init; } = "Waiting";
    public double MovementScore { get; init; }
    public double Distance { get; init; }
    public double Confidence { get; init; }
}

internal sealed class HandFramePayload
{
    [System.Text.Json.Serialization.JsonPropertyName("left")]
    public List<double>? Left { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("right")]
    public List<double>? Right { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("left_held")]
    public bool LeftHeld { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("right_held")]
    public bool RightHeld { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("preview_jpeg_base64")]
    public string? PreviewJpegBase64 { get; init; }
}

internal static class FeatureMapper
{
    public static Dictionary<string, double> BuildRawFeatureMap(HandFramePayload frame)
    {
        var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        MapHand(values, "left", frame.Left);
        MapHand(values, "right", frame.Right);

        values["left_roll"] = 0d;
        values["left_pitch"] = 0d;
        values["left_yaw"] = 0d;
        values["right_roll"] = 0d;
        values["right_pitch"] = 0d;
        values["right_yaw"] = 0d;
        values["left_points_visible"] = frame.Left?.Count == 42 ? 21d : 0d;
        values["right_points_visible"] = frame.Right?.Count == 42 ? 21d : 0d;
        values["left_fallback_used"] = frame.LeftHeld ? 1d : 0d;
        values["right_fallback_used"] = frame.RightHeld ? 1d : 0d;

        return values;
    }

    private static void MapHand(Dictionary<string, double> values, string prefix, List<double>? hand)
    {
        for (var i = 0; i < 21; i++)
        {
            values[$"{prefix}_{i}_x"] = 0d;
            values[$"{prefix}_{i}_y"] = 0d;
        }

        if (hand == null)
        {
            return;
        }

        for (var i = 0; i < Math.Min(21, hand.Count / 2); i++)
        {
            values[$"{prefix}_{i}_x"] = hand[i * 2];
            values[$"{prefix}_{i}_y"] = hand[(i * 2) + 1];
        }
    }
}

internal sealed class NearestCentroidRuntimeModel
{
    private readonly Dictionary<string, double[]> _centroids;

    private NearestCentroidRuntimeModel(List<string> labels, List<string> featureNames, double[] means, double[] stds, Dictionary<string, double[]> centroids)
    {
        Labels = labels;
        FeatureNames = featureNames;
        Means = means;
        Stds = stds;
        _centroids = centroids;
    }

    public List<string> Labels { get; }
    public List<string> FeatureNames { get; }
    public double[] Means { get; }
    public double[] Stds { get; }

    public static NearestCentroidRuntimeModel Load(string path)
    {
        var json = File.ReadAllText(path);
        var payload = JsonSerializer.Deserialize<SerializableModel>(json)
            ?? throw new InvalidOperationException($"Invalid model payload: {path}");

        return new NearestCentroidRuntimeModel(payload.Labels, payload.FeatureNames, payload.Means, payload.Stds, payload.Centroids);
    }

    public PredictionResult Predict(double[] rawFeatures)
    {
        var scaled = new double[rawFeatures.Length];
        for (var i = 0; i < rawFeatures.Length; i++)
        {
            var std = Stds[i] == 0d ? 1d : Stds[i];
            scaled[i] = (rawFeatures[i] - Means[i]) / std;
        }

        string bestLabel = "Unknown";
        double bestDistance = double.MaxValue;
        double secondDistance = double.MaxValue;

        foreach (var pair in _centroids)
        {
            var distance = 0d;
            for (var i = 0; i < scaled.Length; i++)
            {
                var diff = scaled[i] - pair.Value[i];
                distance += diff * diff;
            }

            if (distance < bestDistance)
            {
                secondDistance = bestDistance;
                bestDistance = distance;
                bestLabel = pair.Key;
            }
            else if (distance < secondDistance)
            {
                secondDistance = distance;
            }
        }

        var confidence = secondDistance == double.MaxValue
            ? 1d
            : Math.Clamp((secondDistance - bestDistance) / Math.Max(secondDistance, 1e-6), 0d, 1d);

        return new PredictionResult(bestLabel, bestDistance, confidence);
    }

    private sealed class SerializableModel
    {
        public required List<string> Labels { get; init; }
        public required List<string> FeatureNames { get; init; }
        public required double[] Means { get; init; }
        public required double[] Stds { get; init; }
        public required Dictionary<string, double[]> Centroids { get; init; }
    }
}

internal sealed record PredictionResult(string Label, double Distance, double Confidence);

public enum Stage3DisplayMode
{
    InAppStream,
    ExternalWindow
}
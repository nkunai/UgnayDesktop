using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UgnayDesktop.Services;

public sealed class GloveStartupService : IDisposable
{
    private static readonly TimeSpan OnlineWindow = TimeSpan.FromSeconds(2);

    private readonly object _gate = new();
    private readonly string? _configuredLeftIp;
    private readonly string? _configuredRightIp;
    private readonly int _port;

    private string? _detectedLeftIp;
    private string? _detectedRightIp;

    public GloveStartupService(string? leftIp, string? rightIp, int port = UdpSensorListener.DefaultPort)
    {
        _configuredLeftIp = string.IsNullOrWhiteSpace(leftIp) ? null : leftIp.Trim();
        _configuredRightIp = string.IsNullOrWhiteSpace(rightIp) ? null : rightIp.Trim();
        _port = port;
        LatestSnapshot = new GloveStartupSnapshot();
        UdpSensorListener.Shared.PacketReceived += OnPacketReceived;
    }

    public GloveStartupSnapshot LatestSnapshot { get; private set; }

    public void Start()
    {
        UdpSensorListener.Shared.Start(_port);
    }

    private void OnPacketReceived(IPEndPoint endpoint, string payload)
    {
        if (!TryParseCsv(payload, out var values))
        {
            return;
        }

        var senderIp = endpoint.Address.ToString();

        lock (_gate)
        {
            var snapshot = LatestSnapshot;
            var target = ResolveTarget(senderIp, values.Length);
            var receivedAtUtc = DateTime.UtcNow;

            if (target == GloveSide.Left && values.Length >= 3)
            {
                snapshot = snapshot with
                {
                    LeftIp = ResolveLeftIp(),
                    LeftLastSeenUtc = receivedAtUtc,
                    Left = new GlovePose(values[0], values[1], values[2], true)
                };
            }
            else if (target == GloveSide.Right && values.Length >= 5)
            {
                snapshot = snapshot with
                {
                    RightIp = ResolveRightIp(),
                    RightLastSeenUtc = receivedAtUtc,
                    Right = new GlovePose(values[0], values[1], values[2], true)
                };
            }

            LatestSnapshot = snapshot;
        }
    }

    public async Task WaitForBothGlovesAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (GetSnapshot().HasBothOnline)
            {
                return;
            }

            await Task.Delay(150, token);
        }
    }

    public async Task SendCalibrationCommandAsync(CancellationToken token)
    {
        var targets = new List<IPEndPoint>();

        if (IPAddress.TryParse(ResolveLeftIp(), out var leftIp))
        {
            targets.Add(new IPEndPoint(leftIp, _port));
        }

        if (IPAddress.TryParse(ResolveRightIp(), out var rightIp) &&
            !targets.Any(endpoint => endpoint.Address.Equals(rightIp)))
        {
            targets.Add(new IPEndPoint(rightIp, _port));
        }

        if (targets.Count == 0)
        {
            targets.Add(new IPEndPoint(IPAddress.Broadcast, _port));
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

    public async Task CalibrateAsync(int sampleCount, CancellationToken token)
    {
        var leftSamples = new List<GlovePose>(sampleCount);
        var rightSamples = new List<GlovePose>(sampleCount);
        var lastProgress = Stopwatch.StartNew();
        var previousCounts = (Left: 0, Right: 0);

        while ((leftSamples.Count < sampleCount || rightSamples.Count < sampleCount) && !token.IsCancellationRequested)
        {
            var snapshot = GetSnapshot();

            if (leftSamples.Count < sampleCount && snapshot.Left.Valid)
            {
                leftSamples.Add(snapshot.Left);
            }

            if (rightSamples.Count < sampleCount && snapshot.Right.Valid)
            {
                rightSamples.Add(snapshot.Right);
            }

            lock (_gate)
            {
                LatestSnapshot = LatestSnapshot with
                {
                    CalibrationLeftSamples = leftSamples.Count,
                    CalibrationRightSamples = rightSamples.Count,
                    CalibrationTargetSamples = sampleCount
                };
            }

            if (previousCounts.Left != leftSamples.Count || previousCounts.Right != rightSamples.Count)
            {
                previousCounts = (leftSamples.Count, rightSamples.Count);
                lastProgress.Restart();
            }
            else if (lastProgress.Elapsed > TimeSpan.FromSeconds(25))
            {
                throw new InvalidOperationException(
                    "No glove telemetry arrived during calibration. Check Wi-Fi and both glove IPs.");
            }

            await Task.Delay(10, token);
        }

        var leftOffset = new GlovePose(
            (float)leftSamples.Average(sample => sample.Roll),
            (float)leftSamples.Average(sample => sample.Pitch),
            (float)leftSamples.Average(sample => sample.Yaw),
            true);

        var rightOffset = new GlovePose(
            (float)rightSamples.Average(sample => sample.Roll),
            (float)rightSamples.Average(sample => sample.Pitch),
            (float)rightSamples.Average(sample => sample.Yaw),
            true);

        lock (_gate)
        {
            LatestSnapshot = LatestSnapshot with
            {
                LeftOffset = leftOffset,
                RightOffset = rightOffset,
                CalibrationLeftSamples = sampleCount,
                CalibrationRightSamples = sampleCount,
                CalibrationTargetSamples = sampleCount,
                IsCalibrated = true
            };
        }
    }

    public GloveStartupSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            var nowUtc = DateTime.UtcNow;
            return LatestSnapshot with
            {
                LeftIp = ResolveLeftIp(),
                RightIp = ResolveRightIp(),
                LeftOnline = nowUtc - LatestSnapshot.LeftLastSeenUtc <= OnlineWindow,
                RightOnline = nowUtc - LatestSnapshot.RightLastSeenUtc <= OnlineWindow
            };
        }
    }

    private GloveSide ResolveTarget(string senderIp, int valueCount)
    {
        if (!string.IsNullOrWhiteSpace(_configuredLeftIp) && senderIp == _configuredLeftIp)
        {
            return GloveSide.Left;
        }

        if (!string.IsNullOrWhiteSpace(_configuredRightIp) && senderIp == _configuredRightIp)
        {
            return GloveSide.Right;
        }

        if (valueCount == 3)
        {
            _detectedLeftIp ??= senderIp;
            return GloveSide.Left;
        }

        if (valueCount >= 5)
        {
            _detectedRightIp ??= senderIp;
            return GloveSide.Right;
        }

        return GloveSide.Unknown;
    }

    private string? ResolveLeftIp() => _configuredLeftIp ?? _detectedLeftIp;

    private string? ResolveRightIp() => _configuredRightIp ?? _detectedRightIp;

    private static bool TryParseCsv(string line, out float[] values)
    {
        values = Array.Empty<float>();
        var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 3)
        {
            return false;
        }

        var parsed = new float[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out parsed[i]))
            {
                return false;
            }
        }

        values = parsed;
        return true;
    }

    public void Dispose()
    {
        UdpSensorListener.Shared.PacketReceived -= OnPacketReceived;
    }
}

public sealed record GloveStartupSnapshot
{
    public string? LeftIp { get; init; }
    public string? RightIp { get; init; }
    public DateTime LeftLastSeenUtc { get; init; } = DateTime.MinValue;
    public DateTime RightLastSeenUtc { get; init; } = DateTime.MinValue;
    public bool LeftOnline { get; init; }
    public bool RightOnline { get; init; }
    public GlovePose Left { get; init; } = new(0, 0, 0, false);
    public GlovePose Right { get; init; } = new(0, 0, 0, false);
    public GlovePose LeftOffset { get; init; } = new(0, 0, 0, false);
    public GlovePose RightOffset { get; init; } = new(0, 0, 0, false);
    public int CalibrationLeftSamples { get; init; }
    public int CalibrationRightSamples { get; init; }
    public int CalibrationTargetSamples { get; init; }
    public bool IsCalibrated { get; init; }
    public bool HasBothOnline => LeftOnline && RightOnline;
}

public readonly record struct GlovePose(float Roll, float Pitch, float Yaw, bool Valid);

internal enum GloveSide
{
    Unknown,
    Left,
    Right
}

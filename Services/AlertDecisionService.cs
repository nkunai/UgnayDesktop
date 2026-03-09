using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public enum AlertSeverity
{
    Normal = 0,
    Info = 1,
    Warning = 2,
    Critical = 3,
}

public sealed record AlertDecision(
    AlertSeverity Severity,
    IReadOnlyList<string> Messages,
    bool IsSuppressed,
    DateTime? CooldownUntilUtc);

public class AlertDecisionService
{
    private static readonly ConcurrentDictionary<string, DateTime> LastRaisedByFingerprint = new();

    private static readonly TimeSpan WarningCooldown = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan CriticalCooldown = TimeSpan.FromSeconds(120);

    private readonly GestureQualityService _gestureQualityService = new();

    public AlertDecision Evaluate(SensorReading reading, DateTime utcNow)
    {
        var findings = new List<(AlertSeverity Severity, string Message)>();

        if (reading.BodyTemperatureC is >= 39.0)
        {
            findings.Add((AlertSeverity.Critical, $"Critical temp {reading.BodyTemperatureC:0.0}C"));
        }
        else if (reading.BodyTemperatureC is > 38.0)
        {
            findings.Add((AlertSeverity.Warning, $"High temp {reading.BodyTemperatureC:0.0}C"));
        }

        if (reading.Spo2 is < 90.0)
        {
            findings.Add((AlertSeverity.Critical, $"Critical SpO2 {reading.Spo2:0.0}%"));
        }
        else if (reading.Spo2 is < 92.0)
        {
            findings.Add((AlertSeverity.Warning, $"Low SpO2 {reading.Spo2:0.0}%"));
        }

        if (reading.HeartRate is > 130.0)
        {
            findings.Add((AlertSeverity.Critical, $"Critical HR {reading.HeartRate:0}"));
        }
        else if (reading.HeartRate is > 120.0)
        {
            findings.Add((AlertSeverity.Warning, $"High HR {reading.HeartRate:0}"));
        }

        var gestureQuality = _gestureQualityService.Evaluate(reading);
        if (!gestureQuality.IsTrusted && !string.IsNullOrWhiteSpace(gestureQuality.QualityMessage))
        {
            findings.Add((AlertSeverity.Info, gestureQuality.QualityMessage));
        }

        if (findings.Count == 0)
        {
            return new AlertDecision(AlertSeverity.Normal, Array.Empty<string>(), false, null);
        }

        var severity = findings.Max(f => f.Severity);
        var messages = findings
            .Where(f => f.Severity == severity)
            .Select(f => f.Message)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (severity is AlertSeverity.Warning or AlertSeverity.Critical)
        {
            var cooldown = severity == AlertSeverity.Critical ? CriticalCooldown : WarningCooldown;
            var fingerprint = BuildFingerprint(reading.DeviceId, severity, messages);

            if (LastRaisedByFingerprint.TryGetValue(fingerprint, out var lastRaisedUtc)
                && utcNow - lastRaisedUtc < cooldown)
            {
                return new AlertDecision(severity, messages, true, lastRaisedUtc + cooldown);
            }

            LastRaisedByFingerprint[fingerprint] = utcNow;
            TrimOldEntries(utcNow);

            return new AlertDecision(severity, messages, false, null);
        }

        return new AlertDecision(severity, messages, false, null);
    }

    private static string BuildFingerprint(string deviceId, AlertSeverity severity, IEnumerable<string> messages)
    {
        var safeDeviceId = string.IsNullOrWhiteSpace(deviceId) ? "unknown" : deviceId.Trim().ToLowerInvariant();
        var normalizedMessages = messages
            .OrderBy(m => m, StringComparer.OrdinalIgnoreCase)
            .Select(m => m.Trim().ToLowerInvariant());

        return $"{safeDeviceId}|{severity}|{string.Join("|", normalizedMessages)}";
    }

    private static void TrimOldEntries(DateTime utcNow)
    {
        if (LastRaisedByFingerprint.Count < 2000)
        {
            return;
        }

        var cutoff = utcNow - TimeSpan.FromMinutes(30);
        foreach (var kvp in LastRaisedByFingerprint)
        {
            if (kvp.Value < cutoff)
            {
                LastRaisedByFingerprint.TryRemove(kvp.Key, out _);
            }
        }
    }
}

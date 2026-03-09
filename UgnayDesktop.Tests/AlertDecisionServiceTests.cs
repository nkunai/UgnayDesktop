using System;
using UgnayDesktop.Models;
using UgnayDesktop.Services;

namespace UgnayDesktop.Tests;

public class AlertDecisionServiceTests
{
    [Fact]
    public void Evaluate_ReturnsNormalWhenNoFindings()
    {
        var service = new AlertDecisionService();
        var reading = BuildReading(
            bodyTemperatureC: 36.8,
            spo2: 98,
            heartRate: 75,
            handTracked: true,
            handGestureConfidence: 0.90);

        var decision = service.Evaluate(reading, DateTime.UtcNow);

        Assert.Equal(AlertSeverity.Normal, decision.Severity);
        Assert.False(decision.IsSuppressed);
        Assert.Null(decision.CooldownUntilUtc);
        Assert.Empty(decision.Messages);
    }

    [Fact]
    public void Evaluate_PicksCriticalWhenCriticalAndWarningFindingsExist()
    {
        var service = new AlertDecisionService();
        var reading = BuildReading(
            bodyTemperatureC: 39.1,
            spo2: 91,
            heartRate: 82,
            handTracked: true,
            handGestureConfidence: 0.95);

        var decision = service.Evaluate(reading, DateTime.UtcNow);

        Assert.Equal(AlertSeverity.Critical, decision.Severity);
        Assert.False(decision.IsSuppressed);
        Assert.Contains(decision.Messages, message => message.Contains("Critical temp", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(decision.Messages, message => message.Contains("Low SpO2", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_SuppressesRepeatedWarningInsideCooldown()
    {
        var service = new AlertDecisionService();
        var now = DateTime.UtcNow;
        var reading = BuildReading(
            bodyTemperatureC: 38.4,
            spo2: 95,
            heartRate: 88,
            handTracked: true,
            handGestureConfidence: 0.91,
            deviceId: $"cam-{Guid.NewGuid():N}");

        var first = service.Evaluate(reading, now);
        var second = service.Evaluate(reading, now.AddSeconds(30));

        Assert.Equal(AlertSeverity.Warning, first.Severity);
        Assert.False(first.IsSuppressed);
        Assert.Null(first.CooldownUntilUtc);

        Assert.Equal(AlertSeverity.Warning, second.Severity);
        Assert.True(second.IsSuppressed);
        Assert.NotNull(second.CooldownUntilUtc);
        Assert.True(second.CooldownUntilUtc > now.AddSeconds(30));
    }

    [Fact]
    public void Evaluate_ReturnsInfoForUntrustedGestureWithoutVitalsAlert()
    {
        var service = new AlertDecisionService();
        var reading = BuildReading(
            bodyTemperatureC: 36.9,
            spo2: 98,
            heartRate: 70,
            handTracked: false,
            handGestureConfidence: 0.95,
            deviceId: $"cam-{Guid.NewGuid():N}");

        var decision = service.Evaluate(reading, DateTime.UtcNow);

        Assert.Equal(AlertSeverity.Info, decision.Severity);
        Assert.False(decision.IsSuppressed);
        Assert.Contains(decision.Messages, message => message.Contains("Gesture untrusted", StringComparison.OrdinalIgnoreCase));
    }

    private static SensorReading BuildReading(
        double bodyTemperatureC,
        double spo2,
        double heartRate,
        bool? handTracked,
        double? handGestureConfidence,
        string? deviceId = null)
    {
        return new SensorReading
        {
            DeviceId = deviceId ?? $"cam-{Guid.NewGuid():N}",
            ReceivedAtUtc = DateTime.UtcNow,
            BodyTemperatureC = bodyTemperatureC,
            Spo2 = spo2,
            HeartRate = heartRate,
            HandTracked = handTracked,
            HandGestureConfidence = handGestureConfidence,
            HandGesture = "open_palm",
        };
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using UgnayDesktop.Data;
using UgnayDesktop.Models;
using UgnayDesktop.Services;

namespace UgnayDesktop.Tests;

public class AlertOutboxServiceTests
{
    private readonly AlertOutboxService _service = new();

    public AlertOutboxServiceTests()
    {
        DbInitializer.Seed();
        ClearOutbox();
    }

    [Fact]
    public void TryEnqueueFromDecision_WarningCreatesPendingMessage()
    {
        var reading = BuildReading();
        var decision = new AlertDecision(
            AlertSeverity.Warning,
            new[] { "High temp 38.4C" },
            IsSuppressed: false,
            CooldownUntilUtc: null);

        var queued = _service.TryEnqueueFromDecision(reading, decision, "UnitTest", out var outboxMessage);

        Assert.True(queued);
        Assert.NotNull(outboxMessage);

        using var db = new AppDbContext();
        var persisted = db.AlertOutboxMessages.FirstOrDefault(x => x.Id == outboxMessage!.Id);

        Assert.NotNull(persisted);
        Assert.Equal(AlertOutboxStatus.Pending, persisted!.Status);
        Assert.Equal("Warning", persisted.Severity);
        Assert.Equal(0, persisted.AttemptCount);
        Assert.NotNull(persisted.NextAttemptAtUtc);
        Assert.Contains("High temp", persisted.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryEnqueueFromDecision_DeduplicatesByDedupKey()
    {
        var reading = BuildReading();
        var decision = new AlertDecision(
            AlertSeverity.Critical,
            new[] { "Critical SpO2 89.0%" },
            IsSuppressed: false,
            CooldownUntilUtc: null);

        var firstQueued = _service.TryEnqueueFromDecision(reading, decision, "UnitTest", out var first);
        var secondQueued = _service.TryEnqueueFromDecision(reading, decision, "UnitTest", out var second);

        Assert.True(firstQueued);
        Assert.False(secondQueued);
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first!.Id, second!.Id);

        using var db = new AppDbContext();
        var count = db.AlertOutboxMessages.Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public void TryEnqueueFromDecision_SuppressedDecisionIsNotQueued()
    {
        var reading = BuildReading();
        var decision = new AlertDecision(
            AlertSeverity.Warning,
            new[] { "High HR 121" },
            IsSuppressed: true,
            CooldownUntilUtc: DateTime.UtcNow.AddSeconds(10));

        var queued = _service.TryEnqueueFromDecision(reading, decision, "UnitTest", out var outboxMessage);

        Assert.False(queued);
        Assert.Null(outboxMessage);

        using var db = new AppDbContext();
        Assert.Empty(db.AlertOutboxMessages.ToList());
    }

    [Fact]
    public void StatusTracking_ProcessingFailedAndSentUpdatesPersistedState()
    {
        var reading = BuildReading();
        var decision = new AlertDecision(
            AlertSeverity.Warning,
            new[] { "Low SpO2 91.0%" },
            IsSuppressed: false,
            CooldownUntilUtc: null);

        _service.TryEnqueueFromDecision(reading, decision, "UnitTest", out var queuedMessage);
        Assert.NotNull(queuedMessage);

        var processingAt = DateTime.UtcNow;
        var processing = _service.TryMarkProcessing(queuedMessage!.Id, processingAt);
        Assert.NotNull(processing);
        Assert.Equal(AlertOutboxStatus.Processing, processing!.Status);
        Assert.Equal(1, processing.AttemptCount);

        var failedAt = processingAt.AddSeconds(5);
        var failed = _service.MarkFailed(
            queuedMessage.Id,
            "Temporary Twilio timeout",
            failedAt,
            TimeSpan.FromMinutes(2),
            "+639123456789");
        Assert.True(failed);

        using (var db = new AppDbContext())
        {
            var failedState = db.AlertOutboxMessages.First(x => x.Id == queuedMessage.Id);
            Assert.Equal(AlertOutboxStatus.Failed, failedState.Status);
            Assert.Contains("timeout", failedState.LastError!, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(failedAt.AddMinutes(2), failedState.NextAttemptAtUtc);
            Assert.Equal("+639123456789", failedState.RecipientPhoneNumber);
        }

        var sentAt = failedAt.AddMinutes(3);
        var sent = _service.MarkSent(queuedMessage.Id, sentAt, "SM000001", "+639123456789");
        Assert.True(sent);

        using var finalDb = new AppDbContext();
        var sentState = finalDb.AlertOutboxMessages.First(x => x.Id == queuedMessage.Id);
        Assert.Equal(AlertOutboxStatus.Sent, sentState.Status);
        Assert.Equal(sentAt, sentState.SentAtUtc);
        Assert.Equal("SM000001", sentState.ProviderMessageId);
        Assert.Equal("+639123456789", sentState.RecipientPhoneNumber);
        Assert.Null(sentState.LastError);
        Assert.Null(sentState.NextAttemptAtUtc);
    }

    [Fact]
    public void GetDueMessages_ExcludesFutureRetryItems()
    {
        var reading1 = BuildReading(deviceId: "camera-due-1", receivedAtUtc: DateTime.Parse("2026-03-09T12:00:00Z").ToUniversalTime());
        var reading2 = BuildReading(deviceId: "camera-due-2", receivedAtUtc: DateTime.Parse("2026-03-09T12:01:00Z").ToUniversalTime());

        var decision = new AlertDecision(
            AlertSeverity.Warning,
            new[] { "High temp 38.4C" },
            IsSuppressed: false,
            CooldownUntilUtc: null);

        _service.TryEnqueueFromDecision(reading1, decision, "UnitTest", out var dueMessage);
        _service.TryEnqueueFromDecision(reading2, decision, "UnitTest", out var delayedMessage);

        Assert.NotNull(dueMessage);
        Assert.NotNull(delayedMessage);

        var processing = _service.TryMarkProcessing(delayedMessage!.Id, DateTime.UtcNow);
        Assert.NotNull(processing);

        _service.MarkFailed(delayedMessage.Id, "Temporary issue", DateTime.UtcNow, TimeSpan.FromMinutes(10));

        var due = _service.GetDueMessages(10, DateTime.UtcNow);

        Assert.Contains(due, item => item.Id == dueMessage!.Id);
        Assert.DoesNotContain(due, item => item.Id == delayedMessage.Id);
    }

    [Fact]
    public void RecoverStaleProcessing_RequeuesStaleItems()
    {
        var reading = BuildReading(deviceId: "camera-stale-1", receivedAtUtc: DateTime.Parse("2026-03-09T12:02:00Z").ToUniversalTime());
        var decision = new AlertDecision(
            AlertSeverity.Critical,
            new[] { "Critical SpO2 88.0%" },
            IsSuppressed: false,
            CooldownUntilUtc: null);

        _service.TryEnqueueFromDecision(reading, decision, "UnitTest", out var queuedMessage);
        Assert.NotNull(queuedMessage);

        var staleAttemptTime = DateTime.UtcNow.AddMinutes(-10);
        var processing = _service.TryMarkProcessing(queuedMessage!.Id, staleAttemptTime);
        Assert.NotNull(processing);

        var recoveredCount = _service.RecoverStaleProcessing(DateTime.UtcNow, TimeSpan.FromMinutes(3));

        Assert.Equal(1, recoveredCount);

        using var db = new AppDbContext();
        var recovered = db.AlertOutboxMessages.First(x => x.Id == queuedMessage.Id);
        Assert.Equal(AlertOutboxStatus.Failed, recovered.Status);
        Assert.NotNull(recovered.NextAttemptAtUtc);
        Assert.True(recovered.NextAttemptAtUtc <= DateTime.UtcNow.AddSeconds(1));
    }

    private static void ClearOutbox()
    {
        using var db = new AppDbContext();
        db.Database.ExecuteSqlRaw("DELETE FROM AlertOutboxMessages;");
    }

    private static SensorReading BuildReading(string? deviceId = null, DateTime? receivedAtUtc = null)
    {
        return new SensorReading
        {
            DeviceId = deviceId ?? "camera-test-01",
            ReceivedAtUtc = receivedAtUtc ?? DateTime.Parse("2026-03-09T12:00:00Z").ToUniversalTime(),
            HandGesture = "open_palm",
            HandGestureConfidence = 0.91,
            HandTracked = true,
            BodyTemperatureC = 38.4,
            HeartRate = 118,
            Spo2 = 95,
        };
    }
}

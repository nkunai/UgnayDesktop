using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UgnayDesktop.Data;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public class AlertOutboxService
{
    public bool TryEnqueueFromDecision(SensorReading reading, AlertDecision decision, string source, out AlertOutboxMessage? queuedMessage)
    {
        queuedMessage = null;

        if (decision.IsSuppressed)
        {
            return false;
        }

        if (decision.Severity is not AlertSeverity.Warning and not AlertSeverity.Critical)
        {
            return false;
        }

        var utcNow = DateTime.UtcNow;
        var normalizedMessages = decision.Messages
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Select(message => message.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(message => message, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var summary = normalizedMessages.Length == 0
            ? $"{decision.Severity} alert"
            : string.Join("; ", normalizedMessages);

        var dedupKey = BuildDedupKey(reading.DeviceId, reading.ReceivedAtUtc, decision.Severity, normalizedMessages);

        var outboxMessage = new AlertOutboxMessage
        {
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            Source = string.IsNullOrWhiteSpace(source) ? "unknown" : source,
            DeviceId = string.IsNullOrWhiteSpace(reading.DeviceId) ? "unknown" : reading.DeviceId,
            Severity = decision.Severity.ToString(),
            Message = summary,
            DedupKey = dedupKey,
            Status = AlertOutboxStatus.Pending,
            AttemptCount = 0,
            NextAttemptAtUtc = utcNow,
            PayloadJson = JsonSerializer.Serialize(new
            {
                reading.DeviceId,
                reading.ReceivedAtUtc,
                decision.Severity,
                Messages = normalizedMessages,
                reading.HandGesture,
                reading.HandGestureConfidence,
                reading.HandTracked,
                reading.BodyTemperatureC,
                reading.HeartRate,
                reading.Spo2,
            })
        };

        using var db = new AppDbContext();

        var existing = db.AlertOutboxMessages
            .OrderByDescending(item => item.Id)
            .FirstOrDefault(item => item.DedupKey == outboxMessage.DedupKey);
        if (existing != null)
        {
            queuedMessage = existing;
            return false;
        }

        try
        {
            db.AlertOutboxMessages.Add(outboxMessage);
            db.SaveChanges();
            queuedMessage = outboxMessage;
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            var existingAfterConflict = db.AlertOutboxMessages
                .OrderByDescending(item => item.Id)
                .FirstOrDefault(item => item.DedupKey == outboxMessage.DedupKey);

            if (existingAfterConflict != null)
            {
                queuedMessage = existingAfterConflict;
                return false;
            }

            throw;
        }
    }

    public IReadOnlyList<AlertOutboxMessage> GetDueMessages(int maxBatchSize, DateTime utcNow)
    {
        var batchSize = maxBatchSize <= 0 ? 1 : maxBatchSize;

        using var db = new AppDbContext();
        return db.AlertOutboxMessages
            .AsNoTracking()
            .Where(item =>
                (item.Status == AlertOutboxStatus.Pending || item.Status == AlertOutboxStatus.Failed)
                && (!item.NextAttemptAtUtc.HasValue || item.NextAttemptAtUtc <= utcNow))
            .OrderBy(item => item.CreatedAtUtc)
            .ThenBy(item => item.Id)
            .Take(batchSize)
            .ToList();
    }

    public int RecoverStaleProcessing(DateTime utcNow, TimeSpan staleAfter)
    {
        if (staleAfter <= TimeSpan.Zero)
        {
            staleAfter = TimeSpan.FromMinutes(1);
        }

        var cutoff = utcNow - staleAfter;

        using var db = new AppDbContext();
        var staleItems = db.AlertOutboxMessages
            .Where(item => item.Status == AlertOutboxStatus.Processing
                && item.LastAttemptAtUtc.HasValue
                && item.LastAttemptAtUtc <= cutoff)
            .ToList();

        if (staleItems.Count == 0)
        {
            return 0;
        }

        foreach (var staleItem in staleItems)
        {
            staleItem.Status = AlertOutboxStatus.Failed;
            staleItem.UpdatedAtUtc = utcNow;
            staleItem.NextAttemptAtUtc = utcNow;
            staleItem.LastError = string.IsNullOrWhiteSpace(staleItem.LastError)
                ? "Recovered stale processing state"
                : staleItem.LastError;
        }

        db.SaveChanges();
        return staleItems.Count;
    }

    public AlertOutboxMessage? TryMarkProcessing(int id, DateTime utcNow)
    {
        using var db = new AppDbContext();
        var item = db.AlertOutboxMessages.FirstOrDefault(x => x.Id == id);
        if (item == null)
        {
            return null;
        }

        if (item.Status is not AlertOutboxStatus.Pending and not AlertOutboxStatus.Failed)
        {
            return null;
        }

        item.Status = AlertOutboxStatus.Processing;
        item.UpdatedAtUtc = utcNow;
        item.LastAttemptAtUtc = utcNow;
        item.AttemptCount += 1;
        db.SaveChanges();
        return item;
    }

    public bool MarkSent(int id, DateTime utcNow, string? providerMessageId = null, string? recipientPhoneNumber = null)
    {
        using var db = new AppDbContext();
        var item = db.AlertOutboxMessages.FirstOrDefault(x => x.Id == id);
        if (item == null)
        {
            return false;
        }

        item.Status = AlertOutboxStatus.Sent;
        item.UpdatedAtUtc = utcNow;
        item.SentAtUtc = utcNow;
        item.LastError = null;
        item.NextAttemptAtUtc = null;

        if (!string.IsNullOrWhiteSpace(providerMessageId))
        {
            item.ProviderMessageId = providerMessageId;
        }

        if (!string.IsNullOrWhiteSpace(recipientPhoneNumber))
        {
            item.RecipientPhoneNumber = recipientPhoneNumber;
        }

        db.SaveChanges();
        return true;
    }

    public bool MarkFailed(int id, string error, DateTime utcNow, TimeSpan retryDelay, string? recipientPhoneNumber = null)
    {
        using var db = new AppDbContext();
        var item = db.AlertOutboxMessages.FirstOrDefault(x => x.Id == id);
        if (item == null)
        {
            return false;
        }

        item.Status = AlertOutboxStatus.Failed;
        item.UpdatedAtUtc = utcNow;
        item.LastError = string.IsNullOrWhiteSpace(error) ? "Unknown error" : error.Trim();
        item.NextAttemptAtUtc = utcNow + retryDelay;

        if (!string.IsNullOrWhiteSpace(recipientPhoneNumber))
        {
            item.RecipientPhoneNumber = recipientPhoneNumber;
        }

        db.SaveChanges();
        return true;
    }

    private static string BuildDedupKey(string deviceId, DateTime receivedAtUtc, AlertSeverity severity, string[] messages)
    {
        var safeDeviceId = string.IsNullOrWhiteSpace(deviceId)
            ? "unknown"
            : deviceId.Trim().ToLowerInvariant();

        var normalizedMessages = messages
            .Select(message => message.Trim().ToLowerInvariant());

        return $"{safeDeviceId}|{receivedAtUtc:O}|{severity}|{string.Join("|", normalizedMessages)}";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}

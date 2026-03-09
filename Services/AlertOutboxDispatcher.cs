using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UgnayDesktop.Data;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public sealed class AlertOutboxDispatcher : IDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(4);
    private static readonly TimeSpan ProcessingStaleAfter = TimeSpan.FromMinutes(3);
    private const int MaxBatchSize = 10;

    private readonly AlertOutboxService _outboxService = new();
    private readonly TwilioService _twilioService = new();
    private readonly System.Threading.Timer _timer;
    private readonly SemaphoreSlim _drainLock = new(1, 1);

    private bool _started;
    private bool _disposed;

    public AlertOutboxDispatcher()
    {
        _timer = new System.Threading.Timer(_ => _ = DrainAsync(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Start()
    {
        if (_disposed || _started)
        {
            return;
        }

        _started = true;
        _timer.Change(InitialDelay, PollInterval);
        AppLogger.Info("AlertOutboxDispatcher", "Background alert dispatcher started.", eventName: "alert_dispatcher_started");
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }

        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _started = false;
        AppLogger.Info("AlertOutboxDispatcher", "Background alert dispatcher stopped.", eventName: "alert_dispatcher_stopped");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _timer.Dispose();
        _drainLock.Dispose();
        _disposed = true;
    }

    private async Task DrainAsync()
    {
        if (_disposed || !_started)
        {
            return;
        }

        if (!await _drainLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            DrainCore();
        }
        catch (Exception ex)
        {
            AppLogger.Error("AlertOutboxDispatcher", "Unexpected error while draining outbox queue.", ex, eventName: "alert_outbox_drain_failed");
        }
        finally
        {
            _drainLock.Release();
        }
    }

    private void DrainCore()
    {
        var utcNow = DateTime.UtcNow;

        var recovered = _outboxService.RecoverStaleProcessing(utcNow, ProcessingStaleAfter);
        if (recovered > 0)
        {
            AppLogger.Warning("AlertOutboxDispatcher", "Recovered stale processing outbox items.", eventName: "alert_outbox_recovered", context: new { recovered });
        }

        var dueMessages = _outboxService.GetDueMessages(MaxBatchSize, utcNow);
        foreach (var dueMessage in dueMessages)
        {
            var processing = _outboxService.TryMarkProcessing(dueMessage.Id, DateTime.UtcNow);
            if (processing == null)
            {
                continue;
            }

            ProcessMessage(processing);
        }
    }

    private void ProcessMessage(AlertOutboxMessage message)
    {
        try
        {
            var recipientPhone = ResolveRecipientPhoneNumber();
            if (string.IsNullOrWhiteSpace(recipientPhone))
            {
                throw new InvalidOperationException("No alert recipient configured. Set UGNAY_ALERT_TO_PHONE_NUMBER or add a teacher phone number.");
            }

            var fromPhone = _twilioService.GetConfiguredFromPhoneNumber();
            if (string.IsNullOrWhiteSpace(fromPhone))
            {
                throw new InvalidOperationException("Missing TWILIO_FROM_PHONE_NUMBER environment variable.");
            }

            var body = BuildSmsBody(message);
            var sid = _twilioService.SendSms(fromPhone, recipientPhone, body);

            _outboxService.MarkSent(message.Id, DateTime.UtcNow, sid, recipientPhone);
            AppLogger.Info("AlertOutboxDispatcher", "Sent outbox message.", eventName: "alert_outbox_sent", context: new { message.Id, message.DeviceId, message.Severity, RecipientPhone = recipientPhone, ProviderMessageId = sid });
        }
        catch (Exception ex)
        {
            var retryDelay = AlertRetryPolicy.ComputeRetryDelay(message.AttemptCount);
            _outboxService.MarkFailed(message.Id, ex.Message, DateTime.UtcNow, retryDelay, ResolveBestEffortRecipient());
            AppLogger.Warning("AlertOutboxDispatcher", "Outbox send failed.", ex, eventName: "alert_outbox_send_failed", context: new { message.Id, message.DeviceId, message.Severity, RetrySeconds = retryDelay.TotalSeconds });
        }
    }

    private string? ResolveRecipientPhoneNumber()
    {
        var configuredRecipient = _twilioService.GetConfiguredAlertRecipientPhoneNumber();
        if (!string.IsNullOrWhiteSpace(configuredRecipient))
        {
            return configuredRecipient;
        }

        using var db = new AppDbContext();
        return db.Users
            .Where(user => user.Role == "Teacher" && user.TeacherPhoneNumber != null && user.TeacherPhoneNumber.Trim() != string.Empty)
            .OrderBy(user => user.Id)
            .Select(user => user.TeacherPhoneNumber)
            .FirstOrDefault();
    }

    private string? ResolveBestEffortRecipient()
    {
        try
        {
            return ResolveRecipientPhoneNumber();
        }
        catch
        {
            return null;
        }
    }

    private static string BuildSmsBody(AlertOutboxMessage message)
    {
        var text = $"[Ugnay] {message.Severity.ToUpperInvariant()} | Device {message.DeviceId} | {message.Message}";
        return text.Length > 300 ? text.Substring(0, 297) + "..." : text;
    }
}



using System;

namespace UgnayDesktop.Services;

public static class AlertRetryPolicy
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(30);

    public static TimeSpan ComputeRetryDelay(int attemptCount)
    {
        var safeAttemptCount = Math.Max(1, attemptCount);
        var exponent = Math.Min(safeAttemptCount - 1, 10);
        var delaySeconds = InitialDelay.TotalSeconds * Math.Pow(2, exponent);
        var delay = TimeSpan.FromSeconds(delaySeconds);

        return delay > MaxDelay ? MaxDelay : delay;
    }
}

using System;
using UgnayDesktop.Services;

namespace UgnayDesktop.Tests;

public class AlertRetryPolicyTests
{
    [Fact]
    public void ComputeRetryDelay_StartsAtThirtySeconds()
    {
        var delay = AlertRetryPolicy.ComputeRetryDelay(1);
        Assert.Equal(TimeSpan.FromSeconds(30), delay);
    }

    [Fact]
    public void ComputeRetryDelay_DoublesWithAttempts()
    {
        Assert.Equal(TimeSpan.FromMinutes(1), AlertRetryPolicy.ComputeRetryDelay(2));
        Assert.Equal(TimeSpan.FromMinutes(2), AlertRetryPolicy.ComputeRetryDelay(3));
        Assert.Equal(TimeSpan.FromMinutes(4), AlertRetryPolicy.ComputeRetryDelay(4));
    }

    [Fact]
    public void ComputeRetryDelay_IsCappedAtThirtyMinutes()
    {
        var delay = AlertRetryPolicy.ComputeRetryDelay(20);
        Assert.Equal(TimeSpan.FromMinutes(30), delay);
    }
}

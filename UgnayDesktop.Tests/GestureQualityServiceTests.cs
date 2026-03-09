using UgnayDesktop.Models;
using UgnayDesktop.Services;

namespace UgnayDesktop.Tests;

public class GestureQualityServiceTests
{
    [Fact]
    public void Evaluate_UntrustedWhenHandNotTracked()
    {
        var service = new GestureQualityService();
        var reading = new SensorReading
        {
            HandTracked = false,
            HandGestureConfidence = 0.95,
            HandGesture = "fist",
        };

        var result = service.Evaluate(reading);

        Assert.False(result.IsTrusted);
        Assert.Contains("no hand tracked", result.GestureStatusText);
        Assert.Contains("Gesture untrusted", result.QualityMessage);
    }

    [Fact]
    public void Evaluate_UntrustedWhenConfidenceMissing()
    {
        var service = new GestureQualityService();
        var reading = new SensorReading
        {
            HandTracked = true,
            HandGestureConfidence = null,
            HandGesture = "fist",
        };

        var result = service.Evaluate(reading);

        Assert.False(result.IsTrusted);
        Assert.Contains("no confidence", result.GestureStatusText);
        Assert.Contains("missing confidence", result.QualityMessage);
    }

    [Fact]
    public void Evaluate_UntrustedWhenConfidenceBelowThreshold()
    {
        var service = new GestureQualityService(0.80);
        var reading = new SensorReading
        {
            HandTracked = true,
            HandGestureConfidence = 0.75,
            HandGesture = "fist",
        };

        var result = service.Evaluate(reading);

        Assert.False(result.IsTrusted);
        Assert.Contains("low confidence", result.GestureStatusText);
        Assert.Contains("<0.80", result.QualityMessage);
    }

    [Fact]
    public void Evaluate_TrustedWhenGestureAndConfidenceAreValid()
    {
        var service = new GestureQualityService();
        var reading = new SensorReading
        {
            HandTracked = true,
            HandGestureConfidence = 0.91,
            HandGesture = "thumbs_up",
        };

        var result = service.Evaluate(reading);

        Assert.True(result.IsTrusted);
        Assert.Contains("thumbs_up", result.GestureStatusText);
        Assert.Null(result.QualityMessage);
        Assert.Equal(0.91, result.Confidence);
    }
}

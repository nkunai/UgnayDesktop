using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public sealed record GestureQualityResult(
    bool IsTrusted,
    string GestureStatusText,
    string? QualityMessage,
    double? Confidence,
    double MinConfidence);

public class GestureQualityService
{
    public const double DefaultMinConfidence = 0.70;

    private readonly double _minConfidence;

    public GestureQualityService(double minConfidence = DefaultMinConfidence)
    {
        _minConfidence = minConfidence;
    }

    public GestureQualityResult Evaluate(SensorReading reading)
    {
        if (reading.HandTracked == false)
        {
            return new GestureQualityResult(
                IsTrusted: false,
                GestureStatusText: "gesture: unavailable (no hand tracked)",
                QualityMessage: "Gesture untrusted: no hand tracked",
                Confidence: reading.HandGestureConfidence,
                MinConfidence: _minConfidence);
        }

        if (reading.HandGestureConfidence is null)
        {
            return new GestureQualityResult(
                IsTrusted: false,
                GestureStatusText: "gesture: unavailable (no confidence)",
                QualityMessage: "Gesture untrusted: missing confidence",
                Confidence: null,
                MinConfidence: _minConfidence);
        }

        if (reading.HandGestureConfidence < _minConfidence)
        {
            return new GestureQualityResult(
                IsTrusted: false,
                GestureStatusText: $"gesture: unavailable (low confidence {reading.HandGestureConfidence:0.00})",
                QualityMessage: $"Gesture untrusted: low confidence {reading.HandGestureConfidence:0.00} (<{_minConfidence:0.00})",
                Confidence: reading.HandGestureConfidence,
                MinConfidence: _minConfidence);
        }

        if (string.IsNullOrWhiteSpace(reading.HandGesture))
        {
            return new GestureQualityResult(
                IsTrusted: true,
                GestureStatusText: "gesture: n/a",
                QualityMessage: null,
                Confidence: reading.HandGestureConfidence,
                MinConfidence: _minConfidence);
        }

        return new GestureQualityResult(
            IsTrusted: true,
            GestureStatusText: $"gesture: {reading.HandGesture} ({reading.HandGestureConfidence:0.00})",
            QualityMessage: null,
            Confidence: reading.HandGestureConfidence,
            MinConfidence: _minConfidence);
    }
}

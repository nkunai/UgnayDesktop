using System;
using System.Globalization;
using System.Text.Json;

namespace UgnayDesktop.Services;

public sealed class CameraPreviewParser
{
    public bool IsPreviewTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return false;
        }

        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 3
            && string.Equals(parts[0], "esp32", StringComparison.OrdinalIgnoreCase)
            && string.Equals(parts[2], "preview", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(parts[1]);
    }

    public bool TryParse(string topic, string payload, out CameraPreviewFrame? frame)
    {
        frame = null;

        if (!IsPreviewTopic(topic))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            if (!root.TryGetProperty("camera", out var camera) || camera.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!camera.TryGetProperty("frameBase64", out var frameBase64Value))
            {
                return false;
            }

            var frameBase64 = frameBase64Value.ValueKind == JsonValueKind.String
                ? frameBase64Value.GetString()
                : frameBase64Value.ToString();

            if (string.IsNullOrWhiteSpace(frameBase64))
            {
                return false;
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(frameBase64);
            }
            catch (FormatException)
            {
                return false;
            }

            var topicParts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var deviceId = topicParts[1];

            frame = new CameraPreviewFrame(
                deviceId,
                imageBytes,
                TryParseTimestamp(root));

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static DateTime? TryParseTimestamp(JsonElement root)
    {
        if (!root.TryGetProperty("ts", out var ts))
        {
            return null;
        }

        var raw = ts.ValueKind == JsonValueKind.String
            ? ts.GetString()
            : ts.ToString();

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return DateTime.TryParse(
            raw,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed
            : null;
    }
}

public sealed record CameraPreviewFrame(
    string DeviceId,
    byte[] ImageBytes,
    DateTime? TimestampUtc);


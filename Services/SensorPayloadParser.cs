using System.Text.Json;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

internal static class SensorPayloadParser
{
    public static bool TryParse(string payload, out SensorReading reading, out string? error)
    {
        reading = new SensorReading();
        error = null;

        if (string.IsNullOrWhiteSpace(payload))
        {
            error = "empty payload";
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var mpu = TryGetObject(root, "mpu6050");
            var max = TryGetObject(root, "max30192") ?? TryGetObject(root, "max30102");

            reading = new SensorReading
            {
                DeviceId = GetString(root, "deviceId") ?? "unknown",
                ReceivedAtUtc = GetDateTime(root, "ts") ?? DateTime.UtcNow,
                FlexValue = GetNumber(root, "flex") ?? GetNumber(root, "flexSensor"),
                AccelX = mpu is null ? null : GetNumber(mpu.Value, "accelX"),
                AccelY = mpu is null ? null : GetNumber(mpu.Value, "accelY"),
                AccelZ = mpu is null ? null : GetNumber(mpu.Value, "accelZ"),
                GyroX = mpu is null ? null : GetNumber(mpu.Value, "gyroX"),
                GyroY = mpu is null ? null : GetNumber(mpu.Value, "gyroY"),
                GyroZ = mpu is null ? null : GetNumber(mpu.Value, "gyroZ"),
                HeartRate = max is null ? null : GetNumber(max.Value, "heartRate"),
                Spo2 = max is null ? null : GetNumber(max.Value, "spo2"),
                GsrValue = GetNumber(root, "gsr") ?? GetNumber(root, "gsrValue"),
                BodyTemperatureC = GetNumber(root, "ds18b20") ?? GetNumber(root, "temperatureC"),
                RawJson = payload
            };

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static JsonElement? TryGetObject(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Object)
        {
            return value;
        }

        return null;
    }

    private static double? GetNumber(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string? GetString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static DateTime? GetDateTime(JsonElement element, string property)
    {
        var raw = GetString(element, property);
        if (raw == null)
        {
            return null;
        }

        return DateTime.TryParse(raw, out var parsed) ? parsed.ToUniversalTime() : null;
    }
}

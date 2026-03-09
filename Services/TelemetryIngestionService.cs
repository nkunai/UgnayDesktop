using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using UgnayDesktop.Data;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public class TelemetryIngestionService
{
    public bool IsTelemetryTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return false;
        }

        return topic.EndsWith("/data", StringComparison.OrdinalIgnoreCase);
    }

    public SensorReading IngestPayload(string payload)
    {
        SensorReading reading;
        try
        {
            reading = ParsePayload(payload);
        }
        catch (Exception ex)
        {
            AppLogger.Warning(
                "TelemetryIngestionService",
                "Payload parse failed.",
                ex,
                eventName: "telemetry_payload_parse_failed",
                context: new
                {
                    PayloadLength = payload.Length,
                    Payload = TruncatePayload(payload),
                });
            throw;
        }

        reading.RawJson = payload;

        using var db = new AppDbContext();

        var existing = FindExistingReading(db, reading);
        if (existing != null)
        {
            return existing;
        }

        try
        {
            db.SensorReadings.Add(reading);
            db.SaveChanges();
            return reading;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            var existingAfterConflict = FindExistingReading(db, reading);
            if (existingAfterConflict != null)
            {
                AppLogger.Info(
                    "TelemetryIngestionService",
                    "Duplicate telemetry payload ignored.",
                    eventName: "telemetry_duplicate_ignored",
                    context: new
                    {
                        reading.DeviceId,
                        reading.ReceivedAtUtc,
                    });

                return existingAfterConflict;
            }

            AppLogger.Warning(
                "TelemetryIngestionService",
                "Unique-key conflict occurred but existing row was not found.",
                ex,
                eventName: "telemetry_unique_conflict_unresolved",
                context: new
                {
                    reading.DeviceId,
                    reading.ReceivedAtUtc,
                });

            throw;
        }
    }

    public SensorReading ParsePayload(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var mpu = TryGetObject(root, "mpu6050");
        var max = TryGetObject(root, "max30192") ?? TryGetObject(root, "max30102");
        var camera = TryGetObject(root, "camera") ?? TryGetObject(root, "handTracking") ?? TryGetObject(root, "vision");

        return new SensorReading
        {
            DeviceId = GetString(root, "deviceId") ?? "unknown",
            ReceivedAtUtc = GetDateTime(root, "ts") ?? DateTime.UtcNow,
            HandGesture = GetString(root, "handGesture")
                ?? GetString(root, "gesture")
                ?? (camera is null ? null : GetString(camera.Value, "gesture"))
                ?? (camera is null ? null : GetString(camera.Value, "label")),
            HandGestureConfidence = GetNumber(root, "gestureConfidence")
                ?? GetNumber(root, "handGestureConfidence")
                ?? (camera is null ? null : GetNumber(camera.Value, "confidence"))
                ?? (camera is null ? null : GetNumber(camera.Value, "score")),
            HandTracked = GetBoolean(root, "handTracked")
                ?? GetBoolean(root, "hasHand")
                ?? (camera is null ? null : GetBoolean(camera.Value, "tracked"))
                ?? (camera is null ? null : GetBoolean(camera.Value, "handDetected")),
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
        };
    }

    private static SensorReading? FindExistingReading(AppDbContext db, SensorReading reading)
    {
        return db.SensorReadings
            .OrderByDescending(r => r.Id)
            .FirstOrDefault(r => r.DeviceId == reading.DeviceId
                && r.ReceivedAtUtc == reading.ReceivedAtUtc
                && r.RawJson == reading.RawJson);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
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
        if (!element.TryGetProperty(property, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var n)) return n;

        if (value.ValueKind == JsonValueKind.String)
        {
            var raw = value.GetString();
            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedInvariant)) return parsedInvariant;
            if (double.TryParse(raw, out var parsedCurrent)) return parsedCurrent;
        }

        return null;
    }

    private static bool? GetBoolean(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value)) return null;
        if (value.ValueKind == JsonValueKind.True) return true;
        if (value.ValueKind == JsonValueKind.False) return false;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var n)) return n != 0;

        if (value.ValueKind == JsonValueKind.String)
        {
            var raw = value.GetString();
            if (bool.TryParse(raw, out var parsedBool)) return parsedBool;
            if (int.TryParse(raw, out var parsedInt)) return parsedInt != 0;
        }

        return null;
    }

    private static string? GetString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static DateTime? GetDateTime(JsonElement element, string property)
    {
        var raw = GetString(element, property);
        if (raw == null) return null;

        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;
    }

    private static string? TruncatePayload(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return payload;
        }

        return payload.Length <= 1024
            ? payload
            : payload.Substring(0, 1024) + "...";
    }
}



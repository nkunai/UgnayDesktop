using System;
using UgnayDesktop.Services;

namespace UgnayDesktop.Tests;

public class TelemetryIngestionServiceTests
{
    private readonly TelemetryIngestionService _sut = new();

    [Fact]
    public void ParsePayload_MapsNestedCameraAndSensorFields()
    {
        const string payload = """
        {
          "deviceId": "camera-student-01",
          "ts": "2026-03-09T10:00:00Z",
          "camera": {
            "label": "thumbs_up",
            "score": 0.93,
            "tracked": true
          },
          "mpu6050": {
            "accelX": 1.1,
            "accelY": 2.2,
            "accelZ": 3.3,
            "gyroX": 4.4,
            "gyroY": 5.5,
            "gyroZ": 6.6
          },
          "max30102": {
            "heartRate": 88,
            "spo2": 97
          },
          "gsr": 123,
          "temperatureC": 36.8
        }
        """;

        var reading = _sut.ParsePayload(payload);

        Assert.Equal("camera-student-01", reading.DeviceId);
        Assert.Equal(DateTime.Parse("2026-03-09T10:00:00Z").ToUniversalTime(), reading.ReceivedAtUtc);
        Assert.Equal("thumbs_up", reading.HandGesture);
        Assert.Equal(0.93, reading.HandGestureConfidence);
        Assert.True(reading.HandTracked);
        Assert.Equal(1.1, reading.AccelX);
        Assert.Equal(2.2, reading.AccelY);
        Assert.Equal(3.3, reading.AccelZ);
        Assert.Equal(4.4, reading.GyroX);
        Assert.Equal(5.5, reading.GyroY);
        Assert.Equal(6.6, reading.GyroZ);
        Assert.Equal(88, reading.HeartRate);
        Assert.Equal(97, reading.Spo2);
        Assert.Equal(123, reading.GsrValue);
        Assert.Equal(36.8, reading.BodyTemperatureC);
    }

    [Fact]
    public void ParsePayload_ParsesStringValuesFromFallbackObjects()
    {
        const string payload = """
        {
          "handTracking": {
            "gesture": "open_palm",
            "confidence": "0.82",
            "handDetected": "1"
          },
          "max30192": {
            "heartRate": "101",
            "spo2": "95"
          },
          "gsrValue": "210",
          "ds18b20": "37.2"
        }
        """;

        var reading = _sut.ParsePayload(payload);

        Assert.Equal("unknown", reading.DeviceId);
        Assert.Equal("open_palm", reading.HandGesture);
        Assert.Equal(0.82, reading.HandGestureConfidence);
        Assert.True(reading.HandTracked);
        Assert.Equal(101, reading.HeartRate);
        Assert.Equal(95, reading.Spo2);
        Assert.Equal(210, reading.GsrValue);
        Assert.Equal(37.2, reading.BodyTemperatureC);
    }

    [Fact]
    public void ParsePayload_UsesUtcNowWhenTimestampMissing()
    {
        var before = DateTime.UtcNow;
        var reading = _sut.ParsePayload("{\"deviceId\":\"cam-02\"}");
        var after = DateTime.UtcNow;

        Assert.Equal("cam-02", reading.DeviceId);
        Assert.InRange(reading.ReceivedAtUtc, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public void ParsePayload_PrefersRootGestureFieldsOverCameraFields()
    {
        const string payload = """
        {
          "deviceId": "camera-student-02",
          "gesture": "fist",
          "gestureConfidence": 0.88,
          "hasHand": 1,
          "camera": {
            "label": "open_palm",
            "score": 0.42,
            "tracked": false
          }
        }
        """;

        var reading = _sut.ParsePayload(payload);

        Assert.Equal("fist", reading.HandGesture);
        Assert.Equal(0.88, reading.HandGestureConfidence);
        Assert.True(reading.HandTracked);
    }

    [Fact]
    public void ParsePayload_ParsesVisionPayloadWithStringAndNumericBooleans()
    {
        const string payload = """
        {
          "deviceId": "camera-student-03",
          "vision": {
            "gesture": "no_hand",
            "confidence": "0.67",
            "handDetected": "0"
          }
        }
        """;

        var reading = _sut.ParsePayload(payload);

        Assert.Equal("camera-student-03", reading.DeviceId);
        Assert.Equal("no_hand", reading.HandGesture);
        Assert.Equal(0.67, reading.HandGestureConfidence);
        Assert.False(reading.HandTracked);
    }

    [Fact]
    public void IsTelemetryTopic_ReturnsTrueForCameraAndEsp32DataTopics()
    {
        Assert.True(_sut.IsTelemetryTopic("camera/data"));
        Assert.True(_sut.IsTelemetryTopic("camera/student-01/data"));
        Assert.True(_sut.IsTelemetryTopic("esp32/student-01/data"));
        Assert.False(_sut.IsTelemetryTopic("camera/status"));
    }
}


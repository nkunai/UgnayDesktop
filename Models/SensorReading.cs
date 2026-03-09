namespace UgnayDesktop.Models;

public class SensorReading
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = "unknown";
    public DateTime ReceivedAtUtc { get; set; }

    public string? HandGesture { get; set; }
    public double? HandGestureConfidence { get; set; }
    public bool? HandTracked { get; set; }

    public double? AccelX { get; set; }
    public double? AccelY { get; set; }
    public double? AccelZ { get; set; }
    public double? GyroX { get; set; }
    public double? GyroY { get; set; }
    public double? GyroZ { get; set; }

    public double? HeartRate { get; set; }
    public double? Spo2 { get; set; }

    public double? GsrValue { get; set; }
    public double? BodyTemperatureC { get; set; }

    public string RawJson { get; set; } = string.Empty;
}

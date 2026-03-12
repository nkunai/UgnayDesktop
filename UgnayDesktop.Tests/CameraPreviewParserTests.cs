using System;
using System.Text;
using UgnayDesktop.Services;

namespace UgnayDesktop.Tests;

public class CameraPreviewParserTests
{
    private readonly CameraPreviewParser _sut = new();

    [Fact]
    public void TryParse_ParsesValidPreviewPayload()
    {
        var bytes = Encoding.UTF8.GetBytes("preview-frame");
        var payload = $$"""
        {
          "ts": "2026-03-10T12:00:00Z",
          "camera": {
            "frameBase64": "{{Convert.ToBase64String(bytes)}}"
          }
        }
        """;

        var ok = _sut.TryParse("esp32/camera-student-01/preview", payload, out var frame);

        Assert.True(ok);
        Assert.NotNull(frame);
        Assert.Equal("camera-student-01", frame!.DeviceId);
        Assert.Equal(bytes, frame.ImageBytes);
        Assert.Equal(DateTime.Parse("2026-03-10T12:00:00Z").ToUniversalTime(), frame.TimestampUtc);
    }

    [Fact]
    public void TryParse_ReturnsFalseForInvalidJson()
    {
        var ok = _sut.TryParse("esp32/camera-student-01/preview", "{", out var frame);

        Assert.False(ok);
        Assert.Null(frame);
    }

    [Fact]
    public void TryParse_ReturnsFalseWhenFrameFieldMissing()
    {
        const string payload = """
        {
          "camera": {
            "label": "open_palm"
          }
        }
        """;

        var ok = _sut.TryParse("esp32/camera-student-01/preview", payload, out var frame);

        Assert.False(ok);
        Assert.Null(frame);
    }

    [Fact]
    public void TryParse_ReturnsFalseWhenBase64Invalid()
    {
        const string payload = """
        {
          "camera": {
            "frameBase64": "not-base64"
          }
        }
        """;

        var ok = _sut.TryParse("esp32/camera-student-01/preview", payload, out var frame);

        Assert.False(ok);
        Assert.Null(frame);
    }

    [Fact]
    public void TryParse_ReturnsNullTimestampWhenMissing()
    {
        var payload = $$"""
        {
          "camera": {
            "frameBase64": "{{Convert.ToBase64String(Encoding.UTF8.GetBytes("preview-frame"))}}"
          }
        }
        """;

        var ok = _sut.TryParse("esp32/camera-student-01/preview", payload, out var frame);

        Assert.True(ok);
        Assert.NotNull(frame);
        Assert.Null(frame!.TimestampUtc);
    }

    [Fact]
    public void IsPreviewTopic_ReturnsTrueForExpectedTopic()
    {
        Assert.True(_sut.IsPreviewTopic("esp32/camera-student-01/preview"));
        Assert.False(_sut.IsPreviewTopic("esp32/camera-student-01/data"));
    }
}


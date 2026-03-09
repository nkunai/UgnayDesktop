using System;
using System.Text;
using MQTTnet;
using MQTTnet.Protocol;

namespace UgnayDesktop.Services;

public class MqttService
{
    private readonly IMqttClient _client;

    public event Action<string, string>? MessageReceived;

    public MqttService()
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();

        _client.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            try
            {
                MessageReceived?.Invoke(topic, payload);
            }
            catch (Exception ex)
            {
                AppLogger.Error("MqttService", $"Message handler failed for topic '{topic}'.", ex);
            }

            return Task.CompletedTask;
        };
    }

    public async Task EnsureConnectedAsync(string host = "localhost", int port = 1883)
    {
        if (_client.IsConnected)
        {
            return;
        }

        var options = new MqttClientOptionsBuilder()
            .WithClientId($"ugnay-desktop-{Guid.NewGuid():N}")
            .WithTcpServer(host, port)
            .Build();

        await _client.ConnectAsync(options);
        AppLogger.Info("MqttService", $"Connected to MQTT broker {host}:{port}.");
    }

    public async Task SubscribeAsync(string topic, string host = "localhost", int port = 1883)
    {
        await EnsureConnectedAsync(host, port);
        await _client.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtMostOnce);
        AppLogger.Info("MqttService", $"Subscribed to topic '{topic}'.");
    }

    public async Task<bool> RunLoopbackTestAsync(
        string topic = "ugnaydesktop/test",
        int timeoutMs = 5000)
    {
        await EnsureConnectedAsync();

        var payload = $"ping-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(string messageTopic, string messagePayload)
        {
            if (messageTopic == topic && messagePayload == payload)
            {
                received.TrySetResult(true);
            }
        }

        MessageReceived += Handler;

        try
        {
            await _client.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtMostOnce);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await _client.PublishAsync(message);

            var completed = await Task.WhenAny(received.Task, Task.Delay(timeoutMs));
            var ok = completed == received.Task && received.Task.Result;

            if (ok)
            {
                AppLogger.Info("MqttService", "Loopback test passed.");
            }
            else
            {
                AppLogger.Warning("MqttService", "Loopback test timed out.");
            }

            return ok;
        }
        finally
        {
            MessageReceived -= Handler;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
            AppLogger.Info("MqttService", "Disconnected from MQTT broker.");
        }
    }
}

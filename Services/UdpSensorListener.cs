using System.Net;
using System.Net.Sockets;
using System.Text;
using UgnayDesktop.Data;
using UgnayDesktop.Models;

namespace UgnayDesktop.Services;

public sealed class UdpSensorListener : IDisposable
{
    public const int DefaultPort = 5005;

    private readonly object _gate = new();
    private CancellationTokenSource? _cts;
    private Task? _listenTask;
    private UdpClient? _udp;

    public static UdpSensorListener Shared { get; } = new();

    public event Action<SensorReading>? SensorReadingReceived;
    public event Action<IPEndPoint, string>? PacketReceived;

    public bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _listenTask is { IsCompleted: false };
            }
        }
    }

    public void Start(int port = DefaultPort)
    {
        lock (_gate)
        {
            if (_listenTask is { IsCompleted: false })
            {
                return;
            }

            _cts = new CancellationTokenSource();
            _udp = new UdpClient(port);
            _listenTask = Task.Run(() => ListenLoopAsync(_udp, _cts.Token), _cts.Token);
        }
    }

    private async Task ListenLoopAsync(UdpClient udp, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            UdpReceiveResult packet;

            try
            {
                packet = await udp.ReceiveAsync(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch
            {
                await Task.Delay(250, token);
                continue;
            }

            var payload = Encoding.UTF8.GetString(packet.Buffer).Trim();
            PacketReceived?.Invoke(packet.RemoteEndPoint, payload);

            if (!SensorPayloadParser.TryParse(payload, out var reading, out _))
            {
                continue;
            }

            try
            {
                using var db = new AppDbContext();
                db.SensorReadings.Add(reading);
                db.SaveChanges();
            }
            catch
            {
                continue;
            }

            SensorReadingReceived?.Invoke(reading);
        }
    }

    public void Dispose()
    {
        CancellationTokenSource? cts;
        UdpClient? udp;
        Task? listenTask;

        lock (_gate)
        {
            cts = _cts;
            udp = _udp;
            listenTask = _listenTask;
            _cts = null;
            _udp = null;
            _listenTask = null;
        }

        cts?.Cancel();
        udp?.Dispose();

        try
        {
            listenTask?.Wait(1000);
        }
        catch
        {
            // ignored
        }

        cts?.Dispose();
    }
}

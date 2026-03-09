using System.Text.Json;
using System.Text.Json.Serialization;

namespace UgnayDesktop.Services;

public static class AppLogger
{
    private const int RetentionDays = 14;
    private static readonly object Sync = new();
    private static readonly string LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static DateTime _lastCleanupDateUtc = DateTime.MinValue.Date;

    public static void Info(string source, string message, string? eventName = null, object? context = null)
    {
        Write("INFO", source, message, eventName, context, null);
    }

    public static void Warning(string source, string message, Exception? ex = null, string? eventName = null, object? context = null)
    {
        Write("WARN", source, message, eventName, context, ex);
    }

    public static void Error(string source, string message, Exception? ex = null, string? eventName = null, object? context = null)
    {
        Write("ERROR", source, message, eventName, context, ex);
    }

    private static void Write(string level, string source, string message, string? eventName, object? context, Exception? ex)
    {
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(LogDirectory);
                CleanupOldLogsIfNeeded();

                var utcNow = DateTime.UtcNow;
                var path = Path.Combine(LogDirectory, $"ugnay-{utcNow:yyyyMMdd}.log");

                var entry = new LogEntry
                {
                    TimestampUtc = utcNow,
                    Level = level,
                    Source = source,
                    Event = eventName,
                    Message = message,
                    Context = context,
                    Exception = ex == null
                        ? null
                        : new LogException
                        {
                            Type = ex.GetType().FullName ?? ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                        }
                };

                var line = JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine;
                File.AppendAllText(path, line);
            }
        }
        catch
        {
            // Logging should never crash the app.
        }
    }

    private static void CleanupOldLogsIfNeeded()
    {
        var todayUtc = DateTime.UtcNow.Date;
        if (_lastCleanupDateUtc == todayUtc)
        {
            return;
        }

        _lastCleanupDateUtc = todayUtc;

        var threshold = todayUtc.AddDays(-RetentionDays);
        var files = Directory.GetFiles(LogDirectory, "ugnay-*.log");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var dateToken = fileName.Replace("ugnay-", string.Empty);

            if (!DateTime.TryParseExact(dateToken, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var fileDateUtc))
            {
                continue;
            }

            if (fileDateUtc.Date < threshold)
            {
                File.Delete(file);
            }
        }
    }

    private sealed class LogEntry
    {
        public DateTime TimestampUtc { get; init; }
        public string Level { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string? Event { get; init; }
        public string Message { get; init; } = string.Empty;
        public object? Context { get; init; }
        public LogException? Exception { get; init; }
    }

    private sealed class LogException
    {
        public string Type { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? StackTrace { get; init; }
    }
}


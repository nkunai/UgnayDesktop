using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace UgnayDesktop.Services;

public class TextBeeService
{
    private const string DefaultApiBaseUrl = "https://api.textbee.dev";
    private const string TextBeeDashboardUrl = "https://textbee.dev/dashboard";
    private const string TextBeeWebhooksUrl = "https://textbee.dev/dashboard/webhooks";
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
    };

    public string GetDashboardUrl() => TextBeeDashboardUrl;

    public string GetWebhookSetupUrl() => TextBeeWebhooksUrl;

    public void OpenWebhookSetup()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = TextBeeWebhooksUrl,
            UseShellExecute = true,
        });
    }

    public bool IsConfigured()
    {
        return GetMissingConfigKeys().Count == 0;
    }

    public IReadOnlyList<string> GetMissingConfigKeys()
    {
        var missing = new List<string>();

        var apiKey = GetConfigValue("TEXTBEE_API_KEY");
        var deviceId = GetConfigValue("TEXTBEE_DEVICE_ID");

        if (string.IsNullOrWhiteSpace(apiKey)) missing.Add("TEXTBEE_API_KEY");
        if (string.IsNullOrWhiteSpace(deviceId)) missing.Add("TEXTBEE_DEVICE_ID");

        return missing;
    }

    public async Task<string> SendSmsAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
        {
            throw new ArgumentException("Recipient phone number is required.", nameof(toPhoneNumber));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message is required.", nameof(message));
        }

        var apiKey = GetConfigValue("TEXTBEE_API_KEY");
        var deviceId = GetConfigValue("TEXTBEE_DEVICE_ID");

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(deviceId))
        {
            throw new InvalidOperationException("TextBee credentials are missing. Set TEXTBEE_API_KEY and TEXTBEE_DEVICE_ID.");
        }

        var baseUrl = GetConfigValue("TEXTBEE_API_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = DefaultApiBaseUrl;
        }

        var endpoint = $"{baseUrl.TrimEnd('/')}/api/v1/gateway/devices/{Uri.EscapeDataString(deviceId)}/send-sms";
        var payload = new
        {
            recipients = new[] { toPhoneNumber },
            message,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-api-key", apiKey);

        using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(BuildFailureMessage(response.StatusCode, responseBody));
        }

        return ExtractDeliveryReference(response.StatusCode, responseBody);
    }

    public Task<string> SendOtpAsync(string toPhoneNumber, string otpCode, CancellationToken cancellationToken = default)
    {
        var message = $"Your Ugnay verification code is {otpCode}. It expires in 5 minutes.";
        return SendSmsAsync(toPhoneNumber, message, cancellationToken);
    }

    public Task<string> SendTestNotificationToTeacherAsync(string teacherPhoneNumber, string teacherName, CancellationToken cancellationToken = default)
    {
        var body = $"UgnayDesktop TextBee test notification for {teacherName} at {DateTime.Now:G}.";
        return SendSmsAsync(teacherPhoneNumber, body, cancellationToken);
    }

    private static string BuildFailureMessage(HttpStatusCode statusCode, string responseBody)
    {
        var detail = TryReadStringField(responseBody, "message")
            ?? TryReadStringField(responseBody, "error")
            ?? Compact(responseBody, 240);

        return $"TextBee request failed (HTTP {(int)statusCode}): {detail}";
    }

    private static string ExtractDeliveryReference(HttpStatusCode statusCode, string responseBody)
    {
        var reference = TryReadStringField(responseBody, "smsBatchId")
            ?? TryReadStringField(responseBody, "smsId")
            ?? TryReadStringField(responseBody, "_id")
            ?? TryReadStringField(responseBody, "id");

        return string.IsNullOrWhiteSpace(reference)
            ? $"HTTP {(int)statusCode}"
            : reference;
    }

    private static string? TryReadStringField(string rawJson, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            return TryReadStringField(doc.RootElement, fieldName);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? TryReadStringField(JsonElement element, string fieldName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(fieldName))
                {
                    return property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString()
                        : property.Value.ToString();
                }

                var nested = TryReadStringField(property.Value, fieldName);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }

            return null;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = TryReadStringField(item, fieldName);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static string Compact(string text, int maxLength)
    {
        var compact = string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : text.Replace(Environment.NewLine, " ").Trim();

        if (compact.Length <= maxLength)
        {
            return compact;
        }

        return compact[..maxLength] + "...";
    }

    private static string? GetConfigValue(string key)
    {
        var process = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(process))
        {
            return process;
        }

        var user = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        if (!string.IsNullOrWhiteSpace(user))
        {
            return user;
        }

        var machine = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
        return string.IsNullOrWhiteSpace(machine) ? null : machine;
    }
}

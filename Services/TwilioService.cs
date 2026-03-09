using System;
using System.Collections.Generic;
using System.Diagnostics;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace UgnayDesktop.Services;

public class TwilioService
{
    private const string TwilioConsoleUrl = "https://console.twilio.com/";

    public string GetConsoleUrl() => TwilioConsoleUrl;

    public void OpenConsole()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = TwilioConsoleUrl,
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

        var sid = GetConfigValue("TWILIO_ACCOUNT_SID");
        var token = GetConfigValue("TWILIO_AUTH_TOKEN");
        var from = GetConfigValue("TWILIO_FROM_PHONE_NUMBER");

        if (string.IsNullOrWhiteSpace(sid)) missing.Add("TWILIO_ACCOUNT_SID");
        if (string.IsNullOrWhiteSpace(token)) missing.Add("TWILIO_AUTH_TOKEN");
        if (string.IsNullOrWhiteSpace(from)) missing.Add("TWILIO_FROM_PHONE_NUMBER");

        return missing;
    }

    public string? GetConfiguredFromPhoneNumber()
    {
        return GetConfigValue("TWILIO_FROM_PHONE_NUMBER");
    }

    public string? GetConfiguredAlertRecipientPhoneNumber()
    {
        return GetConfigValue("UGNAY_ALERT_TO_PHONE_NUMBER");
    }

    public string SendSms(string fromPhoneNumber, string toPhoneNumber, string message)
    {
        var sid = GetConfigValue("TWILIO_ACCOUNT_SID");
        var token = GetConfigValue("TWILIO_AUTH_TOKEN");

        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Twilio credentials are missing. Set TWILIO_ACCOUNT_SID and TWILIO_AUTH_TOKEN.");
        }

        TwilioClient.Init(sid, token);

        var sms = MessageResource.Create(
            from: new PhoneNumber(fromPhoneNumber),
            to: new PhoneNumber(toPhoneNumber),
            body: message);

        return sms.Sid;
    }

    public string SendTestNotificationToTeacher(string teacherPhoneNumber, string teacherName)
    {
        var from = GetConfiguredFromPhoneNumber();
        if (string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException("Missing TWILIO_FROM_PHONE_NUMBER environment variable.");
        }

        var body = $"UgnayDesktop Twilio test notification for {teacherName} at {DateTime.Now:G}.";
        return SendSms(from, teacherPhoneNumber, body);
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

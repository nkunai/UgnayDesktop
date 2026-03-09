namespace UgnayDesktop.Services;

public interface IEmailVerificationSender
{
    void SendCode(string recipientEmail, string code, DateTime expiresAtUtc);
}

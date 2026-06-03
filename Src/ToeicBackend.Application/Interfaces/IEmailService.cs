namespace ToeicBackend.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetOtpAsync(string email, string otp);
    Task SendWelcomeEmailAsync(string email, string userName);
}

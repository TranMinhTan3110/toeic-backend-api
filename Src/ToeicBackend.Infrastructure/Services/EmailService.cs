using MailKit.Net.Smtp;
using MimeKit;
using ToeicBackend.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ToeicBackend.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendPasswordResetOtpAsync(string email, string otp)
    {
        try
        {
            var gmailEmail = _configuration["Gmail:Email"];
            var gmailPassword = _configuration["Gmail:AppPassword"];

            if (string.IsNullOrEmpty(gmailEmail) || string.IsNullOrEmpty(gmailPassword))
            {
                _logger.LogWarning("[EmailService] Gmail credentials not configured");
                throw new InvalidOperationException("Gmail credentials not configured in appsettings.json");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("TOEIC Master", gmailEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "TOEIC Master - Mã xác nhận đặt lại mật khẩu";

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ text-align: center; padding: 20px; background: linear-gradient(135deg, #FF8C42 0%, #FF6B35 100%); color: white; border-radius: 8px 8px 0 0; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ background-color: #fff; padding: 30px; border-radius: 0 0 8px 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .otp-section {{ text-align: center; margin: 30px 0; }}
        .otp-code {{ 
            font-size: 40px; 
            font-weight: bold; 
            color: #FF8C42; 
            letter-spacing: 8px; 
            background-color: #f0f0f0; 
            padding: 20px; 
            border-radius: 8px; 
            font-family: 'Courier New', monospace;
            margin: 20px 0;
        }}
        .expiration {{ color: #888; font-size: 14px; margin-top: 15px; font-style: italic; }}
        .footer {{ text-align: center; color: #999; font-size: 12px; margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; }}
        .security-note {{ background-color: #fff3cd; padding: 15px; border-radius: 4px; margin: 20px 0; color: #856404; font-size: 13px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0; font-size: 28px; line-height: 36px;'>
                <img src='https://img.icons8.com/fluency/96/target.png' width='36' height='36' style='vertical-align: middle; margin-right: 8px;' />
                <span style='vertical-align: middle;'>TOEIC Master</span>
            </h1>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác nhận bên dưới để tiếp tục:</p>
            
            <div class='otp-section'>
                <div class='otp-code'>{otp}</div>
                <p class='expiration' style='margin-top: 15px;'>
                    <img src='https://img.icons8.com/fluency/48/time.png' width='18' height='18' style='vertical-align: middle; margin-right: 6px;' />
                    <span style='vertical-align: middle;'>Mã này sẽ hết hạn sau 10 phút</span>
                </p>
            </div>
            
            <div class='security-note'>
                <strong>⚠️ Lưu ý bảo mật:</strong> Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này. Không bao giờ chia sẻ mã này với bất kỳ ai.
            </div>
            
            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội hỗ trợ của chúng tôi.</p>
            
            <div class='footer'>
                <p>© 2024 TOEIC Master. All rights reserved.</p>
                <p>Đây là email tự động, vui lòng không trả lời.</p>
            </div>
        </div>
    </div>
</body>
</html>";

            message.Body = new TextPart("html") { Text = htmlContent };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(gmailEmail, gmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation($"[EmailService] Password reset OTP sent successfully to {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[EmailService] Error sending password reset OTP to {email}");
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var gmailEmail = _configuration["Gmail:Email"];
            var gmailPassword = _configuration["Gmail:AppPassword"];

            if (string.IsNullOrEmpty(gmailEmail) || string.IsNullOrEmpty(gmailPassword))
            {
                _logger.LogWarning("[EmailService] Gmail credentials not configured");
                throw new InvalidOperationException("Gmail credentials not configured in appsettings.json");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("TOEIC Master", gmailEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Chào mừng đến TOEIC Master 🎓";

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ text-align: center; padding: 20px; background: linear-gradient(135deg, #FF8C42 0%, #FF6B35 100%); color: white; border-radius: 8px 8px 0 0; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ background-color: #fff; padding: 30px; border-radius: 0 0 8px 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .footer {{ text-align: center; color: #999; font-size: 12px; margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0; font-size: 28px; line-height: 36px;'>
                <img src='https://img.icons8.com/fluency/96/target.png' width='36' height='36' style='vertical-align: middle; margin-right: 8px;' />
                <span style='vertical-align: middle;'>TOEIC Master</span>
            </h1>
        </div>
        <div class='content'>
            <p>Chào mừng <strong>{userName}</strong>,</p>
            <p>Tài khoản của bạn đã được tạo thành công! 🎉</p>
            <p>Bây giờ bạn đã sẵn sàng bắt đầu hành trình luyện thi TOEIC của mình. Chúng tôi cung cấp:</p>
            <ul>
                <li>📚 Ngân hàng câu hỏi TOEIC đầy đủ</li>
                <li>🎯 Luyện thi theo từng kỹ năng</li>
                <li>📊 Theo dõi tiến độ học tập</li>
                <li>🏆 Gamification & Leaderboard</li>
            </ul>
            <div class='footer'>
                <p>© 2024 TOEIC Master. All rights reserved.</p>
            </div>
        </div>
    </div>
</body>
</html>";

            message.Body = new TextPart("html") { Text = htmlContent };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(gmailEmail, gmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation($"[EmailService] Welcome email sent successfully to {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[EmailService] Error sending welcome email to {email}");
            throw;
        }
    }
}

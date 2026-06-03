using Microsoft.AspNetCore.Mvc;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IEmailService emailService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncUser([FromBody] SyncUserRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "Token không được để trống" });
        }

        var result = await _authService.SyncUserAsync(request.Token);
        
        if (result)
        {
            return Ok(new { message = "Đồng bộ User thành công" });
        }

        return BadRequest(new { message = "Đồng bộ User thất bại hoặc Token không hợp lệ" });
    }

    /// <summary>
    /// Gửi OTP code để reset mật khẩu
    /// </summary>
    [HttpPost("send-reset-otp")]
    public async Task<IActionResult> SendResetPasswordOtp([FromBody] SendResetOtpRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email không được để trống" });
            }

            // Kiểm tra email có tồn tại không
            var userExists = await _authService.UserExistsByEmailAsync(request.Email);
            if (!userExists)
            {
                // Không nên leak thông tin user tồn tại hay không, trả về success
                return Ok(new { message = "Nếu email tồn tại, mã OTP sẽ được gửi" });
            }

            // Generate OTP (6 chữ số)
            var otp = GenerateOtp();
            
            // Lưu OTP vào cache hoặc database với expiry 10 phút
            await _authService.SaveResetOtpAsync(request.Email, otp);

            // Gửi email
            await _emailService.SendPasswordResetOtpAsync(request.Email, otp);

            _logger.LogInformation($"OTP sent to {request.Email}");
            return Ok(new { message = "Mã xác nhận đã được gửi tới email của bạn" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reset OTP");
            return StatusCode(500, new { message = "Có lỗi xảy ra, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Xác nhận OTP code
    /// </summary>
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetPasswordOtp([FromBody] VerifyResetOtpRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(new { message = "Email và OTP không được để trống" });
            }

            // Verify OTP
            var isValid = await _authService.VerifyResetOtpAsync(request.Email, request.Otp);
            
            if (!isValid)
            {
                return BadRequest(new { message = "Mã xác nhận không chính xác hoặc đã hết hạn" });
            }

            // Generate temporary token cho phép reset password
            var resetToken = await _authService.GeneratePasswordResetTokenAsync(request.Email);

            _logger.LogInformation($"OTP verified for {request.Email}");
            return Ok(new { 
                message = "Xác nhận thành công", 
                resetToken = resetToken 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying reset OTP");
            return StatusCode(500, new { message = "Có lỗi xảy ra, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Reset mật khẩu với OTP đã được verify
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.ResetToken))
            {
                return BadRequest(new { message = "Dữ liệu không đầy đủ" });
            }

            // Validate password
            if (request.NewPassword.Length < 8)
            {
                return BadRequest(new { message = "Mật khẩu phải có ít nhất 8 ký tự" });
            }

            // Verify reset token
            var isValidToken = await _authService.ValidatePasswordResetTokenAsync(request.Email, request.ResetToken);
            if (!isValidToken)
            {
                return BadRequest(new { message = "Token reset không hợp lệ hoặc đã hết hạn" });
            }

            // Reset password
            var success = await _authService.ResetPasswordAsync(request.Email, request.NewPassword);
            
            if (!success)
            {
                return BadRequest(new { message = "Không thể reset mật khẩu, vui lòng thử lại" });
            }

            _logger.LogInformation($"Password reset for {request.Email}");
            return Ok(new { message = "Mật khẩu đã được cập nhật thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { message = "Có lỗi xảy ra, vui lòng thử lại" });
        }
    }

    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}

// DTOs
public class SyncUserRequestDto
{
    public string Token { get; set; } = string.Empty;
}

public class SendResetOtpRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyResetOtpRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
}

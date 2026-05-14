using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.ViewModels;
using Microsoft.Extensions.Logging;

namespace Core.Shared.Services;

/// <summary>
/// Service xác thực chung cho Client và Admin
/// </summary>
public class UnifiedAuthService : IUnifiedAuthService
{
    private readonly ReaderRepository _readerRepository;
    private readonly AccountRepository _accountRepository;
    private readonly ILogger<UnifiedAuthService> _logger;

    public UnifiedAuthService(
        ReaderRepository readerRepository,
        AccountRepository accountRepository,
        ILogger<UnifiedAuthService> logger)
    {
        _readerRepository = readerRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <summary>
    /// Đăng nhập bằng email + password
    /// Kiểm tra cả Reader và Account (Admin/Staff)
    /// </summary>
    public async Task<LoginResultViewModel> LoginAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return new LoginResultViewModel
                {
                    Success = false,
                    Message = "Email và mật khẩu không được để trống"
                };
            }

            // Kiểm tra xem email có phải Reader không
            var readers = await _readerRepository.GetAllAsync();
            var reader = readers.FirstOrDefault(r => r.Email?.ToLower() == email.ToLower());

            if (reader != null && !string.IsNullOrEmpty(reader.PasswordHash))
            {
                // Verify password cho Reader
                if (BCrypt.Net.BCrypt.Verify(password, reader.PasswordHash))
                {
                    _logger.LogInformation($"Reader {reader.ReaderId} đã đăng nhập thành công");
                    return new LoginResultViewModel
                    {
                        Success = true,
                        Message = "Đăng nhập thành công",
                        UserType = "Reader",
                        UserId = reader.ReaderId,
                        UserName = reader.FullName,
                        AvatarUrl = reader.AvatarUrl
                    };
                }
            }

            // Kiểm tra xem email có phải Account (Admin/Staff) không
            var accounts = await _accountRepository.GetAllAsync();
            var account = accounts.FirstOrDefault(a => a.Email?.ToLower() == email.ToLower());

            if (account != null && !string.IsNullOrEmpty(account.Password))
            {
                // Verify password cho Account
                if (BCrypt.Net.BCrypt.Verify(password, account.Password))
                {
                    _logger.LogInformation($"Account {account.Username} đã đăng nhập thành công");
                    return new LoginResultViewModel
                    {
                        Success = true,
                        Message = "Đăng nhập thành công",
                        UserType = account.Role ?? "Admin", // "Admin" hoặc "Staff"
                        UserId = account.Username,
                        UserName = account.FullName ?? account.Username,
                        AvatarUrl = account.AvatarUrl
                    };
                }
            }

            // Email hoặc password không đúng
            return new LoginResultViewModel
            {
                Success = false,
                Message = "Email hoặc mật khẩu không đúng"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            return new LoginResultViewModel
            {
                Success = false,
                Message = $"Lỗi hệ thống: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Kiểm tra email đã được sử dụng chưa
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        // Kiểm tra Reader
        var readers = await _readerRepository.GetAllAsync();
        if (readers.Any(r => r.Email?.ToLower() == email.ToLower()))
            return true;

        // Kiểm tra Account
        var accounts = await _accountRepository.GetAllAsync();
        if (accounts.Any(a => a.Email?.ToLower() == email.ToLower()))
            return true;

        return false;
    }

    /// <summary>
    /// Lấy loại tài khoản từ email
    /// </summary>
    public async Task<string> GetAccountTypeByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return "Unknown";

        // Kiểm tra Reader
        var readers = await _readerRepository.GetAllAsync();
        if (readers.Any(r => r.Email?.ToLower() == email.ToLower()))
            return "Reader";

        // Kiểm tra Account
        var accounts = await _accountRepository.GetAllAsync();
        if (accounts.Any(a => a.Email?.ToLower() == email.ToLower()))
            return "Admin";

        return "Unknown";
    }
}

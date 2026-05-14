using Core.Shared.Interfaces;
using Core.Shared.Repositories;
using Core.Shared.ViewModels;
using Microsoft.Extensions.Logging;

namespace Core.Shared.Services;

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

    public async Task<LoginResultViewModel> LoginAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return Fail("Email và mật khẩu không được để trống");

            // ── Kiểm tra Reader ──────────────────────────────────────────
            var readers = await _readerRepository.GetAllAsync();
            var reader = readers.FirstOrDefault(r =>
                r.Email?.ToLower() == email.ToLower());

            if (reader != null)
            {
                // Reader tồn tại nhưng chưa có mật khẩu (tạo từ Admin, chưa set password)
                if (string.IsNullOrEmpty(reader.PasswordHash))
                    return Fail("Tài khoản này chưa được đặt mật khẩu. Vui lòng liên hệ thủ thư.");

                if (!BCrypt.Net.BCrypt.Verify(password, reader.PasswordHash))
                    return Fail("Email hoặc mật khẩu không đúng");

                _logger.LogInformation("Reader {ReaderId} đã đăng nhập thành công", reader.ReaderId);
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

            // ── Kiểm tra Account (Admin/Staff) ───────────────────────────
            var accounts = await _accountRepository.GetAllAsync();
            var account = accounts.FirstOrDefault(a =>
                a.Email?.ToLower() == email.ToLower());

            if (account != null)
            {
                if (string.IsNullOrEmpty(account.Password))
                    return Fail("Tài khoản này chưa có mật khẩu.");

                if (!BCrypt.Net.BCrypt.Verify(password, account.Password))
                    return Fail("Email hoặc mật khẩu không đúng");

                _logger.LogInformation("Account {Username} đã đăng nhập thành công", account.Username);
                return new LoginResultViewModel
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    UserType = account.Role ?? "Admin",
                    UserId = account.Username,
                    UserName = account.FullName ?? account.Username,
                    AvatarUrl = account.AvatarUrl
                };
            }

            // Email không tồn tại trong cả 2 bảng
            return Fail("Email hoặc mật khẩu không đúng");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            return Fail($"Lỗi hệ thống: {ex.Message}");
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;

        var readers = await _readerRepository.GetAllAsync();
        if (readers.Any(r => r.Email?.ToLower() == email.ToLower()))
            return true;

        var accounts = await _accountRepository.GetAllAsync();
        return accounts.Any(a => a.Email?.ToLower() == email.ToLower());
    }

    public async Task<string> GetAccountTypeByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) return "Unknown";

        var readers = await _readerRepository.GetAllAsync();
        if (readers.Any(r => r.Email?.ToLower() == email.ToLower()))
            return "Reader";

        var accounts = await _accountRepository.GetAllAsync();
        if (accounts.Any(a => a.Email?.ToLower() == email.ToLower()))
            return "Admin";

        return "Unknown";
    }

    private static LoginResultViewModel Fail(string message) =>
        new() { Success = false, Message = message };
}

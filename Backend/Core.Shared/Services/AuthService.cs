using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Core.Shared.Constants;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class AuthService : IAuthService
{
    private readonly AccountRepository _repo;

    // Tài khoản Admin gốc không được phép xóa
    private const string RootAdmin = "admin";

    public AuthService(AccountRepository repo)
    {
        _repo = repo;
    }

    // ------------------------------------------------------------------ //
    //  AUTH
    // ------------------------------------------------------------------ //

    public async Task<Account?> LoginAsync(string username, string password)
    {
        // Cố gắng tìm tài khoản theo username trước, nếu không có thì tìm theo email
        var account = await _repo.GetByUsernameAsync(username);
        if (account is null)
        {
            account = await _repo.GetByEmailAsync(username);
        }

        if (account is null) return null;

        // Xác thực password đã hash bằng BCrypt
        bool valid = BCrypt.Net.BCrypt.Verify(password, account.Password);
        return valid ? account : null;
    }

    // ------------------------------------------------------------------ //
    //  STAFF CRUD
    // ------------------------------------------------------------------ //

    public async Task<IEnumerable<Account>> GetAllStaffAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        return await _repo.GetByUsernameAsync(username);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _repo.ExistsAsync(username);
    }

    public async Task<string?> CreateAccountAsync(Account account, string rawPassword)
    {
        // --- Validate ---
        if (string.IsNullOrWhiteSpace(account.Username) ||
            string.IsNullOrWhiteSpace(rawPassword) ||
            string.IsNullOrWhiteSpace(account.FullName))
            return MessageConstants.DataEmpty;

        if (await _repo.ExistsAsync(account.Username))
            return MessageConstants.UsernameExists;

        if (!IsValidEmail(account.Email))
            return MessageConstants.EmailInvalid;

        if (!IsStrongPassword(rawPassword))
            return MessageConstants.PasswordWeak;

        if (!RoleConstants.IsValid(account.Role))
            return MessageConstants.RoleInvalid;

        // --- Xử lý ---
        account.Password = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        account.CreatedAt = DateTime.Now;

        await _repo.AddAsync(account);
        return null; // null = thành công
    }

    public async Task<string?> UpdateAccountAsync(Account account, string? newRawPassword)
    {
        var existing = await _repo.GetByUsernameAsync(account.Username);
        if (existing is null) return MessageConstants.AccountNotFound;

        if (!IsValidEmail(account.Email))
            return MessageConstants.EmailInvalid;

        if (!RoleConstants.IsValid(account.Role))
            return MessageConstants.RoleInvalid;

        // Cập nhật các trường được phép sửa
        existing.FullName = account.FullName;
        existing.Email = account.Email;
        existing.Role = account.Role;
        existing.AvatarUrl = account.AvatarUrl;

        // Đổi mật khẩu nếu có nhập mới
        if (!string.IsNullOrWhiteSpace(newRawPassword))
        {
            if (!IsStrongPassword(newRawPassword))
                return MessageConstants.PasswordWeak;

            existing.Password = BCrypt.Net.BCrypt.HashPassword(newRawPassword);
        }

        await _repo.UpdateAsync(existing);
        return null;
    }

    public async Task<string?> DeleteAccountAsync(string username)
    {
        // Không cho xóa tài khoản Admin gốc
        if (username.ToLower() == RootAdmin)
            return MessageConstants.DeleteAdminRoot;

        var account = await _repo.GetByUsernameAsync(username);
        if (account is null) return MessageConstants.AccountNotFound;

        // Không cho xóa nếu còn phiếu mượn liên kết
        if (await _repo.HasBorrowTicketsAsync(username))
            return MessageConstants.DeleteHasBorrow;

        await _repo.DeleteAsync(account);
        return null;
    }

    // ------------------------------------------------------------------ //
    //  HELPERS (private)
    // ------------------------------------------------------------------ //

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);
    }

    /// Tối thiểu 8 ký tự, 1 hoa, 1 thường, 1 số, 1 ký tự đặc biệt
    private static bool IsStrongPassword(string password)
    {
        if (password.Length < 8) return false;
        return Regex.IsMatch(password, @"[A-Z]") &&
               Regex.IsMatch(password, @"[a-z]") &&
               Regex.IsMatch(password, @"[0-9]") &&
               Regex.IsMatch(password, @"[^a-zA-Z0-9]");
    }
}


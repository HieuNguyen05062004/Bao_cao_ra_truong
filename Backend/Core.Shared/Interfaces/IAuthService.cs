using System;
using System.Collections.Generic;
using System.Text;
using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface IAuthService
{
    // ---------- Auth ----------
    /// <summary>Xác thực đăng nhập. Trả về Account nếu hợp lệ, null nếu sai.</summary>
    Task<Account?> LoginAsync(string username, string password);

    // ---------- Staff CRUD ----------
    Task<IEnumerable<Account>> GetAllStaffAsync();
    Task<Account?> GetByUsernameAsync(string username);

    /// <summary>Trả về null nếu thành công, trả về chuỗi lỗi nếu thất bại.</summary>
    Task<string?> CreateAccountAsync(Account account, string rawPassword);
    Task<string?> UpdateAccountAsync(Account account, string? newRawPassword);
    Task<string?> DeleteAccountAsync(string username);

    // ---------- Helper ----------
    Task<bool> UsernameExistsAsync(string username);
}


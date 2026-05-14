using Core.Shared.Entities;
using Core.Shared.Repositories;
using Core.Shared.ViewModels;

namespace Core.Shared.Interfaces;

/// <summary>
/// Interface cho service xác thực chung (Client + Admin)
/// </summary>
public interface IUnifiedAuthService
{
    /// <summary>
    /// Đăng nhập bằng email và password
    /// Trả về thông tin loại tài khoản (Admin/Reader)
    /// </summary>
    Task<LoginResultViewModel> LoginAsync(string email, string password);

    /// <summary>
    /// Kiểm tra email có tồn tại không
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Lấy loại tài khoản từ email
    /// </summary>
    Task<string> GetAccountTypeByEmailAsync(string email);
}

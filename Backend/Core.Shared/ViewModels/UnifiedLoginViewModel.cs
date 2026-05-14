using System.ComponentModel.DataAnnotations;

namespace Core.Shared.ViewModels;

/// <summary>
/// ViewModel cho form đăng nhập chung (Client + Admin)
/// </summary>
public class UnifiedLoginViewModel
{
    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Mật Khẩu")]
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ tôi")]
    public bool RememberMe { get; set; }
}

/// <summary>
/// ViewModel cho kết quả đăng nhập
/// </summary>
public class LoginResultViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty; // "Admin" hoặc "Reader"
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

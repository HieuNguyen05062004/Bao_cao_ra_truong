using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels;

/// <summary>
/// ViewModel cho form đăng nhập bạn đọc
/// </summary>
public class LoginViewModel
{
    [Display(Name = "Mã Bạn Đọc")]
    [Required(ErrorMessage = "Mã bạn đọc không được để trống")]
    [StringLength(20)]
    public string ReaderId { get; set; } = string.Empty;

    [Display(Name = "Mật Khẩu")]
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ tôi")]
    public bool RememberMe { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels;

public class RegisterViewModel
{
    [Display(Name = "Họ và Tên")]
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ và tên không được quá 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    // Không [Required] — form Register không có trường Phone
    [Display(Name = "Số Điện Thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu với 0 và có 10 chữ số")]
    [StringLength(15)]
    public string? Phone { get; set; }

    [Display(Name = "Mật Khẩu")]
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ngày Sinh")]
    [DataType(DataType.Date)]
    public DateTime? DoB { get; set; }

    [Display(Name = "Giới Tính")]
    public string? Gender { get; set; } = "Nam";

    [Display(Name = "Địa Chỉ")]
    [StringLength(255)]
    public string? Address { get; set; }

    [Display(Name = "Ảnh Đại Diện")]
    [DataType(DataType.Upload)]
    public IFormFile? AvatarFile { get; set; }
}

public class RegisterResultViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ReaderId { get; set; }
    public string? RedirectUrl { get; set; }
}

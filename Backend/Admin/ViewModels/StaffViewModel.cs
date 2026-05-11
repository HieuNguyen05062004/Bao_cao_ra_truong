using System.ComponentModel.DataAnnotations;
using Core.Shared.Constants;

namespace Admin.ViewModels;

public class StaffViewModel
{
    // ---------- Dùng cho Create ----------
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(50, ErrorMessage = "Tối đa 50 ký tự.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    // Bắt buộc khi Create, tuỳ chọn khi Edit
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string? Password { get; set; }

    // ---------- Dùng chung Create / Edit ----------
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, ErrorMessage = "Tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Tối đa 100 ký tự.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn quyền hạn.")]
    [Display(Name = "Quyền hạn")]
    public string Role { get; set; } = RoleConstants.Staff;

    [Display(Name = "Ảnh đại diện")]
    public IFormFile? AvatarFile { get; set; }

    // Dùng để hiển thị ảnh cũ khi Edit
    public string? AvatarUrl { get; set; }

    // ---------- Dùng cho Index (hiển thị) ----------
    public DateTime? CreatedAt { get; set; }
}

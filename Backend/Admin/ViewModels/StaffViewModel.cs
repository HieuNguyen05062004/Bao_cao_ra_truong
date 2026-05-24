using System.ComponentModel.DataAnnotations;
using Core.Shared.Constants;

namespace Admin.ViewModels;

public class StaffViewModel
{
    // Username sẽ được tự động tạo từ Email, không cần nhập
    [Display(Name = "Tên đăng nhập")]
    public string? Username { get; set; }

    // Bắt buộc khi Create, tuỳ chọn khi Edit
    [Required(ErrorMessage = "Mật khẩu phải dài hơn 8 ký tự, bao gồm chữ hoa, số và ký tự đặc biệt.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{9,}$",
    ErrorMessage = "Mật khẩu phải dài hơn 8 ký tự, bao gồm chữ hoa, số và ký tự đặc biệt.")]
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

    [Required(ErrorMessage = "Vui lòng chọn quyền hạn cho tài khoản.")]
    [Display(Name = "Quyền hạn")]
    public string Role { get; set; } = RoleConstants.Staff;

    [Display(Name = "Ảnh đại diện")]
    public IFormFile? AvatarFile { get; set; }

    // Dùng để hiển thị ảnh cũ khi Edit
    public string? AvatarUrl { get; set; }


    // ---------- Dùng cho Index (hiển thị) ----------
    public DateTime? CreatedAt { get; set; }
}

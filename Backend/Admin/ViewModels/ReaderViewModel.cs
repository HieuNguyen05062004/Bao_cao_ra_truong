using System.ComponentModel.DataAnnotations;
using Core.Shared.Entities;

namespace Admin.ViewModels;

// Dùng cho Index / Create / Edit
public class ReaderViewModel
{
    // Chỉ dùng để hiển thị, không nhập tay — hệ thống tự sinh
    public string? ReaderId { get; set; }

    [Required(ErrorMessage = "Họ và tên không được để trống và phải từ 5 - 20 ký tự.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Họ và tên không được để trống và phải từ 5 - 20 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
    [Display(Name = "Ngày sinh")]
    public DateOnly? DoB { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }
    [Required(ErrorMessage = "Địa chỉ không được để trống và phải từ 5 - 100 ký tự.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Địa chỉ không được để trống và phải từ 5 - 100 ký tự.")]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại phải nhập đúng đủ 10 chữ số.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải nhập đúng đủ 10 chữ số.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Email không hợp lệ (Phải đúng định dạng @gmail.com).")]
    [RegularExpression(@"^[a-zA-Z0-9._%+\-]+@gmail\.com$",
    ErrorMessage = "Email không hợp lệ (Phải đúng định dạng @gmail.com).")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string? Password { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public IFormFile? AvatarFile { get; set; }

    public string? AvatarUrl { get; set; }

    // Dùng cho Index (hiển thị thống kê nhanh)
    public int BorrowingCount { get; set; }
    public int OverdueCount { get; set; }
}

// Dùng riêng cho trang Details (kèm danh sách phiếu mượn)
public class ReaderDetailViewModel
{
    public string ReaderId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DoB { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }

    // Thống kê
    public int TotalBorrow { get; set; }
    public int PendingCount { get; set; }      // Chờ duyệt
    public int ApprovedCount { get; set; }     // Đã duyệt
    public int BorrowingCount { get; set; }    // Đang mượn
    public int RejectedCount { get; set; }     // Bị từ chối
    public int OverdueCount { get; set; }      // Quá hạn
    public int ReturnedCount { get; set; }     // Đã trả

    // Danh sách phiếu mượn (sau khi lọc)
    public IEnumerable<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();

    // Bộ lọc hiện tại: "all" | "borrowing" | "overdue" | "returned"
    public string Filter { get; set; } = "all";
}

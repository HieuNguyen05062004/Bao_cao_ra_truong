using System.ComponentModel.DataAnnotations;
using Core.Shared.Entities;

namespace Admin.ViewModels;

// Dùng cho Index / Create / Edit
public class ReaderViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mã bạn đọc.")]
    [StringLength(20, ErrorMessage = "Tối đa 20 ký tự.")]
    [Display(Name = "Mã bạn đọc")]
    public string ReaderId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, ErrorMessage = "Tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Ngày sinh")]
    public DateOnly? DoB { get; set; }

    [Display(Name = "Giới tính")]
    public string? Gender { get; set; }

    [StringLength(255, ErrorMessage = "Tối đa 255 ký tự.")]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [StringLength(15, ErrorMessage = "Tối đa 15 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Tối đa 100 ký tự.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

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
    public int BorrowingCount { get; set; }
    public int OverdueCount { get; set; }
    public int ReturnedCount { get; set; }

    // Danh sách phiếu mượn (sau khi lọc)
    public IEnumerable<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();

    // Bộ lọc hiện tại: "all" | "borrowing" | "overdue" | "returned"
    public string Filter { get; set; } = "all";
}

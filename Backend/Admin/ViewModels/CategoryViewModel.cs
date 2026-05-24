using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels;

/// <summary>
/// ViewModel (mô hình dữ liệu cho View) của danh mục sách.
/// Tách biệt với Entity để tránh expose trực tiếp model database ra ngoài View.
/// Data Annotations ở đây điều khiển validation phía client và server.
/// </summary>
public class CategoryViewModel
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống và phải từ 5 - 20 ký tự.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Tên danh mục không được để trống và phải từ 5 - 20 ký tự.")]
    [Display(Name = "Tên danh mục")]
    public string CategoryName { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel dùng cho trang Index: danh sách + ô tìm kiếm.
/// </summary>
public class CategoryIndexViewModel
{
    public List<CategoryViewModel> Categories { get; set; } = new();
    public string? Keyword { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// ViewModel dùng cho trang Details:
/// thông tin danh mục + danh sách sách thuộc danh mục đó.
/// </summary>
public class CategoryDetailsViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Danh sách sách thuộc danh mục này (có thể rỗng).</summary>
    public List<BookSummaryViewModel> Books { get; set; } = new();
}

/// <summary>
/// ViewModel dùng cho trang Delete: hiển thị thông tin trước khi xác nhận xóa.
/// Tách riêng với CategoryDetailsViewModel vì trang Delete cần thêm cảnh báo
/// khi danh mục còn sách (không cho phép xóa).
/// </summary>
public class CategoryDeleteViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Số lượng sách còn trong danh mục. > 0 thì không cho xóa.</summary>
    public int BookCount { get; set; }

    public bool HasBooks => BookCount > 0;
}

/// <summary>
/// Thông tin tóm tắt của một cuốn sách, dùng trong trang Details của danh mục.
/// Không cần toàn bộ field của Book entity — chỉ lấy những gì cần hiển thị.
/// </summary>
public class BookSummaryViewModel
{
    public string BookId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Status { get; set; }
}

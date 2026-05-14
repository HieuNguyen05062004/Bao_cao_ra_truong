using System.ComponentModel.DataAnnotations;
using Core.Shared.Entities;
using Microsoft.AspNetCore.Http;

namespace Admin.ViewModels;

public class BookViewModel
{
    // Bỏ [Required] — ID do hệ thống tự sinh, không cần nhập tay
    // Vẫn giữ để dùng khi Edit (truyền ID hiện tại)
    public string? BookId { get; set; }

    [Display(Name = "Tên sách")]
    [Required(ErrorMessage = "Tên sách không được để trống")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Display(Name = "Tác giả")]
    public string? Author { get; set; }

    [Display(Name = "Nhà xuất bản")]
    public string? Publisher { get; set; }

    [Display(Name = "Năm xuất bản")]
    [Range(1000, 9999, ErrorMessage = "Năm xuất bản không hợp lệ")]
    public int? PublishYear { get; set; }

    [Display(Name = "Số lượng")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
    public int? Quantity { get; set; }

    [Display(Name = "Tình trạng")]
    public string? Status { get; set; }

    public string? ImageUrl { get; set; }

    [Display(Name = "Hình ảnh")]
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// Danh sách CategoryId đã chọn (many-to-many).
    /// </summary>
    public List<int> CategoryIds { get; set; } = new();

    /// <summary>Danh sách tất cả danh mục để render chips.</summary>
    public List<Category> Categories { get; set; } = new();
}
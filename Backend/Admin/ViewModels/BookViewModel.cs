using System.ComponentModel.DataAnnotations;
using Core.Shared.Entities;
using Microsoft.AspNetCore.Http;

namespace Admin.ViewModels;

public class BookViewModel
{
    [Display(Name = "Mã sách")]
    [Required(ErrorMessage = "Mã sách không được để trống")]
    [StringLength(20)]
    public string BookId { get; set; } = null!;

    [Display(Name = "Tên sách")]
    [Required(ErrorMessage = "Tên sách không được để trống")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Display(Name = "Tác giả")]
    public string? Author { get; set; }

    [Display(Name = "Nhà xuất bản")]
    public string? Publisher { get; set; }

    [Display(Name = "Năm xuất bản")]
    public int? PublishYear { get; set; }

    [Display(Name = "Số lượng")]
    public int? Quantity { get; set; }

    [Display(Name = "Tình trạng")]
    public string? Status { get; set; }

    public string? ImageUrl { get; set; }

    [Display(Name = "Hình ảnh")]
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// Danh sách ID danh mục đã chọn (many-to-many).
    /// Model binding tự điền khi form POST nhiều hidden input cùng tên "CategoryIds".
    /// </summary>
    public List<int> CategoryIds { get; set; } = new();

    /// <summary>Danh sách tất cả danh mục để render chips.</summary>
    public List<Category> Categories { get; set; } = new();
}

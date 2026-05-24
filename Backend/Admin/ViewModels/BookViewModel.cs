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
    [Required(ErrorMessage = "Tên sách không được để trống và phải từ 5 - 20 ký tự.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Tên sách không được để trống và phải từ 5 - 20 ký tự.")]
    public string Title { get; set; } = null!;



    [Display(Name = "Tác giả")]
    [Required(ErrorMessage = "Tên tác giả không được để trống và phải từ 5 - 20 ký tự.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Tên tác giả không được để trống và phải từ 5 - 20 ký tự.")]
    public string Author { get; set; } = null!;

    [Display(Name = "Nhà xuất bản")]
    [Required(ErrorMessage = "Nhà xuất bản không được để trống và phải từ 5 - 20 ký tự.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Nhà xuất bản không được để trống và phải từ 5 - 20 ký tự.")]
    public string Publisher { get; set; } = null!;

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

    [Display(Name = "Mô tả sách")]
    [Required(ErrorMessage = "Mô tả không được để trống và phải từ 10 - 900 ký tự.")]
    [StringLength(900, MinimumLength = 10, ErrorMessage = "Mô tả không được để trống và phải từ 10 - 900 ký tự.")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Danh sách CategoryId đã chọn (many-to-many).
    /// </summary>
    public List<int> CategoryIds { get; set; } = new();

    /// <summary>Danh sách tất cả danh mục để render chips.</summary>
    public List<Category> Categories { get; set; } = new();
}
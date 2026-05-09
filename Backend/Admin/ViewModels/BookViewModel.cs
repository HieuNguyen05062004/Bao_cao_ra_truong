using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Admin.ViewModels
{
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

        [Display(Name = "Thể loại")]
        public int? CategoryId { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Tình trạng")]
        public string? Status { get; set; }

        // Đường dẫn ảnh lưu DB
        public string? ImageUrl { get; set; }

        // File upload
        [Display(Name = "Hình ảnh")]
        public IFormFile? ImageFile { get; set; }

        public string? CategoryName { get; set; }
    }
}
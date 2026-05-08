using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels
{
    /// <summary>
    /// ViewModel cho trang quản lý sách
    /// </summary>
    public class BookViewModel
    {
        [Display(Name = "Mã sách")]
        [Required(ErrorMessage = "Mã sách không được để trống")]
        [StringLength(20, ErrorMessage = "Mã sách không được quá 20 ký tự")]
        public string BookId { get; set; } = null!;

        [Display(Name = "Tên sách")]
        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(255, ErrorMessage = "Tên sách không được quá 255 ký tự")]
        public string Title { get; set; } = null!;

        [Display(Name = "Tác giả")]
        [StringLength(100, ErrorMessage = "Tên tác giả không được quá 100 ký tự")]
        public string? Author { get; set; }

        [Display(Name = "Nhà xuất bản")]
        [StringLength(100, ErrorMessage = "Tên nhà xuất bản không được quá 100 ký tự")]
        public string? Publisher { get; set; }

        [Display(Name = "Năm xuất bản")]
        [Range(1000, 2100, ErrorMessage = "Năm xuất bản không hợp lệ")]
        public int? PublishYear { get; set; }

        [Display(Name = "Thể loại")]
        public int? CategoryId { get; set; }

        [Display(Name = "Số lượng")]
        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int? Quantity { get; set; }

        [Display(Name = "Tình trạng")]
        [StringLength(50, ErrorMessage = "Tình trạng không được quá 50 ký tự")]
        public string? Status { get; set; }

        [Display(Name = "URL hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Tên thể loại")]
        public string? CategoryName { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sách không được vượt quá 200 ký tự")]
        [Display(Name = "Tên sách")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhà xuất bản không được để trống")]
        [StringLength(200)]
        [Display(Name = "Nhà xuất bản")]
        public string Publisher { get; set; } = string.Empty;

        [Display(Name = "Năm xuất bản")]
        public int? PublishedYear { get; set; }

        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Số lượng tổng")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ")]
        public int TotalQuantity { get; set; }

        [Display(Name = "Số lượng hiện có")]
        public int AvailableQuantity { get; set; }

        [Display(Name = "Ảnh bìa")]
        public string? CoverImage { get; set; }

        [Required]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}

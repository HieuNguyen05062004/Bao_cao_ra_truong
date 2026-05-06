using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    public enum BorrowStatus
    {
        Borrowing,      // Đang mượn
        Returned,       // Đã trả
        Overdue,        // Trả quá hạn
        LostOrDamaged   // Mất/hư hỏng
    }

    public class BorrowRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Bạn đọc")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Sách")]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [Required]
        [Display(Name = "Ngày mượn")]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; }

        [Required]
        [Display(Name = "Hạn trả")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Ngày trả thực tế")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Trạng thái")]
        public BorrowStatus Status { get; set; } = BorrowStatus.Borrowing;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Nhân viên xử lý")]
        public string? ProcessedByUserId { get; set; }

        [ForeignKey("ProcessedByUserId")]
        public ApplicationUser? ProcessedBy { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

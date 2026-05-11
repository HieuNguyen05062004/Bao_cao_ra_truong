using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Entities;

public partial class Book
{
    [Key]
    [Column("BookID")]
    [StringLength(20)]
    [Unicode(false)]
    public string BookId { get; set; } = null!;

    [StringLength(255)]
    public string Title { get; set; } = null!;

    [StringLength(100)]
    public string? Author { get; set; }

    [StringLength(100)]
    public string? Publisher { get; set; }

    public int? PublishYear { get; set; }

    public int? Quantity { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column("ImageURL")]
    public string? ImageUrl { get; set; }

    // ── CategoryId cũ đã bị xóa ──────────────────────────────────────────────
    // Database mới không có cột CategoryID trên bảng Books.
    // Quan hệ sách ↔ danh mục được quản lý hoàn toàn qua bảng trung gian BookCategories.

    /// <summary>
    /// Danh mục của sách này (many-to-many qua BookCategories).
    /// </summary>
    public virtual ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();

    /// <summary>
    /// Phiếu mượn liên quan đến sách này.
    /// </summary>
    [ForeignKey("BookId")]
    [InverseProperty("Books")]
    public virtual ICollection<BorrowTicket> Tickets { get; set; } = new List<BorrowTicket>();
}

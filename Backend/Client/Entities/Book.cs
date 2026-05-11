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

    /// <summary>Thời điểm sách được thêm vào hệ thống — dùng để lấy sách mới nhất.</summary>
    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();

    [ForeignKey("BookId")]
    [InverseProperty("Books")]
    public virtual ICollection<BorrowTicket> Tickets { get; set; } = new List<BorrowTicket>();
}

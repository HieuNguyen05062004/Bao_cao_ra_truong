using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Shared.Entities;

public partial class Category
{
    [Key]
    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    // ── Navigation Books cũ đã bị xóa ────────────────────────────────────────
    // Không còn FK trực tiếp Books.CategoryID trong database mới.
    // Truy cập sách của danh mục qua: BookCategories.Select(bc => bc.Book)

    /// <summary>
    /// Danh sách liên kết many-to-many với Books qua bảng BookCategories.
    /// </summary>
    public virtual ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}

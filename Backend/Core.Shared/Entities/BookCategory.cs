using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Shared.Entities;

/// <summary>
/// Entity đại diện cho bảng trung gian BookCategories.
/// Thể hiện quan hệ nhiều-nhiều (many-to-many) giữa Book và Category.
/// </summary>
public class BookCategory
{
    [Column("BookID")]
    [StringLength(20)]
    public string BookId { get; set; } = null!;

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    // Navigation properties (thuộc tính điều hướng) để EF Core load dữ liệu liên quan
    [ForeignKey("BookId")]
    public virtual Book Book { get; set; } = null!;

    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; } = null!;
}


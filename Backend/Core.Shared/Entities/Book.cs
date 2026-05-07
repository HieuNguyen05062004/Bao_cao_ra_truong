using System;
using System.Collections.Generic;
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

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    public int? Quantity { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column("ImageURL")]
    public string? ImageUrl { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Books")]
    public virtual Category? Category { get; set; }

    [ForeignKey("BookId")]
    [InverseProperty("Books")]
    public virtual ICollection<BorrowTicket> Tickets { get; set; } = new List<BorrowTicket>();
}

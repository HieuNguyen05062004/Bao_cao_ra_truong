using System;
using System.Collections.Generic;

namespace Client.Areas.Admin.Models;

public partial class Book
{
    public string BookId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Author { get; set; }

    public string? Publisher { get; set; }

    public int? PublishYear { get; set; }

    public int? CategoryId { get; set; }

    public int? Quantity { get; set; }

    public string? Status { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<BorrowTicket> Tickets { get; set; } = new List<BorrowTicket>();
}

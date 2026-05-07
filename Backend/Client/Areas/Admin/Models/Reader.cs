using System;
using System.Collections.Generic;

namespace Client.Areas.Admin.Models;

public partial class Reader
{
    public string ReaderId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? DoB { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual ICollection<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();
}

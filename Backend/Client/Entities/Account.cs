using System;
using System.Collections.Generic;

namespace Client.Entities;

public partial class Account
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Role { get; set; }

    public string? Email { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();
}

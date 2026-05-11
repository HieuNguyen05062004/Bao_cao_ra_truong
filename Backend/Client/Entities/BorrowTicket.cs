using System;
using System.Collections.Generic;

namespace Client.Entities;

public partial class BorrowTicket
{
    public int TicketId { get; set; }

    public string? ReaderId { get; set; }

    public string? StaffUsername { get; set; }

    public DateTime? BorrowDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string? Status { get; set; }

    public virtual Reader? Reader { get; set; }

    public virtual Account? StaffUsernameNavigation { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}

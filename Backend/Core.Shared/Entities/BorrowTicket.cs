using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Entities;

public partial class BorrowTicket
{
    [Key]
    [Column("TicketID")]
    public int TicketId { get; set; }

    [Column("ReaderID")]
    [StringLength(20)]
    [Unicode(false)]
    public string? ReaderId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? StaffUsername { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BorrowDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DueDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReturnDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("ReaderId")]
    [InverseProperty("BorrowTickets")]
    public virtual Reader? Reader { get; set; }

    [ForeignKey("StaffUsername")]
    [InverseProperty("BorrowTickets")]
    public virtual Account? StaffUsernameNavigation { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("Tickets")]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Entities;

public partial class Reader
{
    [Key]
    [Column("ReaderID")]
    [StringLength(20)]
    [Unicode(false)]
    public string ReaderId { get; set; } = null!;

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    public DateOnly? DoB { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [Column("AvatarURL")]
    public string? AvatarUrl { get; set; }

    [InverseProperty("Reader")]
    public virtual ICollection<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();
}

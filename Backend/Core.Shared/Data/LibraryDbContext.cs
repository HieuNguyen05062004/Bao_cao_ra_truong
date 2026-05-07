using System;
using System.Collections.Generic;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Data;

public partial class LibraryDbContext : DbContext
{
    public LibraryDbContext()
    {
    }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BorrowTicket> BorrowTickets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Reader> Readers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LOCALHOST\\SQLEXPRESS;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__Accounts__536C85E5CD83A2C9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C227EC6A4A96");

            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue("Có thể mượn");

            entity.HasOne(d => d.Category).WithMany(p => p.Books).HasConstraintName("FK__Books__CategoryI__2180FB33");
        });

        modelBuilder.Entity<BorrowTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__BorrowTi__712CC62799789BCC");

            entity.Property(e => e.BorrowDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Reader).WithMany(p => p.BorrowTickets).HasConstraintName("FK__BorrowTic__Reade__2A164134");

            entity.HasOne(d => d.StaffUsernameNavigation).WithMany(p => p.BorrowTickets).HasConstraintName("FK__BorrowTic__Staff__2B0A656D");

            entity.HasMany(d => d.Books).WithMany(p => p.Tickets)
                .UsingEntity<Dictionary<string, object>>(
                    "BorrowDetail",
                    r => r.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BorrowDet__BookI__2EDAF651"),
                    l => l.HasOne<BorrowTicket>().WithMany()
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BorrowDet__Ticke__2DE6D218"),
                    j =>
                    {
                        j.HasKey("TicketId", "BookId").HasName("PK__BorrowDe__12F2CA05C68F0A44");
                        j.ToTable("BorrowDetails");
                        j.IndexerProperty<int>("TicketId").HasColumnName("TicketID");
                        j.IndexerProperty<string>("BookId")
                            .HasMaxLength(20)
                            .IsUnicode(false)
                            .HasColumnName("BookID");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2BF324084E");
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(e => e.ReaderId).HasName("PK__Readers__8E67A581B0EBE500");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

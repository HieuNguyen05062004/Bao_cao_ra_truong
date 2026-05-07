using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Client.Areas.Admin.Models;

public partial class ThuVienContext : DbContext
{
    public ThuVienContext()
    {
    }

    public ThuVienContext(DbContextOptions<ThuVienContext> options)
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
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__Accounts__536C85E5CD83A2C9");

            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C227EC6A4A96");

            entity.Property(e => e.BookId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BookID");
            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
            entity.Property(e => e.Publisher).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Có thể mượn");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Books__CategoryI__2180FB33");
        });

        modelBuilder.Entity<BorrowTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__BorrowTi__712CC62799789BCC");

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.BorrowDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.ReaderId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ReaderID");
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");
            entity.Property(e => e.StaffUsername)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Reader).WithMany(p => p.BorrowTickets)
                .HasForeignKey(d => d.ReaderId)
                .HasConstraintName("FK__BorrowTic__Reade__2A164134");

            entity.HasOne(d => d.StaffUsernameNavigation).WithMany(p => p.BorrowTickets)
                .HasForeignKey(d => d.StaffUsername)
                .HasConstraintName("FK__BorrowTic__Staff__2B0A656D");

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

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(e => e.ReaderId).HasName("PK__Readers__8E67A581B0EBE500");

            entity.Property(e => e.ReaderId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ReaderID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Data;

public partial class LibraryDbContext : DbContext
{
    public LibraryDbContext() { }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options) { }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Book> Books { get; set; }
    public virtual DbSet<BorrowTicket> BorrowTickets { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Reader> Readers { get; set; }
    public virtual DbSet<BookCategory> BookCategories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=LOCALHOST\\SQLEXPRESS;Database=ThuVien;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Account ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__Accounts__536C85E5CD83A2C9");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        // ── Book ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C227EC6A4A96");

            entity.Property(e => e.BookId)
                  .HasColumnName("BookID")
                  .HasMaxLength(20)
                  .IsUnicode(false);

            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue("Có thể mượn");
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");

            // Không còn cột CategoryID trên bảng Books — bỏ hoàn toàn cấu hình FK cũ
        });

        // ── BookCategory (many-to-many Books ↔ Categories) ───────────────────
        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(bc => new { bc.BookId, bc.CategoryId });
            entity.ToTable("BookCategories");

            entity.Property(bc => bc.BookId)
                  .HasColumnName("BookID")
                  .HasMaxLength(20)
                  .IsUnicode(false);

            entity.Property(bc => bc.CategoryId)
                  .HasColumnName("CategoryID");

            entity.HasOne(bc => bc.Book)
                  .WithMany(b => b.BookCategories)
                  .HasForeignKey(bc => bc.BookId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bc => bc.Category)
                  .WithMany(c => c.BookCategories)
                  .HasForeignKey(bc => bc.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Category ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2BF324084E");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        // ── BorrowTicket ──────────────────────────────────────────────────────
        modelBuilder.Entity<BorrowTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__BorrowTi__712CC62799789BCC");
            entity.Property(e => e.BorrowDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Reader)
                  .WithMany(p => p.BorrowTickets)
                  .HasForeignKey(d => d.ReaderId)
                  .HasConstraintName("FK__BorrowTic__Reade__2A164134");

            entity.HasOne(d => d.StaffUsernameNavigation)
                  .WithMany(p => p.BorrowTickets)
                  .HasForeignKey(d => d.StaffUsername)
                  .HasConstraintName("FK__BorrowTic__Staff__2B0A656D");

            entity.HasMany(d => d.Books)
                  .WithMany(p => p.Tickets)
                  .UsingEntity<Dictionary<string, object>>(
                      "BorrowDetail",
                      r => r.HasOne<Book>().WithMany()
                            .HasForeignKey("BookId")
                            .OnDelete(DeleteBehavior.ClientSetNull),
                      l => l.HasOne<BorrowTicket>().WithMany()
                            .HasForeignKey("TicketId")
                            .OnDelete(DeleteBehavior.ClientSetNull),
                      j =>
                      {
                          j.HasKey("TicketId", "BookId")
                           .HasName("PK__BorrowDe__12F2CA05C68F0A44");
                          j.ToTable("BorrowDetails");
                          j.IndexerProperty<int>("TicketId").HasColumnName("TicketID");
                          j.IndexerProperty<string>("BookId")
                           .HasMaxLength(20).IsUnicode(false).HasColumnName("BookID");
                      });
        });

        // ── Reader ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(e => e.ReaderId).HasName("PK__Readers__8E67A581B0EBE500");
            entity.Property(e => e.ReaderId).HasColumnName("ReaderID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BorrowRecord>()
                .HasOne(br => br.User)
                .WithMany(u => u.BorrowRecords)
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BorrowRecord>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BorrowRecord>()
                .HasOne(br => br.ProcessedBy)
                .WithMany()
                .HasForeignKey(br => br.ProcessedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Công nghệ thông tin", Description = "Sách về lập trình, mạng máy tính, CNTT" },
                new Category { Id = 2, Name = "Văn học", Description = "Tiểu thuyết, truyện ngắn, thơ ca" },
                new Category { Id = 3, Name = "Khoa học tự nhiên", Description = "Toán, Lý, Hóa, Sinh" },
                new Category { Id = 4, Name = "Kinh tế", Description = "Kinh doanh, quản trị, tài chính" },
                new Category { Id = 5, Name = "Lịch sử - Địa lý", Description = "Lịch sử Việt Nam và thế giới, địa lý" }
            );

            // Seed Books
            builder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Lập trình Java cơ bản",
                    Author = "Nguyễn Văn A",
                    Publisher = "NXB Giáo dục",
                    PublishedYear = 2022,
                    ISBN = "978-604-0001",
                    Description = "Giáo trình lập trình Java cho người mới bắt đầu",
                    TotalQuantity = 10,
                    AvailableQuantity = 10,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Book
                {
                    Id = 2,
                    Title = "C# và .NET Framework",
                    Author = "Trần Thị B",
                    Publisher = "NXB Đại học Quốc gia",
                    PublishedYear = 2023,
                    ISBN = "978-604-0002",
                    Description = "Lập trình C# chuyên sâu với .NET",
                    TotalQuantity = 8,
                    AvailableQuantity = 8,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Book
                {
                    Id = 3,
                    Title = "Python cho người mới học",
                    Author = "Lê Văn C",
                    Publisher = "NXB Trẻ",
                    PublishedYear = 2023,
                    ISBN = "978-604-0003",
                    Description = "Học lập trình Python từ cơ bản đến nâng cao",
                    TotalQuantity = 12,
                    AvailableQuantity = 12,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Book
                {
                    Id = 4,
                    Title = "Truyện Kiều",
                    Author = "Nguyễn Du",
                    Publisher = "NXB Văn học",
                    PublishedYear = 2020,
                    ISBN = "978-604-0004",
                    Description = "Tác phẩm văn học kinh điển của Việt Nam",
                    TotalQuantity = 15,
                    AvailableQuantity = 15,
                    CategoryId = 2,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Book
                {
                    Id = 5,
                    Title = "Quản trị kinh doanh hiện đại",
                    Author = "Phạm Văn D",
                    Publisher = "NXB Kinh tế",
                    PublishedYear = 2022,
                    ISBN = "978-604-0005",
                    Description = "Kiến thức quản trị kinh doanh trong thời đại số",
                    TotalQuantity = 6,
                    AvailableQuantity = 6,
                    CategoryId = 4,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}

using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services
{
    public interface IBookService
    {
        Task<List<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task<List<Book>> SearchAsync(string? title, string? author, string? category);
        Task<Book> CreateAsync(Book book);
        Task<Book> UpdateAsync(Book book);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteMultipleAsync(IEnumerable<int> ids);
        Task UpdateQuantityAsync(int bookId, int delta);
        Task<bool> IsAvailableAsync(int bookId);
    }

    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Book>> SearchAsync(string? title, string? author, string? category)
        {
            var query = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(b => b.Title.ToLower().Contains(title.ToLower()));

            if (!string.IsNullOrWhiteSpace(author))
                query = query.Where(b => b.Author.ToLower().Contains(author.ToLower()));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(b => b.Category != null && b.Category.Name.ToLower().Contains(category.ToLower()));

            return await query.OrderBy(b => b.Title).ToListAsync();
        }

        public async Task<Book> CreateAsync(Book book)
        {
            book.AvailableQuantity = book.TotalQuantity;
            book.CreatedAt = DateTime.Now;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<Book> UpdateAsync(Book book)
        {
            var existing = await _context.Books.FindAsync(book.Id);
            if (existing == null) throw new InvalidOperationException("Không tìm thấy sách");

            int delta = book.TotalQuantity - existing.TotalQuantity;
            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.Publisher = book.Publisher;
            existing.PublishedYear = book.PublishedYear;
            existing.ISBN = book.ISBN;
            existing.Description = book.Description;
            existing.TotalQuantity = book.TotalQuantity;
            existing.AvailableQuantity = Math.Max(0, existing.AvailableQuantity + delta);
            existing.CategoryId = book.CategoryId;
            if (book.CoverImage != null)
                existing.CoverImage = book.CoverImage;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return false;

            if (book.BorrowRecords.Any(br => br.Status == BorrowStatus.Borrowing))
                throw new InvalidOperationException("Không thể xóa sách đang được mượn");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMultipleAsync(IEnumerable<int> ids)
        {
            var books = await _context.Books
                .Include(b => b.BorrowRecords)
                .Where(b => ids.Contains(b.Id))
                .ToListAsync();

            foreach (var book in books)
            {
                if (book.BorrowRecords.Any(br => br.Status == BorrowStatus.Borrowing))
                    throw new InvalidOperationException($"Không thể xóa sách '{book.Title}' đang được mượn");
            }

            _context.Books.RemoveRange(books);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateQuantityAsync(int bookId, int delta)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) throw new InvalidOperationException("Không tìm thấy sách");
            book.AvailableQuantity = Math.Max(0, book.AvailableQuantity + delta);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsAvailableAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            return book != null && book.AvailableQuantity > 0;
        }
    }
}

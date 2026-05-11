using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class BookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    // ─── READ ────────────────────────────────────────────────────────────────

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(string bookId)
    {
        return await _context.Books
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.BookId == bookId);
    }

    public async Task<List<Book>> SearchAsync(string searchTerm)
    {
        return await _context.Books
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .Where(b => b.Title.Contains(searchTerm)
                     || (b.Author != null && b.Author.Contains(searchTerm))
                     || b.BookId.Contains(searchTerm))
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<List<Book>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Books
            .Include(b => b.BookCategories)
            .Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId))
            .OrderBy(b => b.Title)
            .ToListAsync();
    }

    public async Task<List<Book>> GetAvailableAsync()
    {
        return await _context.Books
            .Where(b => b.Status == "Có thể mượn" && b.Quantity > 0)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string bookId)
    {
        return await _context.Books.AnyAsync(b => b.BookId == bookId);
    }

    public async Task<bool> IsBookBorrowedAsync(string bookId)
    {
        return await _context.BorrowTickets
            .AnyAsync(t => t.Books.Any(b => b.BookId == bookId));
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    // ─── WRITE ───────────────────────────────────────────────────────────────

    public async Task<bool> AddAsync(Book book, List<int> categoryIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            foreach (var catId in categoryIds.Distinct())
            {
                _context.BookCategories.Add(new BookCategory
                {
                    BookId = book.BookId,
                    CategoryId = catId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Book book, List<int> categoryIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.Books
                .Include(b => b.BookCategories)
                .FirstOrDefaultAsync(b => b.BookId == book.BookId);

            if (existing == null) return false;

            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.Publisher = book.Publisher;
            existing.PublishYear = book.PublishYear;
            existing.Quantity = book.Quantity;
            existing.Status = book.Status;
            existing.ImageUrl = book.ImageUrl;

            // Xóa liên kết danh mục cũ → thêm lại mới
            _context.BookCategories.RemoveRange(existing.BookCategories);

            foreach (var catId in categoryIds.Distinct())
            {
                _context.BookCategories.Add(new BookCategory
                {
                    BookId = book.BookId,
                    CategoryId = catId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string bookId)
    {
        try
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateQuantityAsync(string bookId, int quantityChange)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
        if (book == null) return false;

        book.Quantity = (book.Quantity ?? 0) + quantityChange;
        await _context.SaveChangesAsync();
        return true;
    }
}

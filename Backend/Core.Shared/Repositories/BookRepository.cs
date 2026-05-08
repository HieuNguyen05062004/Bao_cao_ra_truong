using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class BookRepository
{
    private readonly LibraryDbContext _dbContext;

    public BookRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _dbContext.Books
            .Include(x => x.Category)
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(string bookId)
    {
        return await _dbContext.Books
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.BookId == bookId);
    }

    public async Task<bool> ExistsAsync(string bookId)
    {
        return await _dbContext.Books.AnyAsync(x => x.BookId == bookId);
    }

    public async Task<bool> IsUsedInBorrowAsync(string bookId)
    {
        return await _dbContext.BorrowTickets.AnyAsync(x => x.Books.Any(b => b.BookId == bookId));
    }

    public async Task AddAsync(Book book)
    {
        await _dbContext.Books.AddAsync(book);
    }

    public Task UpdateAsync(Book book)
    {
        _dbContext.Books.Update(book);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Book book)
    {
        _dbContext.Books.Remove(book);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

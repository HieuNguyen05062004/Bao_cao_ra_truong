using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class ReaderRepository
{
    private readonly LibraryDbContext _dbContext;

    public ReaderRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Reader>> GetAllAsync()
    {
        return await _dbContext.Readers.OrderBy(x => x.FullName).ToListAsync();
    }

    public async Task<Reader?> GetByIdAsync(string readerId)
    {
        return await _dbContext.Readers.FirstOrDefaultAsync(x => x.ReaderId == readerId);
    }

    public async Task<bool> ExistsAsync(string readerId)
    {
        return await _dbContext.Readers.AnyAsync(x => x.ReaderId == readerId);
    }

    public async Task<bool> HasActiveBorrowAsync(string readerId)
    {
        return await _dbContext.BorrowTickets.AnyAsync(x => x.ReaderId == readerId && x.ReturnDate == null);
    }

    public async Task AddAsync(Reader reader)
    {
        await _dbContext.Readers.AddAsync(reader);
    }

    public Task UpdateAsync(Reader reader)
    {
        _dbContext.Readers.Update(reader);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Reader reader)
    {
        _dbContext.Readers.Remove(reader);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

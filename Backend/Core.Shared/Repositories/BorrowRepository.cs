using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class BorrowRepository
{
    private readonly LibraryDbContext _dbContext;

    public BorrowRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BorrowTicket>> GetAllAsync()
    {
        return await _dbContext.BorrowTickets
            .Include(x => x.Reader)
            .Include(x => x.StaffUsernameNavigation)
            .Include(x => x.Books)
            .OrderByDescending(x => x.BorrowDate)
            .ToListAsync();
    }

    public async Task<BorrowTicket?> GetByIdAsync(int ticketId)
    {
        return await _dbContext.BorrowTickets
            .Include(x => x.Reader)
            .Include(x => x.StaffUsernameNavigation)
            .Include(x => x.Books)
            .FirstOrDefaultAsync(x => x.TicketId == ticketId);
    }

    public async Task AddAsync(BorrowTicket ticket)
    {
        await _dbContext.BorrowTickets.AddAsync(ticket);
    }

    public Task UpdateAsync(BorrowTicket ticket)
    {
        _dbContext.BorrowTickets.Update(ticket);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

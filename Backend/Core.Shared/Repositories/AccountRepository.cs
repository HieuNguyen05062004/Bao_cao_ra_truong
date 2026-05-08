using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class AccountRepository
{
    private readonly LibraryDbContext _dbContext;

    public AccountRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Account>> GetStaffAccountsAsync()
    {
        return await _dbContext.Accounts
            .Where(x => x.Role == "Admin" || x.Role == "Staff")
            .OrderBy(x => x.Username)
            .ToListAsync();
    }

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<bool> ExistsAsync(string username)
    {
        return await _dbContext.Accounts.AnyAsync(x => x.Username == username);
    }

    public async Task<bool> IsUsedInBorrowAsync(string username)
    {
        return await _dbContext.BorrowTickets.AnyAsync(x => x.StaffUsername == username);
    }

    public async Task AddAsync(Account account)
    {
        await _dbContext.Accounts.AddAsync(account);
    }

    public Task UpdateAsync(Account account)
    {
        _dbContext.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Account account)
    {
        _dbContext.Accounts.Remove(account);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

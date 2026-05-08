using Core.Shared.Constants;
using Core.Shared.Data;
using Core.Shared.Interfaces;
using Core.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Services;

public class StatisticsService : IStatisticsService
{
    private readonly LibraryDbContext _dbContext;

    public StatisticsService(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var yearStart = new DateTime(now.Year, 1, 1);

        return new DashboardStats
        {
            TotalBooks = await _dbContext.Books.SumAsync(x => x.Quantity ?? 0),
            BorrowingBooks = await _dbContext.BorrowTickets
                .Where(x => x.Status == BorrowStatusConstants.Borrowing)
                .SelectMany(x => x.Books)
                .CountAsync(),
            OverdueTickets = await _dbContext.BorrowTickets.CountAsync(x => x.Status == BorrowStatusConstants.Overdue),
            TotalReaders = await _dbContext.Readers.CountAsync(),
            BorrowCountToday = await _dbContext.BorrowTickets.CountAsync(x => x.BorrowDate.HasValue && x.BorrowDate.Value >= today),
            BorrowCountThisMonth = await _dbContext.BorrowTickets.CountAsync(x => x.BorrowDate.HasValue && x.BorrowDate.Value >= monthStart),
            BorrowCountThisYear = await _dbContext.BorrowTickets.CountAsync(x => x.BorrowDate.HasValue && x.BorrowDate.Value >= yearStart)
        };
    }
}

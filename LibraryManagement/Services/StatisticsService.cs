using LibraryManagement.Data;
using LibraryManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsViewModel> GetOverallStatisticsAsync();
        Task<PersonalStatisticsViewModel> GetPersonalStatisticsAsync(string userId);
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StatisticsViewModel> GetOverallStatisticsAsync()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            var readerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Reader");
            var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");

            int totalReaders = 0, totalStaff = 0;
            if (readerRole != null)
                totalReaders = await _context.UserRoles.CountAsync(ur => ur.RoleId == readerRole.Id);
            if (staffRole != null)
                totalStaff = await _context.UserRoles.CountAsync(ur => ur.RoleId == staffRole.Id);

            var totalBorrowing = await _context.BorrowRecords
                .CountAsync(br => br.Status == Models.BorrowStatus.Borrowing);
            var totalReturned = await _context.BorrowRecords
                .CountAsync(br => br.Status == Models.BorrowStatus.Returned);
            var totalOverdue = await _context.BorrowRecords
                .CountAsync(br => br.Status == Models.BorrowStatus.Borrowing && br.DueDate < DateTime.Today);

            // Monthly borrow stats (last 12 months)
            var cutoff = DateTime.Today.AddMonths(-11);
            var borrows = await _context.BorrowRecords
                .Where(br => br.BorrowDate >= cutoff)
                .ToListAsync();

            var monthlyBorrow = borrows
                .GroupBy(br => new { br.BorrowDate.Year, br.BorrowDate.Month })
                .Select(g => new MonthlyStats { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToList();

            var returns = await _context.BorrowRecords
                .Where(br => br.ReturnDate.HasValue && br.ReturnDate >= cutoff)
                .ToListAsync();

            var monthlyReturn = returns
                .GroupBy(br => new { br.ReturnDate!.Value.Year, br.ReturnDate!.Value.Month })
                .Select(g => new MonthlyStats { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToList();

            // Category stats
            var categoryStats = await _context.Categories
                .Include(c => c.Books)
                    .ThenInclude(b => b.BorrowRecords)
                .Select(c => new CategoryStats
                {
                    CategoryName = c.Name,
                    BookCount = c.Books.Count,
                    BorrowCount = c.Books.SelectMany(b => b.BorrowRecords).Count()
                })
                .ToListAsync();

            // Top borrowed books
            var topBooks = await _context.Books
                .Include(b => b.BorrowRecords)
                .OrderByDescending(b => b.BorrowRecords.Count)
                .Take(5)
                .Select(b => new TopBook
                {
                    Title = b.Title,
                    Author = b.Author,
                    BorrowCount = b.BorrowRecords.Count
                })
                .ToListAsync();

            return new StatisticsViewModel
            {
                TotalBooks = totalBooks,
                TotalCategories = totalCategories,
                TotalReaders = totalReaders,
                TotalStaff = totalStaff,
                TotalBorrowing = totalBorrowing,
                TotalReturned = totalReturned,
                TotalOverdue = totalOverdue,
                MonthlyBorrowStats = monthlyBorrow,
                MonthlyReturnStats = monthlyReturn,
                CategoryStats = categoryStats,
                TopBorrowedBooks = topBooks
            };
        }

        public async Task<PersonalStatisticsViewModel> GetPersonalStatisticsAsync(string userId)
        {
            var records = await _context.BorrowRecords
                .Include(br => br.Book)
                    .ThenInclude(b => b!.Category)
                .Where(br => br.UserId == userId)
                .OrderByDescending(br => br.CreatedAt)
                .ToListAsync();

            var currentBorrows = records
                .Where(br => br.Status == Models.BorrowStatus.Borrowing)
                .ToList();

            return new PersonalStatisticsViewModel
            {
                TotalBorrowed = records.Count,
                CurrentlyBorrowing = currentBorrows.Count,
                TotalReturned = records.Count(br => br.Status == Models.BorrowStatus.Returned),
                Overdue = records.Count(br => br.Status == Models.BorrowStatus.Borrowing && br.DueDate < DateTime.Today),
                CurrentBorrows = currentBorrows,
                BorrowHistory = records.Where(br => br.Status != Models.BorrowStatus.Borrowing).ToList()
            };
        }
    }
}

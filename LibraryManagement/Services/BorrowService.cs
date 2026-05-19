using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services
{
    public interface IBorrowService
    {
        Task<List<BorrowRecord>> GetAllAsync();
        Task<List<BorrowRecord>> GetByUserAsync(string userId);
        Task<BorrowRecord?> GetByIdAsync(int id);
        Task<BorrowRecord> BorrowBookAsync(string userId, int bookId, DateTime borrowDate, DateTime dueDate, string? notes = null);
        Task<BorrowRecord> ReturnBookAsync(int borrowId, string processedByUserId, string? notes = null);
        Task<List<BorrowRecord>> GetOverdueAsync();
        Task UpdateOverdueStatusAsync();
    }

    public class BorrowService : IBorrowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookService _bookService;

        public BorrowService(ApplicationDbContext context, IBookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        public async Task<List<BorrowRecord>> GetAllAsync()
        {
            return await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.Book)
                    .ThenInclude(b => b!.Category)
                .Include(br => br.ProcessedBy)
                .OrderByDescending(br => br.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BorrowRecord>> GetByUserAsync(string userId)
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                    .ThenInclude(b => b!.Category)
                .Where(br => br.UserId == userId)
                .OrderByDescending(br => br.CreatedAt)
                .ToListAsync();
        }

        public async Task<BorrowRecord?> GetByIdAsync(int id)
        {
            return await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.Book)
                    .ThenInclude(b => b!.Category)
                .Include(br => br.ProcessedBy)
                .FirstOrDefaultAsync(br => br.Id == id);
        }

        public async Task<BorrowRecord> BorrowBookAsync(string userId, int bookId, DateTime borrowDate, DateTime dueDate, string? notes = null)
        {
            if (!await _bookService.IsAvailableAsync(bookId))
                throw new InvalidOperationException("Sách hiện không còn đủ số lượng để mượn");

            var alreadyBorrowing = await _context.BorrowRecords
                .AnyAsync(br => br.UserId == userId && br.BookId == bookId && br.Status == BorrowStatus.Borrowing);
            if (alreadyBorrowing)
                throw new InvalidOperationException("Bạn đang mượn cuốn sách này rồi");

            var record = new BorrowRecord
            {
                UserId = userId,
                BookId = bookId,
                BorrowDate = borrowDate,
                DueDate = dueDate,
                Status = BorrowStatus.Borrowing,
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            _context.BorrowRecords.Add(record);
            await _bookService.UpdateQuantityAsync(bookId, -1);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<BorrowRecord> ReturnBookAsync(int borrowId, string processedByUserId, string? notes = null)
        {
            var record = await _context.BorrowRecords.FindAsync(borrowId);
            if (record == null) throw new InvalidOperationException("Không tìm thấy bản ghi mượn");
            if (record.Status != BorrowStatus.Borrowing)
                throw new InvalidOperationException("Sách này đã được trả rồi");

            record.ReturnDate = DateTime.Today;
            record.ProcessedByUserId = processedByUserId;
            if (notes != null) record.Notes = notes;

            if (record.ReturnDate > record.DueDate)
                record.Status = BorrowStatus.Overdue;
            else
                record.Status = BorrowStatus.Returned;

            await _bookService.UpdateQuantityAsync(record.BookId, 1);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<List<BorrowRecord>> GetOverdueAsync()
        {
            return await _context.BorrowRecords
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => br.Status == BorrowStatus.Borrowing && br.DueDate < DateTime.Today)
                .ToListAsync();
        }

        public async Task UpdateOverdueStatusAsync()
        {
            var overdueRecords = await _context.BorrowRecords
                .Where(br => br.Status == BorrowStatus.Borrowing && br.DueDate < DateTime.Today)
                .ToListAsync();

            foreach (var record in overdueRecords)
            {
                // Keep status as Borrowing but flag as overdue visually (handled in view)
                _ = record; // no status change needed — overdue is determined by DueDate comparison
            }
            await _context.SaveChangesAsync();
        }
    }
}

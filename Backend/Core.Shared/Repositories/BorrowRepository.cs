using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class BorrowRepository
{
    private readonly LibraryDbContext _context;

    public BorrowRepository(LibraryDbContext context)
    {
        _context = context;
    }

    // ── Base query — luôn Include Reader + Books ──────────────────────────
    private IQueryable<BorrowTicket> BaseQuery() =>
        _context.BorrowTickets
            .Include(bt => bt.Reader)
            .Include(bt => bt.StaffUsernameNavigation)
            .Include(bt => bt.Books)
            .AsSplitQuery();

    // ── Truy vấn ─────────────────────────────────────────────────────────

    public async Task<IEnumerable<BorrowTicket>> GetAllAsync()
    {
        return await BaseQuery()
            .OrderByDescending(bt => bt.BorrowDate)
            .ToListAsync();
    }

    public async Task<BorrowTicket?> GetByIdAsync(int ticketId)
    {
        return await BaseQuery()
            .FirstOrDefaultAsync(bt => bt.TicketId == ticketId);
    }

    public async Task<IEnumerable<BorrowTicket>> GetByReaderIdAsync(string readerId)
    {
        return await BaseQuery()
            .Where(bt => bt.ReaderId == readerId)
            .OrderByDescending(bt => bt.BorrowDate)
            .ToListAsync();
    }

    // Phiếu chưa trả
    public async Task<IEnumerable<BorrowTicket>> GetBorrowingAsync()
    {
        return await BaseQuery()
            .Where(bt => bt.ReturnDate == null)
            .OrderBy(bt => bt.DueDate)
            .ToListAsync();
    }

    // Phiếu quá hạn
    public async Task<IEnumerable<BorrowTicket>> GetOverdueAsync()
    {
        var now = DateTime.Now;
        return await BaseQuery()
            .Where(bt => bt.ReturnDate == null && bt.DueDate < now)
            .OrderBy(bt => bt.DueDate)
            .ToListAsync();
    }

    // Tìm kiếm theo tên bạn đọc hoặc tên sách
    public async Task<IEnumerable<BorrowTicket>> SearchAsync(string keyword)
    {
        keyword = keyword.Trim().ToLower();
        return await BaseQuery()
            .Where(bt =>
                (bt.Reader != null && bt.Reader.FullName.ToLower().Contains(keyword)) ||
                (bt.ReaderId != null && bt.ReaderId.ToLower().Contains(keyword)) ||
                bt.Books.Any(b => b.Title.ToLower().Contains(keyword)))
            .OrderByDescending(bt => bt.BorrowDate)
            .ToListAsync();
    }

    // Kiểm tra sách có đang được mượn không
    public async Task<bool> IsBookBorrowedAsync(string bookId)
    {
        return await _context.BorrowTickets
            .Where(bt => bt.ReturnDate == null)
            .AnyAsync(bt => bt.Books.Any(b => b.BookId == bookId));
    }

    // ── CRUD ──────────────────────────────────────────────────────────────

    public async Task AddAsync(BorrowTicket ticket)
    {
        await _context.BorrowTickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(BorrowTicket ticket)
    {
        _context.BorrowTickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(BorrowTicket ticket)
    {
        _context.BorrowTickets.Remove(ticket);
        await _context.SaveChangesAsync();
    }

    // Lấy Book entities theo danh sách BookId (dùng khi tạo phiếu mượn)
    public async Task<List<Book>> GetBooksByIdsAsync(IEnumerable<string> bookIds)
    {
        return await _context.Books
            .Where(b => bookIds.Contains(b.BookId))
            .ToListAsync();
    }
}

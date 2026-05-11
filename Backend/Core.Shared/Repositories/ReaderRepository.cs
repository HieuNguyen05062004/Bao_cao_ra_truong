using Core.Shared.Data;
using Core.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Repositories;

public class ReaderRepository
{
    private readonly LibraryDbContext _context;

    public ReaderRepository(LibraryDbContext context)
    {
        _context = context;
    }

    // Lấy tất cả bạn đọc
    public async Task<IEnumerable<Reader>> GetAllAsync()
    {
        return await _context.Readers
            .OrderBy(r => r.FullName)
            .ToListAsync();
    }

    // Lấy theo ID, kèm danh sách phiếu mượn + sách (không load Category của sách)
    public async Task<Reader?> GetByIdAsync(string readerId)
    {
        return await _context.Readers
            .Include(r => r.BorrowTickets)
                .ThenInclude(bt => bt.Books)
            .AsSplitQuery()   // tránh cartesian explosion khi join nhiều bảng
            .FirstOrDefaultAsync(r => r.ReaderId == readerId);
    }

    public async Task<bool> ExistsAsync(string readerId)
    {
        return await _context.Readers
            .AnyAsync(r => r.ReaderId == readerId);
    }

    // Kiểm tra còn phiếu mượn chưa trả
    public async Task<bool> HasActiveBorrowAsync(string readerId)
    {
        return await _context.BorrowTickets
            .AnyAsync(bt => bt.ReaderId == readerId &&
                            bt.ReturnDate == null);
    }

    public async Task AddAsync(Reader reader)
    {
        await _context.Readers.AddAsync(reader);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Reader reader)
    {
        _context.Readers.Update(reader);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Reader reader)
    {
        _context.Readers.Remove(reader);
        await _context.SaveChangesAsync();
    }

    // Tìm kiếm theo keyword
    public async Task<IEnumerable<Reader>> SearchAsync(string keyword)
    {
        keyword = keyword.Trim().ToLower();
        return await _context.Readers
            .Where(r =>
                r.ReaderId.ToLower().Contains(keyword) ||
                r.FullName.ToLower().Contains(keyword) ||
                (r.Email != null && r.Email.ToLower().Contains(keyword)) ||
                (r.Phone != null && r.Phone.ToLower().Contains(keyword)))
            .ToListAsync();
    }

    // Lấy số lượng sách đang mượn / quá hạn
    public async Task<int> CountBorrowingAsync(string readerId)
    {
        return await _context.BorrowTickets
            .CountAsync(bt => bt.ReaderId == readerId &&
                              bt.ReturnDate == null);
    }

    public async Task<int> CountOverdueAsync(string readerId)
    {
        var now = DateTime.Now;
        return await _context.BorrowTickets
            .CountAsync(bt => bt.ReaderId == readerId &&
                              bt.ReturnDate == null &&
                              bt.DueDate < now);
    }
}

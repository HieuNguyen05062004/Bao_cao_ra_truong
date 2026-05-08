using Core.Shared.Constants;
using Core.Shared.Data;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Models;
using Core.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Core.Shared.Services;

public class BorrowService : IBorrowService
{
    private readonly BorrowRepository _borrowRepository;
    private readonly LibraryDbContext _dbContext;

    public BorrowService(BorrowRepository borrowRepository, LibraryDbContext dbContext)
    {
        _borrowRepository = borrowRepository;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<BorrowTicket>> GetAllTicketsAsync(string? readerId = null, string? status = null)
    {
        var tickets = await _borrowRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(readerId))
        {
            tickets = tickets.Where(x => x.ReaderId == readerId).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            tickets = tickets.Where(x => x.Status == status).ToList();
        }

        return tickets;
    }

    public async Task<BorrowTicket?> GetTicketByIdAsync(int ticketId)
    {
        return await _borrowRepository.GetByIdAsync(ticketId);
    }

    public async Task<(bool Success, string Message, BorrowTicket? Data)> BorrowAsync(BorrowRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReaderId) || request.BookIds.Count == 0)
        {
            return (false, MessageConstants.InvalidData, null);
        }

        var reader = await _dbContext.Readers.FirstOrDefaultAsync(x => x.ReaderId == request.ReaderId);
        if (reader is null)
        {
            return (false, "Không tồn tại bạn đọc.", null);
        }

        if (!string.IsNullOrWhiteSpace(request.StaffUsername))
        {
            var staffExists = await _dbContext.Accounts.AnyAsync(x => x.Username == request.StaffUsername);
            if (!staffExists)
            {
                return (false, "Không tồn tại tài khoản nhân viên xử lý.", null);
            }
        }

        var hasOverdue = await _dbContext.BorrowTickets.AnyAsync(x =>
            x.ReaderId == request.ReaderId &&
            (x.Status == BorrowStatusConstants.Overdue || (x.ReturnDate == null && x.DueDate.HasValue && x.DueDate.Value < DateTime.UtcNow)));

        if (hasOverdue)
        {
            return (false, MessageConstants.ReaderHasOverdue, null);
        }

        var requestedBookIds = request.BookIds.Distinct().ToList();
        var books = await _dbContext.Books.Where(x => requestedBookIds.Contains(x.BookId)).ToListAsync();

        if (books.Count != requestedBookIds.Count)
        {
            return (false, "Có sách không tồn tại.", null);
        }

        if (books.Any(x => (x.Quantity ?? 0) <= 0))
        {
            return (false, MessageConstants.BookOutOfStock, null);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var ticket = new BorrowTicket
            {
                ReaderId = request.ReaderId,
                StaffUsername = request.StaffUsername,
                BorrowDate = DateTime.UtcNow,
                DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(14),
                Status = BorrowStatusConstants.Borrowing,
                Books = books
            };

            await _borrowRepository.AddAsync(ticket);

            foreach (var book in books)
            {
                book.Quantity = (book.Quantity ?? 0) - 1;
                book.Status = (book.Quantity ?? 0) > 0 ? BorrowStatusConstants.Available : BorrowStatusConstants.BorrowedOut;
            }

            await _borrowRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            var fullTicket = await _borrowRepository.GetByIdAsync(ticket.TicketId);
            return (true, "Tạo phiếu mượn thành công.", fullTicket);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(bool Success, string Message, BorrowTicket? Data)> ReturnAsync(int ticketId, ReturnRequest request)
    {
        var ticket = await _borrowRepository.GetByIdAsync(ticketId);
        if (ticket is null)
        {
            return (false, MessageConstants.NotFound, null);
        }

        if (ticket.ReturnDate.HasValue)
        {
            return (false, "Phiếu mượn đã được trả trước đó.", null);
        }

        var returnDate = request.ReturnDate ?? DateTime.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            ticket.ReturnDate = returnDate;
            ticket.Status = ticket.DueDate.HasValue && returnDate > ticket.DueDate.Value
                ? BorrowStatusConstants.Overdue
                : BorrowStatusConstants.Returned;

            foreach (var book in ticket.Books)
            {
                book.Quantity = (book.Quantity ?? 0) + 1;
                book.Status = (book.Quantity ?? 0) > 0 ? BorrowStatusConstants.Available : BorrowStatusConstants.BorrowedOut;
            }

            await _borrowRepository.UpdateAsync(ticket);
            await _borrowRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            var updated = await _borrowRepository.GetByIdAsync(ticket.TicketId);
            return (true, "Trả sách thành công.", updated);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

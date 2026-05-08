using Core.Shared.Entities;
using Core.Shared.Models;

namespace Core.Shared.Interfaces;

public interface IBorrowService
{
    Task<IEnumerable<BorrowTicket>> GetAllTicketsAsync(string? readerId = null, string? status = null);
    Task<BorrowTicket?> GetTicketByIdAsync(int ticketId);
    Task<(bool Success, string Message, BorrowTicket? Data)> BorrowAsync(BorrowRequest request);
    Task<(bool Success, string Message, BorrowTicket? Data)> ReturnAsync(int ticketId, ReturnRequest request);
}

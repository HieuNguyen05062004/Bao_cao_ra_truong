using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface IBorrowService
{
    // ── Truy vấn ─────────────────────────────────────────────────────────
    Task<IEnumerable<BorrowTicket>> GetAllBorrowTicketsAsync();
    Task<BorrowTicket?> GetBorrowTicketByIdAsync(int ticketId);
    Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByReaderIdAsync(string readerId);
    Task<IEnumerable<BorrowTicket>> GetBorrowingAsync();
    Task<IEnumerable<BorrowTicket>> GetOverdueAsync();

    // ── Nghiệp vụ Admin ───────────────────────────────────────────────────
    /// <summary>Duyệt yêu cầu mượn. Trả về (success, message).</summary>
    Task<(bool Success, string Message)> ApproveBorrowRequestAsync(int ticketId, string staffUsername);

    /// <summary>Từ chối yêu cầu mượn. Trả về (success, message).</summary>
    Task<(bool Success, string Message)> RejectBorrowRequestAsync(int ticketId, string reason);

    /// <summary>Xác nhận trả sách. Trả về (success, message).</summary>
    Task<(bool Success, string Message)> ReturnBooksAsync(int ticketId);

    // ── Nghiệp vụ Client ──────────────────────────────────────────────────
    /// <summary>Bạn đọc gửi yêu cầu mượn. Trả về (success, message, ticketId).</summary>
    Task<(bool Success, string Message, int TicketId)> CreateBorrowRequestAsync(
        string readerId,
        List<string> bookIds,
        DateTime borrowDate,
        DateTime dueDate);

    // ── Tìm kiếm ─────────────────────────────────────────────────────────
    Task<IEnumerable<BorrowTicket>> SearchAsync(string keyword);
    Task<string?> DeleteAsync(int ticketId);
}

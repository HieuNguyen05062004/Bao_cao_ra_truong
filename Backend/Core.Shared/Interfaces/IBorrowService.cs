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
    Task<IEnumerable<BorrowTicket>> SearchAsync(string keyword);

    // ── Nghiệp vụ Client ──────────────────────────────────────────────────
    /// <summary>Bạn đọc gửi yêu cầu mượn. Trả về (success, message, ticketId).</summary>
    Task<(bool Success, string Message, int TicketId)> CreateBorrowRequestAsync(
        string readerId,
        List<string> bookIds,
        DateTime borrowDate,
        DateTime dueDate);

    // ── Nghiệp vụ Admin ───────────────────────────────────────────────────
    /// <summary>Duyệt yêu cầu mượn → trạng thái "Đã duyệt", giảm Quantity sách.</summary>
    Task<(bool Success, string Message)> ApproveBorrowRequestAsync(int ticketId, string staffUsername);

    /// <summary>Từ chối yêu cầu mượn → trạng thái "Bị từ chối".</summary>
    Task<(bool Success, string Message)> RejectBorrowRequestAsync(int ticketId, string reason);

    /// <summary>Xác nhận đã giao sách → trạng thái "Đang mượn".</summary>
    Task<(bool Success, string Message)> ConfirmBorrowingAsync(int ticketId, string staffUsername);

    /// <summary>Xác nhận trả sách → trạng thái "Đã trả", hoàn lại Quantity sách.</summary>
    Task<(bool Success, string Message)> ReturnBooksAsync(int ticketId);

    // ── Xóa phiếu ────────────────────────────────────────────────────────
    Task<string?> DeleteAsync(int ticketId);
}

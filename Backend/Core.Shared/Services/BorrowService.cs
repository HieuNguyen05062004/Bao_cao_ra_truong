using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class BorrowService : IBorrowService
{
    private readonly BorrowRepository _repo;

    public BorrowService(BorrowRepository repo)
    {
        _repo = repo;
    }

    // ── Truy vấn ─────────────────────────────────────────────────────────

    public async Task<IEnumerable<BorrowTicket>> GetAllBorrowTicketsAsync()
        => await _repo.GetAllAsync();

    public async Task<BorrowTicket?> GetBorrowTicketByIdAsync(int ticketId)
        => await _repo.GetByIdAsync(ticketId);

    public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByReaderIdAsync(string readerId)
        => await _repo.GetByReaderIdAsync(readerId);

    public async Task<IEnumerable<BorrowTicket>> GetBorrowingAsync()
        => await _repo.GetBorrowingAsync();

    public async Task<IEnumerable<BorrowTicket>> GetOverdueAsync()
        => await _repo.GetOverdueAsync();

    public async Task<IEnumerable<BorrowTicket>> SearchAsync(string keyword)
        => await _repo.SearchAsync(keyword);

    // ── Client: Gửi yêu cầu mượn sách ───────────────────────────────────

    public async Task<(bool Success, string Message, int TicketId)> CreateBorrowRequestAsync(
        string readerId,
        List<string> bookIds,
        DateTime borrowDate,
        DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(readerId))
            return (false, "Vui lòng chọn bạn đọc.", 0);

        if (!bookIds.Any())
            return (false, "Vui lòng chọn ít nhất một cuốn sách.", 0);

        var books = await _repo.GetBooksByIdsAsync(bookIds);

        if (books.Count != bookIds.Count)
            return (false, "Một số sách không tồn tại trong hệ thống.", 0);

        // Kiểm tra số lượng tồn kho
        var outOfStock = books.Where(b => b.Quantity <= 0).ToList();
        if (outOfStock.Any())
        {
            var titles = string.Join(", ", outOfStock.Select(b => b.Title));
            return (false, $"Sách đã hết: {titles}", 0);
        }

        var ticket = new BorrowTicket
        {
            ReaderId = readerId,
            BorrowDate = borrowDate,
            DueDate = dueDate,
            Status = "Pending",  // Chờ Admin duyệt
            Books = books
        };

        await _repo.AddAsync(ticket);
        return (true, "Yêu cầu mượn đã được gửi.", ticket.TicketId);
    }

    // ── Admin: Duyệt yêu cầu ─────────────────────────────────────────────

    public async Task<(bool Success, string Message)> ApproveBorrowRequestAsync(
        int ticketId, string staffUsername)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return (false, "Phiếu mượn không tồn tại.");

        if (ticket.Status != "Pending")
            return (false, $"Phiếu đang ở trạng thái '{ticket.Status}', không thể duyệt.");

        // Kiểm tra lại số lượng sách lúc duyệt
        var outOfStock = ticket.Books.Where(b => b.Quantity <= 0).ToList();
        if (outOfStock.Any())
        {
            var titles = string.Join(", ", outOfStock.Select(b => b.Title));
            return (false, $"Sách đã hết khi duyệt: {titles}");
        }

        // Giảm số lượng sách
        foreach (var book in ticket.Books)
            book.Quantity -= 1;

        ticket.Status = "Approved";
        ticket.StaffUsername = staffUsername;

        await _repo.UpdateAsync(ticket);
        return (true, "Đã duyệt phiếu mượn.");
    }

    // ── Admin: Từ chối yêu cầu ───────────────────────────────────────────

    public async Task<(bool Success, string Message)> RejectBorrowRequestAsync(
        int ticketId, string reason)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return (false, "Phiếu mượn không tồn tại.");

        if (ticket.Status != "Pending")
            return (false, $"Phiếu đang ở trạng thái '{ticket.Status}', không thể từ chối.");

        ticket.Status = "Rejected";

        await _repo.UpdateAsync(ticket);
        return (true, "Đã từ chối phiếu mượn.");
    }

    // ── Admin: Xác nhận trả sách ─────────────────────────────────────────

    public async Task<(bool Success, string Message)> ReturnBooksAsync(int ticketId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return (false, "Phiếu mượn không tồn tại.");

        if (ticket.ReturnDate != null)
            return (false, "Phiếu này đã được trả trước đó.");

        if (ticket.Status != "Approved")
            return (false, "Chỉ có thể trả sách với phiếu đã được duyệt.");

        ticket.ReturnDate = DateTime.Now;
        ticket.Status = DateTime.Now > ticket.DueDate ? "Trả trễ" : "Returned";

        // Hoàn lại số lượng sách
        foreach (var book in ticket.Books)
            book.Quantity += 1;

        await _repo.UpdateAsync(ticket);
        return (true, "Đã xác nhận trả sách.");
    }

    // ── Xóa phiếu ────────────────────────────────────────────────────────

    public async Task<string?> DeleteAsync(int ticketId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return "Phiếu mượn không tồn tại.";

        if (ticket.ReturnDate == null && ticket.Status == "Approved")
            return "Không thể xóa phiếu đang mượn. Vui lòng xử lý trả sách trước.";

        await _repo.DeleteAsync(ticket);
        return null;
    }
}

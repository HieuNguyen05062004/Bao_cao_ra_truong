using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Core.Shared.Repositories;

namespace Core.Shared.Services;

public class BorrowService : IBorrowService
{
    private readonly BorrowRepository _repo;

    // ── Hằng số trạng thái ───────────────────────────────────────────────
    public const string StatusPending = "Chờ duyệt";
    public const string StatusApproved = "Đã duyệt";
    public const string StatusBorrowing = "Đang mượn";
    public const string StatusReturned = "Đã trả";
    public const string StatusRejected = "Bị từ chối";

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
        var now = DateTime.Now;
        var currentTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        if (string.IsNullOrWhiteSpace(readerId))
            return (false, "Vui lòng chọn bạn đọc.", 0);

        if (!bookIds.Any())
            return (false, "Vui lòng chọn ít nhất một cuốn sách.", 0);

        if (borrowDate < currentTime)
            return (false, "Ngày mượn không được nhỏ hơn thời gian hiện tại.", 0);

        if (dueDate < currentTime)
            return (false, "Ngày trả không được nhỏ hơn thời gian hiện tại.", 0);

        if (dueDate < borrowDate)
            return (false, "Ngày trả phải lớn hơn hoặc bằng ngày mượn.", 0);

        var books = await _repo.GetBooksByIdsAsync(bookIds);

        if (books.Count != bookIds.Count)
            return (false, "Một số sách không tồn tại trong hệ thống.", 0);

        // Kiểm tra số lượng tồn kho
        var outOfStock = books.Where(b => (b.Quantity ?? 0) <= 0).ToList();
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
            Status = StatusPending,   // "Chờ duyệt"
            Books = books
        };

        await _repo.AddAsync(ticket);
        return (true, "Yêu cầu mượn đã được gửi. Vui lòng chờ Admin duyệt.", ticket.TicketId);
    }

    // ── Admin: Duyệt yêu cầu → "Đã duyệt" ───────────────────────────────

    public async Task<(bool Success, string Message)> ApproveBorrowRequestAsync(
        int ticketId, string staffUsername)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return (false, "Phiếu mượn không tồn tại.");

        if (ticket.Status != StatusPending)
            return (false, $"Phiếu đang ở trạng thái '{ticket.Status}', không thể duyệt.");

        // Kiểm tra lại tồn kho tại thời điểm duyệt
        var outOfStock = ticket.Books.Where(b => (b.Quantity ?? 0) <= 0).ToList();
        if (outOfStock.Any())
        {
            var titles = string.Join(", ", outOfStock.Select(b => b.Title));
            return (false, $"Sách đã hết khi duyệt: {titles}");
        }

        // Giảm số lượng sách, cập nhật Status sách
        foreach (var book in ticket.Books)
        {
            book.Quantity -= 1;
            book.Status = (book.Quantity ?? 0) <= 0 ? "Hết sách" : "Có thể mượn";
        }

        ticket.Status = StatusApproved;   // "Đã duyệt"
        ticket.StaffUsername = staffUsername;

        await _repo.UpdateAsync(ticket);
        return (true, "Đã duyệt phiếu mượn thành công.");
    }

    // ── Admin: Xác nhận đã giao sách → "Đang mượn" ───────────────────────

    public async Task<(bool Success, string Message)> ConfirmBorrowingAsync(
        int ticketId, string staffUsername)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return (false, "Phiếu mượn không tồn tại.");

        if (ticket.Status != StatusApproved)
            return (false, $"Phiếu đang ở trạng thái '{ticket.Status}', không thể xác nhận giao sách.");

        ticket.Status = StatusBorrowing;  // "Đang mượn"
        ticket.StaffUsername = staffUsername;

        await _repo.UpdateAsync(ticket);
        return (true, "Đã xác nhận giao sách cho bạn đọc.");
    }

    // ── Admin: Từ chối yêu cầu → "Bị từ chối" ────────────────────────────

    public async Task<(bool Success, string Message)> RejectBorrowRequestAsync(
        int ticketId, string reason)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return (false, "Phiếu mượn không tồn tại.");

        if (ticket.Status != StatusPending)
            return (false, $"Phiếu đang ở trạng thái '{ticket.Status}', không thể từ chối.");

        ticket.Status = StatusRejected;  // "Bị từ chối"
        // Không thay đổi Quantity sách

        await _repo.UpdateAsync(ticket);
        return (true, "Đã từ chối phiếu mượn.");
    }

    // ── Admin: Xác nhận trả sách → "Đã trả" ──────────────────────────────

    public async Task<(bool Success, string Message)> ReturnBooksAsync(int ticketId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return (false, "Phiếu mượn không tồn tại.");

        if (ticket.Status != StatusBorrowing && ticket.Status != StatusApproved)
            return (false, $"Phiếu đang ở trạng thái '{ticket.Status}', không thể xác nhận trả.");

        if (ticket.ReturnDate != null)
            return (false, "Phiếu này đã được trả trước đó.");

        ticket.ReturnDate = DateTime.Now;
        ticket.Status = StatusReturned;  // "Đã trả"

        // Hoàn lại số lượng sách, cập nhật Status sách
        foreach (var book in ticket.Books)
        {
            book.Quantity += 1;
            book.Status = "Có thể mượn";
        }

        await _repo.UpdateAsync(ticket);
        return (true, "Đã xác nhận trả sách thành công.");
    }

    // ── Xóa phiếu ────────────────────────────────────────────────────────

    public async Task<string?> DeleteAsync(int ticketId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket is null)
            return "Phiếu mượn không tồn tại.";

        if (ticket.Status == StatusBorrowing || ticket.Status == StatusApproved)
            return "Không thể xóa phiếu đang hoạt động. Vui lòng xử lý trả sách trước.";

        await _repo.DeleteAsync(ticket);
        return null;
    }
}

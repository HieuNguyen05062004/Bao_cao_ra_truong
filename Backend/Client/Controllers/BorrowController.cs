using Client.Extensions;
using Client.ViewModels;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

public class BorrowController : ClientBaseController
{
    private readonly IBorrowService _borrowService;
    private readonly IBookService _bookService;

    public BorrowController(IBorrowService borrowService, IBookService bookService)
    {
        _borrowService = borrowService;
        _bookService = bookService;
    }

    /// <summary>
    /// Kiểm tra login
    /// </summary>
    private IActionResult RequireLogin()
    {
        if (!HttpContext.Session.IsReaderLoggedIn())
        {
            TempData["Warning"] = "Vui lòng đăng nhập để mượn sách";
            return RedirectToAction("Login", "Account");
        }
        return null!;
    }

    /// <summary>
    /// Hiển thị form mượn sách với thông tin sách được chọn
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBorrowRequest(string bookIds)
    {
        var loginCheck = RequireLogin();
        if (loginCheck != null) return loginCheck;

        try
        {
            // Parse danh sách BookIds từ query string
            var selectedBookIds = bookIds?
                .Split(',')
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct()
                .ToList() ?? new List<string>();

            if (selectedBookIds.Count == 0)
                return BadRequest("Vui lòng chọn ít nhất một cuốn sách.");

            // Lấy thông tin sách
            var books = new List<Core.Shared.Entities.Book>();
            foreach (var bookId in selectedBookIds)
            {
                var book = await _bookService.GetBookByIdAsync(bookId);
                if (book != null)
                    books.Add(book);
            }

            if (books.Count == 0)
                return BadRequest("Không tìm thấy sách được chọn.");

            // Lấy thông tin bạn đọc từ session
            var readerId = HttpContext.Session.GetReaderId();
            var readerName = HttpContext.Session.GetReaderName();

            // Tạo view model
            var model = new BorrowRequestViewModel
            {
                ReaderId = readerId!,
                ReaderName = readerName!,
                SelectedBookIds = selectedBookIds,
                SelectedBookTitles = books.Select(b => b.Title).ToList(),
                BorrowDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7)
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return BadRequest($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Gửi yêu cầu mượn sách
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitBorrowRequest(BorrowRequestViewModel model)
    {
        try
        {
            if (model == null || string.IsNullOrEmpty(model.ReaderId))
                return BadRequest("Thông tin không hợp lệ");

            // Validate ngày
            if (model.BorrowDate >= model.DueDate)
                return BadRequest("Ngày mượn phải trước ngày trả");

            if (model.BorrowDate < DateTime.Today)
                return BadRequest("Ngày mượn không được là ngày quá khứ");

            // Tạo yêu cầu mượn
            var (success, message, ticketId) = await _borrowService.CreateBorrowRequestAsync(
                model.ReaderId,
                model.SelectedBookIds,
                model.BorrowDate,
                model.DueDate
            );

            if (success)
            {
                TempData["SuccessMessage"] = "Yêu cầu mượn sách đã được gửi thành công!";
                return RedirectToAction(nameof(BorrowHistory));
            }

            TempData["ErrorMessage"] = message;
            return View("CreateBorrowRequest", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return View("CreateBorrowRequest", model);
        }
    }

    /// <summary>
    /// Xem lịch sử mượn sách của người dùng
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BorrowHistory(string readerId)
    {
        try
        {
            // TODO: Lấy readerId từ Session hoặc User Identity
            if (string.IsNullOrEmpty(readerId))
                readerId = "R001";  // Placeholder

            var borrowTickets = await _borrowService.GetBorrowTicketsByReaderIdAsync(readerId);
            return View(borrowTickets);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return View(new List<Core.Shared.Entities.BorrowTicket>());
        }
    }

    /// <summary>
    /// Chi tiết yêu cầu mượn
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BorrowDetail(int ticketId)
    {
        try
        {
            var borrowTicket = await _borrowService.GetBorrowTicketByIdAsync(ticketId);
            if (borrowTicket == null)
                return NotFound();

            return View(borrowTicket);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(BorrowHistory));
        }
    }
}

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

    // ── Kiểm tra đăng nhập ───────────────────────────────────────────────
    private IActionResult? RequireLogin()
    {
        if (!HttpContext.Session.IsReaderLoggedIn())
        {
            TempData["Warning"] = "Vui lòng đăng nhập để mượn sách.";
            return RedirectToAction("Login", "Account");
        }
        return null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /Borrow/CreateBorrowRequest?bookIds=B001,B002
    // Hiển thị form mượn sách — dùng GET để link từ trang chi tiết sách
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> CreateBorrowRequest(string bookIds)
    {
        var loginCheck = RequireLogin();
        if (loginCheck != null) return loginCheck;

        if (string.IsNullOrWhiteSpace(bookIds))
        {
            TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một cuốn sách.";
            return RedirectToAction("Index", "Search");
        }

        var selectedBookIds = bookIds
            .Split(',')
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct()
            .ToList();

        var books = new List<Core.Shared.Entities.Book>();
        foreach (var bookId in selectedBookIds)
        {
            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book != null) books.Add(book);
        }

        if (books.Count == 0)
        {
            TempData["ErrorMessage"] = "Không tìm thấy sách được chọn.";
            return RedirectToAction("Index", "Search");
        }

        var model = new BorrowRequestViewModel
        {
            ReaderId = HttpContext.Session.GetReaderId()!,
            ReaderName = HttpContext.Session.GetReaderName()!,
            SelectedBookIds = selectedBookIds,
            SelectedBookTitles = books.Select(b => b.Title).ToList(),
            BorrowDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(7)
        };

        return View(model);
    }

    // ─────────────────────────────────────────────────────────────────────
    // POST /Borrow/SubmitBorrowRequest
    // Gửi yêu cầu mượn
    // ─────────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitBorrowRequest(BorrowRequestViewModel model)
    {
        var loginCheck = RequireLogin();
        if (loginCheck != null) return loginCheck;

        // Đảm bảo ReaderId luôn lấy từ Session (tránh giả mạo)
        model.ReaderId = HttpContext.Session.GetReaderId()!;
        model.ReaderName = HttpContext.Session.GetReaderName()!;

        if (string.IsNullOrEmpty(model.ReaderId))
        {
            TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login", "Account");
        }

        var now = DateTime.Now;
        var currentTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        if (model.BorrowDate == default)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn ngày mượn.";
            return View("CreateBorrowRequest", model);
        }

        if (model.DueDate == default)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn ngày trả.";
            return View("CreateBorrowRequest", model);
        }

        if (model.BorrowDate < currentTime)
        {
            TempData["ErrorMessage"] = "Ngày mượn không được nhỏ hơn thời gian hiện tại.";
            return View("CreateBorrowRequest", model);
        }

        if (model.DueDate < currentTime)
        {
            TempData["ErrorMessage"] = "Ngày trả không được nhỏ hơn thời gian hiện tại.";
            return View("CreateBorrowRequest", model);
        }

        if (model.DueDate < model.BorrowDate)
        {
            TempData["ErrorMessage"] = "Ngày trả phải lớn hơn hoặc bằng ngày mượn.";
            return View("CreateBorrowRequest", model);
        }

        if (!model.SelectedBookIds.Any())
        {
            TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một cuốn sách.";
            return View("CreateBorrowRequest", model);
        }

        var (success, message, ticketId) = await _borrowService.CreateBorrowRequestAsync(
            model.ReaderId,
            model.SelectedBookIds,
            model.BorrowDate,
            model.DueDate
        );

        if (success)
        {
            TempData["SuccessMessage"] = "Yêu cầu mượn sách đã được gửi! Vui lòng chờ Admin duyệt.";
            return RedirectToAction(nameof(BorrowHistory));
        }

        TempData["ErrorMessage"] = message;
        return View("CreateBorrowRequest", model);
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /Borrow/BorrowHistory
    // Lịch sử mượn sách — luôn lấy readerId từ Session
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> BorrowHistory()
    {
        var loginCheck = RequireLogin();
        if (loginCheck != null) return loginCheck;

        var readerId = HttpContext.Session.GetReaderId()!;

        var tickets = await _borrowService.GetBorrowTicketsByReaderIdAsync(readerId);
        return View(tickets.ToList());
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /Borrow/BorrowDetail/{ticketId}
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> BorrowDetail(int ticketId)
    {
        var loginCheck = RequireLogin();
        if (loginCheck != null) return loginCheck;

        var ticket = await _borrowService.GetBorrowTicketByIdAsync(ticketId);
        if (ticket == null) return NotFound();

        // Ngăn bạn đọc xem phiếu của người khác
        var readerId = HttpContext.Session.GetReaderId();
        if (ticket.ReaderId != readerId)
        {
            TempData["ErrorMessage"] = "Bạn không có quyền xem phiếu mượn này.";
            return RedirectToAction(nameof(BorrowHistory));
        }

        return View(ticket);
    }
}

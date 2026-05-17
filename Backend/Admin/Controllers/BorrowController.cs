using Core.Shared.Interfaces;
using Core.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class BorrowController : Controller
{
    private readonly IBorrowService _borrowService;
    private readonly IBookService _bookService;

    // Danh sách trạng thái hiển thị trên UI
    private static readonly string[] StatusList =
    {
        BorrowService.StatusPending,    // Chờ duyệt
        BorrowService.StatusApproved,   // Đã duyệt
        BorrowService.StatusBorrowing,  // Đang mượn
        BorrowService.StatusReturned,   // Đã trả
        BorrowService.StatusRejected    // Bị từ chối
    };

    public BorrowController(IBorrowService borrowService, IBookService bookService)
    {
        _borrowService = borrowService;
        _bookService = bookService;
    }

    // ── Lấy username nhân viên từ Session ────────────────────────────────
    private string GetStaffUsername()
        => HttpContext.Session.GetString("Username") ?? User.Identity?.Name ?? "admin";

    // ─────────────────────────────────────────────────────────────────────
    // GET /Admin/Borrow/Index?status=
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(string status = "", string keyword = "")
    {
        try
        {
            IEnumerable<Core.Shared.Entities.BorrowTicket> tickets;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                tickets = await _borrowService.SearchAsync(keyword);
            }
            else
            {
                tickets = await _borrowService.GetAllBorrowTicketsAsync();
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
                tickets = tickets.Where(t => t.Status == status);

            ViewBag.SelectedStatus = status;
            ViewBag.Keyword = keyword;
            ViewBag.StatusList = StatusList;

            // Đếm theo từng trạng thái để hiển thị badge
            var all = await _borrowService.GetAllBorrowTicketsAsync();
            ViewBag.CountPending = all.Count(t => t.Status == BorrowService.StatusPending);
            ViewBag.CountApproved = all.Count(t => t.Status == BorrowService.StatusApproved);
            ViewBag.CountBorrowing = all.Count(t => t.Status == BorrowService.StatusBorrowing);
            ViewBag.CountReturned = all.Count(t => t.Status == BorrowService.StatusReturned);
            ViewBag.CountRejected = all.Count(t => t.Status == BorrowService.StatusRejected);

            return View(tickets);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tải danh sách: " + ex.Message;
            return View(Enumerable.Empty<Core.Shared.Entities.BorrowTicket>());
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /Admin/Borrow/Details/{id}
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var ticket = await _borrowService.GetBorrowTicketByIdAsync(id);
            if (ticket == null) return NotFound();

            return View(ticket);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // POST /Admin/Borrow/Approve/{id}
    // Chờ duyệt → Đã duyệt + giảm Quantity sách
    // ─────────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var staff = GetStaffUsername();
            var (success, message) = await _borrowService.ApproveBorrowRequestAsync(id, staff);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // POST /Admin/Borrow/ConfirmBorrowing/{id}
    // Đã duyệt → Đang mượn (xác nhận đã giao sách)
    // ─────────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmBorrowing(int id)
    {
        try
        {
            var staff = GetStaffUsername();
            var (success, message) = await _borrowService.ConfirmBorrowingAsync(id, staff);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // POST /Admin/Borrow/Reject/{id}
    // Chờ duyệt → Bị từ chối
    // ─────────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason = "")
    {
        try
        {
            var (success, message) = await _borrowService.RejectBorrowRequestAsync(id, reason);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // POST /Admin/Borrow/ReturnBooks/{id}
    // Đang mượn / Đã duyệt → Đã trả + hoàn Quantity sách
    // ─────────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnBooks(int id)
    {
        try
        {
            var (success, message) = await _borrowService.ReturnBooksAsync(id);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // POST /Admin/Borrow/Delete/{id}
    // ─────────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var error = await _borrowService.DeleteAsync(id);
            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa phiếu mượn thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}

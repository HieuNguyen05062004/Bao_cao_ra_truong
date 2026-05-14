using Admin.ViewModels;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class BorrowController : Controller
{
    private readonly IBorrowService _borrowService;
    private readonly IBookService _bookService;

    public BorrowController(IBorrowService borrowService, IBookService bookService)
    {
        _borrowService = borrowService;
        _bookService = bookService;
    }

    /// <summary>
    /// Danh sách tất cả yêu cầu mượn sách
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string status = "")
    {
        try
        {
            var borrowTickets = await _borrowService.GetAllBorrowTicketsAsync();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Borrowing")
                {
                    // "Đang mượn sách" = Approved và chưa trả (ReturnDate == null)
                    borrowTickets = borrowTickets.Where(b => b.Status == "Approved" && !b.ReturnDate.HasValue).ToList();
                }
                else
                {
                    borrowTickets = borrowTickets.Where(b => b.Status == status).ToList();
                }
            }

            ViewBag.SelectedStatus = status;
            ViewBag.StatusList = new[] { "Pending", "Approved", "Returned", "Rejected", "Borrowing" };

            return View(borrowTickets);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tải danh sách mượn sách: " + ex.Message;
            return View(new List<Core.Shared.Entities.BorrowTicket>());
        }
    }

    /// <summary>
    /// Chi tiết yêu cầu mượn sách
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var borrowTicket = await _borrowService.GetBorrowTicketByIdAsync(id);
            if (borrowTicket == null)
                return NotFound();

            return View(borrowTicket);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Duyệt yêu cầu mượn sách (Approve)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            // TODO: Lấy username từ User.Identity
            var staffUsername = User.Identity?.Name ?? "admin";

            var (success, message) = await _borrowService.ApproveBorrowRequestAsync(id, staffUsername);

            if (success)
            {
                TempData["SuccessMessage"] = "Yêu cầu mượn đã được duyệt. Số lượng sách đã cập nhật.";
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Từ chối yêu cầu mượn sách (Reject)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason = "")
    {
        try
        {
            var (success, message) = await _borrowService.RejectBorrowRequestAsync(id, reason);

            if (success)
            {
                TempData["SuccessMessage"] = "Yêu cầu mượn đã bị từ chối.";
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Xác nhận trả sách (Return)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnBooks(int id)
    {
        try
        {
            var (success, message) = await _borrowService.ReturnBooksAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Sách đã được trả thành công. Số lượng sách đã cập nhật.";
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

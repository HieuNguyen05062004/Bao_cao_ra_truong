using LibraryManagement.Models;
using LibraryManagement.Services;
using LibraryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class BorrowController : Controller
    {
        private readonly IBorrowService _borrowService;
        private readonly IBookService _bookService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BorrowController(IBorrowService borrowService, IBookService bookService, UserManager<ApplicationUser> userManager)
        {
            _borrowService = borrowService;
            _bookService = bookService;
            _userManager = userManager;
        }

        // Admin/Staff: View all borrow records
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var records = await _borrowService.GetAllAsync();
            return View(records);
        }

        // Reader: View their own borrows
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> MyBorrows()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var records = await _borrowService.GetByUserAsync(user.Id);
            return View(records);
        }

        public async Task<IActionResult> Details(int id)
        {
            var record = await _borrowService.GetByIdAsync(id);
            if (record == null) return NotFound();

            // Readers can only see their own records
            if (User.IsInRole("Reader"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (record.UserId != user?.Id) return Forbid();
            }
            return View(record);
        }

        [HttpGet]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var book = await _bookService.GetByIdAsync(bookId);
            if (book == null) return NotFound();

            if (!await _bookService.IsAvailableAsync(bookId))
            {
                TempData["Error"] = "Sách hiện không còn đủ số lượng để mượn";
                return RedirectToAction("Index", "Search");
            }

            var model = new BorrowRequestViewModel
            {
                BookId = bookId,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BorrowDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> Borrow(BorrowRequestViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            try
            {
                await _borrowService.BorrowBookAsync(user.Id, model.BookId, model.BorrowDate, model.DueDate, model.Notes);
                TempData["Success"] = $"Mượn sách '{model.BookTitle}' thành công! Hạn trả: {model.DueDate:dd/MM/yyyy}";
                return RedirectToAction(nameof(MyBorrows));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Return(int id)
        {
            var record = await _borrowService.GetByIdAsync(id);
            if (record == null) return NotFound();
            if (record.Status != BorrowStatus.Borrowing)
            {
                TempData["Error"] = "Sách này đã được trả rồi";
                return RedirectToAction(nameof(Index));
            }
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Return(int id, string? notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            try
            {
                var record = await _borrowService.ReturnBookAsync(id, user.Id, notes);
                string statusMsg = record.Status == BorrowStatus.Overdue ? "quá hạn" : "đúng hạn";
                TempData["Success"] = $"Xác nhận trả sách thành công ({statusMsg})!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Overdue()
        {
            var records = await _borrowService.GetOverdueAsync();
            return View(records);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(int? bookId)
        {
            var books = await _bookService.GetAllAsync();
            ViewBag.Books = books.Where(b => b.AvailableQuantity > 0).ToList();

            var readerRole = await _userManager.GetUsersInRoleAsync("Reader");
            ViewBag.Readers = readerRole.OrderBy(u => u.FullName).ToList();

            var model = new AdminBorrowCreateViewModel
            {
                BookId = bookId ?? 0,
                BorrowDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(AdminBorrowCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var books = await _bookService.GetAllAsync();
                ViewBag.Books = books.Where(b => b.AvailableQuantity > 0).ToList();
                ViewBag.Readers = (await _userManager.GetUsersInRoleAsync("Reader"))
                    .OrderBy(u => u.FullName).ToList();
                return View(model);
            }

            try
            {
                var record = await _borrowService.BorrowBookAsync(
                    model.UserId, model.BookId, model.BorrowDate, model.DueDate, model.Notes);
                var book = await _bookService.GetByIdAsync(model.BookId);
                TempData["Success"] = $"Tạo phiếu mượn sách '{book?.Title}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var books = await _bookService.GetAllAsync();
                ViewBag.Books = books.Where(b => b.AvailableQuantity > 0).ToList();
                ViewBag.Readers = (await _userManager.GetUsersInRoleAsync("Reader"))
                    .OrderBy(u => u.FullName).ToList();
                return View(model);
            }
        }
    }
}

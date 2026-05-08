using Admin.ViewModels;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Admin.Controllers
{
    /// <summary>
    /// Controller quản lý sách (Admin/Nhân viên)
    /// </summary>
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Trang danh sách sách
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            try
            {
                var books = string.IsNullOrEmpty(searchTerm)
                    ? await _bookService.GetAllBooksAsync()
                    : await _bookService.SearchBooksAsync(searchTerm);

                ViewBag.SearchTerm = searchTerm;
                return View(books);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách sách: " + ex.Message;
                return View(new List<Book>());
            }
        }

        /// <summary>
        /// Trang chi tiết sách
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(Index));

            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách cần xem chi tiết";
                    return RedirectToAction(nameof(Index));
                }

                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết sách: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Trang thêm sách mới
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
                return View(new BookViewModel());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu thể loại: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xử lý thêm sách mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
                return View(model);
            }

            try
            {
                // Chuyển đổi ViewModel sang Entity
                var book = new Book
                {
                    BookId = model.BookId?.Trim(),
                    Title = model.Title?.Trim(),
                    Author = model.Author?.Trim(),
                    Publisher = model.Publisher?.Trim(),
                    PublishYear = model.PublishYear,
                    CategoryId = model.CategoryId,
                    Quantity = model.Quantity ?? 0,
                    Status = model.Status ?? "Có thể mượn",
                    ImageUrl = model.ImageUrl
                };

                var (success, message) = await _bookService.AddBookAsync(book);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                    var categories = await _bookService.GetAllCategoriesAsync();
                    ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm sách: " + ex.Message;
                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
                return View(model);
            }
        }

        /// <summary>
        /// Trang sửa thông tin sách
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(Index));

            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách cần sửa";
                    return RedirectToAction(nameof(Index));
                }

                var model = new BookViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Publisher = book.Publisher,
                    PublishYear = book.PublishYear,
                    CategoryId = book.CategoryId,
                    Quantity = book.Quantity,
                    Status = book.Status,
                    ImageUrl = book.ImageUrl,
                    CategoryName = book.Category?.CategoryName
                };

                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", book.CategoryId);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu sách: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xử lý cập nhật thông tin sách
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, BookViewModel model)
        {
            if (id != model.BookId)
                return BadRequest("Mã sách không khớp");

            if (!ModelState.IsValid)
            {
                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
                return View(model);
            }

            try
            {
                var book = new Book
                {
                    BookId = model.BookId,
                    Title = model.Title?.Trim(),
                    Author = model.Author?.Trim(),
                    Publisher = model.Publisher?.Trim(),
                    PublishYear = model.PublishYear,
                    CategoryId = model.CategoryId,
                    Quantity = model.Quantity ?? 0,
                    Status = model.Status ?? "Có thể mượn",
                    ImageUrl = model.ImageUrl
                };

                var (success, message) = await _bookService.UpdateBookAsync(book);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Details), new { id = model.BookId });
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                    var categories = await _bookService.GetAllCategoriesAsync();
                    ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật sách: " + ex.Message;
                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
                return View(model);
            }
        }

        /// <summary>
        /// Trang xác nhận xóa sách
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(Index));

            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sách cần xóa";
                    return RedirectToAction(nameof(Index));
                }

                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu sách: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xử lý xóa sách
        /// </summary>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(Index));

            try
            {
                var (success, message) = await _bookService.DeleteBookAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa sách: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Tìm kiếm sách (API)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return Json(new List<Book>());

            try
            {
                var books = await _bookService.SearchBooksAsync(term);
                return Json(books);
            }
            catch
            {
                return Json(new List<Book>());
            }
        }
    }
}

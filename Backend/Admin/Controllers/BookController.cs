using Admin.ViewModels;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Admin.Controllers
{
    /// <summary>
    /// Controller quản lý sách (Admin/Nhân viên)
    /// </summary>
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BookController(
    IBookService bookService,
    IWebHostEnvironment webHostEnvironment)
        {
            _bookService = bookService;
            _webHostEnvironment = webHostEnvironment;
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
                string imagePath = string.Empty;

                if (model.ImageFile != null)
                {
                    string uploadFolder = Path.Combine(
    Directory.GetCurrentDirectory(),
    "..",
    "Core.Shared",
    "Uploads",
    "books"
);

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string fileName = Guid.NewGuid().ToString()
                        + Path.GetExtension(model.ImageFile.FileName);

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    imagePath = "/book-images/" + fileName;
                }

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
                    ImageUrl = imagePath
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
                string imagePath = model.ImageUrl ?? string.Empty;

                // Nếu chọn ảnh mới
                if (model.ImageFile != null)
                {
                    string uploadFolder = Path.Combine(
    Directory.GetCurrentDirectory(),
    "..",
    "Core.Shared",
    "Uploads",
    "books"
);

                    // Tạo folder nếu chưa có
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Tạo tên file ngẫu nhiên
                    string fileName = Guid.NewGuid().ToString()
                        + Path.GetExtension(model.ImageFile.FileName);

                    // Đường dẫn vật lý
                    string filePath = Path.Combine(uploadFolder, fileName);

                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    // Đường dẫn lưu DB
                    imagePath = "/book-images/" + fileName;
                }

                // Tạo object Book
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
                    ImageUrl = imagePath
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

                    ViewBag.Categories = new SelectList(
                        categories,
                        "CategoryId",
                        "CategoryName",
                        model.CategoryId
                    );

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật sách: " + ex.Message;

                var categories = await _bookService.GetAllCategoriesAsync();

                ViewBag.Categories = new SelectList(
                    categories,
                    "CategoryId",
                    "CategoryName",
                    model.CategoryId
                );

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

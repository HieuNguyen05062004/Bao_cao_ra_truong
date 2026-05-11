using Admin.ViewModels;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

public class BookController : Controller
{
    private readonly IBookService _bookService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BookController(IBookService bookService, IWebHostEnvironment webHostEnvironment)
    {
        _bookService = bookService;
        _webHostEnvironment = webHostEnvironment;
    }

    // ─── INDEX ───────────────────────────────────────────────────────────────

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

    // ─── DETAILS ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

        try
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách cần xem chi tiết.";
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

    // ─── CREATE ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            return View(new BookViewModel
            {
                Categories = await _bookService.GetAllCategoriesAsync()
            });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tải danh mục: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _bookService.GetAllCategoriesAsync();
            return View(model);
        }

        try
        {
            var book = new Book
            {
                BookId = model.BookId?.Trim(),
                Title = model.Title?.Trim(),
                Author = model.Author?.Trim(),
                Publisher = model.Publisher?.Trim(),
                PublishYear = model.PublishYear,
                Quantity = model.Quantity ?? 0,
                Status = model.Status ?? "Có thể mượn",
                ImageUrl = model.ImageFile != null
                                ? await SaveImageAsync(model.ImageFile)
                                : string.Empty
            };

            // Truyền CategoryIds (danh sách từ chips) vào Service
            var (success, message) = await _bookService.AddBookAsync(book, model.CategoryIds);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = message;
            model.Categories = await _bookService.GetAllCategoriesAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi thêm sách: " + ex.Message;
            model.Categories = await _bookService.GetAllCategoriesAsync();
            return View(model);
        }
    }

    // ─── EDIT ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

        try
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách cần sửa.";
                return RedirectToAction(nameof(Index));
            }

            var model = new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Publisher = book.Publisher,
                PublishYear = book.PublishYear,
                Quantity = book.Quantity,
                Status = book.Status,
                ImageUrl = book.ImageUrl,
                Categories = await _bookService.GetAllCategoriesAsync(),

                // Lấy danh sách ID từ bảng trung gian BookCategories để highlight chips
                CategoryIds = book.BookCategories.Select(bc => bc.CategoryId).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu sách: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, BookViewModel model)
    {
        if (id != model.BookId) return BadRequest("Mã sách không khớp.");

        if (!ModelState.IsValid)
        {
            model.Categories = await _bookService.GetAllCategoriesAsync();
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
                Quantity = model.Quantity ?? 0,
                Status = model.Status ?? "Có thể mượn",
                ImageUrl = model.ImageFile != null
                                ? await SaveImageAsync(model.ImageFile)
                                : model.ImageUrl ?? string.Empty
            };

            var (success, message) = await _bookService.UpdateBookAsync(book, model.CategoryIds);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Details), new { id = model.BookId });
            }

            TempData["ErrorMessage"] = message;
            model.Categories = await _bookService.GetAllCategoriesAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi cập nhật sách: " + ex.Message;
            model.Categories = await _bookService.GetAllCategoriesAsync();
            return View(model);
        }
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

        try
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách cần xóa.";
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

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

        try
        {
            var (success, message) = await _bookService.DeleteBookAsync(id);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi xóa sách: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // ─── SEARCH (API) ─────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        if (string.IsNullOrEmpty(term) || term.Length < 2)
            return Json(new List<Book>());
        try
        {
            return Json(await _bookService.SearchBooksAsync(term));
        }
        catch { return Json(new List<Book>()); }
    }

    // ─── PRIVATE HELPER ──────────────────────────────────────────────────────

    private async Task<string> SaveImageAsync(IFormFile imageFile)
    {
        string uploadFolder = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(), "..", "Core.Shared", "Uploads", "books"));

        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        using var stream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create);
        await imageFile.CopyToAsync(stream);

        return "/book-images/" + fileName;
    }
}

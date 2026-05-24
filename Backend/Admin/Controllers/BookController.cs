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
            ViewBag.Categories = await _bookService.GetAllCategoriesAsync();
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
        // ── Validate thủ công IFormFile & CategoryIds ──────────────────
        if (model.ImageFile == null || model.ImageFile.Length == 0)
            ModelState.AddModelError(nameof(model.ImageFile),
                "Vui lòng tải lên hình ảnh bìa sách.");

        if (model.CategoryIds == null || !model.CategoryIds.Any())
            ModelState.AddModelError(nameof(model.CategoryIds),
                "Vui lòng chọn ít nhất một thể loại sách.");
        // ──────────────────────────────────────────────────────────────

        if (!ModelState.IsValid)
        {
            model.Categories = await _bookService.GetAllCategoriesAsync();
            return View(model);
        }

        try
        {
            var book = new Book
            {
                BookId = null,
                Title = model.Title?.Trim(),
                Author = model.Author?.Trim(),
                Publisher = model.Publisher?.Trim(),
                PublishYear = model.PublishYear,
                Quantity = model.Quantity ?? 0,
                Status = model.Status ?? "Có thể mượn",
                Description = model.Description?.Trim(),
                CreatedAt = DateTime.Now,
                ImageUrl = await SaveImageAsync(model.ImageFile!)
            };

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

    // ─── CREATE AJAX ─────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAjax([FromForm] BookAjaxModel model)
    {
        try
        {
            var book = new Book
            {
                BookId = null,
                Title = model.Title?.Trim(),
                Author = model.Author?.Trim(),
                Publisher = model.Publisher?.Trim(),
                PublishYear = model.PublishYear,
                Quantity = model.Quantity ?? 0,
                Status = string.IsNullOrWhiteSpace(model.Status)
                    ? "Có thể mượn"
                    : model.Status,
                Description = model.Description?.Trim(),
                CreatedAt = DateTime.Now,
                ImageUrl = model.ImageFile != null
                    ? await SaveImageAsync(model.ImageFile)
                    : string.Empty
            };

            var categoryIds = model.CategoryIds ?? new List<int>();
            var (success, message) = await _bookService.AddBookAsync(book, categoryIds);
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
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
                Description = book.Description,
                ImageUrl = book.ImageUrl,
                Categories = await _bookService.GetAllCategoriesAsync(),
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

        // ── Validate thủ công CategoryIds (ImageFile không bắt buộc khi Edit) ──
        if (model.CategoryIds == null || !model.CategoryIds.Any())
            ModelState.AddModelError(nameof(model.CategoryIds),
                "Vui lòng chọn ít nhất một thể loại sách.");
        // ──────────────────────────────────────────────────────────────────────

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
                Description = model.Description?.Trim(),
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

    // ─── EDIT AJAX ───────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAjax([FromForm] BookAjaxModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.BookId))
                return Json(new { success = false, message = "Mã sách không hợp lệ." });

            var book = new Book
            {
                BookId = model.BookId,
                Title = model.Title?.Trim(),
                Author = model.Author?.Trim(),
                Publisher = model.Publisher?.Trim(),
                PublishYear = model.PublishYear,
                Quantity = model.Quantity ?? 0,
                Status = string.IsNullOrWhiteSpace(model.Status)
                    ? "Có thể mượn"
                    : model.Status,
                Description = model.Description?.Trim(),
                ImageUrl = model.ImageFile != null
                    ? await SaveImageAsync(model.ImageFile)
                    : model.ImageUrl ?? string.Empty
            };

            var categoryIds = model.CategoryIds ?? new List<int>();
            var (success, message) = await _bookService.UpdateBookAsync(book, categoryIds);
            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
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

    // ─── DELETE MANY ─────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMany(List<string> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            TempData["ErrorMessage"] = "Không có sách nào được chọn để xóa.";
            return RedirectToAction(nameof(Index));
        }

        int successCount = 0;
        var failedIds = new List<string>();

        foreach (var id in ids)
        {
            var (success, _) = await _bookService.DeleteBookAsync(id);
            if (success) successCount++;
            else failedIds.Add(id);
        }

        if (failedIds.Count == 0)
            TempData["SuccessMessage"] = $"Đã xóa thành công {successCount} cuốn sách.";
        else
            TempData["ErrorMessage"] =
                $"Xóa được {successCount}/{ids.Count} sách. " +
                $"Không thể xóa: {string.Join(", ", failedIds)} (có thể đang được mượn).";

        return RedirectToAction(nameof(Index));
    }

    // ─── SEARCH API ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        if (string.IsNullOrEmpty(term) || term.Length < 2)
            return Json(new List<Book>());
        try
        {
            return Json(await _bookService.SearchBooksAsync(term));
        }
        catch
        {
            return Json(new List<Book>());
        }
    }

    // ─── PRIVATE HELPER ──────────────────────────────────────────────────────

    private async Task<string> SaveImageAsync(IFormFile imageFile)
    {
        string uploadFolder = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(), "..", "Core.Shared", "Uploads", "books"));

        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        string filePath = Path.Combine(uploadFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        return "/book-images/" + fileName;
    }
}

// ─── MODEL BIND CHO AJAX ─────────────────────────────────────────────────────

public class BookAjaxModel
{
    public string? BookId { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public int? PublishYear { get; set; }
    public int? Quantity { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public List<int>? CategoryIds { get; set; }
}
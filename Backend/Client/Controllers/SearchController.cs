using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

/// <summary>
/// Controller tìm kiếm sách cho bạn đọc.
/// Hỗ trợ 2 mode: tìm kiếm thường (GET Index) và tìm kiếm AI (POST AiSearch).
/// </summary>
public class SearchController : Controller
{
    private readonly IBookService _bookService;
    private readonly IAiSearchService _aiSearchService;

    public SearchController(IBookService bookService, IAiSearchService aiSearchService)
    {
        _bookService = bookService;
        _aiSearchService = aiSearchService;
    }

    // ── Tìm kiếm thường ──────────────────────────────────────────────────────

    /// <summary>
    /// Trang tìm kiếm và danh sách sách (không đổi so với bản cũ).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm = "", int categoryId = 0)
    {
        try
        {
            List<Book> books;

            if (!string.IsNullOrEmpty(searchTerm))
                books = await _bookService.SearchBooksAsync(searchTerm);
            else if (categoryId > 0)
                books = await _bookService.GetBooksByCategoryAsync(categoryId);
            else
                books = await _bookService.GetAvailableBooksAsync();

            await PopulateViewBagAsync(searchTerm, categoryId);
            return View(books);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tải danh sách sách: " + ex.Message;
            return View(new List<Book>());
        }
    }

    // ── Tìm kiếm AI ──────────────────────────────────────────────────────────

    /// <summary>
    /// Nhận câu tự nhiên từ form AI, gọi Claude phân tích,
    /// rồi dùng keyword trả về để tìm trong database.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AiSearch(string aiQuery)
    {
        if (string.IsNullOrWhiteSpace(aiQuery))
            return RedirectToAction(nameof(Index));

        try
        {
            // 1. Gọi AI phân tích câu hỏi
            var aiResult = await _aiSearchService.ParseSearchQueryAsync(aiQuery);

            // 2. Dùng keyword đã được AI tinh lọc để tìm trong DB
            var books = await _bookService.SearchBooksAsync(aiResult.Keyword);

            // 3. Truyền thông tin AI xuống view
            await PopulateViewBagAsync(aiResult.Keyword, 0);
            ViewBag.AiInterpretedQuery = aiResult.InterpretedQuery;
            ViewBag.OriginalAiQuery = aiQuery;
            ViewBag.IsAiSearch = true;

            // Nếu AI fallback về tìm kiếm thường thì không hiển thị nhãn AI
            if (!aiResult.IsSuccess)
                ViewBag.IsAiSearch = false;

            return View("Index", books);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tìm kiếm: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // ── Các action cũ giữ nguyên ─────────────────────────────────────────────

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
                TempData["ErrorMessage"] = "Không tìm thấy sách";
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

    [HttpGet]
    public async Task<IActionResult> FilterByCategory(int categoryId)
    {
        try
        {
            var books = categoryId > 0
                ? await _bookService.GetBooksByCategoryAsync(categoryId)
                : await _bookService.GetAvailableBooksAsync();
            return Json(books);
        }
        catch { return Json(new List<Book>()); }
    }

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

    // ── Helper ───────────────────────────────────────────────────────────────

    private async Task PopulateViewBagAsync(string searchTerm, int categoryId)
    {
        ViewBag.Categories = await _bookService.GetAllCategoriesAsync();
        ViewBag.SearchTerm = searchTerm;
        ViewBag.SelectedCategory = categoryId;
    }
}

using Client.ViewModels;
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

    private const int PageSize = 9;

    public SearchController(IBookService bookService, IAiSearchService aiSearchService)
    {
        _bookService = bookService;
        _aiSearchService = aiSearchService;
    }

    // ── Tìm kiếm thường ──────────────────────────────────────────────────────

    /// <summary>
    /// Trang danh sách sách với bộ lọc đa danh mục, tìm kiếm và phân trang.
    /// categoryIds: List[int] — ASP.NET Core tự bind từ ?categoryIds=1&categoryIds=3
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        List<int> categoryIds,
        string keyword = "",
        string sort = "newest",
        int page = 1)
    {
        try
        {
            // 1. Lấy sách theo điều kiện lọc
            List<Book> books;

            if (!string.IsNullOrWhiteSpace(keyword))
                books = await _bookService.SearchBooksAsync(keyword.Trim());
            else if (categoryIds.Any())
                books = await _bookService.GetBooksByMultipleCategoriesAsync(categoryIds);
            else
                books = await _bookService.GetAllBooksAsync();

            // 2. Sắp xếp
            books = sort == "oldest"
                ? books.OrderBy(b => b.PublishYear).ToList()
                : books.OrderByDescending(b => b.PublishYear).ToList();

            // 3. Phân trang
            int total = books.Count;
            int totalPgs = (int)Math.Ceiling(total / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPgs)));
            var paged = books.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            // 4. Build ViewModel
            var vm = new BookListViewModel
            {
                Books = paged,
                Categories = await _bookService.GetAllCategoriesAsync(),
                SelectedCategoryIds = categoryIds,
                Keyword = keyword,
                Sort = sort,
                CurrentPage = page,
                TotalPages = totalPgs == 0 ? 1 : totalPgs,
                TotalCount = total,
                PageSize = PageSize,
            };

            return View(vm);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tải danh sách sách: " + ex.Message;
            return View(new BookListViewModel
            {
                Categories = await _bookService.GetAllCategoriesAsync()
            });
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

            // 3. Sắp xếp mặc định newest
            books = books.OrderByDescending(b => b.PublishYear).ToList();

            // 4. Phân trang
            int total = books.Count;
            int totalPgs = (int)Math.Ceiling(total / (double)PageSize);
            var paged = books.Take(PageSize).ToList();

            // 5. Build ViewModel
            var vm = new BookListViewModel
            {
                Books = paged,
                Categories = await _bookService.GetAllCategoriesAsync(),
                SelectedCategoryIds = new List<int>(),
                Keyword = aiResult.Keyword,
                Sort = "newest",
                CurrentPage = 1,
                TotalPages = totalPgs == 0 ? 1 : totalPgs,
                TotalCount = total,
                PageSize = PageSize,
                AiInterpretedQuery = aiResult.InterpretedQuery,
                OriginalAiQuery = aiQuery,
                IsAiSearch = aiResult.IsSuccess,
            };

            return View("Index", vm);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi tìm kiếm: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // ── Chi tiết sách ─────────────────────────────────────────────────────────

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

    // ── API endpoints (AJAX) ──────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> FilterByCategory(int categoryId)
    {
        try
        {
            var books = categoryId > 0
                ? await _bookService.GetBooksByCategoryAsync(categoryId)
                : await _bookService.GetAllBooksAsync();
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
}

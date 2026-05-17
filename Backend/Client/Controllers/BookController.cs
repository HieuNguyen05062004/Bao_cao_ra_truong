using Client.Extensions;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

public class BookController : ClientBaseController
{
    private readonly IBookService _bookService;
    private readonly ICategoryService _categoryService;

    public BookController(IBookService bookService, ICategoryService categoryService)
    {
        _bookService = bookService;
        _categoryService = categoryService;
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /Book/Index?keyword=&categoryId=
    // Danh sách sách — tìm kiếm + lọc danh mục
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(string keyword = "", int categoryId = 0, int page = 1)
    {
        const int pageSize = 12;

        var categories = await _categoryService.GetAllCategoriesAsync();

        var books = !string.IsNullOrWhiteSpace(keyword)
            ? await _bookService.SearchBooksAsync(keyword.Trim())
            : categoryId > 0
                ? await _bookService.GetBooksByCategoryAsync(categoryId)
                : await _bookService.GetAllBooksAsync();

        int total = books.Count;
        int totalPages = (int)Math.Ceiling(total / (double)pageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        var paged = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Categories = categories;
        ViewBag.CategoryId = categoryId;
        ViewBag.Keyword = keyword;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
        ViewBag.TotalCount = total;

        return View(paged);
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET /Book/Details/{id}
    // Chi tiết sách + nút Mượn sách
    // ─────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
            return RedirectToAction(nameof(Index));

        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy sách.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.IsLoggedIn = HttpContext.Session.IsReaderLoggedIn();
        return View(book);
    }
}

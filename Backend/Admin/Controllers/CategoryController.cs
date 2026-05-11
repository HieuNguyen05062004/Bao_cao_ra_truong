using Admin.ViewModels;
using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

/// <summary>
/// Controller xử lý toàn bộ HTTP request (yêu cầu HTTP) liên quan đến danh mục sách.
/// Nhận request → gọi Service → trả kết quả về View.
/// Không chứa logic nghiệp vụ — việc đó là của Service.
/// </summary>
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    // Dependency Injection (tiêm phụ thuộc): ASP.NET Core tự truyền ICategoryService vào
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ─── INDEX ───────────────────────────────────────────────────────────────

    /// <summary>
    /// GET: /Category
    /// GET: /Category?keyword=cntt
    /// Hiển thị danh sách danh mục, hỗ trợ tìm kiếm theo keyword.
    /// </summary>
    public async Task<IActionResult> Index(string? keyword)
    {
        var categories = await _categoryService.SearchCategoriesAsync(keyword ?? string.Empty);

        // Map (ánh xạ) từ Entity → ViewModel
        var viewModel = new CategoryIndexViewModel
        {
            Keyword = keyword,
            Categories = categories.Select(c => new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            }).ToList(),

            // Lấy thông báo từ TempData (dữ liệu tạm) do redirect trước truyền lại
            SuccessMessage = TempData["SuccessMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string
        };

        return View(viewModel);
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    /// <summary>GET: /Category/Create — Hiển thị form thêm danh mục.</summary>
    public IActionResult Create()
    {
        return View(new CategoryViewModel());
    }

    /// <summary>
    /// POST: /Category/Create
    /// Nhận dữ liệu form, validate, gọi Service để thêm danh mục mới.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF (Cross-Site Request Forgery)
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        // ModelState.IsValid: kiểm tra Data Annotations trên ViewModel có thỏa mãn không
        if (!ModelState.IsValid)
            return View(model);

        var entity = new Category
        {
            CategoryName = model.CategoryName
        };

        var (success, message) = await _categoryService.AddCategoryAsync(entity);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // Thêm lỗi từ Service vào ModelState để hiển thị trên View
        ModelState.AddModelError(string.Empty, message);
        return View(model);
    }

    // ─── EDIT ────────────────────────────────────────────────────────────────

    /// <summary>
    /// GET: /Category/Edit/5
    /// Lấy dữ liệu danh mục hiện tại, đổ vào form để chỉnh sửa.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CategoryViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName
        };

        return View(model);
    }

    /// <summary>
    /// POST: /Category/Edit/5
    /// Nhận dữ liệu đã sửa, validate, gọi Service để cập nhật.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        // Đảm bảo ID trên URL khớp với ID trong form (chống giả mạo)
        if (id != model.CategoryId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var entity = new Category
        {
            CategoryId = model.CategoryId,
            CategoryName = model.CategoryName
        };

        var (success, message) = await _categoryService.UpdateCategoryAsync(entity);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        return View(model);
    }

    // ─── DETAILS ─────────────────────────────────────────────────────────────

    /// <summary>
    /// GET: /Category/Details/5
    /// Hiển thị thông tin chi tiết danh mục kèm danh sách sách thuộc danh mục đó.
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var category = await _categoryService.GetCategoryWithBooksAsync(id);

        if (category == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new CategoryDetailsViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Books = category.BookCategories.Select(bc => new BookSummaryViewModel
            {
                BookId = bc.Book.BookId,
                Title = bc.Book.Title,
                Author = bc.Book.Author,
                Status = bc.Book.Status
            }).ToList()
        };

        return View(viewModel);
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    /// <summary>
    /// GET: /Category/Delete/5
    /// Hiển thị trang xác nhận xóa với thông tin danh mục và số sách còn lại.
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetCategoryWithBooksAsync(id);

        if (category == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new CategoryDeleteViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            BookCount = category.BookCategories.Count
        };

        return View(viewModel);
    }

    /// <summary>
    /// POST: /Category/Delete/5
    /// Xóa danh mục sau khi admin đã xác nhận trên trang Delete.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (success, message) = await _categoryService.DeleteCategoryAsync(id);

        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index));
    }
}

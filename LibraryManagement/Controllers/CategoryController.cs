using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            try
            {
                await _categoryService.CreateAsync(category);
                TempData["Success"] = $"Thêm danh mục '{category.Name}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(category);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return BadRequest();
            if (!ModelState.IsValid) return View(category);

            try
            {
                await _categoryService.UpdateAsync(category);
                TempData["Success"] = $"Cập nhật danh mục '{category.Name}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                TempData["Success"] = "Xóa danh mục thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một danh mục để xóa";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var idList = ids.Split(',').Select(int.Parse);
                await _categoryService.DeleteMultipleAsync(idList);
                TempData["Success"] = "Xóa danh mục thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

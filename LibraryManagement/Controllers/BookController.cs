using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookController(IBookService bookService, ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _bookService = bookService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllAsync();
            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new Book());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(book);
            }

            try
            {
                await _bookService.CreateAsync(book);
                TempData["Success"] = $"Thêm sách '{book.Title}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateCategoriesAsync();
                return View(book);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null) return NotFound();
            await PopulateCategoriesAsync(book.CategoryId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(book.CategoryId);
                return View(book);
            }

            try
            {
                await _bookService.UpdateAsync(book);
                TempData["Success"] = $"Cập nhật sách '{book.Title}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateCategoriesAsync(book.CategoryId);
                return View(book);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _bookService.DeleteAsync(id);
                TempData["Success"] = "Xóa sách thành công!";
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
                TempData["Error"] = "Vui lòng chọn ít nhất một sách để xóa";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var idList = ids.Split(',').Select(int.Parse);
                await _bookService.DeleteMultipleAsync(idList);
                TempData["Success"] = "Xóa sách thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync(int? selectedId = null)
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }
    }
}

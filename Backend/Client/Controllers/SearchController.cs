using Core.Shared.Entities;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Client.Controllers
{
    /// <summary>
    /// Controller tìm kiếm sách cho bạn đọc
    /// </summary>
    public class SearchController : Controller
    {
        private readonly IBookService _bookService;

        public SearchController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Trang tìm kiếm và danh sách sách
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", int categoryId = 0)
        {
            try
            {
                List<Book> books;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    books = await _bookService.SearchBooksAsync(searchTerm);
                }
                else if (categoryId > 0)
                {
                    books = await _bookService.GetBooksByCategoryAsync(categoryId);
                }
                else
                {
                    books = await _bookService.GetAvailableBooksAsync();
                }

                var categories = await _bookService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedCategory = categoryId;

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

        /// <summary>
        /// Lọc sách theo thể loại (API)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FilterByCategory(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    var allBooks = await _bookService.GetAvailableBooksAsync();
                    return Json(allBooks);
                }

                var books = await _bookService.GetBooksByCategoryAsync(categoryId);
                return Json(books);
            }
            catch
            {
                return Json(new List<Book>());
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

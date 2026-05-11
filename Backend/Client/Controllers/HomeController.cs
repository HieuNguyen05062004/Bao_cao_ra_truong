using Client.Models;
using Core.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client.Controllers
{
    /// <summary>
    /// Controller trang chủ cho bạn đọc
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookService _bookService;

        public HomeController(ILogger<HomeController> logger, IBookService bookService)
        {
            _logger = logger;
            _bookService = bookService;
        }

        /// <summary>
        /// Trang chủ - hiển thị sách còn hàng
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var books = await _bookService.GetAvailableBooksAsync();
                var categories = await _bookService.GetAllCategoriesAsync();
                var featuredBooks = await _bookService.GetFeaturedBooksAsync(5);
                var trendingBooks = await _bookService.GetTrendingBooksAsync(5);

                ViewBag.Categories = categories;
                ViewBag.FeaturedBooks = featuredBooks;
                ViewBag.TrendingBooks = trendingBooks;
                return View(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chủ");
                TempData["ErrorMessage"] = "Lỗi khi tải trang chủ";
                return View(new List<Core.Shared.Entities.Book>());
            }
        }

        /// <summary>
        /// Trang chính sách bảo mật
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Trang lỗi
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


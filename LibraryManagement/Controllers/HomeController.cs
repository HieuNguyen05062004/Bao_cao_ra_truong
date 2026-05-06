using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBookService _bookService;
    private readonly ICategoryService _categoryService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, IBookService bookService,
        ICategoryService categoryService, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _bookService = bookService;
        _categoryService = categoryService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllAsync();
        var categories = await _categoryService.GetAllAsync();
        ViewBag.LatestBooks = books.OrderByDescending(b => b.CreatedAt).Take(6).ToList();
        ViewBag.Categories = categories;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

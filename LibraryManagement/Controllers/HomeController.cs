using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Identity;
using LibraryManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBookService _bookService;
    private readonly ICategoryService _categoryService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, IBookService bookService,
        ICategoryService categoryService, UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _logger = logger;
        _bookService = bookService;
        _categoryService = categoryService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllAsync();
        var categories = await _categoryService.GetAllAsync();
        ViewBag.LatestBooks = books.OrderByDescending(b => b.CreatedAt).Take(6).ToList();
        ViewBag.Categories = categories;

        // Dashboard statistics
        ViewBag.TotalBooks = books.Count;
        ViewBag.TotalCategories = categories.Count;
        ViewBag.TotalBorrowing = await _context.BorrowRecords
            .CountAsync(br => br.Status == BorrowStatus.Borrowing);
        ViewBag.TotalOverdue = await _context.BorrowRecords
            .CountAsync(br => br.Status == BorrowStatus.Borrowing && br.DueDate < DateTime.Today);

        // Counts by role (single query)
        var roleMap = await _context.Roles
            .Where(r => r.Name == "Reader" || r.Name == "Staff")
            .ToDictionaryAsync(r => r.Name!, r => r.Id);
        var readerRoleId = roleMap.GetValueOrDefault("Reader");
        var staffRoleId = roleMap.GetValueOrDefault("Staff");
        ViewBag.TotalReaders = readerRoleId != null
            ? await _context.UserRoles.CountAsync(ur => ur.RoleId == readerRoleId) : 0;
        ViewBag.TotalStaff = staffRoleId != null
            ? await _context.UserRoles.CountAsync(ur => ur.RoleId == staffRoleId) : 0;

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

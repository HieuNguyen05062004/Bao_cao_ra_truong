using LibraryManagement.Services;
using LibraryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(ISearchService searchService, UserManager<ApplicationUser> userManager)
        {
            _searchService = searchService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Show all books on initial load
            var result = await _searchService.BasicSearchAsync(null, null, null);
            result.SearchType = "initial";
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Basic(string? title, string? author, string? category)
        {
            var result = await _searchService.BasicSearchAsync(title, author, category);
            return View("Index", result);
        }

        [HttpGet]
        public async Task<IActionResult> Ai(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View("Index", new SearchViewModel { SearchType = "ai" });

            var result = await _searchService.AiSearchAsync(query);
            return View("AiSearch", result);
        }

        [HttpPost]
        public async Task<IActionResult> AiSearch(string query)
        {
            var result = await _searchService.AiSearchAsync(query);
            return View("AiSearch", result);
        }
    }
}

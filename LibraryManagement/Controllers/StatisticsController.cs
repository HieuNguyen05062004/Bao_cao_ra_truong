using LibraryManagement.Services;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(IStatisticsService statisticsService, UserManager<ApplicationUser> userManager)
        {
            _statisticsService = statisticsService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var stats = await _statisticsService.GetOverallStatisticsAsync();
            return View(stats);
        }

        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> Personal()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var stats = await _statisticsService.GetPersonalStatisticsAsync(user.Id);
            return View(stats);
        }
    }
}

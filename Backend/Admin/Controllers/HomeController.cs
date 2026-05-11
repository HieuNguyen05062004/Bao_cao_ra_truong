using Admin.Models;
using Core.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Chưa đăng nhập → về Login
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // Truyền thông tin user sang View
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            ViewBag.AvatarUrl = HttpContext.Session.GetString("AvatarUrl");

            return View();
        }

        public IActionResult Privacy()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

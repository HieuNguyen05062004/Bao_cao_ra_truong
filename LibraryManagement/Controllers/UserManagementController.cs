using LibraryManagement.Models;
using LibraryManagement.Services;
using LibraryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? role)
        {
            IList<ApplicationUser> users;
            if (!string.IsNullOrWhiteSpace(role))
                users = await _userManager.GetUsersInRoleAsync(role);
            else
                users = _userManager.Users.OrderBy(u => u.FullName).ToList();

            // Build a dictionary of user ID -> roles for display
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);

            ViewBag.SelectedRole = role;
            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.Roles = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new[] { "Admin", "Staff", "Reader" };
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model, string selectedRole)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new[] { "Admin", "Staff", "Reader" };
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var role = string.IsNullOrWhiteSpace(selectedRole) ? "Reader" : selectedRole;
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
                await _userManager.AddToRoleAsync(user, role);
                TempData["Success"] = $"Tạo tài khoản '{user.FullName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.Roles = new[] { "Admin", "Staff", "Reader" };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                Email = user.Email,
                Roles = roles
            };
            ViewBag.UserId = id;
            ViewBag.AllRoles = new[] { "Admin", "Staff", "Reader" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProfileViewModel model, string newRole)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                ViewBag.AllRoles = new[] { "Admin", "Staff", "Reader" };
                model.Roles = await _userManager.GetRolesAsync(user);
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                // Update role
                if (!string.IsNullOrWhiteSpace(newRole))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!await _roleManager.RoleExistsAsync(newRole))
                        await _roleManager.CreateAsync(new IdentityRole(newRole));
                    await _userManager.AddToRoleAsync(user, newRole);
                }
                TempData["Success"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.UserId = id;
            ViewBag.AllRoles = new[] { "Admin", "Staff", "Reader" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent self-deletion
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "Không thể xóa tài khoản của chính mình";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                TempData["Success"] = "Xóa tài khoản thành công!";
            else
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một tài khoản để xóa";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var idList = ids.Split(',').Where(i => i != currentUser?.Id);

            foreach (var id in idList)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                    await _userManager.DeleteAsync(user);
            }

            TempData["Success"] = "Xóa tài khoản thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = user.IsActive ? "Đã kích hoạt tài khoản" : "Đã vô hiệu hóa tài khoản";
            return RedirectToAction(nameof(Index));
        }
    }
}

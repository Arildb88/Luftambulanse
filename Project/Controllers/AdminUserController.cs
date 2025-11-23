using Gruppe4NLA.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gruppe4NLA.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager,
                                    RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Loads the view from Views/Home/AdminUsers/Adminpage.cshtml
        public async Task<IActionResult> Adminpage()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.AllRoles = await _roleManager.Roles
                                 .Select(r => r.Name!)
                                 .OrderBy(n => n)
                                 .ToListAsync();
            return View("~/Views/Home/AdminUsers/Adminpage.cshtml", users);
        }

        // Set exactly ONE role for a user (replaces any existing roles)
        [HttpPost]
        public async Task<IActionResult> SetRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            // Remove any existing roles (since you allow only one)
            var current = await _userManager.GetRolesAsync(user);
            if (current.Any())
                await _userManager.RemoveFromRolesAsync(user, current);

            // If a role was chosen (not the blank option), add it
            if (!string.IsNullOrWhiteSpace(role))
            {
                // optional: verify role exists
                // if (!await _roleManager.RoleExistsAsync(role)) return BadRequest("Role does not exist.");
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Adminpage));
        }

        // Delete a user with safety checks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            // Find the target user to delete
            var target = await _userManager.FindByIdAsync(userId);
            if (target is null)
            {
                TempData["AdminUsersError"] = "User not found.";
                return RedirectToAction(nameof(Adminpage));
            }

            // Get the currently logged in admin user
            var currentAdmin = await _userManager.GetUserAsync(User);
            if (currentAdmin is null)
            {
                TempData["AdminUsersError"] = "Could not fetch the logged in user.";
                return RedirectToAction(nameof(Adminpage));
            }

            // 1) Admin cant delete himself)
            if (currentAdmin.Id == target.Id)
            {
                TempData["AdminUsersError"] = "You cannot delete yourself.";
                return RedirectToAction(nameof(Adminpage));
            }

            // 2) Do not delete tha last remaining admin
            var isTargetAdmin = await _userManager.IsInRoleAsync(target, "Admin");
            if (isTargetAdmin)
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var otherAdminsCount = admins.Count(u => u.Id != target.Id);
                if (otherAdminsCount == 0)
                {
                    TempData["AdminUsersError"] = "Cannot delete the last Admin user.";
                    return RedirectToAction(nameof(Adminpage));
                }
            }

            // 3) Do the deletion
            var result = await _userManager.DeleteAsync(target);
            if (!result.Succeeded)
            {
                TempData["AdminUsersError"] = "Error on deletion: " + string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Adminpage));
            }

            TempData["AdminUsersMessage"] = $"User {target.Email} was deleted.";
            return RedirectToAction(nameof(Adminpage));
        }
    }
}


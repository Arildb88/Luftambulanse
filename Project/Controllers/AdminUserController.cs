using Gruppe4NLA.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gruppe4NLA.Controllers
{
    // Ensures all requests validate CSRF tokens automatically
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

        // Displays the admin page with all users and available roles
        public async Task<IActionResult> Adminpage()
        {
            var users = await _userManager.Users.ToListAsync();
            
            ViewBag.AllRoles = await _roleManager.Roles
                                 .Select(r => r.Name!)
                                 .OrderBy(n => n)
                                 .ToListAsync();
            
            return View("~/Views/Home/AdminUsers/Adminpage.cshtml", users);
        }

        // Updates a user's role. Replaces any existing role with the new one (single role per user)
        [HttpPost]
        public async Task<IActionResult> SetRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var current = await _userManager.GetRolesAsync(user);
            if (current.Any())
                await _userManager.RemoveFromRolesAsync(user, current);

            if (!string.IsNullOrWhiteSpace(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Adminpage));
        }

        // Deletes a user with safety checks to prevent system lockout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var target = await _userManager.FindByIdAsync(userId);
            if (target is null)
            {
                TempData["AdminUsersError"] = "User not found.";
                return RedirectToAction(nameof(Adminpage));
            }

            var currentAdmin = await _userManager.GetUserAsync(User);
            if (currentAdmin is null)
            {
                TempData["AdminUsersError"] = "Could not fetch the logged in user.";
                return RedirectToAction(nameof(Adminpage));
            }

            // Safety check, prevent self-deletion
            if (currentAdmin.Id == target.Id)
            {
                TempData["AdminUsersError"] = "You cannot delete yourself.";
                return RedirectToAction(nameof(Adminpage));
            }

            // Safety check, prevent deletion of the last admin (avoid system lockout)
            var isTargetAdmin = await _userManager.IsInRoleAsync(target, "Admin");
            if (isTargetAdmin)
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var otherAdminsCount = admins.Count(u => u.Id != target.Id);
                if (otherAdminsCount == 0)
                {
                    TempData["AdminUsersError"] = "Kan ikke slette siste gjenværende admin.";
                    return RedirectToAction(nameof(Adminpage));
                }
            }

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
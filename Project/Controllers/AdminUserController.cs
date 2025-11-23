using Gruppe4NLA.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gruppe4NLA.Controllers
{
    // Ensures all requests validate CSRF tokens automatically
    [AutoValidateAntiforgeryToken]
    // Restricts access to users with Admin role only
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

        /// <summary>
        /// Displays the admin page with all users and available roles
        /// </summary>
        public async Task<IActionResult> Adminpage()
        {
            // Fetch all users from the database
            var users = await _userManager.Users.ToListAsync();
            
            // Get all available roles sorted alphabetically for the dropdown
            ViewBag.AllRoles = await _roleManager.Roles
                                 .Select(r => r.Name!)
                                 .OrderBy(n => n)
                                 .ToListAsync();
            
            return View("~/Views/Home/AdminUsers/Adminpage.cshtml", users);
        }

        /// <summary>
        /// Updates a user's role. Replaces any existing role with the new one (single role per user)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            // Remove all current roles (enforces single-role policy)
            var current = await _userManager.GetRolesAsync(user);
            if (current.Any())
                await _userManager.RemoveFromRolesAsync(user, current);

            // Assign the new role if one was selected
            if (!string.IsNullOrWhiteSpace(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Adminpage));
        }

        /// <summary>
        /// Deletes a user with safety checks to prevent system lockout
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            // Locate the user to be deleted
            var target = await _userManager.FindByIdAsync(userId);
            if (target is null)
            {
                TempData["AdminUsersError"] = "User not found.";
                return RedirectToAction(nameof(Adminpage));
            }

            // Identify the currently logged-in admin
            var currentAdmin = await _userManager.GetUserAsync(User);
            if (currentAdmin is null)
            {
                TempData["AdminUsersError"] = "Could not fetch the logged in user.";
                return RedirectToAction(nameof(Adminpage));
            }

            // Safety check #1: Prevent self-deletion
            if (currentAdmin.Id == target.Id)
            {
                TempData["AdminUsersError"] = "You cannot delete yourself.";
                return RedirectToAction(nameof(Adminpage));
            }

            // Safety check #2: Prevent deletion of the last admin (avoid system lockout)
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

            // Execute the deletion
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
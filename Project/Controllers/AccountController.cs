//using Gruppe4NLA.Areas.Identity.Data;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Gruppe4NLA.Controllers
//{
//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
//    {
//        if (ModelState.IsValid)
//        {
//            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

//            if (result.Succeeded)
//            {
//                var user = await _userManager.FindByEmailAsync(model.Email);
//                var roles = await _userManager.GetRolesAsync(user);

//                if (roles.Contains("Pilot"))
//                    return RedirectToAction("Index", "Map");

//                if (roles.Contains("Admin"))
//                    return RedirectToAction("Index", "Admin");

//                if (roles.Contains("Caseworker"))
//                    return RedirectToAction("Index", "Reports");

//                // default fallback
//                return RedirectToAction("Index", "Home");
//            }

//            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//        }

//        return View(model);
//    }



















//}





// You dont need a AccountController when using Identity with Razor Pages

// If we need to use login functionality we can use:

//@using Gruppe4NLA.Areas.Identity.Data
//@inject SignInManager<ApplicationUser> SignInManager
//@inject UserManager<ApplicationUser> UserManager







//using Gruppe4NLA.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Gruppe4NLA.Areas.Identity.Pages.Account;

//namespace Gruppe4NLA.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly SignInManager<IdentityUser> _signInManager;

//        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//        }

//        [HttpGet]
//        public IActionResult Login()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
//                if (result.Succeeded)
//                {
//                    return RedirectToAction("Index", "Home");
//                }
//                else
//                {
//                    ModelState.AddModelError(string.Empty, "Invalig login attempt. ");
//                }
//            }
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction("Login", "Account");
//        }
//    }
//}

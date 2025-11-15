#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Gruppe4NLA.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gruppe4NLA.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Drop-down data
        public IEnumerable<SelectListItem> RegisterableRoles { get; private set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> AvailableOrganizations { get; private set; } = Array.Empty<SelectListItem>();

        public class InputModel
        {
            [Required, EmailAddress]
            [Display(Name = "*Email")]
            public string Email { get; set; }

            [Required, StringLength(100, ErrorMessage = "Password must be between {2} and {1} characters.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "*Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "*Confirm password")]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }

            
            [Display(Name = "Role, keep blank unless Pilot")]
            public string SelectedRole { get; set; }

            [Display(Name = "Organization (for pilots)")]
            public string SelectedOrganization { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            RegisterableRoles = GetRegisterableRoles();
            AvailableOrganizations = GetAvailableOrganizations();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            RegisterableRoles = GetRegisterableRoles();
            AvailableOrganizations = GetAvailableOrganizations();

            if (!ModelState.IsValid)
                return Page();

            // Require organization if role is Pilot
            if (Input.SelectedRole == "Pilot" && string.IsNullOrWhiteSpace(Input.SelectedOrganization))
            {
                ModelState.AddModelError("Input.SelectedOrganization", "Please select an organization for pilots.");
                return Page();
            }

            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            // Add organization only if Pilot
            if (Input.SelectedRole == "Pilot")
                user.Organization = Input.SelectedOrganization;

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Safe server-side role assignment
                var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "Pilot" };

                if (allowedRoles.Contains(Input.SelectedRole))
                    await _userManager.AddToRoleAsync(user, Input.SelectedRole);

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'.");
            }
        }

        // Roles shown in dropdown
        private static IEnumerable<SelectListItem> GetRegisterableRoles() =>
            new[]
            {
                new SelectListItem("Pilot", "Pilot"),
                
            };

        // Organizations shown for pilots
        private static IEnumerable<SelectListItem> GetAvailableOrganizations() =>
            new[]
            {
                new SelectListItem("NLA Reg Sør", "AvdSør"),
                new SelectListItem("NLA Reg SørØst", "AvdSørØst"),
                new SelectListItem("NLA Reg Nord", "AvdNord"),
                new SelectListItem("NLA Reg Vest", "AvdVest")
            };

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}

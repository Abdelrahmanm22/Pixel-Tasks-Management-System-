using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tasks.Domain.Models.Identity;
using Tasks.Presentation.Helpers;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    public class AccountController : Controller
    {
        private const string UsersFolder = "Users";

        private readonly UserManager<AppUser>   _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
        }
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already authenticated, skip the login page
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // Support login by email OR username in the same field
            var user = model.EmailOrUserName.Contains('@')
                ? await _userManager.FindByEmailAsync(model.EmailOrUserName)
                : await _userManager.FindByNameAsync(model.EmailOrUserName);

            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "These credentials do not match our records.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Redirect to originally requested page or dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "These credentials do not match our records.");
            return View(model);
        }

        [Authorize]
        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ─── PROFILE ─────────────────────────────────────────────────────────

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.Users
                .Include(u => u.Corporation)
                .Include(u => u.Section)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new ProfileViewModel
            {
                FullName        = user.FullName,
                UserName        = user.UserName ?? string.Empty,
                Email           = user.Email ?? string.Empty,
                PhoneNumber     = user.PhoneNumber,
                Gender          = user.Gender,
                ImageUrl        = user.ImageUrl,
                Role            = roles.FirstOrDefault() ?? string.Empty,
                CorporationName = user.Corporation?.Name,
                SectionName     = user.Section?.Name,
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateImage(ProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(_userManager.GetUserId(User)!);
            if (user is null)
                return NotFound();

            if (model.ProfileImage is null || model.ProfileImage.Length == 0)
            {
                TempData["Error"] = "Please select an image file.";
                return RedirectToAction(nameof(Profile));
            }

            var contentType = model.ProfileImage.ContentType;
            if (contentType is not ("image/jpeg" or "image/png" or "image/gif" or "image/webp"))
            {
                TempData["Error"] = "Only image files (jpg, png, gif, webp) are allowed.";
                return RedirectToAction(nameof(Profile));
            }

            if (!string.IsNullOrEmpty(user.ImageUrl))
                DocumentSettings.DeleteFile(Path.GetFileName(user.ImageUrl), UsersFolder);

            var fileName = DocumentSettings.UplaodFile(model.ProfileImage, UsersFolder);
            user.ImageUrl = $"/Files/{UsersFolder}/{fileName}";
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile picture updated successfully.";
            return RedirectToAction(nameof(Profile));
        }
    }
}

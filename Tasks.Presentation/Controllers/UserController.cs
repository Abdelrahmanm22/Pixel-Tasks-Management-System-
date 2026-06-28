using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tasks.Domain;
using Tasks.Domain.Authorization;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Specifications.CorporationSpec;
using Tasks.Domain.Specifications.SectionSpec;
using Tasks.Presentation.Helpers;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    [Authorize(Policy = Permissions.Users.Manage)]
    public class UserController : Controller
    {
        private const string UsersFolder = "Users";

        private readonly UserManager<AppUser>   _userManager;
        private readonly IUnitOfWork            _unitOfWork;

        public UserController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork  = unitOfWork;
        }

        // ─── INDEX ───────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Include(u => u.Corporation)
                .Include(u => u.Section)
                .ToListAsync();

            var viewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModels.Add(new UserViewModel
                {
                    Id              = user.Id,
                    FirstName       = user.FirstName,
                    LastName        = user.LastName,
                    UserName        = user.UserName ?? string.Empty,
                    Email           = user.Email ?? string.Empty,
                    PhoneNumber     = user.PhoneNumber,
                    Gender          = user.Gender,
                    IsActive        = user.IsActive,
                    Role            = roles.FirstOrDefault() ?? string.Empty,
                    CorporationId   = user.CorporationId,
                    CorporationName = user.Corporation?.Name,
                    SectionId       = user.SectionId,
                    SectionName     = user.Section?.Name,
                });
            }

            return View(viewModels);
        }

        // ─── CREATE ──────────────────────────────────────────────────────────

        public async Task<IActionResult> Create()
        {
            var model = new UserViewModel();
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Password is required.");

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            var user = new AppUser
            {
                FirstName      = model.FirstName,
                LastName       = model.LastName,
                UserName       = model.UserName,
                Email          = model.Email,
                PhoneNumber    = model.PhoneNumber,
                Gender         = model.Gender,
                IsActive       = model.IsActive,
                CorporationId  = model.CorporationId,
                SectionId      = model.SectionId,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await PopulateSelectListsAsync(model);
                return View(model);
            }

            if (model.ProfileImage is not null && model.ProfileImage.Length > 0)
            {
                if (!IsImage(model.ProfileImage.ContentType))
                {
                    ModelState.AddModelError(nameof(model.ProfileImage), "Only image files (jpg, png, gif, webp) are allowed.");
                    await _userManager.DeleteAsync(user);
                    await PopulateSelectListsAsync(model);
                    return View(model);
                }
                var fileName = DocumentSettings.UplaodFile(model.ProfileImage, UsersFolder);
                user.ImageUrl = $"/Files/{UsersFolder}/{fileName}";
                await _userManager.UpdateAsync(user);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = $"User \"{user.FullName}\" created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ─── EDIT ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id               = user.Id,
                FirstName        = user.FirstName,
                LastName         = user.LastName,
                UserName         = user.UserName ?? string.Empty,
                Email            = user.Email ?? string.Empty,
                PhoneNumber      = user.PhoneNumber,
                Gender           = user.Gender,
                IsActive         = user.IsActive,
                Role             = roles.FirstOrDefault() ?? string.Empty,
                CorporationId    = user.CorporationId,
                SectionId        = user.SectionId,
                ExistingImageUrl = user.ImageUrl,
            };

            await PopulateSelectListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            // Password fields are optional on edit — remove their validation
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                await PopulateSelectListsAsync(model);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            user.FirstName     = model.FirstName;
            user.LastName      = model.LastName;
            user.UserName      = model.UserName;
            user.Email         = model.Email;
            user.PhoneNumber   = model.PhoneNumber;
            user.Gender        = model.Gender;
            user.IsActive      = model.IsActive;
            user.CorporationId = model.CorporationId;
            user.SectionId     = model.SectionId;

            if (model.ProfileImage is not null && model.ProfileImage.Length > 0)
            {
                if (!IsImage(model.ProfileImage.ContentType))
                {
                    ModelState.AddModelError(nameof(model.ProfileImage), "Only image files (jpg, png, gif, webp) are allowed.");
                    model.ExistingImageUrl = user.ImageUrl;
                    await PopulateSelectListsAsync(model);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(user.ImageUrl))
                    DocumentSettings.DeleteFile(Path.GetFileName(user.ImageUrl), UsersFolder);

                var fileName = DocumentSettings.UplaodFile(model.ProfileImage, UsersFolder);
                user.ImageUrl = $"/Files/{UsersFolder}/{fileName}";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await PopulateSelectListsAsync(model);
                return View(model);
            }

            // Update role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            // Update password only when provided
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var token         = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    await PopulateSelectListsAsync(model);
                    return View(model);
                }
            }

            TempData["Success"] = $"User \"{user.FullName}\" updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ─── TOGGLE ACTIVE ───────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return Json(new { success = false, message = "User not found." });

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var status = user.IsActive ? "activated" : "deactivated";
            return Json(new { success = true, message = $"User \"{user.FullName}\" {status} successfully.", isActive = user.IsActive });
        }

        // ─── REMOTE VALIDATION ───────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CheckUniqueUserName(string userName, string? id)
        {
            var existing = await _userManager.FindByNameAsync(userName);
            var isUnique = existing is null || existing.Id == id;
            return Json(isUnique ? true : "This username is already taken.");
        }

        [HttpGet]
        public async Task<IActionResult> CheckUniqueEmail(string email, string? id)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            var isUnique = existing is null || existing.Id == id;
            return Json(isUnique ? true : "This email is already taken.");
        }

        // ─── AJAX: Load sections by corporation ──────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetSectionsByCorporation(int corporationId)
        {
            var spec     = new SectionByCorporationSpec(corporationId);
            var sections = await _unitOfWork.Repository<Section>().GetAllAsync(spec);
            var items    = sections.Select(s => new { value = s.Id, text = s.Name });
            return Json(items);
        }

        // ─── Private Helpers ─────────────────────────────────────────────────

        private static bool IsImage(string contentType) =>
            contentType is "image/jpeg" or "image/png" or "image/gif" or "image/webp";

        private async Task PopulateSelectListsAsync(UserViewModel model)
        {
            var corpSpec     = new CorporationSpec();
            var corporations = await _unitOfWork.Repository<Corporation>().GetAllAsync(corpSpec);

            model.Corporations = corporations.Select(c => new SelectListItem
            {
                Value    = c.Id.ToString(),
                Text     = c.Name,
                Selected = c.Id == model.CorporationId
            });

            if (model.CorporationId.HasValue)
            {
                var sectionSpec = new SectionByCorporationSpec(model.CorporationId.Value);
                var sections    = await _unitOfWork.Repository<Section>().GetAllAsync(sectionSpec);
                model.Sections  = sections.Select(s => new SelectListItem
                {
                    Value    = s.Id.ToString(),
                    Text     = s.Name,
                    Selected = s.Id == model.SectionId
                });
            }

            model.Roles = new[]
            {
                new SelectListItem { Value = Roles.Admin,    Text = "Admin",    Selected = model.Role == Roles.Admin },
                new SelectListItem { Value = Roles.Employee, Text = "Employee", Selected = model.Role == Roles.Employee },
            };
        }
    }
}

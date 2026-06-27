using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tasks.Domain;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Services;
using Tasks.Domain.Specifications.CorporationSpec;
using Tasks.Domain.Specifications.SectionSpec;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    [Authorize]
    public class SectionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICodeGeneratorService _codeGenerator;
        private readonly UserManager<AppUser> _userManager;

        public SectionController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICodeGeneratorService codeGenerator,
            UserManager<AppUser> userManager)
        {
            _unitOfWork    = unitOfWork;
            _mapper        = mapper;
            _codeGenerator = codeGenerator;
            _userManager   = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var spec     = new SectionSpec();
            var sections = await _unitOfWork.Repository<Section>().GetAllAsync(spec);
            var viewModels = _mapper.Map<IEnumerable<Section>, IEnumerable<SectionViewModel>>(sections);
            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var spec    = new SectionSpec(id);
            var section = await _unitOfWork.Repository<Section>().GetByIdAsync(spec);

            if (section is null)
                return NotFound();

            var viewModel = _mapper.Map<Section, SectionViewModel>(section);
            viewModel.AvailableEmployees = section.Users.Select(u => new AvailableEmployeeViewModel
            {
                Id         = u.Id,
                FullName   = u.FullName,
                Email      = u.Email,
                IsSelected = true
            });
            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var model = new SectionViewModel
            {
                Corporations = await GetCorporationSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Corporations = await GetCorporationSelectListAsync();
                model.AvailableEmployees = await GetAvailableEmployeesAsync(model.CorporationId, null, model.SelectedUserIds);
                return View(model);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var section = _mapper.Map<SectionViewModel, Section>(model);
                section.Code = await _codeGenerator.GenerateCodeAsync<Section>("PXS");

                await _unitOfWork.Repository<Section>().AddAsync(section);
                await _unitOfWork.CompleteAsync();

                await AssignEmployeesAsync(section.Id, model.CorporationId, model.SelectedUserIds, null);

                await _unitOfWork.CommitTransactionAsync();

                TempData["Success"] = $"Section \"{section.Name}\" created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = "An error occurred while creating the section.";
                model.Corporations = await GetCorporationSelectListAsync();
                model.AvailableEmployees = await GetAvailableEmployeesAsync(model.CorporationId, null, model.SelectedUserIds);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var spec    = new SectionSpec(id);
            var section = await _unitOfWork.Repository<Section>().GetByIdAsync(spec);

            if (section is null)
                return NotFound();

            var viewModel = _mapper.Map<Section, SectionViewModel>(section);
            viewModel.Corporations      = await GetCorporationSelectListAsync();
            viewModel.SelectedUserIds   = section.Users.Select(u => u.Id).ToList();
            viewModel.AvailableEmployees = await GetAvailableEmployeesAsync(section.CorporationId, section.Id, viewModel.SelectedUserIds);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SectionViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                model.Corporations = await GetCorporationSelectListAsync();
                model.AvailableEmployees = await GetAvailableEmployeesAsync(model.CorporationId, id, model.SelectedUserIds);
                return View(model);
            }

            var spec    = new SectionSpec(id);
            var section = await _unitOfWork.Repository<Section>().GetByIdAsync(spec);

            if (section is null)
                return NotFound();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                section.Name    = model.Name;
                section.Email   = model.Email;
                section.Fax     = model.Fax;
                section.Phone   = model.Phone;
                section.Address = model.Address;
                section.Telex   = model.Telex;
                section.Notes   = model.Notes;

                _unitOfWork.Repository<Section>().Update(section);
                await _unitOfWork.CompleteAsync();

                await ReconcileEmployeesAsync(section, model.CorporationId, model.SelectedUserIds);

                await _unitOfWork.CommitTransactionAsync();

                TempData["Success"] = $"Section \"{section.Name}\" updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = "An error occurred while updating the section.";
                model.Corporations = await GetCorporationSelectListAsync();
                model.AvailableEmployees = await GetAvailableEmployeesAsync(model.CorporationId, id, model.SelectedUserIds);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var spec    = new SectionSpec(id);
            var section = await _unitOfWork.Repository<Section>().GetByIdAsync(spec);

            if (section is null)
                return Json(new { success = false, message = "Section not found." });

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Detach employees from this section (they keep their corporation)
                foreach (var user in section.Users.ToList())
                {
                    user.SectionId = null;
                    await _userManager.UpdateAsync(user);
                }

                _unitOfWork.Repository<Section>().Delete(section);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();

                return Json(new { success = true, message = $"Section \"{section.Name}\" deleted successfully." });
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Json(new { success = false, message = "An error occurred while deleting the section." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckUniqueName(string name, int id)
        {
            var spec     = new SectionByNameSpec(name);
            var existing = await _unitOfWork.Repository<Section>().GetByIdAsync(spec);

            var isUnique = existing is null || existing.Id == id;
            return Json(isUnique);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableEmployees(int corporationId, int? sectionId)
        {
            var employees = await GetAvailableEmployeesAsync(corporationId, sectionId, new List<string>());
            return Json(employees);
        }

        // ─── Private Helpers ─────────────────────────────────────────────────

        private async Task<IEnumerable<SelectListItem>> GetCorporationSelectListAsync()
        {
            var spec         = new CorporationSpec();
            var corporations = await _unitOfWork.Repository<Corporation>().GetAllAsync(spec);
            return corporations.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text  = c.Name
            });
        }

        private Task<IEnumerable<AvailableEmployeeViewModel>> GetAvailableEmployeesAsync(
            int corporationId, int? sectionId, List<string> selectedUserIds)
        {
            var allUsers = _userManager.Users
                .Where(u => u.IsActive
                         && u.CorporationId == corporationId
                         && (u.SectionId == null || u.SectionId == sectionId))
                .ToList();

            IEnumerable<AvailableEmployeeViewModel> result = allUsers.Select(u => new AvailableEmployeeViewModel
            {
                Id         = u.Id,
                FullName   = u.FullName,
                Email      = u.Email,
                IsSelected = selectedUserIds.Contains(u.Id)
            });

            return Task.FromResult(result);
        }

        private async Task AssignEmployeesAsync(int sectionId, int corporationId, List<string> selectedUserIds, int? existingSectionId)
        {
            foreach (var userId in selectedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null || !user.IsActive || user.CorporationId != corporationId)
                    continue;
                if (user.SectionId != null && user.SectionId != existingSectionId)
                    continue;

                user.SectionId = sectionId;
                await _userManager.UpdateAsync(user);
            }
        }

        private async Task ReconcileEmployeesAsync(Section section, int corporationId, List<string> selectedUserIds)
        {
            // Remove users no longer selected
            foreach (var user in section.Users.ToList())
            {
                if (!selectedUserIds.Contains(user.Id))
                {
                    user.SectionId = null;
                    await _userManager.UpdateAsync(user);
                }
            }

            // Add newly selected users
            var currentUserIds = section.Users.Select(u => u.Id).ToList();
            foreach (var userId in selectedUserIds)
            {
                if (currentUserIds.Contains(userId))
                    continue;

                var user = await _userManager.FindByIdAsync(userId);
                if (user is null || !user.IsActive || user.CorporationId != corporationId)
                    continue;
                if (user.SectionId != null && user.SectionId != section.Id)
                    continue;

                user.SectionId = section.Id;
                await _userManager.UpdateAsync(user);
            }
        }
    }
}

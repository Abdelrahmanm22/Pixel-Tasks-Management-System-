using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Domain;
using Tasks.Domain.Authorization;
using Tasks.Domain.Models;
using Tasks.Domain.Services;
using Tasks.Domain.Specifications.CorporationSpec;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    [Authorize(Policy = Permissions.Corporations.Manage)]
    public class CorporationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICodeGeneratorService _codeGenerator;

        public CorporationController(IUnitOfWork unitOfWork, IMapper mapper, ICodeGeneratorService codeGenerator)
        {
            _unitOfWork    = unitOfWork;
            _mapper        = mapper;
            _codeGenerator = codeGenerator;
        }

        // ─── INDEX ───────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var spec         = new CorporationSpec();
            var corporations = await _unitOfWork.Repository<Corporation>().GetAllAsync(spec);
            var viewModels   = _mapper.Map<IEnumerable<Corporation>, IEnumerable<CorporationViewModel>>(corporations);
            return View(viewModels);
        }

        // ─── DETAILS ─────────────────────────────────────────────────────────

        public async Task<IActionResult> Details(int id)
        {
            var spec        = new CorporationSpec(id);
            var corporation = await _unitOfWork.Repository<Corporation>().GetByIdAsync(spec);

            if (corporation is null)
                return NotFound();

            var viewModel = _mapper.Map<Corporation, CorporationViewModel>(corporation);
            return View(viewModel);
        }

        // ─── CREATE ──────────────────────────────────────────────────────────

        public IActionResult Create()
        {
            return View(new CorporationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CorporationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var corporation = _mapper.Map<CorporationViewModel, Corporation>(model);

            // Generate unique sequential code: PXC-000001
            corporation.Code = await _codeGenerator.GenerateCodeAsync<Corporation>("PXC");

            await _unitOfWork.Repository<Corporation>().AddAsync(corporation);
            await _unitOfWork.CompleteAsync();

            TempData["Success"] = $"Corporation \"{corporation.Name}\" created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ─── EDIT ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Edit(int id)
        {
            var spec        = new CorporationSpec(id);
            var corporation = await _unitOfWork.Repository<Corporation>().GetByIdAsync(spec);

            if (corporation is null)
                return NotFound();

            var viewModel = _mapper.Map<Corporation, CorporationViewModel>(corporation);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CorporationViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var spec        = new CorporationSpec(id);
            var corporation = await _unitOfWork.Repository<Corporation>().GetByIdAsync(spec);

            if (corporation is null)
                return NotFound();

            // Update only the fields the user can edit; keep the auto-generated Code
            corporation.Name   = model.Name;
            corporation.NameAr = model.NameAr;
            corporation.Notes  = model.Notes;

            _unitOfWork.Repository<Corporation>().Update(corporation);
            await _unitOfWork.CompleteAsync();

            TempData["Success"] = $"Corporation \"{corporation.Name}\" updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ─── DELETE ──────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var spec        = new CorporationSpec(id);
            var corporation = await _unitOfWork.Repository<Corporation>().GetByIdAsync(spec);

            if (corporation is null)
                return Json(new { success = false, message = "Corporation not found." });

            _unitOfWork.Repository<Corporation>().Delete(corporation);
            await _unitOfWork.CompleteAsync();

            return Json(new { success = true, message = $"Corporation \"{corporation.Name}\" deleted successfully." });
        }

        // ─── REMOTE VALIDATION ───────────────────────────────────────────────

        /// <summary>
        /// Used by [Remote] on CorporationViewModel.Name to validate uniqueness client-side.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckUniqueName(string name, int id)
        {
            var spec    = new CorporationByNameSpec(name);
            var existing = await _unitOfWork.Repository<Corporation>().GetByIdAsync(spec);

            // Valid when no record exists OR the found record belongs to the current entity being edited
            var isUnique = existing is null || existing.Id == id;
            return Json(isUnique);
        }

    }
}

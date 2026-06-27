using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Domain;
using Tasks.Domain.Authorization;
using Tasks.Domain.Models;
using Tasks.Domain.Specifications.TaskTypeSpec;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    [Authorize(Policy = Permissions.TaskTypes.Manage)]
    public class TaskTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaskTypeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper     = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var spec      = new TaskTypeSpec();
            var taskTypes = await _unitOfWork.Repository<TaskType>().GetAllAsync(spec);
            var viewModels = _mapper.Map<IEnumerable<TaskType>, IEnumerable<TaskTypeViewModel>>(taskTypes);
            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var spec     = new TaskTypeSpec(id);
            var taskType = await _unitOfWork.Repository<TaskType>().GetByIdAsync(spec);

            if (taskType is null)
                return NotFound();

            var viewModel = _mapper.Map<TaskType, TaskTypeViewModel>(taskType);
            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View(new TaskTypeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskTypeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var taskType = _mapper.Map<TaskTypeViewModel, TaskType>(model);

            await _unitOfWork.Repository<TaskType>().AddAsync(taskType);
            await _unitOfWork.CompleteAsync();

            TempData["Success"] = $"Task type \"{taskType.Name}\" created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var spec     = new TaskTypeSpec(id);
            var taskType = await _unitOfWork.Repository<TaskType>().GetByIdAsync(spec);

            if (taskType is null)
                return NotFound();

            var viewModel = _mapper.Map<TaskType, TaskTypeViewModel>(taskType);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskTypeViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var spec     = new TaskTypeSpec(id);
            var taskType = await _unitOfWork.Repository<TaskType>().GetByIdAsync(spec);

            if (taskType is null)
                return NotFound();

            taskType.Name     = model.Name;
            taskType.Category = model.Category;

            _unitOfWork.Repository<TaskType>().Update(taskType);
            await _unitOfWork.CompleteAsync();

            TempData["Success"] = $"Task type \"{taskType.Name}\" updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var spec     = new TaskTypeSpec(id);
            var taskType = await _unitOfWork.Repository<TaskType>().GetByIdAsync(spec);

            if (taskType is null)
                return Json(new { success = false, message = "Task type not found." });

            _unitOfWork.Repository<TaskType>().Delete(taskType);
            await _unitOfWork.CompleteAsync();

            return Json(new { success = true, message = $"Task type \"{taskType.Name}\" deleted successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> CheckUniqueName(string name, int id)
        {
            var spec     = new TaskTypeByNameSpec(name);
            var existing = await _unitOfWork.Repository<TaskType>().GetByIdAsync(spec);

            var isUnique = existing is null || existing.Id == id;
            return Json(isUnique);
        }
    }
}

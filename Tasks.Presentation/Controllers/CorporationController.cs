using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Tasks.Domain;
using Tasks.Domain.Models;
using Tasks.Domain.Specifications.CorporationSpec;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    public class CorporationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CorporationController(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var spec = new CorporationSpec();
            var corporation = await _unitOfWork.Repository<Corporation>().GetAllAsync(spec);
            var corporationViewModel = _mapper.Map<IEnumerable<Corporation>,IEnumerable<CorporationViewModel>>(corporation);
            return View(corporationViewModel);
        }
    }
}

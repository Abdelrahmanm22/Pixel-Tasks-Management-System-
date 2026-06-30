using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tasks.Domain;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Services;
using Tasks.Domain.Specifications.NotificationSpec;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private const int PageSize = 20;
        private const int RecentCount = 10;

        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public NotificationController(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _mapper = mapper;
            _userManager = userManager;
        }

        // Full history page (paged).
        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = _userManager.GetUserId(User)!;
            if (page < 1) page = 1;

            var notifications = await _unitOfWork.Repository<Notification>()
                .GetAllAsync(new NotificationByUserSpec(userId, (page - 1) * PageSize, PageSize));

            var vms = _mapper.Map<IEnumerable<Notification>, IEnumerable<NotificationViewModel>>(notifications).ToList();

            ViewBag.Page = page;
            ViewBag.HasNextPage = vms.Count == PageSize;
            ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return View(vms);
        }

        // Latest few + unread count — consumed by the bell dropdown on page load (AJAX).
        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            var userId = _userManager.GetUserId(User)!;
            var notifications = await _unitOfWork.Repository<Notification>()
                .GetAllAsync(new NotificationByUserSpec(userId, 0, RecentCount));

            var vms = _mapper.Map<IEnumerable<Notification>, IEnumerable<NotificationViewModel>>(notifications);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new
            {
                unreadCount,
                items = vms.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    url = n.Url,
                    isRead = n.IsRead,
                    timeAgo = n.TimeAgo,
                    avatarSrc = n.AvatarSrc,
                    actorName = n.ActorName,
                    icon = n.Icon,
                    colorClass = n.ColorClass
                })
            });
        }

        // Mark one read, then deep-link to its target. Used by dropdown/list item clicks.
        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _notificationService.MarkAsReadAsync(id, userId);

            var notification = await _unitOfWork.Repository<Notification>()
                .GetByIdAsync(new NotificationByIdSpec(id));

            var target = notification?.Url;
            return Redirect(Url.IsLocalUrl(target) ? target! : Url.Action(nameof(Index))!);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User)!;
            await _notificationService.MarkAllAsReadAsync(userId);
            return Json(new { success = true });
        }
    }
}

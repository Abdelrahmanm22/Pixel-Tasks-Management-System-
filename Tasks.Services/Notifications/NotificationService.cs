using Tasks.Domain;
using Tasks.Domain.Enums;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Services;
using Tasks.Domain.Specifications.NotificationSpec;

namespace Tasks.Services.Notifications
{
    /// <summary>
    /// Persists notifications and pushes them to the recipient in real time.
    /// Title/Message and a relative deep-link Url are composed here at creation time,
    /// so views render plain stored strings and future types just supply their own text.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeNotificationPublisher _publisher;

        public NotificationService(IUnitOfWork unitOfWork, IRealtimeNotificationPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public Task NotifyTaskAssignedAsync(AppUser actor, string recipientEmployeeId, WorkTask task) =>
            CreateAsync(
                recipientEmployeeId,
                actor.Id,
                NotificationType.TaskAssigned,
                title: "New task assigned",
                message: $"{actor.FullName} assigned you the task \"{task.Title}\" ({task.Code}).",
                url: $"/Task/Work/{task.Id}",
                workTaskId: task.Id);

        public Task NotifyNewCommentAsync(AppUser actor, string recipientUserId, WorkTask task, int assignmentId, bool recipientIsAdmin) =>
            CreateAsync(
                recipientUserId,
                actor.Id,
                NotificationType.NewComment,
                title: "New message",
                message: $"{actor.FullName} sent a message on \"{task.Title}\" ({task.Code}).",
                url: recipientIsAdmin
                    ? $"/Task/Details/{task.Id}?assignmentId={assignmentId}"
                    : $"/Task/Work/{task.Id}",
                workTaskId: task.Id);

        public Task NotifyNeedsReviewAsync(AppUser actor, string recipientAdminId, WorkTask task, int assignmentId) =>
            CreateAsync(
                recipientAdminId,
                actor.Id,
                NotificationType.TaskNeedsReview,
                title: "Task ready for review",
                message: $"{actor.FullName} completed \"{task.Title}\" ({task.Code}) and it needs your review.",
                url: $"/Task/Details/{task.Id}?assignmentId={assignmentId}",
                workTaskId: task.Id);

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var unread = await _unitOfWork.Repository<Notification>()
                .GetAllAsync(new UnreadNotificationByUserSpec(userId));
            return unread.Count();
        }

        public async Task MarkAsReadAsync(int id, string userId)
        {
            var notification = await _unitOfWork.Repository<Notification>()
                .GetByIdAsync(new NotificationByIdSpec(id));

            if (notification is null || notification.RecipientUserId != userId || notification.IsRead)
                return;

            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await _unitOfWork.Repository<Notification>()
                .GetAllAsync(new UnreadNotificationByUserSpec(userId));

            var any = false;
            foreach (var notification in unread)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification>().Update(notification);
                any = true;
            }

            if (any)
                await _unitOfWork.CompleteAsync();
        }

        // Persist-then-publish: never notifies the actor about their own action.
        private async Task CreateAsync(
            string recipientUserId,
            string? actorUserId,
            NotificationType type,
            string title,
            string message,
            string? url,
            int? workTaskId)
        {
            if (string.IsNullOrEmpty(recipientUserId) || recipientUserId == actorUserId)
                return;

            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                ActorUserId = actorUserId,
                Type = type,
                Title = title,
                Message = message,
                Url = url,
                WorkTaskId = workTaskId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            var unreadCount = await GetUnreadCountAsync(recipientUserId);
            await _publisher.PublishAsync(recipientUserId, notification, unreadCount);
        }
    }
}

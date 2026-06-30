using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;

namespace Tasks.Domain.Services
{
    /// <summary>
    /// Creates, persists, and (via the real-time publisher) pushes in-app notifications.
    /// Intent-named methods keep callers declarative — they describe the event, not the payload.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>An admin assigned <paramref name="task"/> to the employee.</summary>
        Task NotifyTaskAssignedAsync(AppUser actor, string recipientEmployeeId, WorkTask task);

        /// <summary>A new chat message was posted to a task thread.</summary>
        /// <param name="recipientIsAdmin">True when the recipient is the task creator (links to admin Details view).</param>
        Task NotifyNewCommentAsync(AppUser actor, string recipientUserId, WorkTask task, int assignmentId, bool recipientIsAdmin);

        /// <summary>An employee's assignment reached Completed and awaits the admin's review.</summary>
        Task NotifyNeedsReviewAsync(AppUser actor, string recipientAdminId, WorkTask task, int assignmentId);

        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>Marks a single notification read — no-op if it isn't the user's own.</summary>
        Task MarkAsReadAsync(int id, string userId);

        Task MarkAllAsReadAsync(string userId);
    }
}

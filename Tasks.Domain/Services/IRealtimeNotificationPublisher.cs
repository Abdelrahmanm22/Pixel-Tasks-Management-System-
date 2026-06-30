using Tasks.Domain.Models;

namespace Tasks.Domain.Services
{
    /// <summary>
    /// Pushes a freshly created notification to its recipient in real time.
    /// Abstraction lives in the Domain layer; the SignalR implementation lives in
    /// Presentation so the service layer takes no web/SignalR dependency.
    /// </summary>
    public interface IRealtimeNotificationPublisher
    {
        Task PublishAsync(string recipientUserId, Notification notification, int unreadCount);
    }
}

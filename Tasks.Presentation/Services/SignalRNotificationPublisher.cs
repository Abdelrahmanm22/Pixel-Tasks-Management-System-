using Microsoft.AspNetCore.SignalR;
using Tasks.Domain.Models;
using Tasks.Domain.Services;
using Tasks.Presentation.Hubs;

namespace Tasks.Presentation.Services
{
    /// <summary>
    /// SignalR implementation of the Domain's real-time publisher. Pushes a compact
    /// payload to the recipient's connected clients via the "ReceiveNotification" event.
    /// </summary>
    public class SignalRNotificationPublisher : IRealtimeNotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PublishAsync(string recipientUserId, Notification notification, int unreadCount)
        {
            var payload = new
            {
                id        = notification.Id,
                type      = (int)notification.Type,
                title     = notification.Title,
                message   = notification.Message,
                url       = notification.Url,
                createdAt = notification.CreatedAt,
                unreadCount
            };

            return _hubContext.Clients.User(recipientUserId)
                .SendAsync("ReceiveNotification", payload);
        }
    }
}

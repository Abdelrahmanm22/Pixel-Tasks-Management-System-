using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Tasks.Presentation.Hubs
{
    /// <summary>
    /// Real-time channel for in-app notifications. The server only pushes (via
    /// IHubContext), so the hub itself is empty. SignalR's default user-id provider
    /// maps connections to the NameIdentifier claim (= AppUser.Id), so
    /// Clients.User(userId) reaches exactly that user's open tabs.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}

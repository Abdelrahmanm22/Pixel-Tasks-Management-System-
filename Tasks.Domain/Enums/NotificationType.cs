namespace Tasks.Domain.Enums
{
    /// <summary>
    /// The kind of activity a notification represents. Drives the icon/colour
    /// shown in the bell dropdown and history page.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>An admin assigned a task to the recipient (employee).</summary>
        TaskAssigned    = 1,

        /// <summary>A chat message was posted to a thread the recipient is part of.</summary>
        NewComment      = 2,

        /// <summary>An employee completed their assignment — admin needs to review it.</summary>
        TaskNeedsReview = 3
    }
}

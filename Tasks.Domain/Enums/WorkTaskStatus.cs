namespace Tasks.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a task or a per-user task assignment.
    /// Used on both WorkTask (overall status) and TaskAssignment (per-assignee status).
    /// </summary>
    public enum WorkTaskStatus
    {
        Pending    = 1,
        InProgress = 2,
        Completed  = 3,
        Reviewed   = 4
    }
}

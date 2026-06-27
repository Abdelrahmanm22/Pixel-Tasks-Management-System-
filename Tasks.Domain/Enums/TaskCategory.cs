namespace Tasks.Domain.Enums
{
    /// <summary>
    /// Defines the behavioral category of a task type.
    /// Normal = plain task, Point = checklist-based, Counter = numeric progress tracking.
    /// </summary>
    public enum TaskCategory
    {
        Normal  = 1,
        Point   = 2,
        Counter = 3
    }
}

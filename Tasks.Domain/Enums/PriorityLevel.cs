namespace Tasks.Domain.Enums
{
    /// <summary>
    /// Represents the urgency level of a task.
    /// Associated display colors follow popular task management conventions:
    ///   Low      → #10B981 (emerald green)
    ///   Medium   → #F59E0B (amber)
    ///   High     → #F97316 (orange)
    ///   Critical → #EF4444 (red)
    /// Colors are not persisted in the DB; resolve them via PriorityLevelExtensions.
    /// </summary>
    public enum PriorityLevel
    {
        Low      = 1,
        Medium   = 2,
        High     = 3,
        Critical = 4
    }
}

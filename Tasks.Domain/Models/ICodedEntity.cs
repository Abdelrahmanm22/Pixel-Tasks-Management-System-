namespace Tasks.Domain.Models
{
    /// <summary>
    /// Marker interface for entities that have an auto-generated sequential code (e.g., PXC-000001).
    /// Any entity that requires a system-generated code should implement this interface.
    /// </summary>
    public interface ICodedEntity
    {
        string Code { get; set; }
    }
}

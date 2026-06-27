using Tasks.Domain.Models;

namespace Tasks.Domain.Services
{
    /// <summary>
    /// Generates unique sequential codes (e.g., PXC-000001) for any coded entity.
    /// Lives in the Domain layer so any consumer can depend on the abstraction,
    /// not the implementation.
    /// </summary>
    public interface ICodeGeneratorService
    {
        /// <summary>
        /// Generates the next sequential code for <typeparamref name="TEntity"/> using the given prefix.
        /// Format: {prefix}-{number:D6} for numbers up to 999,999;
        ///         {prefix}-{number}     for numbers 1,000,000 and above.
        /// </summary>
        /// <typeparam name="TEntity">An entity that has a Code property.</typeparam>
        /// <param name="prefix">The code prefix (e.g., "PXC" for Corporation, "PXD" for Department).</param>
        Task<string> GenerateCodeAsync<TEntity>(string prefix)
            where TEntity : BaseModel, ICodedEntity;
    }
}

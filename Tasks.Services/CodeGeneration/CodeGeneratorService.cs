using Tasks.Domain;
using Tasks.Domain.Models;
using Tasks.Domain.Services;
using Tasks.Domain.Specifications;

namespace Tasks.Services.CodeGeneration
{
    /// <summary>
    /// Generates sequential unique codes for any entity that implements <see cref="ICodedEntity"/>.
    /// The algorithm is entity-agnostic — the caller supplies the prefix (e.g., "PXC", "PXD").
    /// 
    /// Padding rules:
    ///   - Numbers 1 – 999,999   →  {prefix}-000001  (6-digit zero-padded)
    ///   - Numbers ≥ 1,000,000   →  {prefix}-1000000 (no padding)
    /// </summary>
    public class CodeGeneratorService : ICodeGeneratorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CodeGeneratorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public async Task<string> GenerateCodeAsync<TEntity>(string prefix)
            where TEntity : BaseModel, ICodedEntity
        {
            var spec          = new BaseSpecifications<TEntity>();   // get all, no filter
            var all           = await _unitOfWork.Repository<TEntity>().GetAllAsync(spec);
            var prefixWithDash = $"{prefix}-";

            var maxNumber = all
                .Where(e => e.Code is not null && e.Code.StartsWith(prefixWithDash))
                .Select(e =>
                {
                    if (int.TryParse(e.Code[prefixWithDash.Length..], out var num))
                        return num;
                    return 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            var next = maxNumber + 1;

            // 6-digit padding for numbers within 1–999,999; plain number above that
            return next <= 999_999
                ? $"{prefixWithDash}{next:D6}"
                : $"{prefixWithDash}{next}";
        }
    }
}

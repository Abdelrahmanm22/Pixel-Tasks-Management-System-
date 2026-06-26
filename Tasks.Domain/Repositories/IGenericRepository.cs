using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Models;
using Tasks.Domain.Specifications;

namespace Tasks.Domain.Repositories
{
    public interface IGenericRepository<T> where T : BaseModel
    {
        Task<IEnumerable<T>> GetAllAsync(ISpecifications<T> Spec);
        Task<T> GetByIdAsync(ISpecifications<T> Spec);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Models;

namespace Tasks.Domain.Repositories
{
    public interface IGenericRepository<T> where T : BaseModel
    {
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}

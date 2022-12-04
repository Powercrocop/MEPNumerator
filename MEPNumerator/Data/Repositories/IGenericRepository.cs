using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MEPNumerator.Data.Repositories
{
    public interface IGenericRepository<T>
    {
        Task<T> GetByIdAsync(int id);
        Task SaveAsync();
        bool HasChanges();
        void Add(T model);
        void Remove(T model);
        List<T> GetAll();
    }
}

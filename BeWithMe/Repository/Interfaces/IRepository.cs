using Microsoft.AspNetCore.Identity;

namespace BeWithMe.Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        void Update(T entity);
        void Delete(T entity);
        
    }
}

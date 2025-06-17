using BeWithMe.Data;
using BeWithMe.Models;
using BeWithMe.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BeWithMe.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _context;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Delete(T entity)
        {
            _context.Remove(entity);
        }

        public IEnumerable<T> GetAll()
        {
           return _context.Set<T>().ToList();
        }

        public T GetById(int id)
        {
           return  _context.Set<T>().Find(id);  
        }

        public void Update(T entity) 
        {
           
            _context.Update(entity);
        }
  

            

            /// <summary>
            /// Calculates the age based on the DateOfBirth.
            /// </summary>
            /// <param name="dateOfBirth">The DateOfBirth to calculate the age from.</param>
            /// <returns>The calculated age.</returns>
            private int CalculateAge(DateTime dateOfBirth)
            {
                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;

                // Adjust if the birthday hasn't occurred yet this year
                if (dateOfBirth.Date > today.AddYears(-age))
                {
                    age--;
                }

                return age;
            }
        }
    }
    //private object GetPrimaryKey(T entity)
    //{

    //    var primaryKeyProperty = typeof(T).GetProperty("Id"); // Assumes "Id" is the primary key
    //    if (primaryKeyProperty == null)
    //    {
    //        throw new InvalidOperationException("Primary key property 'Id' not found.");
    //    }

    //    return primaryKeyProperty.GetValue(entity);
    //}

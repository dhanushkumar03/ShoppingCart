using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shopping_Cart_Assignment.Generic_Repository
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(T obj);
    }
}
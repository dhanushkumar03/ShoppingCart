using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Shopping_Cart_Assignment.UnitOfWork
{
        public interface IUnitOfWork<out TContext> where TContext : DbContext, new()
        {
            TContext Context { get; }
            void CreateTransaction();
            void Commit();
            void Rollback();
            void Save();
        }
    
}
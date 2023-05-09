using Shopping_Cart_Assignment.Models;
using Shopping_Cart_Assignment.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shopping_Cart_Assignment.Generic_Repository
{
    public class ProductRepository : GenericRepository<Product>, IShoppingRepository
    {
        public ProductRepository(IUnitOfWork<ShoppingCartContext> unitOfWork)
            : base(unitOfWork)
        {
        }
        public ProductRepository(ShoppingCartContext context)
            : base(context)
        {
        }

        public IEnumerable<Product> GetProductByCategory(string Category)
        {
            return Context.Products.Where(x => x.Category.ToLower() == Category).ToList();
        }

       
        
    }
}
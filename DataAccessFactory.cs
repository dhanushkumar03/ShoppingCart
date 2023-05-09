using Shopping_Cart_Assignment.Models;
using Shopping_Cart_Assignment.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shopping_Cart_Assignment.Generic_Repository
{
    public class DataAccessFactory
    {
        public static IShoppingRepository GetProductDataAccessObj()
        {
            UnitOfWork<ShoppingCartContext> unitOfWork = new UnitOfWork<ShoppingCartContext>();
            return new ProductRepository(unitOfWork);
        }
    }
}
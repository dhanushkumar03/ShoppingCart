using Microsoft.VisualBasic.ApplicationServices;
using Shopping_Cart_Assignment.Models;
using Shopping_Cart_Assignment.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shopping_Cart_Assignment.Generic_Repository
{
    public class CartRepository : GenericRepository<CartReference>, ICartRepository
    {
        public CartRepository(IUnitOfWork<ShoppingCartContext> unitOfWork)
           : base(unitOfWork)
        {
        }
        public CartRepository(ShoppingCartContext context)
            : base(context)
        {
        }

        public IEnumerable<CartReference> GetCartByCartAndProduct(int cartId, int productId)
        {
            return Context.CartReferences.Where(x => x.CartDRefId == cartId && x.ProductRefId == productId);
        }

        public IEnumerable<CartReference> GetCartById(int Id)
        {
            return Context.CartReferences.Where(x => x.CartDRefId == Id).ToList();
        }

      
    }
}
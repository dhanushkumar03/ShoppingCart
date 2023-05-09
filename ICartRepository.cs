using Shopping_Cart_Assignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shopping_Cart_Assignment.Generic_Repository
{
    public interface ICartRepository : IGenericRepository<CartReference>
    {
        IEnumerable<CartReference> GetCartById(int Id);
        IEnumerable<CartReference> GetCartByCartAndProduct(int cartId, int productId);
    }
}
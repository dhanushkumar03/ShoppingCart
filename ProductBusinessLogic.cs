using Shopping_Cart_Assignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shopping_Cart_Assignment.Generic_Repository
{
    public class ProductBusinessLogic
    {
        IShoppingRepository _shoppingRepository;
        public ProductBusinessLogic(IShoppingRepository shoppingRepository)
        {
            _shoppingRepository = shoppingRepository;
        }

        public IEnumerable<Product> GetProductBycategory(string Category)
        {
            return _shoppingRepository.GetProductByCategory(Category);
        }

        public IEnumerable<Product> GetAll()
        {
            return _shoppingRepository.GetAll();
        }

        public Product GetById(int id)
        {
            return _shoppingRepository.GetById(id);
        }

        public void Add(Product product)
        {
            _shoppingRepository.Insert(product);
        }

        public void Update(Product product)
        {
            _shoppingRepository.Update(product);
        }
        public void Delete(Product product)
        {
            _shoppingRepository.Delete(product);
        }
    }
}
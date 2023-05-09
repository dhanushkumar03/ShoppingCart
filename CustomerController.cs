using Newtonsoft.Json;
using Shopping_Cart_Assignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Shopping_Cart_Assignment.Controllers
{
    [Authorize(Roles = "User")]
    public class CustomerController : Controller
    {

        private readonly ShoppingCartContext db = new ShoppingCartContext();
        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }
        

        public ActionResult ViewCart()
        {
            int userId = int.Parse(Session["ID"].ToString());
            List<CartReference> productref = new List<CartReference>();
            productref.AddRange(db.CartReferences.Where(x => x.CartDRefId == userId));
            int totalp = 0;
            int totalq = 0;
            foreach(var item in productref)
            {
                totalq = totalq + item.ProductQuantity;
                totalp = totalp + (item.ProductPrice * item.ProductQuantity);
            }
            ViewBag.TotalPrice = totalp;
            ViewBag.TotalQuantity = totalq;
            return View(productref);
        }
        public ActionResult Filter(string filter)
        {
            List<Product> products = new List<Product>();
            products.AddRange(db.Products.Where(x=>x.Category.ToLower()==filter));
            return View(products);
        }

        public ActionResult Footwear()
        {
            List<Product> products = new List<Product>();
            products.AddRange(db.Products.Where(x => x.Category == "Footwear"));
            return RedirectToAction("Filter", "Customer", new {productlist = products});
        }

        //public ActionResult Add(int id)
        //{
        //    Product product = db.Products.FirstOrDefault(x => x.Id == id);
        //    int productid = product.Id;
        //    int userId = int.Parse(Session["ID"].ToString());
        //    var productjson = JsonConvert.SerializeObject(product).ToString();
        //    //var payload = new StringContent(productjson, Encoding.UTF8, "application/json");
        //    ViewBag.Payload = productjson;
        //    ViewBag.UserId = userId;
        //    return View();
        //}
        public ActionResult AddToCart(int id)
        {
            Product product = db.Products.FirstOrDefault(x => x.Id == id);
            int productid = product.Id;
            int userId = int.Parse(Session["ID"].ToString());
            CartReference c = db.CartReferences.Where(x=> x.CartDRefId == userId && x.ProductRefId==productid).FirstOrDefault();
            if(c != null)
            {
                if (db.Products.Where(x => x.Id == productid).FirstOrDefault().Quantity > c.ProductQuantity)
                {
                    c.ProductQuantity++;
                    db.SaveChanges();
                }
            }
            else if (c == null)
            {
                c = new CartReference
                {
                    CartDRefId = userId,
                    ProductRefId = productid,
                    ProductDescription = product.Description,
                    ProductPrice = product.Price,
                    ProductImage = product.Image,
                    ProductName = product.Name,
                    ProductRating = product.Rating,
                    ProductQuantity = 1
                };
                db.CartReferences.Add(c);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        
        public ActionResult Plus(int pId, int cId)
        {
            int productId = pId;
            int cartId = cId; 
            CartReference c = db.CartReferences.Where(x => x.CartDRefId == cartId && x.ProductRefId == productId).FirstOrDefault();
            if (db.Products.Where(x => x.Id == productId).FirstOrDefault().Quantity > c.ProductQuantity)
            {
                c.ProductQuantity++;
                db.SaveChanges();
            }
            
            return RedirectToAction("ViewCart");
        }
        public ActionResult Minus(int pId, int cId)
        {
            int productId = pId;
            int cartId = cId;
            CartReference c = db.CartReferences.Where(x => x.CartDRefId == cartId && x.ProductRefId == productId).FirstOrDefault();
            if (db.Products.Where(x => x.Id == productId).FirstOrDefault().Quantity >= c.ProductQuantity && c.ProductQuantity>0)
            {
                c.ProductQuantity--;
                db.SaveChanges();
            }
            if (c.ProductQuantity == 0)
            {
                db.CartReferences.Remove(c);
                db.SaveChanges();
            }
          
            return RedirectToAction("ViewCart");
        }

        public ActionResult CheckOut()
        {
            int userId = int.Parse(Session["ID"].ToString());
            List<CartReference> cart = new List<CartReference>();
            cart.AddRange(db.CartReferences.Where(x => x.CartDRefId == userId));
            
            int ordernum = 1;
            if (db.PlacedOrders.Any(x=>x.CartDRefId==userId))
            {
                int check = db.PlacedOrders.Where(x => x.CartDRefId == userId).Count();
                ordernum = check + 1;
            }
            foreach (CartReference c in cart)
            {
                PlacedOrder p = new PlacedOrder
                {
                    CartDRefId = c.CartDRefId,
                    ProductDescription = c.ProductDescription,
                    ProductImage = c.ProductImage,
                    ProductName = c.ProductName,
                    ProductPrice = c.ProductPrice,
                    ProductQuantity = c.ProductQuantity,
                    ProductRating = c.ProductRating,
                    ProductRefId = c.ProductRefId,
                    OrderNumber = ordernum,
                };
                db.PlacedOrders.Add(p);
                db.CartReferences.Remove(c);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
using Shopping_Cart_Assignment.Generic_Repository;
using Shopping_Cart_Assignment.Models;
using Shopping_Cart_Assignment.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Shopping_Cart_Assignment.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private UnitOfWork<ShoppingCartContext> unitOfWork = new UnitOfWork<ShoppingCartContext>();
        private GenericRepository<Product> genericRepository;
        private IShoppingRepository productRepository;
        private ProductBusinessLogic businessLogic;
        public HomeController()
        {
            genericRepository = new GenericRepository<Product>(unitOfWork);
            productRepository = new ProductRepository(unitOfWork);
            businessLogic = new ProductBusinessLogic(productRepository);
        }
        private readonly ShoppingCartContext db = new ShoppingCartContext();
        public ActionResult Index()
        {
            var model = genericRepository.GetAll();
            return View(model);
        }

        public ActionResult Create()
        {
            return View(new Product());
        }

        [HttpPost]
        public ActionResult Create(HttpPostedFileBase file, Product product)
        {
            try
            {
                unitOfWork.CreateTransaction();
                if (ModelState.IsValid)
                {
                    string filename = Path.GetFileName(file.FileName);
                    string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                    string extension = Path.GetExtension(file.FileName);
                    string path = Path.Combine(Server.MapPath("/Content/Images/"), _filename);
                    product.Image = "/Content/Images/" + _filename;
                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png")
                    {
                        if (file.ContentLength < 100000)
                        {
                            db.Products.Add(product);
                            db.SaveChanges();
                            file.SaveAs(path);
                            ModelState.Clear();
                            unitOfWork.Commit();
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.msg = "File Size should be Less than 1 Mb";
                        }
                    }
                    else
                    {
                        ViewBag.msg = "Invalid File Type";
                    }
                }
                return View(new Product());

            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
            }
            return View(new Product());
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = genericRepository.GetById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            Session["ImgPath"] = product.Image;
            return View(product);
        }

        [HttpPost]
        public ActionResult Edit(HttpPostedFileBase file, Product product)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string filename = Path.GetFileName(file.FileName);
                    string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                    string extension = Path.GetExtension(file.FileName);


                    string path = Path.Combine(Server.MapPath("/Content/Images/"), _filename);
                    product.Image = "/Content/Images/" + _filename;

                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png")
                    {
                        if (file.ContentLength < 100000)
                        {
                            string OldImgPath = Request.MapPath(Session["ImgPath"].ToString());
                            genericRepository.Update(product);
                            unitOfWork.Save();

                            file.SaveAs(path);
                            if (System.IO.File.Exists(OldImgPath))
                            {
                                System.IO.File.Delete(OldImgPath);
                            }
                            return RedirectToAction("Index");

                        }
                        else
                        {
                            ViewBag.msg = "File Size should be Less than 1 Mb";
                        }
                    }
                    else
                    {
                        ViewBag.msg = "Invalid File Type";
                    }
                }else if (file == null)
                {
                    product.Image = Session["ImgPath"].ToString();
                    db.Entry(product).State = EntityState.Modified;
                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            else
            {
                product.Image = Session["ImgPath"].ToString();
                db.Entry(product).State = EntityState.Modified;
                if (db.SaveChanges() > 0)
                {
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError("", "Please complete the edit");
            return View(new Product());
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = genericRepository.GetById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = genericRepository.GetById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            Product product = genericRepository.GetById(id);
            if (product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            genericRepository.Delete(product);
            unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}
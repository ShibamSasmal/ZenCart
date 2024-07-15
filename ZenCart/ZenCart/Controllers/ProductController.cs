using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZenCart.Models;

namespace ZenCart.Controllers
{
    public class ProductController : Controller
    {
        ZenCartEntities1 db = new ZenCartEntities1();
        // GET: Product
        public ActionResult AllProducts()
        {
            var data = db.Products.ToList();

            return View(data);
        }


        public ActionResult Details(int? id)
        {
            if (id == 0)
            {

                return RedirectToAction("Index", "Home");
            }

            var data = db.Products.FirstOrDefault(p => p.P_Id == id);

            if (data == null)
            {

                return HttpNotFound();
            }

            return View(data);
        }

        //public ActionResult ProductsByCategory(string category)
        //{
        //    var products = db.Products.Where(p => p.Category.Name == category).ToList();
        //    return View(products);
        //}

        public ActionResult ProductsByCategory(string category = null)
        {
            IQueryable<Product> products = db.Products;

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Name == category);
            }
           // return View(products);

            return PartialView("_ProductListPartial", products.ToList());
        }


        //public ActionResult ProductsByCategory(int categoryId)
        //{
        //    var products = db.Products.Where(p => p.CategoryId == categoryId).ToList();
        //    return View(products);
        //}


        public ActionResult Search(string query)
        {
            var results = db.Products.Where(p => p.Name.Contains(query) || p.Description.Contains(query)).ToList();
            return View(results);
        }

    }
}
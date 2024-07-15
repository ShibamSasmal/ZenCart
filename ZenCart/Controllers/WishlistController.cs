using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZenCart.Models;

namespace ZenCart.Controllers
{
    public class WishlistController : Controller
    {
        ZenCartEntities1 db = new ZenCartEntities1();
        // GET: Wishlist
        public ActionResult WishListProducts()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var wishlistItems = db.WishlistItems.Where(w => w.UserId == userId).ToList();
            return View(wishlistItems);
        }


        [HttpPost]
        public ActionResult AddToWishlist(int productId)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var existingWishlistItem = db.WishlistItems.SingleOrDefault(w => w.UserId == userId && w.ProductId == productId);

            if (existingWishlistItem != null)
            {
                return  Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var newWishlistItem = new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedDate = DateTime.Now
                };
                db.WishlistItems.Add(newWishlistItem);
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult RemoveFromWishlist(int productId)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var wishlistItem = db.WishlistItems.SingleOrDefault(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem != null)
            {
                db.WishlistItems.Remove(wishlistItem);
                db.SaveChanges();
                return Json(new { success = true, message = "Item removed from wishlist." });
            }

            return Json(new { success = false, message = "Failed to remove item from wishlist." });
        }

    }
}
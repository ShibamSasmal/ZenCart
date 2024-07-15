using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using ZenCart.Models;

namespace ZenCart.Controllers
{
    public class AddressController : Controller
    {
        ZenCartEntities1 db = new ZenCartEntities1();
        // GET: Address
        public ActionResult AddressList()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var address = db.Addresses.Where(a => a.UserId == userId).ToList();
            return View(address);
        }

        public ActionResult AddAddress()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var hasAddress = db.Addresses.Any(a => a.UserId == userId);
            ViewBag.IsFirstAddress = !hasAddress;
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AddAddress(Address address)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var userId = Convert.ToInt32(Session["UserId"]);
        //        address.UserId = userId;
        //        db.Addresses.Add(address);
        //        db.SaveChanges();
        //        if (TempData["OrderId"] is int orderId)
        //        {
        //            if (!db.Addresses.Any(a => a.UserId == userId && a.AddressId != address.AddressId))
        //            {
        //                return RedirectToAction("OrderConfirmation", "Cart", new { orderId });
        //            }
        //        }
        //        return RedirectToAction("AddressList");


        //    }
        //    return View(address);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAddress(Address address)
        {
            if (ModelState.IsValid)
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                address.UserId = userId;
                db.Addresses.Add(address);
                db.SaveChanges();

                // Check if this is the first address for the user
                var isFirstAddress = !db.Addresses.Any(a => a.UserId == userId && a.SelectedAddress);

                // If it's the first address, mark it as selected
                if (isFirstAddress)
                {
                    address.SelectedAddress = true;
                    db.Entry(address).State = EntityState.Modified;
                    db.SaveChanges();
                }

                // Redirect to the appropriate page based on whether there was an order in progress
                int? orderId = TempData["OrderId"] as int?;
                if (orderId != null)
                {
                    return RedirectToAction("OrderConfirmation", "Cart", new { orderId = orderId.Value });
                }
                return RedirectToAction("Payment","Cart");
            }
            return View(address);
        }


        public ActionResult EditAddress(int id)
        {
            Address address = db.Addresses.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAddress(Address address)
        {
            if (ModelState.IsValid)
            {
                var userId = db.Addresses.Where(a => a.AddressId == address.AddressId).Select(a => a.UserId).FirstOrDefault();
                address.UserId = userId;
                db.Entry(address).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("AddressList");
            }
            return View(address);
            
        }

        //public ActionResult Delete(int id)
        //{
        //    Address address = db.Addresses.Find(id);
        //    if (address == null)
        //    {
        //        return HttpNotFound();
        //    }
        //        return View(address);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Address address = db.Addresses.Find(id);
        //    db.Addresses.Remove(address);
        //    db.SaveChanges();
        //    return RedirectToAction("AddressList");
        //}

        public ActionResult Delete(int id)
        {
            var deletedrow = db.Addresses.Where(model =>model.AddressId == id).FirstOrDefault();
            if (deletedrow != null)
            {
                db.Entry(deletedrow).State = EntityState.Deleted;
               int a =  db.SaveChanges();
                if(a > 0)
                {
                    TempData["DeleteMessage"] = "<script>alert('Deleted successfully')</script>";
                    return RedirectToAction("AddressList");
                }
                else
                {
                    TempData["DeleteMessage"] = "<script>alert('Not Deleted successfully')</script>";


                }
            }
            return View(deletedrow);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
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
                return RedirectToAction("Payment", "Cart");
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

        [HttpPost]
        public ActionResult SetSelectedAddress(int addressId)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var addresses = db.Addresses.Where(a => a.UserId == userId).ToList();

            foreach (var address in addresses)
            {
                address.SelectedAddress = address.AddressId == addressId;
                db.Entry(address).State = EntityState.Modified;
            }
            db.SaveChanges();

            return RedirectToAction("AddressList");
        }

        public ActionResult Delete(int id)
        {
            var deletedRow = db.Addresses.FirstOrDefault(model => model.AddressId == id);
            if (deletedRow != null)
            {
                db.Addresses.Remove(deletedRow);
                db.SaveChanges();

                // Handle if the deleted address was the selected one
                var userId = Convert.ToInt32(Session["UserId"]);
                if (deletedRow.SelectedAddress)
                {
                    var remainingAddress = db.Addresses.FirstOrDefault(a => a.UserId == userId);
                    if (remainingAddress != null)
                    {
                        remainingAddress.SelectedAddress = true;
                        db.Entry(remainingAddress).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                TempData["DeleteMessage"] = "<script>alert('Deleted successfully')</script>";
            }
            else
            {
                TempData["DeleteMessage"] = "<script>alert('Not Deleted successfully')</script>";
            }
            return RedirectToAction("AddressList");
        }
    }
}

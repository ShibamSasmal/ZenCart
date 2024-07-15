using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.BuilderProperties;
using ZenCart.Models;

namespace ZenCart.Controllers
{
    public class CartController : Controller
    {
        ZenCartEntities1 db = new ZenCartEntities1();
        // GET: Cart

        public ActionResult Cart()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var cartItems = db.Carts.Include("Product").Include("User").Where(c => c.UserId == userId).ToList();
            return View(cartItems);
        }
        //ADD TO CART
        public ActionResult AddToCart(int productId, string size, int quantity)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var existingCartItem = db.Carts.SingleOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (existingCartItem != null)
            {

                //ViewBag.Message = "This product is already in your cart.";
                //return RedirectToAction("Cart");
                TempData["Message"] = "This product is already in your cart.";
                return RedirectToAction("Cart");
            }
            else
            {
                var newCartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Size = size,
                    Quantity = quantity
                };
                db.Carts.Add(newCartItem);
                db.SaveChanges();
                TempData["CartMessage"] = "product added successfully";

            }

            var cartItems = db.Carts.Include("Product").Include("User").Where(c => c.UserId == userId).ToList();
             //return View("Cart", cartItems);
            return RedirectToAction("Cart");
        }

        //UPDATE CART
        public ActionResult UpdateCart(int cartItemId, int quantity)
        {
            var cartItem = db.Carts.Include("Product").Where(x => x.cartId == cartItemId).FirstOrDefault();

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                db.Entry(cartItem).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Cart");
        }

        //REMOVE CART
        public ActionResult RemoveFromCart(int cartItemId)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var cartItem = db.Carts.Where(c => c.UserId == userId && c.cartId == cartItemId).FirstOrDefault();
            if(cartItem != null)
            {
                db.Carts.Remove(cartItem);
                db.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        //CHECK THE ADDRESS BEFORE ORDER

        [HttpPost]
        public ActionResult CheckAddress()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var cartItems = db.Carts.Include("Product").Where(c => c.UserId == userId).ToList();
            var hasAddress = db.Addresses.Any(a => a.UserId == userId);

            if (!hasAddress)
            {
                // If the user doesn't have any addresses, redirect to the address page
                TempData["Message"] = "Please add your address before placing an order.";
                return RedirectToAction("AddAddress", "Address");
            }
            return RedirectToAction("Payment");

        }

        public ActionResult Payment()
        {
            
            var userId = Convert.ToInt32(Session["UserId"]);
            var cartItems = db.Carts.Include("Product").Where(c => c.UserId == userId).ToList();
            decimal totalAmount = 0;
            foreach (var orders in cartItems)
            {
                totalAmount = totalAmount + (orders.Quantity * orders.Product.Price);

            }
            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = totalAmount;

            return View();
        }
        
        //ORDER

        [HttpPost]
        public ActionResult PlaceOrder()
        {

            var userId = Convert.ToInt32(Session["UserId"]);
            var cartItems = db.Carts.Include("Product").Where(c => c.UserId == userId).ToList();
            var addresssId = db.Addresses.Where(a => a.UserId == userId && a.SelectedAddress == true).Select(s => s.AddressId).FirstOrDefault();
            // Create a new order
            decimal totalAmount = 0;
            foreach (var orders in cartItems)
            {
                totalAmount = totalAmount + (orders.Quantity * orders.Product.Price);

            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                OrderStatus = "Success", // You may want to change this depending on your requirements
                AddressId = addresssId
            };
            db.Orders.Add(order);
            db.SaveChanges();
            // Transfer cart items to order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price,

                };

                db.OrderItems.Add(orderItem);
                db.Carts.Remove(cartItem); // Remove cart item since it's now in the order
            }

            db.SaveChanges();
            TempData["OrderId"] = order.OrderId;
            return RedirectToAction("ThankYou", new { orderId = order.OrderId });

            
        }


        // GET: /Cart/OrderConfirmation
        public ActionResult OrderConfirmation()
        {
            // Retrieve orderId from TempData
            int orderId;
            if (TempData["OrderId"] != null && int.TryParse(TempData["OrderId"].ToString(), out orderId))
            {
                // Display order confirmation view with the order details
                var order = db.Orders.Find(orderId);
                return View(order);
            }
            else
            {
                // Handle the case where orderId is not available in TempData
                // For example, redirect to an error page or display an error message
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult ThankYou(int orderId)
        {
            var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product))
                                 .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
            {
                return HttpNotFound();
            }

            return View(order);
        }


    }
}
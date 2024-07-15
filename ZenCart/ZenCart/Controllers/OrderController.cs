using System.Web.Mvc;
using System;
using ZenCart.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using NHibernate.Mapping;

public class OrderController : Controller
{
    private ZenCartEntities1 db = new ZenCartEntities1();

    // GET: Order/AllOrders
    //public ActionResult AllOrders()
    //{
    //    // Check if the user is authenticated
    //    if (Session["UserId"] == null)
    //    {
    //        // Redirect to login page or show error message
    //        return RedirectToAction("Login", "Account");
    //    }

    //    // Get the user ID from the session
    //    var userId = Convert.ToInt32(Session["UserId"]);

    //    // Retrieve all order items for the user
    //    var orderItems = db.OrderItems
    //                        .Where(o => o.Order.UserId == userId) // Filter orders by user ID
    //                        .Include(o => o.Product) // Include the Product for each OrderItem
    //                        .ToList();

    //    // Pass order items to the view
    //    return View(orderItems);
    //}

    public ActionResult AllOrders()
    {
        // Check if the user is authenticated
        if (Session["UserId"] == null)
        {
            // Redirect to login page or show error message
            return RedirectToAction("Login", "Account");
        }

        // Get the user ID from the session
        var userId = Convert.ToInt32(Session["UserId"]);

        // Retrieve all order items for the user along with order status
        var orderItems = db.OrderItems
                            .Where(o => o.Order.UserId == userId) // Filter orders by user ID
                            .Include(o => o.Product) // Include the Product for each OrderItem
                            .Include(o => o.Order) // Include the Order for each OrderItem
                            .OrderByDescending(o => o.Order.OrderDate)
                            .ToList();

        // Pass order items to the view
        return View(orderItems);
    }

    [HttpPost]
    public ActionResult CheckAddress()
    {
        var userId = Convert.ToInt32(Session["UserId"]);
        var hasAddress = db.Addresses.Any(a => a.UserId == userId);

        if (!hasAddress)
        {
            // If the user doesn't have any addresses, redirect to the address page
            TempData["Message"] = "Please add your address before placing an order.";
            return RedirectToAction("AddAddress", "Address");
        }
        return RedirectToAction("Payment");
    }

    public ActionResult Payment(int productId, int quantity)
    {
        var userId = Convert.ToInt32(Session["UserId"]);
        var product = db.Products.Find(productId);

        if (product == null)
        {
            return HttpNotFound();
        }

        decimal totalAmount = product.Price * quantity;
        ViewBag.Product = product;
        ViewBag.Quantity = quantity;
        ViewBag.TotalAmount = totalAmount;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult AddOrder(int productId, int quantity)
    {
        // Check if the user is authenticated
        if (Session["UserId"] == null)
        {
            // Redirect to login page or show error message
            return RedirectToAction("Login", "Account");
        }

        // Get the user ID from the session
        var userId = Convert.ToInt32(Session["UserId"]);
        var selectedAddressId = db.Addresses.Where(a => a.UserId == userId && a.SelectedAddress).Select(a => a.AddressId).FirstOrDefault();
        if (selectedAddressId == 0)
        {
            TempData["Message"] = "Please select or add an address before placing an order.";
            return RedirectToAction("AddAddress", "Address");
        }

        var product = db.Products.Find(productId);
        if (product == null)
        {
            return HttpNotFound();
        }

        decimal totalAmount = product.Price * quantity;

        try
        {
            // Create a new order object
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                AddressId = selectedAddressId,
                OrderStatus = "Success",
                
            };

            // Add the order to the database
            db.Orders.Add(order);
            db.SaveChanges();

            // Add the order item to the database
            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price
            };

            db.OrderItems.Add(orderItem);
            db.SaveChanges();

            // Redirect to Order Confirmation
            return RedirectToAction("ThankYou", new { orderId = order.OrderId });
        }
        catch
        {
            // Log the exception or show an error message
            ModelState.AddModelError("", "An error occurred while placing the order.");
        }

        return View("Payment", new { productId, quantity });
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

    public ActionResult OrderConfirmation(int orderId)
    {
        var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderId == orderId);

        if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
        {
            return HttpNotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult CancelOrder(int orderId)
    {
        // Check if the user is authenticated
        if (Session["UserId"] == null)
        {
            // Redirect to login page or show error message
            return Json(new { success = false, message = "User not authenticated" });
        }

        var userId = Convert.ToInt32(Session["UserId"]);
        var order = db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

        if (order == null)
        {
            return Json(new { success = false, message = "Order not found" });
        }

        // Log the current order status
        System.Diagnostics.Debug.WriteLine("Order Status: " + order.OrderStatus);

        if (order.OrderStatus.Trim() != "Success")
        {
            return Json(new { success = false, message = "Only Success orders can be cancelled" });
        }

        try
        {
            // Update order status to Cancelled
            order.OrderStatus = "Cancelled";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            // Return success response
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            // Log the exception
            // For debugging purposes, return the exception message
            return Json(new { success = false, message = ex.Message });
        }
    }



    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public ActionResult CancelOrder(int orderId)
    //{
    //    // Check if the user is authenticated
    //    if (Session["UserId"] == null)
    //    {
    //        // Redirect to login page or show error message
    //        return RedirectToAction("Login", "Account");
    //    }

    //    var userId = Convert.ToInt32(Session["UserId"]);
    //    var order = db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

    //    if (order == null)
    //    {
    //        return HttpNotFound();
    //    }

    //    if (order.OrderStatus != "Pending")
    //    {
    //        ModelState.AddModelError("", "Only pending orders can be cancelled.");
    //        return View("OrderDetails", order);
    //    }

    //    try
    //    {
    //        // Update order status to Cancelled
    //        order.OrderStatus = "Cancelled";
    //        db.Entry(order).State = EntityState.Modified;
    //        db.SaveChanges();

    //        // Additional logic for handling the cancellation (e.g., refund, inventory adjustment) can be added here

    //        // Redirect to the Order Details or AllOrders page
    //        return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
    //    }
    //    catch
    //    {
    //        ModelState.AddModelError("", "An error occurred while cancelling the order.");
    //        return View("OrderDetails", order);
    //    }
    //}

    public ActionResult OrderDetails(int orderId)
    {
        var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderId == orderId);

        if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
        {
            return HttpNotFound();
        }

        return View(order);
    }



}

using System;
using System.Linq;
using System.Web.Mvc;
using ZenCart.Models;
using System.Data.Entity;

public class OrderController : Controller
{
    private ZenCartEntities1 db = new ZenCartEntities1();

    // AllOrders Action
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


    // Check Address Action
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

    // Payment Action
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

    // AddOrder Action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult AddOrder(int productId, int quantity, string paymentMethod)
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

            // Handle payment method
            switch (paymentMethod)
            {
                case "CashOnDelivery":
                    order.OrderStatus = "Success";
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("ThankYou", new { orderId = order.OrderId });

                case "UPI":
                    return RedirectToAction("UPIPayment", new { orderId = order.OrderId });

                case "NetBanking":
                    return RedirectToAction("NetBankingPayment", new { orderId = order.OrderId });

                default:
                    TempData["Error"] = "Invalid payment method selected.";
                    return RedirectToAction("Payment", new { productId = productId, quantity = quantity });
            }
        }
        catch
        {
            // Log the exception or show an error message
            ModelState.AddModelError("", "An error occurred while placing the order.");
        }

        return View("Payment", new { productId, quantity });
    }

    // UPI Payment Action
    public ActionResult UPIPayment(int orderId)
    {
        var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderId == orderId);

        if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
        {
            return HttpNotFound();
        }

        ViewBag.TotalAmount = order.TotalAmount;
        return View(order);
    }

    // Process UPI Payment Action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ProcessUPIPayment(string upiId, int orderId)
    {
        var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderId == orderId);
        if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
        {
            return HttpNotFound();
        }

        // Simulate UPI payment processing logic
        // In a real-world application, you would integrate with a UPI payment gateway here

        // For demonstration purposes, we'll assume the payment is successful
        bool paymentSuccessful = true;

        if (paymentSuccessful)
        {
            order.OrderStatus = "Success";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ThankYou", new { orderId = order.OrderId });
        }
        else
        {
            TempData["Error"] = "UPI payment failed. Please try again.";
            return RedirectToAction("UPIPayment", new { orderId = order.OrderId });
        }
    }

    // NetBanking Payment Action
    public ActionResult NetBankingPayment(int orderId)
    {
        var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderId == orderId);

        if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
        {
            return HttpNotFound();
        }

        ViewBag.TotalAmount = order.TotalAmount;
        return View(order);
    }

    // Process NetBanking Payment Action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ProcessNetBankingPayment(string bankName, int orderId)
    {
        var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderId == orderId);
        if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
        {
            return HttpNotFound();
        }

        // Simulate NetBanking payment processing logic
        // In a real-world application, you would integrate with a NetBanking payment gateway here

        // For demonstration purposes, we'll assume the payment is successful
        bool paymentSuccessful = true;

        if (paymentSuccessful)
        {
            order.OrderStatus = "Success";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ThankYou", new { orderId = order.OrderId });
        }
        else
        {
            TempData["Error"] = "NetBanking payment failed. Please try again.";
            return RedirectToAction("NetBankingPayment", new { orderId = order.OrderId });
        }
    }

    // Thank You Action
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

    // OrderDetails Action
    public ActionResult OrderDetails(int orderId)
    {
        var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderId == orderId);

        if (order == null || order.UserId != Convert.ToInt32(Session["UserId"]))
        {
            return HttpNotFound();
        }

        return View(order);
    }

    // Cancel Order Action
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
            // Return error response
            return Json(new { success = false, message = "An error occurred while cancelling the order" });
        }
    }
}

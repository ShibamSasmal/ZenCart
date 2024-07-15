using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using ZenCart.Models.Home;

namespace ZenCart.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendEmail(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SendEmailToRecipient(contact);
                    TempData["SuccessMessage"] = "Your message has been sent successfully!";
                    return RedirectToAction("Contact");
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it appropriately
                    ViewBag.ErrorMessage = "An error occurred while sending the email. Please try again later.";
                    return View("Contact", contact);
                }
            }
            return View("Contact", contact);
        }

        private void SendEmailToRecipient(Contact contact)
        {
            using (MailMessage message = new MailMessage())
            {
                message.To.Add("zencart487@gmail.com");
                message.Subject = "Thanks For Contact Us";
                message.From = new MailAddress("your_gmail_username@gmail.com");
                message.Body = $"From: {contact.Email}\n\n{contact.Message}";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("zencart487@gmail.com", "olyv rgdo zbev shpj");
                    smtp.EnableSsl = true;

                    smtp.Send(message);
                }
            }
        }


    }
}
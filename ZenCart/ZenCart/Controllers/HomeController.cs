using System;
using System.Collections.Generic;
using System.Linq;
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
                
                SendEmailToRecipient(contact);
                return RedirectToAction("Contact", "Home");
            }
            return View("Contact", contact); 
        }

        private void SendEmailToRecipient(Contact contact) 
        {
            
            MailMessage message = new MailMessage();
            message.To.Add("shibamsasmal9@.com");
            message.Subject = "New Contact Message";
            message.From = new MailAddress(contact.Email); 
            message.Body = contact.Message; 

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.example.com";
            smtp.Port = 587;
            smtp.Credentials = new System.Net.NetworkCredential("username", "password");
            smtp.EnableSsl = true;
            

            smtp.Send(message);
        }

    }
}
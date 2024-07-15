using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Identity;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using ZenCart.Models;

namespace ZenCart.Controllers
{
    public class AccountController : Controller
    {
        ZenCartEntities1 db = new ZenCartEntities1();
        
        // GET: Account
        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLoginDto u)
        {
            if (ModelState.IsValid)
            {
                var userDetails = db.Users.Where(user => user.Email == u.Email && user.PasswordHash == u.Password).FirstOrDefault();

                if (userDetails != null)
                {
                    ViewBag.LoginSuccess = true;

                    Session["UserId"] = userDetails.Id.ToString();
                    Session["Username"] = userDetails.Username.ToString();
                    TempData["CurrentUserId"] = userDetails.Id.ToString();
                    TempData["CurrentUserName"] = userDetails.Username.ToString();
                   
                    return RedirectToAction("Index", "Home");
                }
                else
                {

                    //ModelState.AddModelError("", "Invalid username or password");
                    u.ErrorMessage = "Email or Password not matched";
                }
            }
            return View(u);
        }

        public ActionResult Register()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Register(User u)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        db.Users.Add(u);
        //        db.SaveChanges();
        //        return RedirectToAction("Login", "Account");
        //    }
        //    return View();
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User u)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(u);
                db.SaveChanges();
                SendRegistrationEmail(u.Email, u.Username);
                return RedirectToAction("Login", "Account");
            }
            return View(u);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); 
            return RedirectToAction("Login", "Account");
        }



        public ActionResult Profiles()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userProfile = new UserProfile
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Mobile_No = user.Mobile_No,
                
            };

            return View(userProfile);
        }

   
        public ActionResult EditProfile()
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userProfile = new UserProfile
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Mobile_No = user.Mobile_No,
                
            };

            return View(userProfile);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(UserProfile u)
        {

            if (ModelState.IsValid)
            {
                var userId = Convert.ToInt32(Session["UserId"]);
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                //var user = db.Users.FirstOrDefault(o => o.Id == u.Id);
                if (user != null)
                {
                    user.Username = u.Username;
                    user.Email = u.Email;
                    user.Mobile_No = u.Mobile_No;
                    db.SaveChanges();

                    TempData["UpdateMessage"] = "<script>alert('Data Updated successfully')</script>";
                    return RedirectToAction("Profiles");
                }
                else
                {
                    TempData["UpdateMessage"] = "<script>alert('User not found')</script>";
                }
            }

            return View(u);
        }

        // GET: ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                ViewBag.Message = "Please provide a valid email address.";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                try
                {
                    // Generate and store a password reset token
                    string token = Guid.NewGuid().ToString();
                    var resetToken = new ResetToken
                    {
                        UserId = user.Id,
                        ResetToken1 = token,
                        ExpirationDate = DateTime.UtcNow.AddHours(1) // Token expires in 1 hour
                    };
                    db.ResetTokens.Add(resetToken);
                    db.SaveChanges();

                    // Send email with reset link
                    SendPasswordResetEmail(user.Email, token);

                    ViewBag.Message = "If the email exists in our system, you will receive instructions to reset your password.";
                }
                catch (Exception ex)
                {
                    // Log the error
                    ViewBag.Message = "An error occurred while processing your request. Please try again later.";
                    LogError($"Error generating password reset token for {email}: {ex.Message}");
                }
            }
            else
            {
                ViewBag.Message = "If the email exists in our system, you will receive instructions to reset your password.";
            }

            return View();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void SendPasswordResetEmail(string email, string token)
        {
            try
            {
                // Generate the reset link
                var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Url.Scheme);

                // Create the mail message
                var mailMessage = new MailMessage("zencart487@gmail.com", email)
                {
                    Subject = "Reset your password",
                    Body = $"Please reset your password by clicking here: {resetLink}",
                    IsBodyHtml = true
                };

                // Set up the SMTP client
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587; // Use port 587 for TLS
                    smtpClient.Credentials = new System.Net.NetworkCredential("zencart487@gmail.com", "olyv rgdo zbev shpj"); // Use your Gmail email address and App Password
                    smtpClient.EnableSsl = true; // Enable SSL/TLS
                    // Send the email
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError($"Error sending email to {email}: {ex.Message}");
                throw; // Rethrow the exception to handle it further if necessary
            }
        }


        private void LogError(string errorMessage)
        {
            // Implement your logging mechanism here, such as writing to a log file or database
            // For demonstration purposes, we'll print the error to the console
            Console.WriteLine(errorMessage);
        }

        private void SendRegistrationEmail(string email, string username)
        {
            try
            {
                var mailMessage = new MailMessage("zencart487@gmail.com", email)
                {
                    Subject = "Welcome to ZenCart",
                    Body = $"Hello {username},<br/><br/>Thank you for registering at ZenCart! We are excited to have you.<br/><br/>Best Regards,<br/>The ZenCart Team",
                    IsBodyHtml = true
                };

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new System.Net.NetworkCredential("zencart487@gmail.com", "olyv rgdo zbev shpj");
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error sending registration email to {email}: {ex.Message}");
                throw;
            }
        }
        public ActionResult ResetPassword(string token)
        {
            // Find the reset token in the database
            var resetToken = db.ResetTokens.FirstOrDefault(rt => rt.ResetToken1 == token && rt.ExpirationDate > DateTime.UtcNow);
            if (resetToken != null)
            {
                var user = db.Users.Find(resetToken.UserId);
                if (user != null)
                {
                    // Create a view model to hold token and new password
                    var model = new ResetPassword
                    {
                        Token = token
                    };
                    ViewBag.Token = token;

                    return View(model);
                }
            }

            // Invalid or expired reset token
            ViewBag.Message = "Invalid or expired reset token.";
            return View("Error");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPassword model)
        {
            if (ModelState.IsValid)
            {
                // Check if the token is provided
                if (string.IsNullOrEmpty(model.Token))
                {
                    ViewBag.Message = "Invalid reset token.";
                    return View("Error");
                }

                try
                {
                    // Find the reset token in the database
                    var resetToken = db.ResetTokens.FirstOrDefault(t => t.ResetToken1 == model.Token && t.ExpirationDate > DateTime.UtcNow);
                    if (resetToken == null)
                    {
                        ViewBag.Message = "Invalid or expired reset token.";
                        return View("Error");
                    }

                    // Find the user associated with the reset token
                    var user = db.Users.Find(resetToken.UserId);
                    if (user == null)
                    {
                        ViewBag.Message = "User not found.";
                        return View("Error");
                    }

                    // Reset the user's password
                    user.PasswordHash = model.NewPassword; // Consider hashing the password before saving it
                    db.ResetTokens.Remove(resetToken); // Remove the reset token from the database
                    db.SaveChanges();

                    // Return a JSON response indicating success
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    // Log the error
                    LogError($"Error resetting password: {ex.Message}");

                    // Return error view
                    ViewBag.Message = "An error occurred while resetting the password. Please try again later.";
                    return View("Error");
                }
            }

            // If ModelState is not valid, redisplay the form with validation errors
            return View(model);
        }
    }
}

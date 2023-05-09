using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.VisualBasic.ApplicationServices;
using Shopping_Cart_Assignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Owin.Security.DataProtection;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Threading;

namespace Shopping_Cart_Assignment.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ShoppingCartContext db = new ShoppingCartContext();
        private string code;
        public ActionResult Login()
        {
            UserModel model = new UserModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserModel model)
        {
            UserModel u = model;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            UserStore<ApplicationUser> Store = new UserStore<ApplicationUser>(new ApplicationDbContext());
            ApplicationUserManager userManager = new ApplicationUserManager(Store);
            var usercheck = await userManager.FindByEmailAsync(model.Email);
            if (usercheck != null)
            {
                if (!await userManager.IsEmailConfirmedAsync(usercheck.Id))
                {
                    ModelState.AddModelError("", "You must have a confirmed email to log on.");
                    return View(model);
                }
            }

            UserDetails user = db.Users.Where(x => x.Email == model.Email).FirstOrDefault();

            if (user != null)
            {
                model.Password = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password)));
                if (user.Password == model.Password)
                {
                    if (user.Role.ToLower() == "admin")
                    {
                        Session["username"] = user.UserName;
                        FormsAuthentication.SetAuthCookie(user.UserName, false);
                        return RedirectToAction("Index", "Home");
                    }
                    if (user.Role.ToLower() == "user")
                    {
                        Session["ID"] = user.Id;
                        Session["username"] = user.UserName;
                        FormsAuthentication.SetAuthCookie(user.UserName, false);
                        return RedirectToAction("Index", "Customer");
                    }

                }
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
        }
       
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["username"] = null;
            return RedirectToAction("Login");
        }

        public ActionResult Create()
        {
            return View(new UserDetails());
        }


        [HttpPost]
        public async Task<ActionResult> Create(UserDetails model)
        {
            if (ModelState.IsValid)
            {
                string email = model.Email;
                Regex regex = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z");
                Match match = regex.Match(email);
                if (!match.Success)
                {
                    ModelState.AddModelError("", "Enter a valid Email");
                    return View(model);
                }
                UserStore<ApplicationUser> Store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                ApplicationUserManager userManager = new ApplicationUserManager(Store);
                var check = await userManager.Users.AnyAsync(x=>x.Email == model.Email);
                if(check)
                {
                    ModelState.AddModelError("", "Email Already Exists");
                    return View(model);
                }
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    var provider = new DpapiDataProtectionProvider("Shopping Cart Assignment");
                    userManager.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<ApplicationUser, string>(provider.Create("EmailConfirmation")) as IUserTokenProvider<ApplicationUser, string>;
                    code = await userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    TempData["code"] = code;
                    TempData.Keep();
                    var callbackUrl = Url.Action("ConfirmEmail", "Authentication", new { userId = user.Id, token = code }, protocol: Request.Url.Scheme);
                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("/Content/AccountConfirmation.html")))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{ConfirmationLink}", callbackUrl);
                    body = body.Replace("{UserName}", model.Email);
                    bool IsSendEmail = EmailSend(model.Email, "Confirm your account", body, true);
                    if (IsSendEmail)
                    {
                        var result1 = userManager.AddToRole(user.Id, "user");
                        model.Role = "user";
                        model.Password = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password)));
                        model.ConfirmPassword = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(model.ConfirmPassword)));
                        db.Users.Add(model);
                        db.SaveChanges();
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        return View(model);
                    }
                   
                }
                AddErrors(result);
            }

           
            return View(model);
        }

        private void AddErrors(Microsoft.AspNet.Identity.IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
       

        public static bool EmailSend(string SenderEmail, string Subject, string Message, bool IsBodyHtml = false)
        {
            bool status = false;
            try
            {
                string HostAddress = ConfigurationManager.AppSettings["Host"].ToString();
                string FormEmailId = ConfigurationManager.AppSettings["MailFrom"].ToString();
                string Password = ConfigurationManager.AppSettings["Password"].ToString();
                string Port = ConfigurationManager.AppSettings["Port"].ToString();
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(FormEmailId);
                mailMessage.Subject = Subject;
                mailMessage.Body = Message;
                mailMessage.IsBodyHtml = IsBodyHtml;
                mailMessage.To.Add(new MailAddress(SenderEmail));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = HostAddress;
                smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential();
                networkCredential.UserName = mailMessage.From.Address;
                networkCredential.Password = Password;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = networkCredential;
                smtp.Port = Convert.ToInt32(Port);
                smtp.Send(mailMessage);
                status = true;
                return status;
            }
            catch (Exception e)
            {
                return status;
            }
        }

        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null) 
            {
                return RedirectToAction("Login", "Authentication");
            }
            UserStore<ApplicationUser> Store = new UserStore<ApplicationUser>(new ApplicationDbContext());
            ApplicationUserManager userManager = new ApplicationUserManager(Store);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                ViewBag.ErrorMessage = $"The user id {userId} is invalid ";
                return View("Not Found");
            }
            var provider = new DpapiDataProtectionProvider("Shopping Cart Assignment");
            userManager.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<ApplicationUser, string>(provider.Create("EmailConfirmation")) as IUserTokenProvider<ApplicationUser, string>;
            var result = await userManager.ConfirmEmailAsync(userId, token);

            if (result.Succeeded)
            {
                return View();
            }
            //if (TempData["code"].ToString() ==token)
            //{
            //    //user.EmailConfirmed = true;
            //    await Store.SetEmailConfirmedAsync(user, confirmed: true);
            //    return RedirectToAction("Login");
               
            //}
            ViewBag.ErrorMessage = "Cannot Verify the Email";
            return View("Error");
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tarik_Gegic.Models;

namespace Tarik_Gegic.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        public ActionResult Index()
        {
            return View();
        }

        //For upload picture in Content/Image folder
        [HttpPost]
        public ActionResult Index(Picture picture)
        {
            //This is just for the Authenticated users to upload picture
            if(User.Identity.IsAuthenticated)
            {
                if (picture.File.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(picture.File.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Image"), fileName);
                    picture.File.SaveAs(path);
                }
            }
            else if(User.Identity.IsAuthenticated){

            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }

        //LogIn if you have created user
        [HttpPost]
        public ActionResult LogIn(Models.UserModel user)
        {
            if(ModelState.IsValid)
            {
                if (IsValid(user.Email, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(user.Email, false);
                    return RedirectToAction("Index", "User");
                }
                else 
                {
                    ModelState.AddModelError("","Login Data is incorrect");
                }
               
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration new user
        [HttpPost]
        public ActionResult Registration(Models.UserModel user)
        {
            if (ModelState.IsValid)
            {
                using (var db = new MyMainDbEntities())
                {
                    var crypto = new SimpleCrypto.PBKDF2();

                    var encrpPass = crypto.Compute(user.Password);

                    var sysUser = db.SystemUsers.Create();

                    sysUser.Email = user.Email;
                    sysUser.Password = encrpPass;
                    sysUser.PasswordSalt = crypto.Salt;
                    sysUser.UserId = Guid.NewGuid();

                    db.SystemUsers.Add(sysUser);
                    db.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("","Login Data is incorrect");
            }
            return View(user);
        }

        //
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index","Home");
        }

        //Here is for password, that the password is encrypted for security
        private bool IsValid(string email, string password)
        {
            var crypto = new SimpleCrypto.PBKDF2();

            bool isValid = false;

            using(var db = new MyMainDbEntities())
            {
                var user = db.SystemUsers.FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    if (user.Password == crypto.Compute(password, user.PasswordSalt))
                    {
                        isValid = true;
                    }
                }
            }
            return isValid;
        }

    }
}

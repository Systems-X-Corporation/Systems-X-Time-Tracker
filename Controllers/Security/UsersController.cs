using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Helpers;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Security
{
    public class UsersController : SystemController
    {
        public ActionResult Users(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Security/Users/Users.cshtml");

        }

        public ActionResult ListUsers()
        {
            var model = db.Users.ToList();
            return PartialView("~/Views/Security/Users/_ListUsers.cshtml", model);
        }

        public ActionResult CreateUsers()
        {

            return PartialView("~/Views/Security/Users/_CreateUsers.cshtml");

        }


        public ActionResult UsersAddNew(FormCollection formCollection)
        {
            var model = db.Users;
            Models.Users newUsers = new Models.Users();
            string password = CreatePassword(8);
            Crypto crypto = new Crypto();
            try
            {

                Company company = db.Company.FirstOrDefault();
                newUsers.Company = company;
                int officeId = Convert.ToInt32(formCollection["OfficeId"]);
                if (officeId > 0)
                {
                    var office = db.Office.FirstOrDefault(x => x.OfficeId == officeId);
                    newUsers.Office = office;
                }
                string mail = formCollection["Email"];
                newUsers.Email = mail;
                newUsers.UserName = formCollection["UserName"];
                newUsers.FirstName = formCollection["FirstName"];
                newUsers.LastName = formCollection["LastName"];
                newUsers.EmailConfirmed = false;
                newUsers.LockoutEnable = "1";
                newUsers.LockoutEndDate = System.DateTime.Now.AddMonths(6);
                newUsers.PasswordCry = crypto.Encrypt(password);
                bool active = false;
                if (formCollection["customCheck"] == "on")
                {
                    active = true;
                }
                newUsers.Active = active;

                model.Add(newUsers);
                db.SaveChanges();

                if (officeId > 0)
                {
                    var officeProjects = db.Project.Where(p => p.Customer.Office.OfficeId == officeId).ToList();
                    foreach (var proj in officeProjects)
                    {
                        UsersProject up = new UsersProject();
                        up.Project = proj;
                        up.Users = newUsers;
                        db.UsersProject.Add(up);
                    }
                    db.SaveChanges();
                }

                var Roles = formCollection["cboRole"].Split(',');
                
                if(Roles.Length > 0)
                {
                    var RoleModel = db.RolesUsers;
                    foreach (var Role in Roles)
                    {
                        RolesUsers rolesUsers = new RolesUsers();
                        var RoleId = Convert.ToInt32(Role);
                        var _role = db.Roles.Where(x=> x.RoleId == RoleId).FirstOrDefault();
                        rolesUsers.Roles = _role;
                        rolesUsers.Users = newUsers;
                        RoleModel.Add(rolesUsers);
                        db.SaveChanges();
                    }
                }




                Mail _mail = new Mail();

                string fullname = formCollection["FirstName"] + " " + formCollection["LastName"];
                string url = "https://www.sx-timetracker.com/";
                //string response = _mail.sendMail(mail_account, "No-Reply Systems X", mail, fullname, "New User", _mail.newUserTemplate(company.CompanyName, fullname, url, password, mail), true,
                        //"174.138.177.202", false, 587, mail_account, mail_pass);

                string response = _mail.sendMailAzure(mail_connectionString, "New User", _mail.newUserTemplate(company.CompanyName, fullname, url, password, mail), mail_SenderDontReply, mail);

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Security/Users/_ListUsers.cshtml", model.ToList());

        }

        public ActionResult UsersUpdate(FormCollection formCollection)
        {
            var model = db.Users;

            try
            {

                int UsersId = Convert.ToInt16(formCollection["EdUsersId"].ToString());

                Users newUsers = db.Users.FirstOrDefault(x => x.UserId == UsersId);

                Company company = db.Company.FirstOrDefault();
                newUsers.Company = company;
                int officeId = Convert.ToInt32(formCollection["eOfficeId"]);
                if (officeId > 0)
                {
                    var office = db.Office.FirstOrDefault(x => x.OfficeId == officeId);
                    newUsers.Office = office;
                }
                newUsers.Email = formCollection["eEmail"];
                newUsers.UserName = formCollection["eUserName"];
                newUsers.FirstName = formCollection["eFirstName"];
                newUsers.LastName = formCollection["eLastName"];
                bool active = false;
                if (formCollection["EActive"] == "on")
                {
                    active = true;
                }
                newUsers.Active = active;

                db.Entry(newUsers).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                if (officeId > 0)
                {
                    var userProjects = db.UsersProject.Where(x => x.UserId == UsersId).ToList();
                    db.UsersProject.RemoveRange(userProjects);
                    db.SaveChanges();

                    var officeProjects = db.Project.Where(p => p.Customer.Office.OfficeId == officeId).ToList();
                    foreach (var proj in officeProjects)
                    {
                        UsersProject up = new UsersProject();
                        up.Project = proj;
                        up.Users = newUsers;
                        db.UsersProject.Add(up);
                    }
                    db.SaveChanges();
                }

                var RoleModel = db.RolesUsers;
                var _roles = db.RolesUsers.Where(x => x.Users.UserId == UsersId).ToList();
                RoleModel.RemoveRange(_roles);
                db.SaveChanges();
                var Roles = formCollection["cboERole"].Split(',');

                if (Roles.Length > 0)
                {
                    
                    foreach (var Role in Roles)
                    {
                        RolesUsers rolesUsers = new RolesUsers();
                        var RoleId = Convert.ToInt32(Role);
                        var _role = db.Roles.Where(x => x.RoleId == RoleId).FirstOrDefault();
                        rolesUsers.Roles = _role;
                        rolesUsers.Users = newUsers;
                        RoleModel.Add(rolesUsers);
                        db.SaveChanges();
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Security/Users/_ListUsers.cshtml", model.ToList());

        }


        public ActionResult DeleteUsers(string id)
        {
            try
            {
                int UsersId = Convert.ToInt32(id);


                using (var ctx = db)
                {
                    var u = (from y in ctx.RolesUsers
                             where y.UserId == UsersId
                             select y).FirstOrDefault();
                    ctx.RolesUsers.Remove(u);
                    ctx.SaveChanges();
                }




                using (var ctx = db)
                {
                    var x = (from y in ctx.Users
                             where y.UserId == UsersId
                             select y).FirstOrDefault();
                    ctx.Users.Remove(x);
                    ctx.SaveChanges();
                }
                var model = db.Users;
                return PartialView("~/Views/Security/Users/_ListUsers.cshtml", model.ToList());
            }
            catch (Exception e)
            {
                var model = db.Users;
                ViewData["EditError"] = e.Message;
                return PartialView("~/Views/Security/Users/_ListUsers.cshtml", model.ToList());
            }
        }



        public JsonResult GetUsers(int id)
        {
            try
            {
                var data = db.Users.Where(x => x.UserId == id).FirstOrDefault();
                var role = db.RolesUsers.Where(z => z.Users.UserId == id).Select(x=> x.Roles.RoleId).ToList();
                
                return Json(new { data.UserId, data.Email, data.Company.CompanyId, data.UserName, data.FirstName, data.Active, data.LastName, role, data.OfficeId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public JsonResult UnlockUser(int id)
        {
            try
            {
                var _user = db.Users.Where(x => x.UserId == id).FirstOrDefault();
                _user.AccessFailedCount = 0;
                db.Entry(_user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new { response = "User Unlock Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }
}
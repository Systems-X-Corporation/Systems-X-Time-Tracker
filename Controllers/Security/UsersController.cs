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
                newUsers.FactorEndDate = System.DateTime.Now.AddMonths(6);
                newUsers.AccessFailedCount = 0;
                newUsers.IsSuperUser = false;
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
                    string selectedProjects = formCollection["SelectedProjects"];
                    if (!string.IsNullOrEmpty(selectedProjects))
                    {
                        var projectIds = selectedProjects.Split(',').Select(int.Parse).ToList();
                        var projects = db.Project.Where(p => projectIds.Contains(p.ProjectId)).ToList();
                        
                        foreach (var proj in projects)
                        {
                            UsersProject up = new UsersProject();
                            up.Project = proj;
                            up.Users = newUsers;
                            db.UsersProject.Add(up);
                        }
                        db.SaveChanges();
                    }
                }

                var rolesString = formCollection["cboRole"];
                if (!string.IsNullOrEmpty(rolesString))
                {
                    var Roles = rolesString.Split(',');
                    
                    if(Roles.Length > 0)
                    {
                        var RoleModel = db.RolesUsers;
                        foreach (var Role in Roles)
                        {
                            if (!string.IsNullOrEmpty(Role))
                            {
                                RolesUsers rolesUsers = new RolesUsers();
                                var RoleId = Convert.ToInt32(Role);
                                var _role = db.Roles.Where(x=> x.RoleId == RoleId).FirstOrDefault();
                                if (_role != null)
                                {
                                    rolesUsers.Roles = _role;
                                    rolesUsers.Users = newUsers;
                                    RoleModel.Add(rolesUsers);
                                    db.SaveChanges();
                                }
                            }
                        }
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

                    string selectedProjects = formCollection["SelectedEditProjects"];
                    if (!string.IsNullOrEmpty(selectedProjects))
                    {
                        var projectIds = selectedProjects.Split(',').Select(int.Parse).ToList();
                        var projects = db.Project.Where(p => projectIds.Contains(p.ProjectId)).ToList();
                        
                        foreach (var proj in projects)
                        {
                            UsersProject up = new UsersProject();
                            up.Project = proj;
                            up.Users = newUsers;
                            db.UsersProject.Add(up);
                        }
                        db.SaveChanges();
                    }
                }

                var RoleModel = db.RolesUsers;
                var _roles = db.RolesUsers.Where(x => x.Users.UserId == UsersId).ToList();
                RoleModel.RemoveRange(_roles);
                db.SaveChanges();
                
                var rolesString = formCollection["cboERole"];
                if (!string.IsNullOrEmpty(rolesString))
                {
                    var Roles = rolesString.Split(',');

                    if (Roles.Length > 0)
                    {
                        
                        foreach (var Role in Roles)
                        {
                            if (!string.IsNullOrEmpty(Role))
                            {
                                RolesUsers rolesUsers = new RolesUsers();
                                var RoleId = Convert.ToInt32(Role);
                                var _role = db.Roles.Where(x => x.RoleId == RoleId).FirstOrDefault();
                                if (_role != null)
                                {
                                    rolesUsers.Roles = _role;
                                    rolesUsers.Users = newUsers;
                                    RoleModel.Add(rolesUsers);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Security/Users/_ListUsers.cshtml", model.ToList());

        }


        public JsonResult DeleteUsers(string id)
        {
            try
            {
                int UsersId = Convert.ToInt32(id);

                // Delete TimeHours first (this was causing the constraint error)
                var timeHours = db.TimeHours.Where(x => x.UserId == UsersId).ToList();
                if (timeHours.Any())
                {
                    db.TimeHours.RemoveRange(timeHours);
                    db.SaveChanges();
                }

                // Delete ProjectManager records
                var projectManagers = db.ProjectManager.Where(x => x.UserId == UsersId).ToList();
                if (projectManagers.Any())
                {
                    db.ProjectManager.RemoveRange(projectManagers);
                    db.SaveChanges();
                }

                // Delete Privilege_User records
                var privilegeUsers = db.Privilege_User.Where(x => x.UserId == UsersId).ToList();
                if (privilegeUsers.Any())
                {
                    db.Privilege_User.RemoveRange(privilegeUsers);
                    db.SaveChanges();
                }

                // Delete user projects
                var userProjects = db.UsersProject.Where(x => x.UserId == UsersId).ToList();
                if (userProjects.Any())
                {
                    db.UsersProject.RemoveRange(userProjects);
                    db.SaveChanges();
                }

                // Delete user roles
                var userRoles = db.RolesUsers.Where(y => y.UserId == UsersId).ToList();
                if (userRoles.Any())
                {
                    db.RolesUsers.RemoveRange(userRoles);
                    db.SaveChanges();
                }

                // Finally, delete the user
                var user = db.Users.Where(y => y.UserId == UsersId).FirstOrDefault();
                if (user != null)
                {
                    db.Users.Remove(user);
                    db.SaveChanges();
                }

                return Json(new { success = true, message = "User deleted successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        public JsonResult GetUsers(int id)
        {
            try
            {
                var data = db.Users.Where(x => x.UserId == id).FirstOrDefault();
                var role = db.RolesUsers.Where(z => z.Users.UserId == id).Select(x=> x.Roles.RoleId).ToList();
                
                int? officeId = data.OfficeId > 0 ? data.OfficeId : (int?)null;
                
                return Json(new { data.UserId, data.Email, data.Company.CompanyId, data.UserName, data.FirstName, data.Active, data.LastName, role, OfficeId = officeId }, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetProjectsByOffice(int officeId)
        {
            try
            {
                var projects = db.Project.Where(p => p.Customer.Office.OfficeId == officeId)
                                       .Select(p => new { 
                                           ProjectId = p.ProjectId, 
                                           ProjectName = p.ProjectName,
                                           CustomerOffice = p.Customer.Office.OfficeDescription,
                                           CustomerName = p.Customer.CustomerName
                                       }).ToList();
                
                return Json(projects, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetUserProjects(int userId)
        {
            try
            {
                var userProjects = db.UsersProject.Where(up => up.UserId == userId)
                                                .Select(up => new {
                                                    ProjectId = up.Project.ProjectId,
                                                    ProjectName = up.Project.ProjectName,
                                                    CustomerOffice = up.Project.Customer.Office.OfficeDescription,
                                                    CustomerName = up.Project.Customer.CustomerName
                                                }).ToList();
                
                return Json(userProjects, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TimeTracker.Helpers;
using TimeTracker.Models;

namespace TimeTracker.Controllers
{
    public class SystemController : Controller
    {
        public Models.timetrackerDBEntities db;
        public string mail_pass = "Fc@9ii375";
        public string mail_account = "no-reply@smarttechcr.com";
        public string mail_server = "174.138.177.202";
        public static string baseURL = "https://sx-timetracker.com";

        public string mail_connectionString = "endpoint=https://time-tracker-communication.communication.azure.com/;accesskey=8hBm5RXt6p7s7A5kX8o8fI5KO2GSqyBJ9+79LUBcUdxTfGIDj0nqA7FELcQZbAGr/Rf8Z5uhsgZ++Ptnz59Q3g==";
        public string mail_SenderDontReply = "dontreply@sx-timetracker.com";

        public Mail _mail;
        public SystemController()
        {
            db = new Models.timetrackerDBEntities();
            _mail = new Mail();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Navbar(string userData)
        {
            try
            {
                if (!string.IsNullOrEmpty(userData))
                {
                    var data = userData.Split('|');
                    int id = Convert.ToInt32(data[0]);
                    var _user = db.Users.FirstOrDefault(x=> x.UserId == id);
                    ViewBag.Id = data[0];
                    ViewBag.UserName = data[1];
                    ViewBag.Mail = data[2];
                    ViewBag.FullName = _user.FirstName + " "+ _user.LastName;
                    ViewBag.profilePic = _user.ProfilePict;
                }
                else
                {
                    var data = User.Identity.Name.Split('|');
                    int id = Convert.ToInt32(data[0]);
                    var _user = db.Users.FirstOrDefault(x => x.UserId == id);
                    ViewBag.Id = data[0];
                    ViewBag.UserName = data[1];
                    ViewBag.Mail = data[2];
                    ViewBag.FullName = _user.FirstName + " " + _user.LastName;
                    ViewBag.profilePic = _user.ProfilePict;
                }

                return PartialView("~/Views/Shared/Navbar.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.LoginResp = ex.Message;
                throw ex;
            }

        }

        public ActionResult Footer()
        {
            return PartialView("~/Views/Shared/Footer.cshtml");
        }

        public ActionResult Sidebar(string userData)
        {
            List<view_menu> pages = new List<view_menu>();

            try
            {
                if (!string.IsNullOrEmpty(userData))
                {
                    var data = userData.Split('|');
                    int UserId = Convert.ToInt32(data[0]);
                    ViewBag.Id = UserId;
                    ViewBag.UserName = data[1];
                    ViewBag.Mail = data[2];
                    ViewBag.FullName = data[3];

                    var _user = db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
                    if (_user.IsSuperUser)
                    {
                        pages = db.view_menu.ToList();
                    }
                    else
                    {
                        var role = db.RolesUsers.Where(x => x.Users.UserId == UserId).FirstOrDefault();
                        if (role != null)
                        {
                            List<string> list = db.RolesPrivileges.Where(x => x.Roles.RoleId == role.Roles.RoleId).Select(x => x.Privilege.PrivilegeId).ToList();

                            pages = (from q in db.view_menu
                                     where list.Contains(q.PageIdAccess)
                                     select q).ToList();
                        }


                    }
                }

                return PartialView("~/Views/Shared/Sidebar.cshtml", pages.ToList());

            }
            catch (Exception ex)
            {
                ViewBag.LoginResp = ex.Message;
                return PartialView("~/Views/Shared/Sidebar.cshtml");
            }

        }

        public ActionResult Header()
        {
            return PartialView("~/Views/Shared/Header.cshtml");
        }

        public string GetUser()
        {
            string user;
            try
            {
                var data = User.Identity.Name.Split('|');
                user = data[0];

                return user;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public ActionResult setHeader(int PageId)
        {
            if (HasAccessPage(PageId))
            {
                var page = db.view_menu.Where(x => x.PageId == PageId).FirstOrDefault();
                ViewBag.page = page.PageTitle;
                ViewBag.area = page.AreaShortDesc;
                ViewBag.module = page.ModuleShortDesc;

                return null;
            }
            else
            {
                ViewBag.Error = "You do not have access to this page";
                return PartialView("~/Views/Shared/Error.cshtml");
            }

        }

        public ActionResult GetCboCustomer()
        {
            try
            {
                var modal = db.Customer.OrderBy(x=>x.CustomerName).ToList();
                return PartialView("~/Views/System/_cboCustomer.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetCboProject(int CustomerId = 0)
        {
            try
            {

                var customer = db.Customer.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                var modal = db.Project.Where(x => x.Customer.CustomerId == CustomerId).OrderBy(x=>x.ProjectName).ToList();
                return PartialView("~/Views/System/_cboProject.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetCboProjectUser()
        {
            try
            {
                int userId = Convert.ToInt32(GetUser());

                var projects = db.UsersProject.Where(x => x.Users.UserId == userId).Select(x => x.Project.ProjectId).ToList();

                var modal = db.Project.Where(x => projects.Contains(x.ProjectId)).ToList();

                return PartialView("~/Views/System/_cboProject.cshtml", modal);
            }
            catch (Exception )
            {
                return View("~/Views/UserAccount/Login.cshtml");
            }
        }

        public ActionResult GetCboProjectManager()
        {
            try
            {
                int userId = Convert.ToInt32(GetUser());

                var user = db.Users.Where(x => x.UserId == userId).Select(x => x.UserId).ToList();

                var modal = db.Project.Where(x => x.ProjectManager.Any(l => user.Contains(l.Users.UserId))).ToList();

                return PartialView("~/Views/System/_cboProject.cshtml", modal);
            }
            catch (Exception)
            {
                return View("~/Views/UserAccount/Login.cshtml");
            }
        }

        public string returnImage(byte[] data)
        {
            return string.Format("data:image/jpeg;base64,{0}", Convert.ToBase64String(data));
        }
        public ActionResult GetCboUsers(int id = 0)
        {
            try
            {

                var user = db.Users.ToList();


                return PartialView("~/Views/System/_cboUsers.cshtml", user);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public ActionResult GetECboUsers(int id = 0)
        {
            try
            {

                var user = db.Users.ToList();


                return PartialView("~/Views/System/_cboEUsers.cshtml", user);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public ActionResult GetCboActivity(int ProjectId = 0)
        {
            try
            {
                var modal = db.Activity.Where(x => x.Project.ProjectId == ProjectId).ToList();
                return PartialView("~/Views/System/_cboActivity.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetCboAllActivity(int id = 0)
        {
            try
            {
                ViewBag.id = id;
                var modal = db.Activity.ToList();
                return PartialView("~/Views/System/_cboAllActivity.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetCboCategory()
        {
            try
            {
                var modal = db.Category.ToList();
                return PartialView("~/Views/System/_cboCategory.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public ActionResult GetECboCustomer()
        {
            try
            {
                var modal = db.Customer.ToList();
                return PartialView("~/Views/System/_cboECustomer.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetECboProject(int CustomerId = 0)
        {
            try
            {
                var customer = db.Customer.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                var modal = db.Project.Where(x => x.Customer.CustomerId == CustomerId).ToList();
                return PartialView("~/Views/System/_cboEProject.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetECboActivity(int ProjectId = 0)
        {
            try
            {
                var modal = db.Activity.Where(x => x.Project.ProjectId == ProjectId).ToList();
                return PartialView("~/Views/System/_cboEActivity.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetECboCategory()
        {
            try
            {
                var modal = db.Category.ToList();
                return PartialView("~/Views/System/_cboECategory.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*-@_=+";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public ActionResult GetCboRoles()
        {
            try
            {

                var model = db.Roles.ToList();
                return PartialView("~/Views/System/_cboRoles.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public ActionResult GetCboMultipleRoles()
        {
            try
            {

                var model = db.Roles.ToList();
                return PartialView("~/Views/System/_cboMultipleRoles.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public ActionResult GetCboEMultipleRoles()
        {
            try
            {

                var model = db.Roles.ToList();
                return PartialView("~/Views/System/_cboEMultipleRoles.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public ActionResult GetCboPM()
        {
            try
            {

                var model = db.Users.ToList();
                return PartialView("~/Views/System/_cboPM.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public ActionResult GetCboEPM(int project = 0)
        {
            try
            {

                var model = db.Users.ToList();
                return PartialView("~/Views/System/_cboEPM.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public ActionResult GetCboSendDays(int id = 0)
        {
            try
            {
                var model = db.TimeHours.Where(x => x.UserId == id && x.DayStatus == "Sent").Select(x => x.THDate).Distinct();

                return PartialView("~/Views/System/_cboSendDays.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public ActionResult GetCboOffice()
        {
            try
            {
                var modal = db.Office.ToList();
                return PartialView("~/Views/System/_cboOffice.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult GetECboOffice()
        {
            try
            {
                var modal = db.Office.ToList();
                return PartialView("~/Views/System/_cboEOffice.cshtml", modal);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult HO2Factor()
        {
            try
            {
                return PartialView("~/Views/System/_cbo2FactorHO.cshtml");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public JsonResult UserConfirmed()
        {
            bool go = false;
            int id = Convert.ToInt32(GetUser());
            var _user = db.Users.Where(x => x.UserId == id).FirstOrDefault();

            if (_user.EmailConfirmed)
            {
                go = true;
            }

            return Json(new { data = go }, JsonRequestBehavior.AllowGet);

        }
        public string CreateCode(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*-@_=+";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public IEnumerable<Activity> getActivityList()
        {
            var model = db.Activity.ToList();
            return model;
        }

        public IEnumerable<Customer> getCustomerList()
        {
            var model = db.Customer.ToList();
            return model;
        }

        public IEnumerable<Project> getProjectList()
        {
            var model = db.Project.ToList();
            return model;
        }

        public IEnumerable<Category> getCategoryList()
        {
            var model = db.Category.ToList();
            return model;
        }
        public IEnumerable<UserFullName> getUsersList()
        {
            var _model = db.Users.Select(x => new { Name = x.FirstName + " " + x.LastName, UserId = x.UserId }).ToList();
            List<UserFullName> _users = new List<UserFullName>();
            foreach (var item in _model)
            {
                UserFullName userFullName = new UserFullName();
                userFullName.Name = item.Name;
                userFullName.UserId = item.UserId;
                _users.Add(userFullName);
            }
            return _users;
        }

        public IEnumerable<string> getStatusList()
        {
            List<string> list = new List<string>();
            list.Add("Approved");
            list.Add("Under Review");
            list.Add("Disregard");
            list.Add("Not Billable");

            return list;
        }

        public bool HasPrivilege(string id)
        {
            List<view_menu> pages = new List<view_menu>();
            int UserId = Convert.ToInt32(GetUser());

            var _user = db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
            if (_user.IsSuperUser)
            {
                return true;
            }

            var role = db.RolesUsers.Where(x => x.Users.UserId == UserId).FirstOrDefault();
            if (role != null)
            {
                return db.RolesPrivileges.Where(x => x.Roles.RoleId == role.Roles.RoleId
                && x.Privilege.PrivilegeId == id).Any();

            }

            return false;

        }

        public bool HasAccessPage(int id)
        {
            List<view_menu> pages = new List<view_menu>();
            int UserId = Convert.ToInt32(GetUser());
            string page = db.Pages.Where(x=>x.PageId == id).Select(x=>x.PageIdAccess).FirstOrDefault(); 
            var _user = db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
            if (_user.IsSuperUser)
            {
                return true;
            }

            var role = db.RolesUsers.Where(x => x.Users.UserId == UserId).FirstOrDefault();
            if (role != null)
            {
                return db.RolesPrivileges.Where(x => x.Roles.RoleId == role.Roles.RoleId
                && x.Privilege.PrivilegeId == page).Any();

            }

            return false;

        }



    }

    public class UserFullName
    {
        public string Name { get; set; }
        public int UserId { get; set; }
    }


}
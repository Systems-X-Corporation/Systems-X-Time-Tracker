using DevExpress.DocumentServices.ServiceModel.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TimeTracker.Models;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;

namespace TimeTracker.Controllers.Manteinance
{
    public class UserProjectController : SystemController
    {
        private int pagesId;
        // GET: UserProject
        public ActionResult UserProject(int pageId)
        {
            pagesId = pageId;
            setHeader(pageId);
            return View("~/Views/Manteinance/UserProject/UserProject.cshtml");

        }
    
        public JsonResult AllUsers(int projectId)
        {
            var UPs = db.UsersProject.Where(x=> x.Project.ProjectId == projectId).Select(x=> x.Users.UserId).ToList();  
            var model = db.Users.Where(x=> !UPs.Contains(x.UserId)).ToList();
            List<UsersView> users = new List<UsersView>();
            foreach (var item in model)
            {
                UsersView usersView = new UsersView();
                usersView.UserId = item.UserId;
                usersView.UserName = item.FirstName + " " + item.LastName;
                users.Add(usersView);
            }
            return Json(users,JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProjectUsers (int projectId)
        {
            var UPs = db.UsersProject.Where(x => x.Project.ProjectId == projectId).Select(x => x.Users.UserId).ToList();
            var model = db.Users.Where(x => UPs.Contains(x.UserId)).ToList();
            List<UsersView> users = new List<UsersView>();
            foreach (var item in model)
            {
                UsersView usersView = new UsersView();
                usersView.UserId = item.UserId;
                usersView.UserName = item.FirstName + " " + item.LastName;
                users.Add(usersView);
            }
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public void UserProjectAddNew (FormCollection formCollection)
        {
            try
            {
                int ProjectId = Convert.ToInt32(formCollection["idProject"]);
                var project = db.Project.Where(x => x.ProjectId == ProjectId).FirstOrDefault();
                var values = formCollection["UserProject"].Split(',');
                var model = db.UsersProject;

                var delete = db.UsersProject.Where(x=> x.Project.ProjectId == ProjectId).ToList();
                model.RemoveRange(delete);
                db.SaveChanges();

                List< UsersProject > users = new List<UsersProject>();
                foreach (var item in values)
                {
                    UsersProject usersProject = new UsersProject();
                    usersProject.Project = project;
                    int _userId = Convert.ToInt32(item);
                    var user = db.Users.Where(x => x.UserId == _userId).FirstOrDefault();
                    usersProject.Users = user;
                    users.Add(usersProject);
                }
                model.AddRange(users);
                db.SaveChanges();


              
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


    }

    public class UsersView
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
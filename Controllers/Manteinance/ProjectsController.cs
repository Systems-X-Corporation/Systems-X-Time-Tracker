using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Manteinance
{
    public class ProjectsController : SystemController
    {

        public ActionResult Project(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Manteinance/Projects/Project.cshtml");

        }

        public ActionResult ListProject()
        {
            var model = db.Project.ToList();
            return PartialView("~/Views/Manteinance/Projects/_ListProject.cshtml", model);
        }

        public ActionResult ProjectAddNew(FormCollection formCollection)
        {
            var model = db.Project;
            Project newProject = new Project();
            try
            {
                int _customerId = Convert.ToInt16(formCollection["CustomerId"]);
                var _customer = db.Customer.Where(x => x.CustomerId == _customerId).FirstOrDefault();
                //newProject.CustomerId = _customerId;
                newProject.Customer = _customer;
                newProject.ProjectName = formCollection["ProjectName"];
                newProject.color = formCollection["color"];
                newProject.CreateUser = GetUser().ToString();
                newProject.CreateDate = DateTime.Now;
                model.Add(newProject);
                db.SaveChanges();
                var pms = formCollection["cboPM"].Split(',');
                var PmsModel = db.ProjectManager;
                foreach (var item in pms)
                {
                    ProjectManager projectManager = new ProjectManager();
                    projectManager.Project = newProject;
                    int UserId = Convert.ToInt32(item);
                    var user = db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
                    projectManager.Users = user;
                    PmsModel.Add(projectManager);
                    db.SaveChanges();
                }

                var users = formCollection["cboUsers"];
                if (!string.IsNullOrEmpty(users))
                {
                    var userModel = db.UsersProject;
                    var selected = users.Split(',');
                    foreach (var item in selected)
                    {
                        UsersProject up = new UsersProject();
                        up.Project = newProject;
                        int uId = Convert.ToInt32(item);
                        var u = db.Users.Where(x => x.UserId == uId).FirstOrDefault();
                        up.Users = u;
                        userModel.Add(up);
                    }
                    db.SaveChanges();
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            model = db.Project;
            return PartialView("~/Views/Manteinance/Projects/_ListProject.cshtml", model.ToList());

        }

        public ActionResult ProjectUpdate(FormCollection formCollection)
        {
            var model = db.Project;

            try
            {
                int _customerId = Convert.ToInt16(formCollection["ECustomerId"]);
                var _customer = db.Customer.Where(x => x.CustomerId == _customerId).FirstOrDefault();

                int ProjectId = Convert.ToInt16(formCollection["EdProjectId"].ToString());
                
                Project newProject = db.Project.FirstOrDefault(x => x.ProjectId == ProjectId);
                newProject.Customer = _customer;
                newProject.Customer.CustomerId = Convert.ToInt16(formCollection["ECustomerId"]);
                newProject.ProjectName = formCollection["EdProjectName"];
                newProject.color = formCollection["ecolor"];
                newProject.ModifyUser = GetUser().ToString();
                newProject.ModifyDate = DateTime.Now;
                db.Entry(newProject).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var pms = formCollection["cboEPM"].Split(',');
                var PmsModel = db.ProjectManager;

                var actualPMs = db.ProjectManager.Where(x => x.Project.ProjectId == ProjectId).ToList();
                PmsModel.RemoveRange(actualPMs);
                db.SaveChanges();

                if (pms != null)
                {
                    foreach (var item in pms)
                    {
                        ProjectManager projectManager = new ProjectManager();
                        projectManager.Project = newProject;
                        int UserId = Convert.ToInt32(item);
                        var user = db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
                        projectManager.Users = user;
                        PmsModel.Add(projectManager);
                        db.SaveChanges();
                    }
                }

                var userModel = db.UsersProject;
                var actualUsers = db.UsersProject.Where(x => x.Project.ProjectId == ProjectId).ToList();
                userModel.RemoveRange(actualUsers);
                db.SaveChanges();

                var users = formCollection["cboEUsers"];
                if (!string.IsNullOrEmpty(users))
                {
                    var selected = users.Split(',');
                    foreach (var item in selected)
                    {
                        UsersProject up = new UsersProject();
                        up.Project = newProject;
                        int uId = Convert.ToInt32(item);
                        var u = db.Users.Where(x => x.UserId == uId).FirstOrDefault();
                        up.Users = u;
                        userModel.Add(up);
                    }
                    db.SaveChanges();
                }





            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Manteinance/Projects/_ListProject.cshtml", model.ToList());

        }


        public ActionResult DeleteProject(string id)
        {
            try
            {
                int ProjectId = Convert.ToInt32(id);
                using (var ctx = new timetrackerDBEntities())
                {
                    var x = (from y in ctx.Project
                             where y.ProjectId == ProjectId
                             select y).FirstOrDefault();
                    ctx.Project.Remove(x);
                    ctx.SaveChanges();
                }
                var model = db.Project;
                return PartialView("~/Views/Manteinance/Projects/_ListProject.cshtml", model.ToList());
            }
            catch (Exception e)
            {
                var model = db.Project;
                ViewData["EditError"] = e.Message;
                return PartialView("~/Views/Manteinance/Projects/_ListProject.cshtml", model.ToList());
            }
        }

        public JsonResult CanDeleteProject(int id)
        {
            try
            {
                var data = db.Activity.Any(x => x.ProjectId == id);

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public JsonResult GetProject(string id)
        {
            try
            {
                int ProjectId = Convert.ToInt32(id);
                var data = db.Project.Where(x => x.ProjectId == ProjectId).FirstOrDefault();
                var PMs = db.ProjectManager.Where(x => x.Project.ProjectId == ProjectId).ToList();
                List<int> pmsList = new List<int>();
                foreach (var item in PMs)
                {
                    pmsList.Add(item.Users.UserId);
                }
                var UPs = db.UsersProject.Where(x => x.Project.ProjectId == ProjectId).ToList();
                List<int> users = new List<int>();
                foreach (var item in UPs)
                {
                    users.Add(item.Users.UserId);
                }
                return Json(new { data.ProjectId, data.ProjectName, data.CreateUser, data.CreateDate, data.Customer.CustomerId, data.color, pms = pmsList, users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


    }

}
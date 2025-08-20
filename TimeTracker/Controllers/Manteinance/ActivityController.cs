using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Manteinance
{
    public class ActivityController : SystemController
    {

        public ActionResult Activity(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Manteinance/Activity/Activity.cshtml");

        }

        public ActionResult ListActivity()
        {
            var model = db.Activity.ToList();
            return PartialView("~/Views/Manteinance/Activity/_ListActivity.cshtml", model);
        }

        public ActionResult ActivityAddNew(FormCollection formCollection)
        {
            var model = db.Activity;
            Activity newActivity = new Activity();
            try
            {
                int _projectId = Convert.ToInt16(formCollection["ProjectId"]);
                var _project = db.Project.Where(x=> x.ProjectId == _projectId).FirstOrDefault();
                //newActivity.ProjectId = _projectId;
                newActivity.Project = _project;
                //int _categoryId = Convert.ToInt32(formCollection["CategoryId"]);
                //var category = db.Category.Where(x => x.CategoryId == _categoryId).FirstOrDefault();
                //newActivity.Category = category;
                //bool billeable = false;
                //if (formCollection["billeable"] == "on")
                //{
                //    billeable = true;
                //}
                //newActivity.Billeable = billeable;

                newActivity.ActivityName = formCollection["ActivityName"];
                newActivity.CreateUser = GetUser().ToString();
                newActivity.CreateDate = DateTime.Now;
                model.Add(newActivity);
                db.SaveChanges();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Manteinance/Activity/_ListActivity.cshtml", model.ToList());

        }

        public ActionResult ActivityUpdate(FormCollection formCollection)
        {
            var model = db.Activity;

            try
            {

                int ActivityId = Convert.ToInt16(formCollection["EdActivityId"].ToString());

                Activity newActivity = db.Activity.FirstOrDefault(x => x.ActivityId == ActivityId);


                int _projectId = Convert.ToInt16(formCollection["EProjectId"]);
                var _project = db.Project.Where(x => x.ProjectId == _projectId).FirstOrDefault();
                //newActivity.ProjectId = _projectId;
                newActivity.Project = _project;
                


                newActivity.ActivityName = formCollection["EdActivityName"];
                newActivity.ModifyUser = GetUser().ToString();
                newActivity.ModifyDate = DateTime.Now;
                db.Entry(newActivity).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Manteinance/Activity/_ListActivity.cshtml", model.ToList());

        }


        public ActionResult DeleteActivity(string id)
        {
            try
            {
                int ActivityId = Convert.ToInt32(id);
                using (var ctx = new timetrackerDBEntities())
                {
                    var x = (from y in ctx.Activity
                             where y.ActivityId == ActivityId
                             select y).FirstOrDefault();
                    ctx.Activity.Remove(x);
                    ctx.SaveChanges();
                }
                var model = db.Activity;
                return PartialView("~/Views/Manteinance/Activity/_ListActivity.cshtml", model.ToList());
            }
            catch (Exception e)
            {
                var model = db.Activity;
                ViewData["EditError"] = e.Message;
                return PartialView("~/Views/Manteinance/Activity/_ListActivity.cshtml", model.ToList());
            }
        }



        public JsonResult GetActivity(string id)
        {
            try
            {
                int ActivityId = Convert.ToInt32(id);
                var data = db.Activity.Where(x => x.ActivityId == ActivityId).FirstOrDefault();

                return Json(new { data.ActivityId, data.ActivityName, data.CreateUser, data.CreateDate, /*data.Category.CategoryId,*/ data.Project.ProjectId, data.Project.Customer.CustomerId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


    }
}
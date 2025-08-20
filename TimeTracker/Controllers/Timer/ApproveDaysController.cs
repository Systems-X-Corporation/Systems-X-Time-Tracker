using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Data.Mask.Internal;
using DevExpress.DocumentServices.ServiceModel.DataContracts;
using DevExpress.Web.Mvc;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Timer
{
    public class ApproveDaysController : SystemController
    {
        // GET: ApproveDays
        public ActionResult ApproveDays(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Timer/ApproveDays/ApproveDays.cshtml");
        }

        public ActionResult ApproveDaysList(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Timer/ApproveDays/ApproveDaysList.cshtml");
        }



        public JsonResult GetDayData(DateTime date, int user)
        {
            var hours = db.TimeHours.Where(x => x.THDate == date && x.UserId == user).ToList();
            List<Hours> _hours = new List<Hours>();
            foreach (var item in hours)
            {
                Hours _hour = new Hours();
                _hour.id = item.TimeHoursId;
                _hour.title = item.Project.ProjectName + " ~ " + item.ActDescription;
                _hour.start = item.THDate.ToString("yyyy-MM-dd") + "T" + item.THFrom;
                _hour.end = item.THDate.ToString("yyyy-MM-dd") + "T" + item.THTo;
                _hour.startTime = item.THFrom;
                _hour.endTime = item.THTo;
                _hour.daystatus = item.DayStatus;
                _hour.approved = item.DayApproved;

                _hour.color = item.Project.color;
                bool edit = true;
                if (item.Exported != null)
                {
                    if (!(bool)item.Exported)
                    {
                        edit = false;
                    }
                }
                _hour.editable = edit;


                _hours.Add(_hour);
            }


            return Json(_hours, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApproveDay(int User, DateTime Date, string dayObservations = "")
        {
            try
            {
                var model = db.DaysUser;
                var model1 = db.TimeHours;

                var daysUser = db.DaysUser.Where(x => x.UserId == User && x.DayDate == Date).FirstOrDefault();
                daysUser.DayStatus = "Approved";
                daysUser.DayObservations = dayObservations;
                model.AddOrUpdate(daysUser);

                var Hours = db.TimeHours.Where(x => x.UserId == User && x.THDate == Date).ToList();

                foreach (var item in Hours)
                {
                    item.DayStatus = "Approved";
                    item.DayApproved = true;
                    model1.AddOrUpdate(item);
                }

                db.SaveChanges();





                return Json(new { msg = "Ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public JsonResult DismissDay(int User, DateTime Date)
        {
            try
            {
                var model = db.DaysUser;
                var model1 = db.TimeHours;

                var daysUser = db.DaysUser.Where(x => x.UserId == User && x.DayDate == Date).FirstOrDefault();
                model.Remove(daysUser);

                var Hours = db.TimeHours.Where(x => x.UserId == User && x.THDate == Date).ToList();

                foreach (var item in Hours)
                {
                    item.DayStatus = null;
                    item.DayApproved = false;
                    model1.AddOrUpdate(item);
                }

                db.SaveChanges();

                return Json(new { msg = "Ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult DaysList(int customer = 0, int project = 0, int user = 0, string from = "", string to = "", string records = "")
        {
            try
            {
                var hours = db.TimeHours.ToList();

                if (customer != 0)
                {
                    hours = hours.Where(x => x.CustomerId  == customer).ToList();
                }
                if (project != 0)
                {
                    hours = hours.Where(x => x.Project.ProjectId == project).ToList();
                }
                if (!string.IsNullOrEmpty(records) && records != "All")
                {
                    hours = hours.Where(x => x.DayStatus == records).ToList();
                }
                if (user != 0)
                {
                    hours = hours.Where(x => x.Users.UserId == user).ToList();
                }
                if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                {
                    DateTime _from = Convert.ToDateTime(from); DateTime _to = Convert.ToDateTime(to);
                    hours = hours.Where(x => x.THDate >= _from && x.THDate <= _to).ToList();
                }

                ViewBag.customer = customer;
                ViewBag.project = project;
                ViewBag.user = user;
                ViewBag.from = from;
                ViewBag.to = to;
                ViewBag.records = records;

                return PartialView("~/Views/Timer/ApproveDays/_ApproveList.cshtml", hours);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ActionResult BatchEditingUpdateModel(MVCxGridViewBatchUpdateValues<TimeHours, int> updateValues,int customer =0, int project = 0, int user = 0, string from = "", string to = "", string records = "")
        {
            try
            {
                var model = db.TimeHours;
                var model1 = db.DaysUser;
                foreach (var item in updateValues.Insert)
                {
                    TimeHours timeHours = new TimeHours();
                    timeHours.THDate = item.THDate;
                    DateTime finalHour = item.THDate.AddHours(8);
                    finalHour = finalHour.AddHours(Convert.ToDouble(item.THours));
                    timeHours.THFrom = "08:00:00";
                    timeHours.THTo = finalHour.ToString("HH:mm:ss");
                    Users _user = db.Users.Where(x => x.UserId == item.Users.UserId).FirstOrDefault();
                    timeHours.Users = _user;
                    timeHours.UserId = item.Users.UserId;
                    Project _project = db.Project.Where(x => x.ProjectId == project).FirstOrDefault();
                    timeHours.Project = _project;
                    Customer _customer = db.Customer.Where(x => x.CustomerId == _project.Customer.CustomerId).FirstOrDefault();
                    timeHours.Customer = _customer;
                    //Models.Activity _activity = db.Activity.Where(x => x.ActivityId == item.Activity.ActivityId).FirstOrDefault();
                    //timeHours.Activity = _activity;
                    timeHours.ActDescription = item.ActDescription;
                    timeHours.Billable = item.Billable;
                    Category _category = db.Category.Where(x=>x.CategoryId == item.Category.CategoryId).FirstOrDefault();
                    timeHours.Category = _category;
                    timeHours.DayStatus = item.DayStatus;
                    timeHours.InternalNote = item.InternalNote;
                    timeHours.TaskAllocation = item.TaskAllocation;
                    timeHours.Visible = item.Visible;
                    timeHours.ApprovalNote = item.ApprovalNote;
                    timeHours.THours = item.THours;
                    model.Add(timeHours);
                    db.SaveChanges();
                }
                foreach (var item in updateValues.Update)
                {
                    TimeHours timeHours = db.TimeHours.Where(x=> x.TimeHoursId == item.TimeHoursId).FirstOrDefault();

                    timeHours.ActDescription = item.ActDescription;
                    timeHours.Billable = item.Billable;
                    Category _category = db.Category.Where(x => x.CategoryId == item.Category.CategoryId).FirstOrDefault();
                    timeHours.Category = _category;
                    timeHours.DayStatus = item.DayStatus;
                    if (item.DayStatus == "Approved")
                    {
                        
                        var daysUser = db.DaysUser.Where(x => x.UserId == item.Users.UserId && x.DayDate == item.THDate).FirstOrDefault();
                        if (daysUser == null)
                        {
                            DaysUser days = new DaysUser();
                            days.UserId = item.Users.UserId;
                            days.DayDate = item.THDate;
                            days.DayStatus = "Approved";
                            days.DayObservations = "";
                            model1.AddOrUpdate(days);
                            timeHours.DayApproved = true;
                        }
                        else
                        {

                            daysUser.DayStatus = "Approved";
                            daysUser.DayObservations = "";
                            model1.AddOrUpdate(daysUser);
                        }
                        timeHours.DayApproved = true;
                    }
                    if (item.DayStatus == "Dismiss")
                    {
                        var daysUser = db.DaysUser.Where(x => x.UserId == item.Users.UserId && x.DayDate == item.THDate).FirstOrDefault();
                        model1.Remove(daysUser);
                        timeHours.DayStatus = null;
                        timeHours.DayApproved = false;
                    }
                    if (item.DayStatus == "Under Review")
                    {

                        var daysUser = db.DaysUser.Where(x => x.UserId == item.Users.UserId && x.DayDate == item.THDate).FirstOrDefault();
                        daysUser.DayStatus = "Under Review";
                        daysUser.DayObservations = "";
                        model1.AddOrUpdate(daysUser);
                    }

                    timeHours.InternalNote = item.InternalNote;
                    timeHours.TaskAllocation = item.TaskAllocation;
                    timeHours.Visible = item.Visible;
                    timeHours.ApprovalNote = item.ApprovalNote;
                    timeHours.THours = item.THours;
                    db.Entry(timeHours).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                }


                var hours = db.TimeHours.Where(x => x.Project.ProjectId == project).ToList();
                if (!string.IsNullOrEmpty(records) && records != "All")
                {
                    hours = hours.Where(x => x.DayStatus == records).ToList();
                }
                if (user != 0)
                {
                    hours = hours.Where(x => x.Users.UserId == user).ToList();
                }
                if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                {
                    DateTime _from = Convert.ToDateTime(from); DateTime _to = Convert.ToDateTime(to);
                    hours = hours.Where(x => x.THDate >= _from && x.THDate <= _to).ToList();
                }
                
                ViewBag.customer = customer;

                ViewBag.project = project;
                ViewBag.user = user;
                ViewBag.from = from;
                ViewBag.to = to;
                ViewBag.records = records;

                return PartialView("~/Views/Timer/ApproveDays/_ApproveList.cshtml", hours);

            }
            catch (Exception)
            {

                throw;
            }
        }

        //public ActionResult AddNewRow(MVCxGridViewBatchUpdateValues<TimeHours, int> updateValues, int project, int user = 0, string from = "", string to = "", string records = "")
        //{
        //    TimeHours timeHours = new TimeHours();
        //    timeHours.THDate = item.THDate;
        //    timeHours.THFrom = "08:00:00";
        //    timeHours.THTo = "09:00:00";
        //    Users _user = db.Users.Where(x => x.UserId == item.UserId).FirstOrDefault();
        //    timeHours.Users = _user;
        //    Project _project = db.Project.Where(x => x.ProjectId == item.Project.ProjectId).FirstOrDefault();
        //    timeHours.Project = _project;
        //    Customer _customer = db.Customer.Where(x => x.CustomerId == _project.Customer.CustomerId).FirstOrDefault();
        //    timeHours.Customer = _customer;
        //    Models.Activity _activity = db.Activity.Where(x => x.ActivityId == item.Activity.ActivityId).FirstOrDefault();
        //    timeHours.Activity = _activity;
        //    timeHours.ActDescription = item.ActDescription;
        //    timeHours.Billable = item.Billable;
        //    Category _category = db.Category.Where(x => x.CategoryId == item.Category.CategoryId).FirstOrDefault();
        //    timeHours.Category = _category;
        //    timeHours.DayStatus = item.DayStatus;
        //    timeHours.InternalNote = item.InternalNote;
        //    timeHours.TaskAllocation = item.TaskAllocation;
        //    timeHours.Visible = item.Visible;
        //    timeHours.ApprovalNote = item.ApprovalNote;
        //    timeHours.THours = item.THours;

        //    var hours = db.TimeHours.Where(x => x.Project.ProjectId == project).ToList();

        //    ViewBag.project = project;
        //    ViewBag.user = user;
        //    ViewBag.from = from;
        //    ViewBag.to = to;
        //    ViewBag.records = records;
        //    return PartialView("~/Views/Timer/ApproveDays/_ApproveList.cshtml", hours);
        //}

        public JsonResult getdata()
        {
            var hours = db.TimeHours.Where(x => x.DayStatus == "Sent").ToList();
            return Json(hours, JsonRequestBehavior.AllowGet);
        }

    }
}
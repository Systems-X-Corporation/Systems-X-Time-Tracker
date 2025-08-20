using DevExpress.XtraRichEdit.Import.Rtf;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;
using Calendar = System.Globalization.Calendar;

namespace TimeTracker.Controllers.Timer
{
    public class TimeController : SystemController
    {
        CultureInfo CI = new CultureInfo("en-US");
        //Calendar calendar;
        // GET: Time
        public ActionResult Time()
        {
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                //OAuthController oauth = new OAuthController();
                //oauth.AllEvents(Convert.ToInt32(GetUser()));
            }
          
            return View("~/Views/Timer/Time/WeekTime.cshtml");
        }

        public JsonResult SaveHours(DateTime date, string start, string end, int projectId, /*int activityId,*/ int categoryId, string description, bool billable, decimal Thours,string InternalNotes, int id = 0, int user = 0)
        {
            try
            {
                int userId;
                if (user == 0)
                {
                    userId = Convert.ToInt32(GetUser());
                }
                else
                {
                    userId = user;
                }
                 
                var model = db.TimeHours;

                if (id == 0)
                {
                    TimeHours timeHours = new TimeHours();

                    timeHours.THDate = date;
                    timeHours.THFrom = start;
                    timeHours.THTo = end;
                    timeHours.Billable = billable;
                    timeHours.ActDescription = description;
                    timeHours.Users = db.Users.Where(x => x.UserId == userId).FirstOrDefault();
                    timeHours.UserId = userId;
                    timeHours.THours = Thours;
                    timeHours.InternalNote = InternalNotes;
                    timeHours.Visible = true;

                    var project = db.Project.Where(x => x.ProjectId == projectId).FirstOrDefault();
                    //var activity = db.Activity.Where(x => x.ActivityId == activityId).FirstOrDefault();
                    var customer = db.Customer.Where(x => x.CustomerId == project.Customer.CustomerId).FirstOrDefault();
                    var category = db.Category.Where(x => x.CategoryId == categoryId).FirstOrDefault();

                    timeHours.Customer = customer;
                    timeHours.Project = project;
                    //timeHours.Activity = activity;
                    timeHours.Category = category;

                    model.Add(timeHours);
                    db.SaveChanges();

                    return Json(new { data = "ok", color = project.color, id = timeHours.TimeHoursId, project = timeHours.Project.ProjectName }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TimeHours timeHours = db.TimeHours.Where(x => x.TimeHoursId == id).FirstOrDefault();

                    timeHours.THDate = date;
                    timeHours.THFrom = start;
                    timeHours.THTo = end;
                    timeHours.Billable = billable;
                    timeHours.ActDescription = description;
                    timeHours.Users = db.Users.Where(x => x.UserId == userId).FirstOrDefault();
                    timeHours.UserId = userId;
                    timeHours.THours = Thours;
                    timeHours.InternalNote = InternalNotes;

                    var project = db.Project.Where(x => x.ProjectId == projectId).FirstOrDefault();
                    //var activity = db.Activity.Where(x => x.ActivityId == activityId).FirstOrDefault();
                    var customer = db.Customer.Where(x => x.CustomerId == project.Customer.CustomerId).FirstOrDefault();

                    timeHours.Customer = customer;
                    timeHours.Project = project;
                    //timeHours.Activity = activity;

                    db.Entry(timeHours).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { data = "ok", color = project.color, id = timeHours.TimeHoursId, project = timeHours.Project.ProjectName }, JsonRequestBehavior.AllowGet);
                }





            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        public JsonResult DeleteHours(int id, DateTime date, string start, string end, int projectId, /*int activityId,*/ string description, bool billable)
        {
            try
            {
                int userId = Convert.ToInt32(GetUser());
                var model = db.TimeHours;
                TimeHours timeHours = db.TimeHours.Where(x => x.THDate == date && x.THFrom == start && x.THTo == end && x.ActDescription == description && x.Users.UserId == userId).FirstOrDefault();

                model.Remove(timeHours);
                db.SaveChanges();

                return Json(new { data = "ok" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult GetHours()
        {
            int Userid = Convert.ToInt32(GetUser());
            var hours = db.TimeHours.Where(x => x.UserId == Userid).ToList();
            List<Hours> _hours = new List<Hours>();
            foreach (var item in hours)
            {
                Hours _hour = new Hours();
                _hour.id = item.TimeHoursId;
                _hour.title = item.Customer.CustomerName + " ~ " + item.ActDescription;
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

        public JsonResult GetHourData(int id)
        {
            var hours = db.TimeHours.Where(x => x.TimeHoursId == id).FirstOrDefault();

            return Json(new
            {
                hours.Customer.CustomerId,
                project = hours.Project.ProjectId,
                //activity = hours.Activity.ActivityId,
                hours.TimeHoursId,
                category = hours.Category.CategoryId,
                hours.Project.color,
                hours.THDate,
                hours.THFrom,
                hours.THTo,
                hours.ActDescription,
                hours.Billable,
                hours.THours, hours.InternalNote
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDaysStatus(DateTime date, string CalView)
        {
            try
            {
                List<view_DaysUserStatus> model = new List<view_DaysUserStatus>();
                DateTime start;
                DateTime end;
                int UserId = Convert.ToInt32(GetUser());
                if (CalView == "timeGridDay")
                {
                    model = db.view_DaysUserStatus.Where(x => x.THDate == date && x.UserId == UserId).ToList();

                }
                if (CalView == "dayGridMonth")
                {
                    if (date.Day > 20)
                    {
                        date = date.AddDays(10);
                    }
                    var dates = Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(date.Year, date.Month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list 
                    start = Convert.ToDateTime(dates.First());
                    end = Convert.ToDateTime(dates.Last());
                    model = db.view_DaysUserStatus.Where(x => x.THDate >= start && x.THDate <= end && x.UserId == UserId).ToList();

                    

                }
                if (CalView == "timeGridWeek")
                {
                    Int32 firstDayOfWeek = (Int32)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                    Int32 dayOfWeek = (Int32)date.DayOfWeek;
                    DateTime startOfWeek = date.AddDays(firstDayOfWeek - dayOfWeek);
                    var valuesDaysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<Int32>();
                    var dates = valuesDaysOfWeek.Select(v => startOfWeek.AddDays(v)).ToList();
                    start = Convert.ToDateTime(dates.First());
                    end = Convert.ToDateTime(dates.Last());
                    model = db.view_DaysUserStatus.Where(x => x.THDate >= start && x.THDate <= end && x.UserId == UserId).ToList();

                }


                return PartialView("~/Views/Home/_DaysStatus.cshtml", model);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public JsonResult SendDay(DateTime date)
        {
            try
            {
                int UserId = Convert.ToInt32(GetUser());
                var model = db.TimeHours;
                var days = db.TimeHours.Where(x => x.THDate == date && x.UserId == UserId).ToList();

                foreach (var item in days)
                {
                    item.DayStatus = "Sent";
                    item.DayApproved = false;
                    model.AddOrUpdate(item);
                }

                var model1 = db.DaysUser;
                DaysUser daysUser = new DaysUser();
                daysUser.DayStatus = "Sent";
                daysUser.DayDate = date;
                daysUser.UserId = UserId;

                model1.Add(daysUser);   
                db.SaveChanges();


                return Json(new { msg = "Ok" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult IsDaySent (DateTime date)
        {
            try
            {
                int user = Convert.ToInt32(GetUser());

                var exist = db.DaysUser.Where(x=>x.UserId == user && x.DayDate == date).Any();
                return Json(exist, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
           
        }
        

    }

    public class Hours
    {
        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string startTime { get; set; }
        public string end { get; set; }
        public string endTime { get; set; }
        public string color { get; set; }
        public bool editable { get; set; }
        public string daystatus { get; set; }
        public bool? approved { get; set; }
        public decimal? Thours { get; set; }

    }
}
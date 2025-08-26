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
                    var category = db.Category.Where(x => x.CategoryId == categoryId).FirstOrDefault();

                    timeHours.Customer = customer;
                    timeHours.Project = project;
                    //timeHours.Activity = activity;
                    timeHours.Category = category;

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


        public JsonResult DeleteHours(int id, DateTime date, string start, string end, int projectId = 0, /*int activityId,*/ string description = "", bool billable = false)
        {
            try
            {
                int userId = Convert.ToInt32(GetUser());
                var model = db.TimeHours;
                TimeHours timeHours = db.TimeHours.Where(x => x.TimeHoursId == id && x.UserId == userId).FirstOrDefault();

                // If this is a Google Calendar event, mark it as manually deleted instead of removing it
                if (timeHours != null && !string.IsNullOrEmpty(timeHours.GCalendarId))
                {
                    // Set Visible to false and add a note that it was manually deleted
                    timeHours.Visible = false;
                    timeHours.InternalNote = "MANUALLY_DELETED_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    db.Entry(timeHours).State = System.Data.Entity.EntityState.Modified;
                }
                else if (timeHours != null)
                {
                    // For regular events, delete normally
                    model.Remove(timeHours);
                }
                
                db.SaveChanges();

                if (timeHours != null && !string.IsNullOrEmpty(timeHours.GCalendarId))
                {
                    return Json(new { data = "ok", type = "google_calendar_hidden", visible = timeHours.Visible }, JsonRequestBehavior.AllowGet);
                }
                else if (timeHours != null)
                {
                    return Json(new { data = "ok", type = "regular_deleted" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = "ok", type = "not_found", warning = "Event not found or already deleted" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetHours()
        {
            int Userid = Convert.ToInt32(GetUser());
            var hours = db.TimeHours.Where(x => x.UserId == Userid && x.Visible != false).ToList();
            List<Hours> _hours = new List<Hours>();
            foreach (var item in hours)
            {
                Hours _hour = new Hours();
                _hour.id = item.TimeHoursId;
                _hour.title = "Google Calendar ~ " + item.ActDescription;
                _hour.start = item.THDate.ToString("yyyy-MM-dd") + "T" + item.THFrom;
                _hour.end = item.THDate.ToString("yyyy-MM-dd") + "T" + item.THTo;
                _hour.startTime = item.THFrom;
                _hour.endTime = item.THTo;
                _hour.daystatus = item.DayStatus;
                _hour.approved = item.DayApproved;

                _hour.color = "blue";
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
                CustomerId = hours.Customer?.CustomerId,
                project = hours.Project?.ProjectId,
                activity = hours.Activity?.ActivityId,
                hours.TimeHoursId,
                category = hours.Category?.CategoryId,
                color = hours.Project?.color,
                hours.THDate,
                hours.THFrom,
                hours.THTo,
                hours.ActDescription,
                hours.Billable,
                hours.THours, 
                hours.InternalNote
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
                
                // Get date range based on calendar view
                if (CalView == "timeGridDay")
                {
                    start = date;
                    end = date;
                }
                else if (CalView == "dayGridMonth")
                {
                    if (date.Day > 20)
                    {
                        date = date.AddDays(10);
                    }
                    var dates = Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month))
                             .Select(day => new DateTime(date.Year, date.Month, day))
                             .ToList();
                    start = Convert.ToDateTime(dates.First());
                    end = Convert.ToDateTime(dates.Last());
                }
                else if (CalView == "timeGridWeek")
                {
                    Int32 firstDayOfWeek = (Int32)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                    Int32 dayOfWeek = (Int32)date.DayOfWeek;
                    DateTime startOfWeek = date.AddDays(firstDayOfWeek - dayOfWeek);
                    var valuesDaysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<Int32>();
                    var dates = valuesDaysOfWeek.Select(v => startOfWeek.AddDays(v)).ToList();
                    start = Convert.ToDateTime(dates.First());
                    end = Convert.ToDateTime(dates.Last());
                }
                else
                {
                    start = date;
                    end = date;
                }

                // Get correct hours calculation directly from TimeHours table
                var timeHoursGrouped = db.TimeHours
                    .Where(x => x.THDate >= start && x.THDate <= end && x.UserId == UserId && x.Visible != false)
                    .GroupBy(x => x.THDate)
                    .Select(g => new {
                        THDate = g.Key,
                        TotalHours = g.Sum(x => x.THours ?? 0)
                    })
                    .ToList();

                // Get DaysUser status information
                var daysUserStatus = db.DaysUser
                    .Where(x => x.DayDate >= start && x.DayDate <= end && x.UserId == UserId)
                    .ToList();

                // Create the model combining both data sources
                var dateRange = new List<DateTime>();
                for (var d = start; d <= end; d = d.AddDays(1))
                {
                    dateRange.Add(d);
                }

                model = dateRange.Select(d => {
                    var hoursData = timeHoursGrouped.FirstOrDefault(x => x.THDate.Date == d.Date);
                    var statusData = daysUserStatus.FirstOrDefault(x => x.DayDate.Date == d.Date);

                    // Create a view_DaysUserStatus-like object
                    return new view_DaysUserStatus
                    {
                        THDate = d,
                        quantity = hoursData?.TotalHours ?? 0,
                        DayStatus = statusData?.DayStatus,
                        UserId = UserId
                    };
                }).Where(x => x.quantity > 0 || x.DayStatus != null).ToList();

                return PartialView("~/Views/Home/_DaysStatus.cshtml", model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult ValidateDayForApproval(DateTime date)
        {
            try
            {
                int userId = Convert.ToInt32(GetUser());
                
                // Get all TimeHours for this date that are visible
                var timeHours = db.TimeHours
                    .Where(x => x.THDate.Date == date.Date && x.UserId == userId && x.Visible != false)
                    .ToList();

                var invalidEvents = new List<string>();

                foreach (var timeHour in timeHours)
                {
                    bool hasIssues = false;
                    var issues = new List<string>();

                    // Check if required fields are missing
                    if (timeHour.CustomerId == null)
                    {
                        issues.Add("missing Customer");
                        hasIssues = true;
                    }

                    if (timeHour.ProjectId == null)
                    {
                        issues.Add("missing Project");
                        hasIssues = true;
                    }

                    if (timeHour.ActivityId == null)
                    {
                        issues.Add("missing Activity");
                        hasIssues = true;
                    }

                    if (timeHour.CategoryId == null)
                    {
                        issues.Add("missing Category");
                        hasIssues = true;
                    }

                    if (hasIssues)
                    {
                        var eventDescription = $"• {timeHour.THFrom}-{timeHour.THTo}: {timeHour.ActDescription} ({string.Join(", ", issues)})";
                        invalidEvents.Add(eventDescription);
                    }
                }

                return Json(new
                {
                    isValid = invalidEvents.Count == 0,
                    invalidEvents = invalidEvents,
                    totalEvents = timeHours.Count,
                    invalidCount = invalidEvents.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isValid = false,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
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

        // Método temporal para debugging de horas diarias
        public JsonResult TestDecimalPrecision()
        {
            try
            {
                // Test the exact scenario described: 20min + 40min = 60min
                decimal twentyMin = (20m / 60m); // 0.33333...
                decimal fortyMin = (40m / 60m);   // 0.66666...
                decimal sum = twentyMin + fortyMin; // Should be 1.00
                
                return Json(new {
                    TwentyMinutes = twentyMin,
                    FortyMinutes = fortyMin,
                    Sum = sum,
                    TwentyFormatted = twentyMin.ToString("F2"),
                    FortyFormatted = fortyMin.ToString("F2"),
                    SumFormatted = sum.ToString("F2"),
                    FormattedSum = decimal.Parse(twentyMin.ToString("F2")) + decimal.Parse(fortyMin.ToString("F2")),
                    Message = "Testing 20min + 40min precision"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DebugDayHours(DateTime date)
        {
            try
            {
                int userId = Convert.ToInt32(GetUser());
                
                // Obtener todos los TimeHours del día específico
                var timeHours = db.TimeHours.Where(x => x.THDate == date && x.UserId == userId).ToList();
                
                var debugInfo = timeHours.Select(x => new {
                    TimeHoursId = x.TimeHoursId,
                    THDate = x.THDate,
                    THFrom = x.THFrom,
                    THTo = x.THTo,
                    THours = x.THours,
                    Description = x.ActDescription,
                    GCalendarId = x.GCalendarId,
                    Visible = x.Visible,
                    InternalNote = x.InternalNote,
                    ProjectName = x.Project?.ProjectName,
                    CustomerName = x.Customer?.CustomerName
                }).ToList();
                
                var totalHours = timeHours.Sum(x => x.THours ?? 0);
                var visibleHours = timeHours.Where(x => x.Visible != false).Sum(x => x.THours ?? 0);
                
                // También obtener los datos de la vista
                var viewData = db.view_DaysUserStatus.Where(x => x.THDate == date && x.UserId == userId).FirstOrDefault();
                
                return Json(new { 
                    DirectTimeHours = debugInfo,
                    TotalHoursFromTimeHours = totalHours,
                    VisibleHoursFromTimeHours = visibleHours,
                    ViewData = viewData != null ? new {
                        THDate = viewData.THDate,
                        Quantity = viewData.quantity,
                        DayStatus = viewData.DayStatus,
                        UserId = viewData.UserId
                    } : null,
                    Date = date.ToString("yyyy-MM-dd"),
                    UserId = userId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
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
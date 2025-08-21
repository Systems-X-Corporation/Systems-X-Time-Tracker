using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeTracker.Controllers.Reports
{
    public class ReportsController : SystemController
    {
        // GET: Reports
        public ActionResult AllData()
        {
            return View("~/Views/Reports/AllData/AllData.cshtml");
        }

        public ActionResult DetailList(int project = 0, int user = 0, string from = "", string to = "", string records = "")
        {
            try
            {
                var hours = db.TimeHours.ToList();
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

                ViewBag.project = project;
                ViewBag.user = user;
                ViewBag.from = from;
                ViewBag.to = to;
                ViewBag.records = records;

                return PartialView("~/Views/Reports/AllData/_AllDataList.cshtml", hours);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}
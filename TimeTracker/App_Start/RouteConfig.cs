using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TimeTracker.Models;

namespace TimeTracker
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
             name: "Home",
             url: "Home",
             defaults: new { controller = "Time", action = "Time", id = UrlParameter.Optional }

         );

            routes.MapRoute(
               name: "Login",
               url: "Login",
               defaults: new { controller = "UserAccount", action = "Login", id = UrlParameter.Optional }
           );

            routes.MapRoute(
              name: "recover",
              url: "recover",
              defaults: new { controller = "UserAccount", action = "NewPassword", mail = UrlParameter.Optional }
          );


            routes.MapRoute(
              name: "projects",
              url: "projects",
              defaults: new { controller = "Projects", action = "Project", pageId = GetPageId("projects") }
          );

            routes.MapRoute(
              name: "customers",
              url: "customers",
              defaults: new { controller = "Customer", action = "customer", pageId = GetPageId("customer") }
          );

            routes.MapRoute(
              name: "week-time",
              url: "week-time",
              defaults: new { controller = "Time", action = "WeekTime", pageId = GetPageId("week-time") }
          );

            routes.MapRoute(
              name: "week-report",
              url: "week-report",
              defaults: new { controller = "Reports", action = "weekReport", pageId = GetPageId("week-report") }
          );

            routes.MapRoute(
              name: "billable-hours",
              url: "billable-hours",
              defaults: new { controller = "Reports", action = "billableHours", pageId = GetPageId("billable-hours") }
          );

            routes.MapRoute(
              name: "activity",
              url: "activity",
              defaults: new { controller = "Activity", action = "Activity", pageId = GetPageId("activity") }
          );

            routes.MapRoute(
              name: "category",
              url: "category",
              defaults: new { controller = "Category", action = "Category", pageId = GetPageId("category") }
          );

            routes.MapRoute(
              name: "users",
              url: "users",
              defaults: new { controller = "Users", action = "Users", pageId = GetPageId("users") }
          );

            routes.MapRoute(
              name: "privileges",
              url: "privileges",
              defaults: new { controller = "Privileges", action = "privileges", pageId = GetPageId("privileges") }
          );

            routes.MapRoute(
              name: "roles",
              url: "roles",
              defaults: new { controller = "Roles", action = "Roles", pageId = GetPageId("roles") }
          );


            routes.MapRoute(
            name: "Userrole",
            url: "user-role",
            defaults: new { controller = "UserRole", action = "UserRole", pageId = GetPageId("roles") }
        );

            routes.MapRoute(
            name: "approve-days",
            url: "approve-days",
            defaults: new { controller = "ApproveDays", action = "ApproveDays", pageId = GetPageId("approve-days") }
        );

            routes.MapRoute(
         name: "approve-list",
         url: "approve-list",
         defaults: new { controller = "ApproveDays", action = "ApproveDaysList", pageId = GetPageId("approve-list") }
     );


            routes.MapRoute(
        name: "UserProject",
        url: "project-users",
        defaults: new { controller = "UserProject", action = "UserProject", pageId = GetPageId("project-users") }
    );


            routes.MapRoute(
      name: "UserProfile",
      url: "UserProfile",
      defaults: new { controller = "UserAccount", action = "UserProfile", pageId = GetPageId("project-users") }
  );
            routes.MapRoute(
      name: "ChangePassword",
      url: "ChangePassword",
      defaults: new { controller = "UserAccount", action = "ChangePassword", pageId = GetPageId("project-users") }
  );

            routes.MapRoute(
 name: "all-data",
 url: "all-data",
 defaults: new { controller = "Reports", action = "AllData", pageId = GetPageId("all-data") }
);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Time", action = "Time", id = UrlParameter.Optional }
            );
        }


        public static int GetPageId(string url)
        {
            timetrackerDBEntities db = new timetrackerDBEntities();
            int id_web = 0;

            bool existe = db.Pages.Any(p => p.PageUrl == url);
            if (existe)
            {
                id_web = db.Pages.Where(p => p.PageUrl == url)
                    .Select(p => p.PageId).FirstOrDefault();
            }

            return id_web;
        }


    }
}

using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Security
{
    public class PrivilegesController : SystemController
    {
        public ActionResult Privileges(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Security/Privileges/Privileges.cshtml");

        }

        public ActionResult GetPrivileges(int id = 0)
        {
            var model = db.Privilege.ToList();
            var model1 = db.RolesPrivileges.Where(x => x.Roles.RoleId == id).Select(x => x.Privilege.PrivilegeId).ToList();
            ViewData["selected"] = model1;
            ViewBag.id = id;
            return PartialView("~/Views/Security/Privileges/_TreeListPrivileges.cshtml", model);
        }

        public ActionResult AsignPrivilegeRole(int role, List<String> privilege)
        {
            var model = db.RolesPrivileges;
            if (ModelState.IsValid)
            {
                try
                {
                    using (var x = new timetrackerDBEntities())
                    {
                        var z = (from y in x.RolesPrivileges
                                 where y.Roles.RoleId == role
                                 select y).ToList();
                        if (z != null)
                        {
                            x.RolesPrivileges.RemoveRange(z);
                            x.SaveChanges();
                        }
                        
                    }
                    if (privilege != null)
                    {
                        foreach (var items in privilege)
                        {
                            Models.RolesPrivileges item = new Models.RolesPrivileges();
                            Privilege privilege1 = db.Privilege.Where(x => x.PrivilegeId == items).FirstOrDefault();
                            item.Privilege = privilege1;
                            Roles role1 = db.Roles.Where(x => x.RoleId == role).FirstOrDefault();
                            item.Roles = role1;


                            model.Add(item);
                        }
                       
                        db.SaveChanges();
                    }
                  

                  
                       
                   
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return GetPrivileges(role);
        }


    }
}
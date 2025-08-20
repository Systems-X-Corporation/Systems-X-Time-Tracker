using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Security
{
    public class RolesController : SystemController
    {
        // GET: Roles
        public ActionResult Roles(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Security/Roles/Roles.cshtml");

        }

        public ActionResult ListRoles()
        {
            var model = db.Roles.ToList();
            return PartialView("~/Views/Security/Roles/_ListRoles.cshtml", model);
        }




        public ActionResult RolesAddNew(FormCollection formCollection)
        {
            var model = db.Roles;
            Models.Roles newRoles = new Models.Roles();

            try
            {


                newRoles.RoleDescription = formCollection["Description"];

                newRoles.RoleComment = formCollection["Comment"];
                newRoles.CreateUser = Convert.ToInt32(GetUser());
                newRoles.CreateDate = DateTime.Now;
                bool active = false;
                if (formCollection["Active"] == "on")
                {
                    active = true;
                }
                newRoles.RoleActive = active;

                model.Add(newRoles);
                db.SaveChanges();
                string classs = JsonConvert.SerializeObject(newRoles);
               



            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Security/Roles/_ListRoles.cshtml", model.ToList());

        }

        public ActionResult RolesUpdate(FormCollection formCollection)
        {
            var model = db.Roles;

            try
            {

                int RolesId = Convert.ToInt16(formCollection["RoleId"].ToString());

                Roles newRoles = db.Roles.FirstOrDefault(x => x.RoleId == RolesId);

                newRoles.RoleDescription = formCollection["eDescription"];
                newRoles.RoleComment = formCollection["eComment"];
                newRoles.ModifyUser = Convert.ToInt32(GetUser());
                newRoles.ModifyDate = DateTime.Now;
                bool active = false;
                if (formCollection["eActive"] == "on")
                {
                    active = true;
                }
                newRoles.RoleActive = active;

                db.Entry(newRoles).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Security/Roles/_ListRoles.cshtml", model.ToList());

        }


        public ActionResult DeleteRoles(string id)
        {
            try
            {
                int RolesId = Convert.ToInt32(id);
                using (var ctx = new timetrackerDBEntities())
                {
                    var x = (from y in ctx.Roles
                             where y.RoleId == RolesId
                             select y).FirstOrDefault();
                    ctx.Roles.Remove(x);
                    ctx.SaveChanges();
                }
                var model = db.Roles;
                return PartialView("~/Views/Security/Roles/_ListRoles.cshtml", model.ToList());
            }
            catch (Exception e)
            {
                var model = db.Roles;
                ViewData["EditError"] = e.Message;
                return PartialView("~/Views/Security/Roles/_ListRoles.cshtml", model.ToList());
            }
        }



        public JsonResult GetRoles(int id)
        {
            try
            {
                var data = db.Roles.Where(x => x.RoleId == id).FirstOrDefault();

                return Json(new { data.RoleId, data.RoleDescription, data.RoleComment, data.RoleActive }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


    }
}
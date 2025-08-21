using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Manteinance
{
    public class CategoryController : SystemController
    {
        public ActionResult Category(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Manteinance/Category/Category.cshtml");

        }

        public ActionResult ListCategory()
        {
            var model = db.Category.ToList();
            return PartialView("~/Views/Manteinance/Category/_ListCategory.cshtml", model);
        }

        public ActionResult CategoryAddNew(FormCollection formCollection)
        {
            var model = db.Category;
            Category newCategory = new Category();
            try
            {
                newCategory.CategoryName = formCollection["CategoryName"];
                newCategory.CreateUser = GetUser().ToString();
                newCategory.CreateDate = DateTime.Now;
                model.Add(newCategory);
                db.SaveChanges();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Manteinance/Category/_ListCategory.cshtml", model.ToList());

        }

        public ActionResult CategoryUpdate(FormCollection formCollection)
        {
            var model = db.Category;

            try
            {

                int CategoryId = Convert.ToInt16(formCollection["EdCategoryId"].ToString());

                Category newCategory = db.Category.FirstOrDefault(x => x.CategoryId == CategoryId);

                newCategory.CategoryName = formCollection["EdCategoryName"];
                newCategory.ModifyUser = GetUser().ToString();
                newCategory.ModifyDate = DateTime.Now;
                db.Entry(newCategory).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Manteinance/Category/_ListCategory.cshtml", model.ToList());

        }


        public ActionResult DeleteCategory(string id)
        {
            try
            {
                int CategoryId = Convert.ToInt32(id);
                using (var ctx = new timetrackerDBEntities())
                {
                    var x = (from y in ctx.Category
                             where y.CategoryId == CategoryId
                             select y).FirstOrDefault();
                    ctx.Category.Remove(x);
                    ctx.SaveChanges();
                }
                var model = db.Category;
                return PartialView("~/Views/Manteinance/Category/_ListCategory.cshtml", model.ToList());
            }
            catch (Exception e)
            {
                var model = db.Category;
                ViewData["EditError"] = e.Message;
                return PartialView("~/Views/Manteinance/Category/_ListCategory.cshtml", model.ToList());
            }
        }



        public JsonResult GetCategory(string id)
        {
            try
            {
                int CategoryId = Convert.ToInt32(id);
                var data = db.Category.Where(x => x.CategoryId == CategoryId).FirstOrDefault();

                return Json(new { data.CategoryId, data.CategoryName, data.CreateUser, data.CreateDate }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }
}
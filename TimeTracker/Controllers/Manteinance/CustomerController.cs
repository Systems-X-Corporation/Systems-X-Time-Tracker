using DevExpress.Utils.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Models;

namespace TimeTracker.Controllers.Manteinance
{
    public class CustomerController : SystemController
    {
        // GET: Customer
        public ActionResult customer(int pageId)
        {
            setHeader(pageId);
            return View("~/Views/Manteinance/Customer/customer.cshtml");

        }

        public ActionResult ListCustomer()
        {
            var model = db.Customer.OrderBy(x=>x.CustomerName).ToList();
            return PartialView("~/Views/Manteinance/Customer/_ListCustomer.cshtml",model);
        }

        public ActionResult CustomerAddNew(FormCollection formCollection)
        {
            var model = db.Customer;
            Customer newCustomer = new Customer();
            try
            {
                int idOffice = Convert.ToInt32(formCollection["OfficeId"]);
                newCustomer.CustomerName = formCollection["CustomerName"];
                newCustomer.CreateUser = GetUser().ToString();
                newCustomer.CreateDate = DateTime.Now;
                newCustomer.OfficeId = idOffice;
                newCustomer.Office = db.Office.FirstOrDefault(x => x.OfficeId == idOffice);
                model.Add(newCustomer);
                db.SaveChanges();
               
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Manteinance/Customer/_ListCustomer.cshtml", model.ToList());

        }

        public ActionResult CustomerUpdate(FormCollection formCollection)
        {
            var model = db.Customer;

            try
            {

                int customerId = Convert.ToInt16(formCollection["EdCustomerId"].ToString());
                
                Customer newCustomer = db.Customer.FirstOrDefault(x => x.CustomerId == customerId);
                int idOffice = Convert.ToInt32(formCollection["eOfficeId"]);
                newCustomer.CustomerName = formCollection["EdCustomerName"];
                newCustomer.ModifyUser = GetUser().ToString();
                newCustomer.ModifyDate = DateTime.Now;
                newCustomer.OfficeId = idOffice;
                newCustomer.Office = db.Office.FirstOrDefault(x => x.OfficeId == idOffice);
                db.Entry(newCustomer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return PartialView("~/Views/Manteinance/Customer/_ListCustomer.cshtml", model.ToList());

        }


        public ActionResult DeleteCustomer(string id)
        {
            try
            {
                int customerId = Convert.ToInt32(id);
                using (var ctx = new timetrackerDBEntities())
                {
                    var x = (from y in ctx.Customer
                             where y.CustomerId == customerId
                             select y).FirstOrDefault();
                    ctx.Customer.Remove(x);
                    ctx.SaveChanges();
                }
                var model = db.Customer;
                return PartialView("~/Views/Manteinance/Customer/_ListCustomer.cshtml", model.ToList());
            }
            catch (Exception e)
            {
                var model = db.Customer;
                ViewData["EditError"] = e.Message;
                return PartialView("~/Views/Manteinance/Customer/_ListCustomer.cshtml", model.ToList());
            }
        }

        public JsonResult CanDeleteCustomer(int id)
        {
            try
            {
                var data = db.Project.Any(x => x.CustomerId == id);

                return Json(data , JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public JsonResult GetCustomer(int id)
        {
            try
            {
                var data = db.Customer.Where(x => x.CustomerId == id).FirstOrDefault();

                return Json(new {data.CustomerId, data.CustomerName, data.CreateUser, data.CreateDate, data.Office.OfficeId}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
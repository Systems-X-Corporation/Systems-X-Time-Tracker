using Azure;
using Azure.Core;
using DevExpress.Data.Mask.Internal;
using DevExpress.XtraRichEdit.Import.Doc;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using NodaTime;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Migrations;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using TimeTracker.Controllers.Timer;
using TimeTracker.Helpers;
using TimeTracker.Models;
using TimeZoneConverter;

namespace TimeTracker.Controllers
{
    public class OAuthController : SystemController
    {
        public string _client_id = "995855232789-vdnfin1cs6dkvi6dappt4guv7f3m43be.apps.googleusercontent.com";
        private string _client_secret = "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";
        //private string idUser;
        // GET: OAuth

        public ActionResult OauthRedirect()
        {


            var client_id = _client_id;

            var redirectUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                         "scope=https://www.googleapis.com/auth/calendar+https://www.googleapis.com/auth/calendar.events&" +
                         "access_type=offline&" +
                         "include_granted_scopes=true&" +
                         "response_type=code&" +
                         "state=hellothere&" +
                         "redirect_uri=https://localhost:44361/oauth/callback&" +
                         "client_id=" + client_id.ToString();

            //var redirectUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
            //            "scope=https://www.googleapis.com/auth/calendar+https://www.googleapis.com/auth/calendar.events&" +
            //            "access_type=offline&" +
            //            "include_granted_scopes=true&" +
            //            "response_type=code&" +
            //            "state=hellothere&" +
            //            "redirect_uri=https://www.sx-timetracker.com/oauth/callback&" +
            //            "client_id=" + client_id.ToString();


            return Redirect(redirectUrl);

        }

        public ActionResult Callback(string code, string error, string state)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                this.GetTokens(code);
            }
            return Redirect("/UserProfile");
        }

        public ActionResult GetTokens(string code)
        {
            RestClient restClient = new RestClient("https://oauth2.googleapis.com/token");
            RestRequest request = new RestRequest();

            request.AddParameter("client_id", _client_id);
            request.AddParameter("client_secret", _client_secret);
            request.AddParameter("code", code);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("redirect_uri", "https://localhost:44361/oauth/callback");
            //request.AddParameter("redirect_uri", "https://www.sx-timetracker.com/oauth/callback");



            var response = restClient.Post(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                int idUser = Convert.ToInt32(GetUser());
                JObject resp = JObject.Parse(response.Content);
                var model = db.GCToken;
                var tok = db.GCToken.Where(x => x.idUsuario == idUser).ToList();
                model.RemoveRange(tok);
                db.SaveChanges();
                GCToken gCToken = new GCToken();
                gCToken.idUsuario = idUser;
                gCToken.access_token = resp["access_token"].ToString();
                if (!string.IsNullOrEmpty((string)resp["refresh_token"]))
                {
                    gCToken.refresh_token = resp["refresh_token"].ToString();
                }
                else
                {
                    gCToken.refresh_token = "";
                }

                gCToken.expires_in = resp["expires_in"].ToString();
                gCToken.scope = resp["scope"].ToString();
                gCToken.token_type = resp["token_type"].ToString();
                gCToken.texto = response.Content.ToString();

                model.Add(gCToken);
                db.SaveChanges();

                return Redirect("/Home");

            }

            return View("Error");

        }

        public void AllEvents(int idUser)
        {
            try
            {
                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                var idUsuario = idUser;
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                request.AddQueryParameter("key", "AIzaSyA0B3OLn5GyjeOTHc5WAgu2QLj73iJyaW8");
                request.AddHeader("Authorization", "Bearer " + DBtoken.access_token);
                request.AddHeader("Accept", "application/json");

                var response = restClient.Get(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject calendarEvents = JObject.Parse(response.Content);
                    var allEvents = calendarEvents["items"].ToObject<IEnumerable<Event>>();

                    DateTime start = DateTime.Now.AddDays(-15);

                    var work = db.Activity.Where(x => x.ActivityName == "Google Calendar Event").FirstOrDefault();
                    var user = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();

                    Project project = db.Project.Where(x => x.ProjectName == "Google Calendar Event").FirstOrDefault();
                    Activity activity = db.Activity.Where(x => x.ActivityName == "Google Calendar Event").FirstOrDefault();
                    Customer customer = db.Customer.Where(x => x.CustomerName == "Google Calendar Event").FirstOrDefault();
                    Category category = db.Category.Where(x => x.CategoryName == "Meetings / Reuniones").FirstOrDefault();
                    Users users = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();

                    List<TimeHours> newevents = new List<TimeHours>();
                    List<TimeHours> updevents = new List<TimeHours>();

                    foreach (var item in allEvents.Where(x => x.Status == "cancelled"))
                    {
                        var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();

                        db.TimeHours.Remove(timeHours);
                        db.SaveChanges();

                    }

                    foreach (var item in allEvents.Where(x => x.Status != "cancelled" && x.Start != null))
                    {
                        TimeSpan timeSpan = Convert.ToDateTime(item.End.DateTime) - Convert.ToDateTime(item.Start.DateTime);


                        var exist = db.TimeHours.Where(x => x.GCalendarId == item.Id).Any();
                        if (exist)
                        {
                            var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();

                            timeHours.THDate = Convert.ToDateTime(item.Start.DateTime);
                            timeHours.THFrom = Convert.ToDateTime(item.Start.DateTime).ToString("HH:mm");
                            timeHours.THTo = Convert.ToDateTime(item.End.DateTime).ToString("HH:mm");
                            timeHours.Duration = 0;
                            timeHours.UserId = idUsuario;
                            timeHours.Users = user;
                            timeHours.Customer = work.Project.Customer;
                            timeHours.Project = work.Project;
                            //timeHours.Activity = work;
                            timeHours.ActDescription = item.Summary;
                            timeHours.Billable = true;

                            //timeHours.Category = category;
                            timeHours.InternalNote = "";
                            db.Entry(timeHours).State = System.Data.Entity.EntityState.Modified;
                            updevents.Add(timeHours);
                        }
                        else
                        {
                            TimeHours timeHours = new TimeHours();

                            timeHours.THDate = Convert.ToDateTime(item.Start.DateTime);
                            timeHours.THFrom = Convert.ToDateTime(item.Start.DateTime).ToString("HH:mm");
                            timeHours.THTo = Convert.ToDateTime(item.End.DateTime).ToString("HH:mm");
                            timeHours.Billable = true;
                            timeHours.ActDescription = item.Summary;

                            timeHours.UserId = users.UserId;

                            timeHours.THours = Convert.ToDecimal(timeSpan.TotalHours);
                            timeHours.InternalNote = "";
                            timeHours.Visible = true;
                            timeHours.GCalendarId = item.Id;

                            timeHours.ActivityId = activity.ActivityId;
                            timeHours.CategoryId = category.CategoryId;
                            timeHours.CustomerId = customer.CustomerId;
                            timeHours.ProjectId = project.ProjectId;

                            db.Entry(timeHours).State = System.Data.Entity.EntityState.Added;



                        }

                    }

                    var modelTH = db.TimeHours;
                    modelTH.AddRange(newevents);
                    db.SaveChanges();


                }

            }

            catch (Exception ex)
            {
                if (ex.Message.Contains("Unauthorized"))
                {
                    RefreshToken(idUser);
                }
                //OauthRedirect();
                return;

            }



        }




        public ActionResult RefreshToken(int idUser)
        {
            try
            {
                RestClient restClient = new RestClient("https://oauth2.googleapis.com/token");
                RestRequest request = new RestRequest();
                var idUsuario = idUser;
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                request.AddQueryParameter("client_id", _client_id);
                request.AddQueryParameter("client_secret", _client_secret);
                request.AddQueryParameter("grant_type", "refresh_token");
                request.AddQueryParameter("refresh_token", DBtoken.refresh_token);

                var respose = restClient.Post(request);

                if (respose.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject newToken = JObject.Parse(respose.Content);
                    DBtoken.access_token = newToken["access_token"].ToString();
                    DBtoken.expires_in = newToken["expires_in"].ToString();
                    DBtoken.scope = newToken["scope"].ToString();
                    DBtoken.token_type = newToken["token_type"].ToString();
                    db.Entry(DBtoken).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("/Home");

            }
            catch (Exception )
            {
                return RedirectToAction("/Home");
            }
        }


        public bool GetLastEvents(RestClient restClient, RestRequest restRequest)
        {
            try
            {

                 RestResponse restResponse = restClient.Get(restRequest);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject calendarEvents = JObject.Parse(restResponse.Content);
                   
                   

                    if (calendarEvents["nextPageToken"] == null)
                    {
                        var idUsuario = Convert.ToInt32(GetUser());
                        var allEvents = calendarEvents["items"].ToObject<IEnumerable<Event>>();
                        var work = db.Activity.Where(x => x.ActivityName == "Google Calendar Event").FirstOrDefault();
                        var user = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();

                        Project project = db.Project.Where(x => x.ProjectName == "Google Calendar Event").FirstOrDefault();
                        Activity activity = db.Activity.Where(x => x.ActivityName == "Google Calendar Event").FirstOrDefault();
                        Customer customer = db.Customer.Where(x => x.CustomerName == "Google Calendar Event").FirstOrDefault();
                        Category category = db.Category.Where(x => x.CategoryName == "Meetings / Reuniones").FirstOrDefault();
                        Users users = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();

                        List<TimeHours> newevents = new List<TimeHours>();
                        List<TimeHours> updevents = new List<TimeHours>();

                        foreach (var item in allEvents.Where(x => x.Status == "cancelled"))
                        {
                            var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                            if (timeHours != null)
                            {
                                db.TimeHours.Remove(timeHours);
                                db.SaveChanges();
                            }


                        }

                        foreach (var item in allEvents.Where(x => x.Status != "cancelled" && x.Start != null))
                        {
                            DateTime end = Convert.ToDateTime(item.End.DateTimeRaw);
                            DateTime start = Convert.ToDateTime(item.Start.DateTimeRaw);

                            DateTime THDate = Convert.ToDateTime(item.Start.DateTimeRaw);
                            string THFrom;
                            string THTo;


                            if (!string.IsNullOrEmpty(item.Start.TimeZone))
                            {
                                string tz = TZConvert.IanaToWindows(item.Start.TimeZone);

                                THDate = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(tz));
                                THFrom = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(tz)).ToString("HH:mm");
                                THTo = TimeZoneInfo.ConvertTime(end, TimeZoneInfo.FindSystemTimeZoneById(tz)).ToString("HH:mm");

                            }
                            else
                            {
                                THDate = start;
                                THFrom = start.ToString("HH:mm");
                                THTo = end.ToString("HH:mm");

                            }

                           
                           
                           

                            TimeSpan timeSpan = end - start;

                            var exist = db.TimeHours.Where(x => x.GCalendarId == item.Id).Any();
                            if (exist)
                            {
                                if (item.Start.DateTime != null)
                                {
                                    var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                                    //TimeZoneInfo.ConvertTime(item.Start.DateTime, TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone));
                                    timeHours.THDate = THDate;
                                    timeHours.THFrom = THFrom;
                                    timeHours.THTo = THTo;
                                    timeHours.Duration = 0;
                                    timeHours.UserId = idUsuario;
                                    timeHours.Users = user;
                                    timeHours.Customer = work.Project.Customer;
                                    timeHours.Project = work.Project;
                                    //timeHours.Activity = work;
                                    timeHours.ActDescription = item.Summary;
                                    if (string.IsNullOrEmpty(item.Summary))
                                    {
                                        timeHours.ActDescription = "";
                                    }
                                    timeHours.Billable = true;

                                    //timeHours.Category = category;
                                    timeHours.InternalNote = "";
                                    db.Entry(timeHours).State = System.Data.Entity.EntityState.Modified;
                                    updevents.Add(timeHours);
                                }
                              
                            }
                            else
                            {
                                if (item.Start.DateTime != null)
                                {
                                    TimeHours timeHours = new TimeHours();

                                    timeHours.THDate = THDate;
                                    timeHours.THFrom = THFrom;
                                    timeHours.THTo = THTo; 
                                    timeHours.Billable = true;
                                    timeHours.ActDescription = item.Summary;
                                    if (string.IsNullOrEmpty(item.Summary))
                                    {
                                        timeHours.ActDescription = "";
                                    }
                                    timeHours.UserId = users.UserId;

                                    timeHours.THours = Convert.ToDecimal(timeSpan.TotalHours);
                                    timeHours.InternalNote = "";
                                    timeHours.Visible = true;
                                    timeHours.GCalendarId = item.Id;

                                    timeHours.ActivityId = activity.ActivityId;
                                    timeHours.CategoryId = category.CategoryId;
                                    timeHours.CustomerId = customer.CustomerId;
                                    timeHours.ProjectId = project.ProjectId;

                                    db.Entry(timeHours).State = System.Data.Entity.EntityState.Added;

                                    newevents.Add(timeHours);
                                }
                            }
                        }

                        var modelTH = db.TimeHours;
                        modelTH.AddRange(newevents);
                        db.SaveChanges();

                    }
                    else
                    {
                        restRequest.Parameters.RemoveParameter("pageToken");
                        restRequest.AddQueryParameter("pageToken", calendarEvents["nextPageToken"].ToString());
                        GetLastEvents(restClient, restRequest);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return true;
        }


        public JsonResult SyncEvents()
        {
            try
            {
                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                var idUsuario = Convert.ToInt32(GetUser());
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                request.AddQueryParameter("key", "AIzaSyA0B3OLn5GyjeOTHc5WAgu2QLj73iJyaW8");
                request.AddQueryParameter("maxResults", "2500");
                request.AddQueryParameter("orderBy", "updated");
                request.AddQueryParameter("singleEvents", "True");

                request.AddHeader("Authorization", "Bearer " + DBtoken.access_token);
                request.AddHeader("Accept", "application/json");

                GetLastEvents(restClient, request);



                return Json(new { data = "All Events Sync Succesfully" }, JsonRequestBehavior.AllowGet);

            }

            catch (Exception ex)
            {
                if (ex.Message.Contains("Unauthorized"))
                {
                    return Json(new { data = "Google Calendar error, try re-connect" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = ex.Message }, JsonRequestBehavior.AllowGet);
                }

            }

        }






        public JsonResult SyncEvents2()
        {
            try
            {
                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                var idUsuario = Convert.ToInt32(GetUser());
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                request.AddQueryParameter("key", "AIzaSyA0B3OLn5GyjeOTHc5WAgu2QLj73iJyaW8");
                request.AddQueryParameter("maxResults", "2500");

                request.AddHeader("Authorization", "Bearer " + DBtoken.access_token);
                request.AddHeader("Accept", "application/json");

                RestResponse response;

                do
                {

                    response = restClient.Get(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        JObject calendarEvents = JObject.Parse(response.Content);
                        var allEvents = calendarEvents["items"].ToObject<IEnumerable<Event>>();

                        DateTime start = DateTime.Now.AddDays(-15);

                        var work = db.Activity.Where(x => x.ActivityName == "Google Calendar Event").FirstOrDefault();
                        var user = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();

                        Project project = db.Project.Where(x => x.ProjectName == "Google Calendar Event").FirstOrDefault();
                        Activity activity = db.Activity.Where(x => x.ActivityName == "Google Calendar Event").FirstOrDefault();
                        Customer customer = db.Customer.Where(x => x.CustomerName == "Google Calendar Event").FirstOrDefault();
                        Category category = db.Category.Where(x => x.CategoryName == "Meetings / Reuniones").FirstOrDefault();
                        Users users = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();

                        List<TimeHours> newevents = new List<TimeHours>();
                        List<TimeHours> updevents = new List<TimeHours>();

                        foreach (var item in allEvents.Where(x => x.Status == "cancelled"))
                        {
                            var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                            if (timeHours != null)
                            {
                                db.TimeHours.Remove(timeHours);
                                db.SaveChanges();
                            }


                        }

                        foreach (var item in allEvents.Where(x => x.Status != "cancelled" && x.Start != null))
                        {
                            TimeSpan timeSpan = Convert.ToDateTime(item.End.DateTime) - Convert.ToDateTime(item.Start.DateTime);


                            var exist = db.TimeHours.Where(x => x.GCalendarId == item.Id).Any();
                            if (exist)
                            {
                                var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();

                                timeHours.THDate = TimeZoneInfo.ConvertTime(item.Start.DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone));
                                timeHours.THFrom = TimeZoneInfo.ConvertTime(item.Start.DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone)).ToString("HH:mm");
                                timeHours.THTo = TimeZoneInfo.ConvertTime(item.End.DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone)).ToString("HH:mm");
                                timeHours.Duration = 0;
                                timeHours.UserId = idUsuario;
                                timeHours.Users = user;
                                //timeHours.Customer = work.Project.Customer;
                                //timeHours.Project = work.Project;
                                //timeHours.Activity = work;
                                //timeHours.ActDescription = item.Summary;
                                //timeHours.Billable = true;

                                //timeHours.Category = category;
                                //timeHours.InternalNote = "";
                                db.Entry(timeHours).State = System.Data.Entity.EntityState.Modified;
                                updevents.Add(timeHours);
                            }
                            else
                            {
                                TimeHours timeHours = new TimeHours();

                                timeHours.THDate = TimeZoneInfo.ConvertTime(item.Start.DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone));
                                timeHours.THFrom = TimeZoneInfo.ConvertTime(item.Start.DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone)).ToString("HH:mm");
                                timeHours.THTo = TimeZoneInfo.ConvertTime(item.End.DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(item.Start.TimeZone)).ToString("HH:mm");
                                timeHours.Billable = true;
                                timeHours.ActDescription = item.Summary;

                                timeHours.UserId = users.UserId;

                                timeHours.THours = Convert.ToDecimal(timeSpan.TotalHours);
                                timeHours.InternalNote = "";
                                timeHours.Visible = true;
                                timeHours.GCalendarId = item.Id;

                                timeHours.ActivityId = activity.ActivityId;
                                timeHours.CategoryId = category.CategoryId;
                                timeHours.CustomerId = customer.CustomerId;
                                timeHours.ProjectId = project.ProjectId;

                                db.Entry(timeHours).State = System.Data.Entity.EntityState.Added;
                            }
                        }

                        var modelTH = db.TimeHours;
                        modelTH.AddRange(newevents);
                        db.SaveChanges();

                        var nextPageToken = calendarEvents["nextPageToken"].ToString();

                        request.Parameters.RemoveParameter("pageToken");
                        if (!string.IsNullOrEmpty(nextPageToken))
                        {
                            request.AddQueryParameter("pageToken", nextPageToken);
                        }



                    }

                    else
                    {
                        return Json(new { data = response.StatusCode }, JsonRequestBehavior.AllowGet);
                    }

                } while (!string.IsNullOrEmpty(response.Content) && !string.IsNullOrEmpty(request.Parameters.TryFind("pageToken")?.Value.ToString()));


                return Json(new { data = "All Events Sync Succesfully" }, JsonRequestBehavior.AllowGet);

            }

            catch (Exception ex)
            {
                if (ex.Message.Contains("Unauthorized"))
                {
                    RefreshToken(Convert.ToInt32(GetUser()));
                    SyncEvents();
                    return Json(new { data = "All Events Sync Succesfully" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = ex.Message }, JsonRequestBehavior.AllowGet);
                }

            }

        }

        public bool IsConnected(int idUser)
        {
            try
            {
                var user = db.GCToken.Any(x=> x.idUsuario ==  idUser);


                return user;
            }
            catch (Exception)
            {

                throw;
            }

        }

    }



}

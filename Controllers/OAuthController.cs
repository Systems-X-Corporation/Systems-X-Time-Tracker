using Azure;
using Azure.Core;
using DevExpress.Data.Mask.Internal;
using DevExpress.XtraRichEdit.Import.Doc;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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
        // Move to configuration instead of hardcoding
        private string _client_id => System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"] ?? "995855232789-vdnfin1cs6dkvi6dappt4guv7f3m43be.apps.googleusercontent.com";
        private string _client_secret => System.Configuration.ConfigurationManager.AppSettings["GoogleClientSecret"] ?? "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";
        private string _redirect_uri => System.Configuration.ConfigurationManager.AppSettings["GoogleRedirectUri"] ?? "https://localhost:44361/oauth/callback";

        /// <summary>
        /// Converts UTC time to the specified IANA timezone safely
        /// </summary>
        /// <param name="utcTime">UTC DateTime to convert</param>
        /// <param name="ianaTimeZone">IANA timezone identifier (e.g., "America/Costa_Rica")</param>
        /// <returns>DateTime converted to the specified timezone, or UTC if conversion fails</returns>
        private DateTime ConvertToTimeZone(DateTime utcTime, string ianaTimeZone)
        {
            try
            {
                // If no timezone specified, return UTC time as-is
                if (string.IsNullOrEmpty(ianaTimeZone))
                {
                    return utcTime;
                }

                // Convert IANA timezone to Windows timezone
                string windowsTimeZone = TZConvert.IanaToWindows(ianaTimeZone);
                
                // Get the TimeZoneInfo for the target timezone
                TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZone);

                // Convert from UTC to target timezone
                var utcDate = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDate, targetTimeZone);

            }
            catch (Exception ex)
            {
                // Log the error if logging is available
                System.Diagnostics.Debug.WriteLine($"Timezone conversion error: {ex.Message}. Using UTC time.");
                
                // Fallback to UTC time
                return utcTime;
            }
        }
        // GET: OAuth

        public ActionResult OauthRedirect()
        {
            // Generate random state for CSRF protection
            var state = System.Guid.NewGuid().ToString();
            Session["oauth_state"] = state;

            var redirectUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                         "scope=https://www.googleapis.com/auth/calendar+https://www.googleapis.com/auth/calendar.events&" +
                         "access_type=offline&" +
                         "include_granted_scopes=true&" +
                         "response_type=code&" +
                         "prompt=consent&" +
                         "state=" + state + "&" +
                         "redirect_uri=" + Uri.EscapeDataString(_redirect_uri) + "&" +
                         "client_id=" + _client_id;

            return Redirect(redirectUrl);
        }

        public ActionResult Callback(string code, string error, string state)
        {
            // Validate state for CSRF protection
            var expectedState = Session["oauth_state"] as string;
            if (string.IsNullOrEmpty(expectedState) || expectedState != state)
            {
                ViewBag.ErrorMessage = "Invalid state parameter. Possible CSRF attack.";
                return View("Error");
            }

            // Clear the state from session
            Session.Remove("oauth_state");

            if (string.IsNullOrWhiteSpace(error))
            {
                try
                {
                    var tokenResult = this.GetTokens(code);
                    
                    // Set success message - real validation will happen in UI
                    TempData["GoogleCalendarConnected"] = true;
                }
                catch (Exception ex)
                {
                    TempData["GoogleCalendarError"] = "Google Calendar connection failed: " + ex.Message;
                }
            }
            else
            {
                TempData["GoogleCalendarError"] = "OAuth authorization denied: " + error;
            }
            
            return Redirect("/UserProfile");
        }

        public ActionResult GetTokens(string code)
        {
            RestClient restClient = new RestClient("https://oauth2.googleapis.com/token");
            RestRequest request = new RestRequest();
            
            // Set content type for form data
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("client_id", _client_id, ParameterType.GetOrPost);
            request.AddParameter("client_secret", _client_secret, ParameterType.GetOrPost);
            request.AddParameter("code", code, ParameterType.GetOrPost);
            request.AddParameter("grant_type", "authorization_code", ParameterType.GetOrPost);
            request.AddParameter("redirect_uri", _redirect_uri, ParameterType.GetOrPost);

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
                // Store creation timestamp in texto field (JSON format for extensibility)
                var tokenInfo = new {
                    created_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    expires_at = DateTime.UtcNow.AddSeconds(double.Parse(resp["expires_in"].ToString())).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    raw_response = response.Content.ToString()
                };
                gCToken.texto = Newtonsoft.Json.JsonConvert.SerializeObject(tokenInfo);

                model.Add(gCToken);
                db.SaveChanges();

                // Get and save user's timezone automatically
                try
                {
                    GetAndSaveUserTimezone(idUser, gCToken.access_token);
                }
                catch (Exception tzEx)
                {
                    // Don't fail the whole process if timezone detection fails
                    // Log the error but continue (user can still use the system)
                    System.Diagnostics.Debug.WriteLine($"Failed to get user timezone: {tzEx.Message}");
                }

                return Redirect("/Home");

            }

            return View("Error");

        }

        private void GetAndSaveUserTimezone(int userId, string accessToken)
        {
            try
            {
                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/users/me/settings/timezone");
                RestRequest request = new RestRequest();
                
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("Accept", "application/json");

                var response = restClient.Get(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject timezoneResponse = JObject.Parse(response.Content);
                    string userTimezone = timezoneResponse["value"]?.ToString();

                    if (!string.IsNullOrEmpty(userTimezone))
                    {
                        // Update user timezone in database
                        var user = db.Users.FirstOrDefault(x => x.UserId == userId);
                        if (user != null)
                        {
                            user.TimeZone = userTimezone;
                            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            
                            System.Diagnostics.Debug.WriteLine($"User {userId} timezone updated to: {userTimezone}");
                        }
                    }
                }
                else
                {
                    throw new Exception($"Google Calendar API returned {response.StatusCode}: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user timezone from Google Calendar: {ex.Message}", ex);
            }
        }

        public void AllEvents(int idUser)
        {
            try
            {
                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                var idUsuario = idUser;
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                if (DBtoken == null || string.IsNullOrEmpty(DBtoken.access_token))
                {
                    throw new UnauthorizedAccessException("No valid token found");
                }

                // Use proper Calendar API parameters - remove API key as we're using OAuth
                request.AddQueryParameter("singleEvents", "true");
                request.AddQueryParameter("orderBy", "startTime");
                request.AddQueryParameter("timeMin", DateTime.Now.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("timeMax", DateTime.Now.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("maxResults", "2500");
                
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
                        DateTime startTime, endTime;
                        string timeFrom, timeTo;
                        decimal hours = 0;

                        // Handle all-day events and timezone conversion
                        if (!string.IsNullOrEmpty(item.Start.Date))
                        {
                            // All-day event - check if it's exactly 12:00 AM to 11:59 PM
                            startTime = DateTime.Parse(item.Start.Date);
                            endTime = DateTime.Parse(item.End.Date).AddDays(-1); // Google returns next day for all-day events
                            timeFrom = "00:00";
                            timeTo = "23:59";
                            hours = 8.0m; // Default 8 hours for all-day events
                            
                            // Skip events that are exactly 12:00 AM to 11:59 PM (00:00 to 23:59)
                            if (timeFrom == "00:00" && timeTo == "23:59")
                            {
                                continue;
                            }
                        }
                        else if (!string.IsNullOrEmpty(item.Start.DateTimeRaw) && !string.IsNullOrEmpty(item.End.DateTimeRaw))
                        {
                            // Timed event - use values directly as they come in correct timezone
                            startTime = item.Start.DateTime.Value;
                            endTime = item.End.DateTime.Value;
                            
                            timeFrom = startTime.ToString("HH:mm");
                            timeTo = endTime.ToString("HH:mm");
                            TimeSpan duration = endTime - startTime;
                            // Calculate hours with high precision to avoid rounding errors\n                            decimal totalMinutes = (decimal)duration.TotalMinutes;\n                            hours = Math.Round(totalMinutes / 60m, 4); // 4 decimal places for precision
                        }
                        else
                        {
                            continue; // Skip events without proper start/end times
                        }

                        // Skip events that are exactly 12:00 AM (00:00) to 11:59 PM (23:59)
                        if (timeFrom == "00:00" && timeTo == "23:59")
                        {
                            continue;
                        }

                        var exist = db.TimeHours.Where(x => x.GCalendarId == item.Id).Any();
                        if (exist)
                        {
                            var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                            if (timeHours != null)
                            {
                                timeHours.THDate = startTime.Date;
                                timeHours.THFrom = timeFrom;
                                timeHours.THTo = timeTo;
                                timeHours.THours = hours;
                                timeHours.ActDescription = item.Summary ?? "";
                                timeHours.Billable = true;
                                timeHours.InternalNote = "";
                                
                                db.Entry(timeHours).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        else
                        {
                            TimeHours timeHours = new TimeHours();
                            timeHours.THDate = startTime.Date;
                            timeHours.THFrom = timeFrom;
                            timeHours.THTo = timeTo;
                            timeHours.THours = hours;
                            timeHours.Billable = false;
                            timeHours.ActDescription = item.Summary ?? "";
                            timeHours.UserId = idUsuario;
                            timeHours.InternalNote = "";
                            timeHours.Visible = true;
                            timeHours.GCalendarId = item.Id;
                            timeHours.ActivityId = activity.ActivityId;
                            timeHours.CategoryId = category.CategoryId;
                            timeHours.CustomerId = customer.CustomerId;
                            timeHours.ProjectId = project.ProjectId;

                            newevents.Add(timeHours);
                        }
                    }

                    // Actually add new events to the database
                    if (newevents.Any())
                    {
                        db.TimeHours.AddRange(newevents);
                    }
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

                if (DBtoken == null || string.IsNullOrEmpty(DBtoken.refresh_token))
                {
                    return RedirectToAction("OauthRedirect");
                }

                // Set content type for form data
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                request.AddParameter("client_id", _client_id, ParameterType.GetOrPost);
                request.AddParameter("client_secret", _client_secret, ParameterType.GetOrPost);
                request.AddParameter("grant_type", "refresh_token", ParameterType.GetOrPost);
                request.AddParameter("refresh_token", DBtoken.refresh_token, ParameterType.GetOrPost);

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
                        // Only get User for validation
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
                            // Use DateTime values directly as they come in correct timezone
                            DateTime start = Convert.ToDateTime(item.Start.DateTimeRaw);
                            DateTime end = Convert.ToDateTime(item.End.DateTimeRaw);

                            DateTime THDate = start;
                            string THFrom = start.ToString("HH:mm");
                            string THTo = end.ToString("HH:mm");

                            // Skip events that are exactly 12:00 AM (00:00) to 11:59 PM (23:59)
                            if (THFrom == "00:00" && THTo == "23:59")
                            {
                                continue;
                            }

                            TimeSpan timeSpan = end - start;

                            var exist = db.TimeHours.Where(x => x.GCalendarId == item.Id).Any();
                            if (exist)
                            {
                                if (item.Start.DateTime != null)
                                {
                                    var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                                    
                                    // Only update time-related fields, preserve user-assigned fields
                                    timeHours.THDate = THDate;
                                    timeHours.THFrom = THFrom;
                                    timeHours.THTo = THTo;
                                    timeHours.Duration = 0;
                                    timeHours.ActDescription = item.Summary;
                                    if (string.IsNullOrEmpty(item.Summary))
                                    {
                                        timeHours.ActDescription = "";
                                    }
                                    
                                    // DO NOT overwrite user-assigned values (CustomerId, ProjectId, ActivityId, CategoryId)
                                    // Only set them to null if they are currently null (first time sync)
                                    if (timeHours.ActivityId == null) timeHours.ActivityId = null;
                                    if (timeHours.CategoryId == null) timeHours.CategoryId = null;  
                                    if (timeHours.CustomerId == null) timeHours.CustomerId = null;
                                    if (timeHours.ProjectId == null) timeHours.ProjectId = null;
                                    
                                    // Preserve visibility and other user settings
                                    if (timeHours.InternalNote != null && timeHours.InternalNote.Contains("MANUALLY_DELETED"))
                                    {
                                        // Don't update manually deleted events
                                        continue;
                                    }
                                    
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
                                    timeHours.Billable = false;
                                    timeHours.ActDescription = item.Summary;
                                    if (string.IsNullOrEmpty(item.Summary))
                                    {
                                        timeHours.ActDescription = "";
                                    }
                                    timeHours.UserId = users.UserId;

                                    // Calculate hours with high precision to avoid rounding errors\n                                    decimal totalMinutes = (decimal)timeSpan.TotalMinutes;\n                                    timeHours.THours = Math.Round(totalMinutes / 60m, 4); // 4 decimal places for precision
                                    timeHours.InternalNote = "";
                                    timeHours.Visible = true;
                                    timeHours.GCalendarId = item.Id;

                                    // Leave CustomerId, ProjectId, ActivityId, and CategoryId as null for manual assignment
                                    timeHours.ActivityId = null;
                                    timeHours.CategoryId = null;
                                    timeHours.CustomerId = null;
                                    timeHours.ProjectId = null;

                                    db.Entry(timeHours).State = System.Data.Entity.EntityState.Added;

                                    newevents.Add(timeHours);
                                }
                            }
                        }

                        // Actually add new events to the database
                        if (newevents.Any())
                        {
                            db.TimeHours.AddRange(newevents);
                        }
                        db.SaveChanges();

                    }
                    else
                    {
                        // Handle pagination properly
                        var existingPageToken = restRequest.Parameters.FirstOrDefault(p => p.Name == "pageToken");
                        if (existingPageToken != null)
                        {
                            restRequest.Parameters.RemoveParameter(existingPageToken);
                        }
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
                var idUsuario = Convert.ToInt32(GetUser());
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                if (DBtoken == null || string.IsNullOrEmpty(DBtoken.access_token))
                {
                    return Json(new { data = "No valid Google Calendar token found. Please reconnect." }, JsonRequestBehavior.AllowGet);
                }

                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                // Remove API key and use proper OAuth parameters
                request.AddQueryParameter("singleEvents", "true");
                request.AddQueryParameter("orderBy", "startTime");
                request.AddQueryParameter("timeMin", DateTime.Now.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("timeMax", DateTime.Now.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("maxResults", "2500");

                request.AddHeader("Authorization", "Bearer " + DBtoken.access_token);
                request.AddHeader("Accept", "application/json");

                GetLastEvents(restClient, request);

                return Json(new { data = "All Events Sync Successfully" }, JsonRequestBehavior.AllowGet);
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
                var idUsuario = Convert.ToInt32(GetUser());
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                if (DBtoken == null || string.IsNullOrEmpty(DBtoken.access_token))
                {
                    return Json(new { data = "No valid Google Calendar token found. Please reconnect." }, JsonRequestBehavior.AllowGet);
                }

                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                // Remove API key and use proper OAuth parameters
                request.AddQueryParameter("singleEvents", "true");
                request.AddQueryParameter("orderBy", "startTime");
                request.AddQueryParameter("timeMin", DateTime.Now.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("timeMax", DateTime.Now.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ"));
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

                        // Get User info including timezone
                        Users users = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();
                        string userTimeZone = users?.TimeZone ?? "America/Costa_Rica"; // Fallback to Costa Rica timezone

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
                            DateTime startTime, endTime;
                            string timeFrom, timeTo;
                            decimal hours = 0;

                            // Handle all-day events and preserve original times (no timezone conversion)
                            if (!string.IsNullOrEmpty(item.Start.Date))
                            {
                                // All-day event - check if it's exactly 12:00 AM to 11:59 PM
                                startTime = DateTime.Parse(item.Start.Date);
                                endTime = DateTime.Parse(item.End.Date).AddDays(-1);
                                timeFrom = "00:00";
                                timeTo = "23:59";
                                hours = 8.0m;
                                
                                // Skip events that are exactly 12:00 AM to 11:59 PM (00:00 to 23:59)
                                if (timeFrom == "00:00" && timeTo == "23:59")
                                {
                                    continue;
                                }
                            }
                            else if (!string.IsNullOrEmpty(item.Start.DateTimeRaw) && !string.IsNullOrEmpty(item.End.DateTimeRaw))
                            {
                                // Timed event - convert to user's local timezone
                                var startDto = DateTimeOffset.Parse(item.Start.DateTimeRaw);
                                var endDto = DateTimeOffset.Parse(item.End.DateTimeRaw);

                                // 2) Normalizar a UTC
                                var startUtc = startDto.UtcDateTime;
                                var endUtc = endDto.UtcDateTime;

                                // 3) Convertir a la zona del usuario (tu función asume UTC)
                                startTime = ConvertToTimeZone(startUtc, userTimeZone);
                                endTime = ConvertToTimeZone(endUtc, userTimeZone);

                                timeFrom = startTime.ToString("HH:mm");
                                timeTo = endTime.ToString("HH:mm");
                                TimeSpan duration = endTime - startTime;
                                // Calculate hours with high precision to avoid rounding errors\n                            decimal totalMinutes = (decimal)duration.TotalMinutes;\n                            hours = Math.Round(totalMinutes / 60m, 4); // 4 decimal places for precision
                            }
                            else
                            {
                                continue;
                            }

                            // Skip events that are exactly 12:00 AM (00:00) to 11:59 PM (23:59)
                            if (timeFrom == "00:00" && timeTo == "23:59")
                            {
                                continue;
                            }

                            var existingTimeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                            
                            // If event was manually deleted but still exists in Google Calendar, 
                            // re-enable it since user is syncing (wants current Google Calendar state)
                            bool wasManuallyDeleted = existingTimeHours != null && 
                                                    existingTimeHours.InternalNote != null && 
                                                    existingTimeHours.InternalNote.StartsWith("MANUALLY_DELETED_");
                            
                            if (existingTimeHours != null)
                            {
                                // Update existing event (including re-enabling manually deleted ones)
                                existingTimeHours.THDate = startTime.Date;
                                existingTimeHours.THFrom = timeFrom;
                                existingTimeHours.THTo = timeTo;
                                existingTimeHours.THours = hours;
                                existingTimeHours.ActDescription = item.Summary ?? "";
                                existingTimeHours.Billable = false;
                                existingTimeHours.Visible = true; // Re-enable if it was hidden
                                
                                // Clear manual deletion marker since event exists in Google Calendar
                                if (wasManuallyDeleted)
                                {
                                    existingTimeHours.InternalNote = $"RE_SYNCED_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                                }
                                else if (string.IsNullOrEmpty(existingTimeHours.InternalNote) || 
                                        !existingTimeHours.InternalNote.StartsWith("MANUALLY_DELETED_"))
                                {
                                    existingTimeHours.InternalNote = "";
                                }
                                
                                db.Entry(existingTimeHours).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                // Create new event
                                TimeHours timeHours = new TimeHours();
                                timeHours.THDate = startTime.Date;
                                timeHours.THFrom = timeFrom;
                                timeHours.THTo = timeTo;
                                timeHours.THours = hours;
                                timeHours.Billable = false;
                                timeHours.ActDescription = item.Summary ?? "";
                                timeHours.UserId = idUsuario;
                                timeHours.InternalNote = "";
                                timeHours.Visible = true;
                                timeHours.GCalendarId = item.Id;
                                // Leave CustomerId, ProjectId, ActivityId, and CategoryId as null for manual assignment
                                timeHours.ActivityId = null;
                                timeHours.CategoryId = null;
                                timeHours.CustomerId = null;
                                timeHours.ProjectId = null;

                                newevents.Add(timeHours);
                            }
                        }

                        // Actually add new events to the database
                        if (newevents.Any())
                        {
                            db.TimeHours.AddRange(newevents);
                        }
                        db.SaveChanges();

                        // Handle pagination properly
                        var nextPageToken = calendarEvents["nextPageToken"]?.ToString();
                        
                        // Remove existing pageToken parameter
                        var existingPageToken = request.Parameters.FirstOrDefault(p => p.Name == "pageToken");
                        if (existingPageToken != null)
                        {
                            request.Parameters.RemoveParameter(existingPageToken);
                        }
                        
                        if (!string.IsNullOrEmpty(nextPageToken))
                        {
                            request.AddQueryParameter("pageToken", nextPageToken);
                        }



                    }

                    else
                    {
                        return Json(new { data = response.StatusCode }, JsonRequestBehavior.AllowGet);
                    }

                } while (response.StatusCode == System.Net.HttpStatusCode.OK && 
                        JObject.Parse(response.Content)["nextPageToken"] != null);


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

        public JsonResult SyncTodayEvents()
        {
            try
            {
                var idUsuario = Convert.ToInt32(GetUser());
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                if (DBtoken == null || string.IsNullOrEmpty(DBtoken.access_token))
                {
                    return Json(new { data = "No valid Google Calendar token found. Please reconnect." }, JsonRequestBehavior.AllowGet);
                }

                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                // Get today's events only
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                request.AddQueryParameter("singleEvents", "true");
                request.AddQueryParameter("orderBy", "startTime");
                request.AddQueryParameter("timeMin", today.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("timeMax", tomorrow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("maxResults", "2500");

                request.AddHeader("Authorization", "Bearer " + DBtoken.access_token);
                request.AddHeader("Accept", "application/json");

                SyncEventsInRange(restClient, request, idUsuario);

                return Json(new { data = "Today's Events Sync Successfully" }, JsonRequestBehavior.AllowGet);
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

        public JsonResult SyncWeekEvents()
        {
            try
            {
                var idUsuario = Convert.ToInt32(GetUser());
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUsuario).FirstOrDefault();

                if (DBtoken == null || string.IsNullOrEmpty(DBtoken.access_token))
                {
                    return Json(new { data = "No valid Google Calendar token found. Please reconnect." }, JsonRequestBehavior.AllowGet);
                }

                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary/events");
                RestRequest request = new RestRequest();

                // Get current week's events (Monday to Sunday)
                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                var endOfWeek = startOfWeek.AddDays(7);

                request.AddQueryParameter("singleEvents", "true");
                request.AddQueryParameter("orderBy", "startTime");
                request.AddQueryParameter("timeMin", startOfWeek.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("timeMax", endOfWeek.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddQueryParameter("maxResults", "2500");

                request.AddHeader("Authorization", "Bearer " + DBtoken.access_token);
                request.AddHeader("Accept", "application/json");

                SyncEventsInRange(restClient, request, idUsuario);

                return Json(new { data = "This Week's Events Sync Successfully" }, JsonRequestBehavior.AllowGet);
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

        private bool SyncEventsInRange(RestClient restClient, RestRequest restRequest, int idUsuario)
        {
            try
            {
                
                RestResponse restResponse;
                do
                {
                    restResponse = restClient.Get(restRequest);

                    if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        JObject calendarEvents = JObject.Parse(restResponse.Content);
                        var allEvents = calendarEvents["items"].ToObject<IEnumerable<Event>>();

                        // Get User info including timezone
                        Users users = db.Users.Where(x => x.UserId == idUsuario).FirstOrDefault();

                        // Validate that user exists
                        if (users == null)
                        {
                            throw new Exception($"User with ID {idUsuario} not found in database.");
                        }

                        string userTimeZone = users.TimeZone ?? "America/Costa_Rica"; // Fallback to Costa Rica timezone

                        List<TimeHours> newevents = new List<TimeHours>();

                        // Handle cancelled events
                        foreach (var item in allEvents.Where(x => x.Status == "cancelled"))
                        {
                            var timeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                            if (timeHours != null)
                            {
                                db.TimeHours.Remove(timeHours);
                                db.SaveChanges();
                            }
                        }

                        // Create a list to track deleted Google Calendar events to avoid recreating them
                        var deletedGCalendarIds = new List<string>();

                        foreach (var item in allEvents.Where(x => x.Status != "cancelled" && x.Start != null))
                        {
                            DateTime startTime, endTime;
                            string timeFrom, timeTo;
                            decimal hours = 0;

                            // Handle all-day events and preserve original times (no timezone conversion)
                            if (!string.IsNullOrEmpty(item.Start.Date))
                            {
                                // All-day event - check if it's exactly 12:00 AM to 11:59 PM
                                startTime = DateTime.Parse(item.Start.Date);
                                endTime = DateTime.Parse(item.End.Date).AddDays(-1);
                                timeFrom = "00:00";
                                timeTo = "23:59";
                                hours = 8.0m;
                                
                                // Skip events that are exactly 12:00 AM to 11:59 PM (00:00 to 23:59)
                                if (timeFrom == "00:00" && timeTo == "23:59")
                                {
                                    continue;
                                }
                            }
                            else if (!string.IsNullOrEmpty(item.Start.DateTimeRaw) && !string.IsNullOrEmpty(item.End.DateTimeRaw))
                            {
                                // Timed event - convert to user's local timezone
                                var startDto = DateTimeOffset.Parse(item.Start.DateTimeRaw);
                                var endDto = DateTimeOffset.Parse(item.End.DateTimeRaw);

                                // 2) Normalizar a UTC
                                var startUtc = startDto.UtcDateTime;
                                var endUtc = endDto.UtcDateTime;

                                // 3) Convertir a la zona del usuario (tu función asume UTC)
                                startTime = ConvertToTimeZone(startUtc, userTimeZone);
                                endTime = ConvertToTimeZone(endUtc, userTimeZone);

                                timeFrom = startTime.ToString("HH:mm");
                                timeTo = endTime.ToString("HH:mm");
                                TimeSpan duration = endTime - startTime;
                                // Calculate hours with high precision to avoid rounding errors\n                            decimal totalMinutes = (decimal)duration.TotalMinutes;\n                            hours = Math.Round(totalMinutes / 60m, 4); // 4 decimal places for precision
                            }
                            else
                            {
                                continue;
                            }

                            // Skip events that are exactly 12:00 AM (00:00) to 11:59 PM (23:59)
                            if (timeFrom == "00:00" && timeTo == "23:59")
                            {
                                continue;
                            }

                            var existingTimeHours = db.TimeHours.Where(x => x.GCalendarId == item.Id).FirstOrDefault();
                            
                            // If event was manually deleted but still exists in Google Calendar, 
                            // re-enable it since user is syncing (wants current Google Calendar state)
                            bool wasManuallyDeleted = existingTimeHours != null && 
                                                    existingTimeHours.InternalNote != null && 
                                                    existingTimeHours.InternalNote.StartsWith("MANUALLY_DELETED_");
                            
                            if (existingTimeHours != null)
                            {
                                // Update existing event (including re-enabling manually deleted ones)
                                existingTimeHours.THDate = startTime.Date;
                                existingTimeHours.THFrom = timeFrom;
                                existingTimeHours.THTo = timeTo;
                                existingTimeHours.THours = hours;
                                existingTimeHours.ActDescription = item.Summary ?? "";
                                existingTimeHours.Billable = false;
                                existingTimeHours.Visible = true; // Re-enable if it was hidden
                                existingTimeHours.InternalNote = "";
                                
                                db.Entry(existingTimeHours).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                // Create new event
                                TimeHours timeHours = new TimeHours();
                                timeHours.THDate = startTime.Date;
                                timeHours.THFrom = timeFrom;
                                timeHours.THTo = timeTo;
                                timeHours.THours = hours;
                                timeHours.Billable = false;
                                timeHours.ActDescription = item.Summary ?? "";
                                timeHours.UserId = idUsuario;
                                timeHours.InternalNote = "";
                                timeHours.Visible = true;
                                timeHours.GCalendarId = item.Id;
                                // Leave CustomerId, ProjectId, ActivityId, and CategoryId as null for manual assignment
                                timeHours.ActivityId = null;
                                timeHours.CategoryId = null;
                                timeHours.CustomerId = null;
                                timeHours.ProjectId = null;

                                newevents.Add(timeHours);
                            }
                        }

                        // Actually add new events to the database
                        if (newevents.Any())
                        {
                            db.TimeHours.AddRange(newevents);
                        }
                        db.SaveChanges();

                        // Handle pagination
                        var nextPageToken = calendarEvents["nextPageToken"]?.ToString();
                        if (!string.IsNullOrEmpty(nextPageToken))
                        {
                            var existingPageToken = restRequest.Parameters.FirstOrDefault(p => p.Name == "pageToken");
                            if (existingPageToken != null)
                            {
                                restRequest.Parameters.RemoveParameter(existingPageToken);
                            }
                            restRequest.AddQueryParameter("pageToken", nextPageToken);
                        }
                        else
                        {
                            break; // No more pages
                        }
                    }
                    else
                    {
                        // Log error details for debugging
                        string errorMessage = $"Google Calendar API error: {restResponse.StatusCode} - {restResponse.Content}";
                        if (restResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            throw new UnauthorizedAccessException("Google Calendar token expired or invalid");
                        }
                        throw new Exception(errorMessage);
                    }
                } while (true);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult IsConnected()
        {
            try
            {
                // Verificar primero si el usuario está autenticado a nivel básico
                if (User == null || User.Identity == null)
                {
                    return Json(new { 
                        connected = false, 
                        error = "No user context available",
                        debug = "User or User.Identity is null"
                    }, JsonRequestBehavior.AllowGet);
                }

                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { 
                        connected = false, 
                        error = "User not authenticated",
                        debug = "User.Identity.IsAuthenticated is false"
                    }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(User.Identity.Name))
                {
                    return Json(new { 
                        connected = false, 
                        error = "User identity name is empty",
                        debug = "User.Identity.Name is null or empty"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Intentar obtener el ID del usuario
                string userIdStr;
                try
                {
                    userIdStr = GetUser();
                }
                catch (Exception getUserEx)
                {
                    return Json(new { 
                        connected = false, 
                        error = "Failed to get user ID",
                        debug = $"GetUser() threw exception: {getUserEx.Message}"
                    }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { 
                        connected = false, 
                        error = "User ID is empty",
                        debug = "GetUser() returned null or empty string"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Convertir ID de usuario
                int idUser;
                if (!int.TryParse(userIdStr, out idUser))
                {
                    return Json(new { 
                        connected = false, 
                        error = "Invalid user ID format",
                        debug = $"Cannot parse '{userIdStr}' to integer"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Verificar conexión en base de datos
                if (db == null)
                {
                    return Json(new { 
                        connected = false, 
                        error = "Database connection error",
                        debug = "Database context is null"
                    }, JsonRequestBehavior.AllowGet);
                }

                var tokenExists = db.GCToken.Any(x => x.idUsuario == idUser);
                
                // Si existe token, verificar que no esté vacío
                if (tokenExists)
                {
                    var token = db.GCToken.FirstOrDefault(x => x.idUsuario == idUser);
                    var hasValidToken = token != null && !string.IsNullOrEmpty(token.access_token);
                    
                    return Json(new { 
                        connected = hasValidToken,
                        userId = idUser,
                        tokenExists = true,
                        hasAccessToken = token != null && !string.IsNullOrEmpty(token.access_token),
                        debug = $"User ID: {idUser}, Token found, Access token valid: {hasValidToken}"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { 
                    connected = false,
                    userId = idUser,
                    tokenExists = false,
                    debug = $"User ID: {idUser}, No token found in database"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    connected = false, 
                    error = ex.Message,
                    debug = ex.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public bool IsUserConnected(int idUser)
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

        public JsonResult TestGoogleCalendarConnection()
        {
            try
            {
                int idUser = Convert.ToInt32(GetUser());
                var DBtoken = db.GCToken.Where(x => x.idUsuario == idUser).FirstOrDefault();

                if (DBtoken == null || string.IsNullOrEmpty(DBtoken.access_token))
                {
                    return Json(new { 
                        connected = false, 
                        error = "No valid Google Calendar token found. Please reconnect." 
                    }, JsonRequestBehavior.AllowGet);
                }

                // Test actual API connection with a minimal calendar list request
                RestClient restClient = new RestClient("https://www.googleapis.com/calendar/v3/calendars/primary");
                RestRequest request = new RestRequest();
                
                request.AddHeader("Authorization", "Bearer " + DBtoken.access_token);
                request.AddHeader("Accept", "application/json");

                RestResponse response = restClient.Get(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Json(new { 
                        connected = true,
                        message = "Google Calendar connection is working properly"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token expired or invalid - try refresh
                    try
                    {
                        RefreshToken(idUser);
                        
                        // Test again with refreshed token
                        var refreshedToken = db.GCToken.Where(x => x.idUsuario == idUser).FirstOrDefault();
                        if (refreshedToken != null && !string.IsNullOrEmpty(refreshedToken.access_token))
                        {
                            request = new RestRequest();
                            request.AddHeader("Authorization", "Bearer " + refreshedToken.access_token);
                            request.AddHeader("Accept", "application/json");
                            
                            response = restClient.Get(request);
                            
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                return Json(new { 
                                    connected = true,
                                    message = "Google Calendar connection restored after token refresh"
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch
                    {
                        // Refresh failed
                    }
                    
                    return Json(new { 
                        connected = false, 
                        error = "Google Calendar token expired and refresh failed. Please reconnect." 
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { 
                        connected = false, 
                        error = $"Google Calendar API error: {response.StatusCode} - {response.ErrorMessage}" 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    connected = false, 
                    error = "Google Calendar connection test failed: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }



}

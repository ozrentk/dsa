using AdapterDb;
using AutoMapper;
//using DigitalSignageAdapter.Cache;
using DigitalSignageAdapter.Filters;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DigitalSignageAdapter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConfigController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        [TimeZoneActionFilter]
        public ActionResult Index()
        {
            var dbOverview = Database.GetOverview();
            var overview = Mapper.Map<Models.Config.Overview>(dbOverview);
            overview.ClientUtcOffset = ViewBag.TimeZoneOffset;
            overview.ServerUtcOffset = DateTimeOffset.Now.Offset.Hours;
            //overview.CachedLineCount = CacheEngine.Instance.GetLineCount();
            overview.CachedLineCount = Database.GetLineCount();

            return View(overview);
        }

        public ActionResult Options()
        {
            var dbConfigItems = Database.GetConfigItems();
            var options = Mapper.Map<Models.Config.Options>(dbConfigItems);

            return View(options);
        }

        [HttpPost]
        public ActionResult Options(Models.Config.Options options)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var dbConfigItems = Mapper.Map<List<AdapterDb.ConfigItem>>(options);

                bool isSaved = Database.SaveConfigItems(dbConfigItems);
                if (!isSaved)
                {
                    options.IsFailed = true;
                    return View(options);
                }

                return RedirectToAction("Options");
            }

            return View(options);
        }

        public ActionResult Businesses()
        {
            var dbBusinesses = Database.GetBusinessLineList(User);
            var businesses = Mapper.Map<List<Models.Config.Business>>(dbBusinesses);

            return View(businesses);
        }

        public ActionResult Business(int? businessId)
        {
            var business = new Models.Config.Business();
            if (businessId.HasValue)
            {
                var dbBusiness = Database.GetBusiness(businessId.Value);
                business = Mapper.Map<Models.Config.Business>(dbBusiness);
            }

            return View(business);
        }

        [HttpPost]
        public ActionResult Business(Models.Config.Business business)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var dbBusiness = Mapper.Map<AdapterDb.Business>(business);

                bool isSaved = Database.SaveBusiness(dbBusiness);
                if (!isSaved)
                {
                    business.IsFailed = true;
                    return View(business);
                }

                return RedirectToAction("Businesses");
            }

            return View(business);
        }

        public ActionResult DeleteBusiness(int businessId)
        {
            var dbBusiness = Database.GetBusiness(businessId);
            var business = Mapper.Map<Models.Config.Business>(dbBusiness);

            return View(business);
        }

        [HttpPost]
        public ActionResult DeleteBusiness(Models.Config.Business business)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                bool isDeleted = Database.DeleteBusiness(business.Id);
                if (!isDeleted)
                {
                    business.IsFailed = true;
                    return View(business);
                }

                return RedirectToAction("Businesses");
            }

            return View(business);
        }

        public ActionResult Line(int businessId, int? lineId)
        {
            var line = new Models.Config.Line();
            if (lineId.HasValue)
            {
                var dbLine = Database.GetLine(lineId.Value);
                line = Mapper.Map<Models.Config.Line>(dbLine);
            }
            else
            {
                line.BusinessId = businessId;
            }

            return View(line);
        }

        [HttpPost]
        public ActionResult Line(Models.Config.Line line)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                if (line.IsDelete)
                {
                    bool isDeleted = Database.DeleteLine(line.Id);
                    if (!isDeleted)
                    {
                        line.IsFailed = true;
                        return View(line);
                    }
                }
                else
                {
                    var dbLine = Mapper.Map<AdapterDb.Line>(line);

                    bool isSaved = Database.SaveLine(dbLine);
                    if (!isSaved)
                    {
                        line.IsFailed = true;
                        return View(line);
                    }
                }

                return RedirectToAction("Businesses");
            }

            return View(line);
        }

        public ActionResult SignupTicketList()
        {
            var dbTickets = Database.GetSignupTicketList();
            var tickets = Mapper.Map<List<Models.Config.SignupTicket>>(dbTickets).ToList();
            return View(tickets);
        }

        public ActionResult SignupTicket(int? ticketId)
        {
            var ticket = new Models.Config.SignupTicket();
            if (ticketId.HasValue)
            {
                var dbTicket = Database.GetSignupTicket(ticketId.Value);
                ticket = Mapper.Map<Models.Config.SignupTicket>(dbTicket);
            }
            else
            {
                // Initialize new GUID
                ticket.Guid = Guid.NewGuid();
            }

            return View(ticket);
        }

        [HttpPost]
        public ActionResult SignupTicket(Models.Config.SignupTicket ticket)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var dbTicket = Mapper.Map<AdapterDb.UserSignupTicket>(ticket);

                bool isSaved = Database.SaveSignupTicket(dbTicket);
                if (!isSaved)
                {
                    ticket.IsFailed = true;
                    return View(ticket);
                }

                return RedirectToAction("SignupTicketList");
            }

            return View(ticket);
        }

        public ActionResult DeleteSignupTicket(int ticketId)
        {
            var dbTicket = Database.GetSignupTicket(ticketId);
            var ticket = Mapper.Map<Models.Config.SignupTicket>(dbTicket);

            return View(ticket);
        }

        [HttpPost]
        public ActionResult DeleteSignupTicket(Models.Config.SignupTicket ticket)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                bool isDeleted = Database.DeleteSignupTicket(ticket.Id);
                if (!isDeleted)
                {
                    ticket.IsFailed = true;
                    return View(ticket);
                }

                return RedirectToAction("SignupTicketList");
            }

            return View(ticket);
        }

        public ActionResult AddBusinessToSignupTicket(int ticketId)
        {
            var model = new Models.Config.AddBusinessToSignupTicket();
            
            var dbTicket = Database.GetSignupTicket(ticketId);
            model.Ticket = Mapper.Map<Models.Config.SignupTicket>(dbTicket);

            var dbBusinessList = Database.GetBusinessLineList(User);
            model.AllBusinessList = Mapper.Map<List<Models.Config.Business>>(dbBusinessList);

            var selectedBizIds = model.Ticket.Businesses.Select(b => b.Id);
            foreach (var biz in model.AllBusinessList.Where(b => selectedBizIds.Contains(b.Id)))
            {
                biz.IsSelected = true;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AddBusinessToSignupTicket(Models.Config.AddBusinessToSignupTicket model)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var selectedBusinesses = model.AllBusinessList.Where(b => b.IsSelected).Select(b => b.Id).ToList();

                bool isSaved = Database.AddBusinessToSignupTicket(model.Ticket.Id, selectedBusinesses);
                if (!isSaved)
                {
                    model.IsFailed = true;
                    return View(model);
                }

                return RedirectToAction("SignupTicketList");
            }

            return View(model);
        }

    }
}
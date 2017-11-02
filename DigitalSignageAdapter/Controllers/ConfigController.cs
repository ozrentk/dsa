using AdapterDb;
using AutoMapper;
//using DigitalSignageAdapter.Cache;
using DigitalSignageAdapter.Filters;
using DigitalSignageAdapter.Models.Config;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DigitalSignageAdapter.Controllers
{
    [Authorize]
    public class ConfigController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

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

        public ActionResult RolesOverview()
        {
            var model = new RolesOverview();
            model.Roles = AdapterDb.Database.GetAll<AdapterDb.Roles, Role>(
                (dbRoles) => {
                    var mappedRoles = Mapper.Map<IEnumerable<AdapterDb.Roles>, IEnumerable<Role>>(dbRoles).OrderBy(p => p.Name).ToList();
                    mappedRoles.ForEach(r => r.Permissions = r.Permissions.OrderBy(p => p.Description).ToList());
                    return mappedRoles;
                });

            return View(model);
        }

        public ActionResult RoleEditor(int? roleId)
        {
            var model = new RoleEditor();

            var allPermissions = AdapterDb.Database.GetAll<AdapterDb.Permission, Models.Config.Permission>(
                (dbPermissions) =>
                {
                    var mappedPermissions = Mapper.Map<IEnumerable<AdapterDb.Permission>, IEnumerable<Models.Config.Permission>>(dbPermissions);
                    foreach (var p in mappedPermissions)
                        p.IsSelected = false;
                    return mappedPermissions.OrderBy(p => p.Description);
                });

            if (roleId.HasValue)
            {
                model.Role = AdapterDb.Database.Find<AdapterDb.Roles, Role>(
                    roleId.Value,
                    (dbRole) =>
                    {
                        var mappedRole = Mapper.Map<AdapterDb.Roles, Role>(dbRole);
                        foreach (var p in mappedRole.Permissions)
                            p.IsSelected = true;
                        return mappedRole;
                    });
                var selectedPermissionIds = model.Role.Permissions.Select(p => p.Id).ToArray();
                model.Role.Permissions.AddRange(allPermissions.Where(p => !selectedPermissionIds.Contains(p.Id)));
            }
            else
            {
                model.Role = new Role { Permissions = new List<Models.Config.Permission>() };
                model.Role.Permissions.AddRange(allPermissions);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult RoleEditor(DigitalSignageAdapter.Models.Config.RoleEditor model)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                if (model.Role.IsDelete)
                {
                    bool isDeleted = Database.DeleteRole(model.Role.Id.Value);
                    if (!isDeleted)
                    {
                        model.IsFailed = true;
                        return View(model);
                    }

                    return RedirectToAction("RolesOverview");
                }
                else
                {
                    var selectedPermissionIds = model.Role.Permissions.Where(p => p.IsSelected).Select(p => p.Id).ToList();
                    var mappedRole = Mapper.Map<Models.Config.Role, AdapterDb.Roles>(model.Role);
                    bool isSaved;
                    if (model.Role.Id.HasValue)
                    {
                        isSaved = Database.ModifyRole(mappedRole, selectedPermissionIds);
                    }
                    else
                    {
                        isSaved = Database.CreateRole(mappedRole.Name, selectedPermissionIds);
                    }

                    if (!isSaved)
                    {
                        model.IsFailed = true;
                        return View(model);
                    }

                    return RedirectToAction("RolesOverview");
                }
            }

            return View(model);
        }

        public ActionResult UsersOverview()
        {
            var model = new UsersOverview();
            model.Users = AdapterDb.Database.GetAll<AdapterDb.User, Models.Config.User>(
                (dbUsers) => Mapper.Map<IEnumerable<AdapterDb.User>, IEnumerable<Models.Config.User>>(dbUsers))
                                   .OrderBy(u => u.Email)
                                   .ToList();

            return View(model);
        }

        public ActionResult UserEditor(int? userId)
        {
            var model = new UserEditor();

            var allRoles = AdapterDb.Database.GetAll<AdapterDb.Roles, Models.Config.Role>(
                (dbRoles) =>
                {
                    var mappedRoles = Mapper.Map<IEnumerable<AdapterDb.Roles>, IEnumerable<Models.Config.Role>>(dbRoles);
                    foreach (var r in mappedRoles)
                        r.IsSelected = false;
                    return mappedRoles.OrderBy(r => r.Name);
                });

            if (userId.HasValue)
            {
                model.User = AdapterDb.Database.Find<AdapterDb.User, Models.Config.User>(
                    userId.Value,
                    (dbUser) =>
                    {
                        var mappedUser = Mapper.Map<AdapterDb.User, Models.Config.User>(dbUser);
                        foreach (var p in mappedUser.Roles)
                            p.IsSelected = true;
                        return mappedUser;
                    });
                var selectedRoleNames = model.User.Roles.Select(p => p.Name).ToArray();
                model.User.Roles.AddRange(allRoles.Where(p => !selectedRoleNames.Contains(p.Name)));
            }
            else
            {
                model.User = new Models.Config.User { Roles = new List<Models.Config.Role>() };
                model.User.Roles.AddRange(allRoles);
            }

            return View(model);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> UserEditor(Models.Config.UserEditor model)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var mappedUser = Mapper.Map<Models.Config.User, Models.ApplicationUser>(model.User);
                var selectedRoleNames = model.User.Roles.Where(p => p.IsSelected).Select(p => p.Name).ToArray();

                if (model.User.Id.HasValue)
                {
                    await ModifyUserAttributes(mappedUser, selectedRoleNames);
                }
                else
                {
                    await CreateNewUser(mappedUser, selectedRoleNames);
                }

                return RedirectToAction("UsersOverview");
            }

            return View(model);
        }

        [NonAction]
        private async System.Threading.Tasks.Task<bool> CreateNewUser(Models.ApplicationUser mappedUser, string[] selectedRoleNames)
        {
            var pwd = Models.ApplicationUser.GeneratePassword((Microsoft.AspNet.Identity.PasswordValidator)UserManager.PasswordValidator);
            var resCreate = await UserManager.CreateAsync(mappedUser, pwd);
            if (!resCreate.Succeeded)
                return false;

            var resRoleAdd = await UserManager.AddToRolesAsync(mappedUser.Id, selectedRoleNames);
            if (!resRoleAdd.Succeeded)
                return false;

            var code = await UserManager.GenerateEmailConfirmationTokenAsync(mappedUser.Id);

            var confirmationModel =
                new EmailConfirmation
                {
                    ConfirmationUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new
                        {
                            userId = mappedUser.Id,
                            code = code
                        },
                        protocol: Request.Url.Scheme),
                    Email = mappedUser.Email,
                    Password = pwd
                };

            string body = RenderViewToString(
                ControllerContext,
                "EmailConfirmation",
                confirmationModel,
                true);

            await UserManager.SendEmailAsync(
                mappedUser.Id,
                "Confirm your account",
                body);

            return true;
        }

        [NonAction]
        private async System.Threading.Tasks.Task<bool> ModifyUserAttributes(Models.ApplicationUser mappedUser, string[] newRoleNames)
        {
            var oldRoleNames = UserManager.GetRoles(mappedUser.Id).ToArray();
            var toRemove = oldRoleNames.Except(newRoleNames).ToArray();
            var toAdd = newRoleNames.Except(oldRoleNames).ToArray();

            if (toRemove.Length > 0)
            {
                var resRemoved = await UserManager.RemoveFromRolesAsync(mappedUser.Id, toRemove);
                if (!resRemoved.Succeeded)
                    return false;
            }

            if (toAdd.Length > 0)
            {
                var resAdded = await UserManager.AddToRolesAsync(mappedUser.Id, toAdd);
                if (!resAdded.Succeeded)
                    return false;
            }

            // HACK: lockout goes around the ASP.NET Identity
            var resActivity = Database.UpdateUserActivity(mappedUser.Id, mappedUser.IsActive);
            if(!resActivity)
                return false;

            return true;
        }

        [NonAction]
        static string RenderViewToString(ControllerContext context,
                            string viewPath,
                            object model = null,
                            bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view,
                                            context.Controller.ViewData,
                                            context.Controller.TempData,
                                            sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }
    }
}
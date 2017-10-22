using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using DigitalSignageAdapter.Filters;
using DigitalSignageAdapter.Models.Home;
using DigitalSignageAdapter.Models.Shared;

namespace DigitalSignageAdapter.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [TimeZoneActionFilter]
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            if (String.IsNullOrEmpty(User.Identity.Name))
            {
                return View();
            }

            var businessIds = AdapterDb.Database.GetBusinessIds(User.Identity.Name);
            if (User.IsInRole("Admin"))
            {
                // Administrators view - everything (incl users and stuff)
                return RedirectToAction("AdminDashboard");
            }
            else if (businessIds.Count > 1)
            {
                // Multiple businesses
                return RedirectToAction("MultipleBusinesses");
            }
            else if (businessIds.Count == 1)
            {
                // Single businesses
                return RedirectToAction("SingleBusiness", new { businessId = businessIds.First() });
            }

            return View();
        }

        #region Non-actions
        [NonAction]
        private MultipleBusinesses GetAggregatedModel(DateTime timeFrom, DateTime timeTo, List<AdapterDb.Business> businessList)
        {
            var businessIds = businessList.Select(b => b.Id).ToArray();

            var aggregateData =
                AdapterDb.Database.GetAggregatedDataForMultipleBusinesses(
                    user: User,
                    businessIds: businessIds,
                    timeFrom: timeFrom,
                    timeTo: timeTo);

            var viewModel =
                new MultipleBusinesses
                {
                    //UtcTimeFrom = timeFrom,
                    //UtcTimeTo = timeTo,
                    DataByBusiness = aggregateData.Where(ad => ad.BusinessId.HasValue && !ad.LineId.HasValue).ToList(),
                    DataByLineName = aggregateData.Where(ad => !ad.BusinessId.HasValue && !String.IsNullOrEmpty(ad.LineName)).ToList(),
                    TotalData = aggregateData.Single(ad => !ad.BusinessId.HasValue && String.IsNullOrEmpty(ad.LineName))
                };

            return viewModel;
        }

        [NonAction]
        private static List<Role> GetRoleActivity(List<Role> roleList, int[] activeRoleIds)
        {
            return (from r in roleList
                    select new Role
                    {
                        Id = r.Id,
                        Name = r.Name,
                        IsActive = activeRoleIds.Contains(r.Id)
                    }).ToList();
        }

        [NonAction]
        private static List<Business> GetBusinessActivity(List<Business> businessList, int[] activeBusinessIds)
        {
            return (from b in businessList
                    select new Business
                    {
                        Id = b.Id,
                        Name = b.Name,
                        IsActive = activeBusinessIds.Contains(b.Id)
                    }).ToList();
        }
        #endregion Non-actions

        [TimeZoneActionFilter]
        [Authorize(Roles = "Admin")]
        public ActionResult AdminDashboard()
        {
            var roleList = AdapterDb.Database.GetAll<AdapterDb.Roles, Role>(
                (dbRoles) => Mapper.Map<IEnumerable<AdapterDb.Roles>, IEnumerable<Role>>(dbRoles));

            var businessList = AdapterDb.Database.GetAll<AdapterDb.Business, Business>(
                (dbBusinessList) => Mapper.Map<IEnumerable<AdapterDb.Business>, IEnumerable<Business>>(dbBusinessList));

            var userList = AdapterDb.Database.GetAll<AdapterDb.User, User>(
                (dbUsers) =>
                {
                    var users = new List<User>();

                    foreach (var dbUser in dbUsers)
                    {
                        var user = Mapper.Map<AdapterDb.User, User>(dbUser);

                        user.Roles = GetRoleActivity(roleList, dbUser.Roles.Select(r => r.Id).ToArray());
                        user.BusinessList = GetBusinessActivity(businessList, dbUser.BusinessMember.Select(bm => bm.BusinessId).ToArray());

                        users.Add(user);
                    }
                    return users;
                });

            var businessLineList = AdapterDb.Database.GetBusinessLineList(User);

            /* BEGIN ADJUST TIMES */
            //DateTime clientCurrentTime = TimeZoneHelper.ClientCurrentTime(ViewBag.TimeZoneOffset);
            DateTime clientCurrentTime = (new DateTime(2017, 9, 13)).AddHours(16);
            DateTime clientToday = clientCurrentTime.Date;

            //var utcTimeFrom = TimeZoneHelper.ClientToUtc(clientToday, ViewBag.TimeZoneOffset);
            //var utcTimeTo = TimeZoneHelper.ClientToUtc(clientCurrentTime, ViewBag.TimeZoneOffset);
            /* END ADJUST TIMES */

            var aggregatedModel = GetAggregatedModel(clientToday, clientCurrentTime, businessLineList);
            aggregatedModel.ClientTimeFrom = clientToday;
            aggregatedModel.ClientTimeTo = clientCurrentTime;

            var employeeList = AdapterDb.Database.GetCalledEmployees(User, null, clientToday, clientCurrentTime);
            var employeeIds = employeeList.Select(e => e.Id).ToArray();
            var employeeTimes = AdapterDb.Database.GetEmployeeTimes(employeeIds, clientToday, clientCurrentTime);
            var employeeData = Mapper.Map<List<AdapterDb.EmployeeTimes>, List<EmployeeTimes>>(employeeTimes).ToList();

            var model = new AdminDashboard
            {
                BusinessData = aggregatedModel,
                UserList = userList,
                EmployeeData = employeeData
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RemoveUser(int userId)
        {
            var user = AdapterDb.Database.GetUser(userId);

            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult RemoveUser(RemoveUser model)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                bool isRemoved = AdapterDb.Database.RemoveUser(model.Id);
                if (!isRemoved)
                {
                    model.IsFailed = true;
                    return View(model);
                }

                return RedirectToAction("AdminDashboard");
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AssignBusiness(int userId)
        {
            var businessList = AdapterDb.Database.GetAll<AdapterDb.Business, Business>(
                (dbBusinessList) => Mapper.Map<IEnumerable<AdapterDb.Business>, IEnumerable<Business>>(dbBusinessList).ToList());

            var user = AdapterDb.Database.Find<AdapterDb.User, User>(
                userId,
                (dbUser) =>
                {
                    var mappedUser = Mapper.Map<AdapterDb.User, User>(dbUser);
                    mappedUser.BusinessList = GetBusinessActivity(businessList, dbUser.BusinessMember.Select(bm => bm.BusinessId).ToArray());
                    return mappedUser;
                });

            var assignBusiness = new AssignBusiness
            {
                User = user
            };

            return View(assignBusiness);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AssignBusiness(AssignBusiness model)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                var selectedBusinesses = model.User.BusinessList.Where(b => b.IsActive).Select(b => b.Id).ToList();

                bool isSaved = AdapterDb.Database.AssignBusinessToUser(model.User.Id, selectedBusinesses);
                if (!isSaved)
                {
                    model.IsFailed = true;
                    return View(model);
                }

                return RedirectToAction("AdminDashboard");
            }

            return View(model);
        }

        [TimeZoneActionFilter]
        public ActionResult MultipleBusinesses()
        {
            var businessList = AdapterDb.Database.GetBusinessLineList(User);

            /* BEGIN ADJUST TIMES */
            //DateTime clientCurrentTime = TimeZoneHelper.ClientCurrentTime(ViewBag.TimeZoneOffset);
            DateTime clientCurrentTime = (new DateTime(2017, 9, 13)).AddHours(16);
            DateTime clientToday = clientCurrentTime.Date;

            //var utcTimeFrom = TimeZoneHelper.ClientToUtc(clientToday, ViewBag.TimeZoneOffset);
            //var utcTimeTo = TimeZoneHelper.ClientToUtc(clientCurrentTime, ViewBag.TimeZoneOffset);
            /* END ADJUST TIMES */

            MultipleBusinesses model = GetAggregatedModel(clientToday, clientCurrentTime, businessList);
            model.ClientTimeFrom = clientToday;
            model.ClientTimeTo = clientCurrentTime;

            var employeeList = AdapterDb.Database.GetCalledEmployees(User, null, clientToday, clientCurrentTime);
            var employeeIds = employeeList.Select(e => e.Id).ToArray();
            var employeeTimes = AdapterDb.Database.GetEmployeeTimes(employeeIds, clientToday, clientCurrentTime);
            model.EmployeeData = Mapper.Map<List<AdapterDb.EmployeeTimes>, List<EmployeeTimes>>(employeeTimes).ToList();

            return View(model);
        }

        [TimeZoneActionFilter]
        public ActionResult SingleBusiness(int businessId)
        {
            List<AdapterDb.Line> lineList = AdapterDb.Database.GetLineList(User, businessId);

            /* BEGIN ADJUST TIMES */
            //DateTime clientCurrentTime = TimeZoneHelper.ClientCurrentTime(ViewBag.TimeZoneOffset);
            DateTime clientCurrentTime = (new DateTime(2017, 9, 13)).AddHours(16);
            DateTime clientToday = clientCurrentTime.Date;

            //var utcTimeFrom = TimeZoneHelper.ClientToUtc(clientToday, ViewBag.TimeZoneOffset);
            //var utcTimeTo = TimeZoneHelper.ClientToUtc(clientCurrentTime, ViewBag.TimeZoneOffset);
            /* END ADJUST TIMES */

            List<AdapterDb.AggregatedData> aggregateData =
                AdapterDb.Database.GetAggregatedDataForSingleBusiness(
                    user: User,
                    businessId: businessId,
                    timeFrom: clientToday,
                    timeTo: clientCurrentTime);

            var multiBiz = AdapterDb.Database.HasMultipleBusiness(User.Identity.Name);
            var biz = AdapterDb.Database.GetBusiness(businessId);

            var employeeList = AdapterDb.Database.GetCalledEmployees(User, businessId, clientToday, clientCurrentTime);
            var employeeIds = employeeList.Select(e => e.Id).ToArray();
            var employeeTimes = AdapterDb.Database.GetEmployeeTimes(employeeIds, clientToday, clientCurrentTime);
            var employeeData = Mapper.Map<List<AdapterDb.EmployeeTimes>, List<EmployeeTimes>>(employeeTimes).ToList();

            return View(new SingleBusiness
            {
                ClientTimeFrom = clientToday,
                ClientTimeTo = clientCurrentTime,
                //UtcTimeFrom = clientToday,
                //UtcTimeTo = clientCurrentTime,
                UserHasMultipleBusinesses = multiBiz,
                BusinessName = biz.Name,
                DataByLine = aggregateData.Where(ad => ad.LineId.HasValue).ToList(),
                TotalData = aggregateData.Single(ad => ad.BusinessId.HasValue && !ad.LineId.HasValue),
                EmployeeData = employeeData
            });
        }

        [TimeZoneActionFilter]
        public ActionResult Compare()
        {
            /* BEGIN ADJUST TIMES */
            DateTime clientCurrentTime = TimeZoneHelper.ClientCurrentTime(ViewBag.TimeZoneOffset);
            DateTime clientTimeFrom = clientCurrentTime.AddDays(-1);

            //var utcTimeFrom = TimeZoneHelper.ClientToUtc(clientTimeFrom, ViewBag.TimeZoneOffset);
            //var utcTimeTo = TimeZoneHelper.ClientToUtc(clientCurrentTime, ViewBag.TimeZoneOffset);
            /* END ADJUST TIMES */

            var dbBusinessList = AdapterDb.Database.GetBusinessLineList(User);
            var businessList = Mapper.Map<List<Business>>(dbBusinessList).ToList();

            Compare model =
                new Compare
                {
                    TimeEntryType = TimeEntryType.Days,
                    TimeInDays = 1,
                    //UtcTimeFrom = utcTimeFrom,
                    //UtcTimeTo = utcTimeTo,
                    ClientTimeFrom = clientTimeFrom,
                    ClientTimeTo = clientCurrentTime,
                    BusinessList = businessList
                };

            return View(model);
        }

        [TimeZoneActionFilter]
        [HttpPost]
        public ActionResult Compare(Compare model)
        {
            /* ACTIONS */
            if (
                // Action unknown?
                (model.ActionIsAdd || !model.ActionIsRemove || !model.ActionIsToggle) &&
                // Entity unknown?
                (model.ActionItemNumber.HasValue || model.ActionBusinessId.HasValue || model.ActionLineId.HasValue || model.ActionEmployeeId.HasValue) &&
                // Too many compare items?
                !(model.ActionIsAdd && model.CompareItems != null && model.CompareItems.Count >= 3))
            {
                ModelState.Clear();

                if (model.ActionItemNumber.HasValue && !model.ActionBusinessId.HasValue && !model.ActionLineId.HasValue && !model.ActionEmployeeId.HasValue)
                {
                    if (model.ActionIsRemove)
                    {
                        // REMOVE ITEM
                        var removeItem = model.CompareItems.First(i => i.ItemNumber == model.ActionItemNumber);
                        model.CompareItems.Remove(removeItem);

                        var itemNumber = 0;
                        model.CompareItems.ForEach(ci => ci.ItemNumber = itemNumber++);
                    }
                }
                else if (model.ActionBusinessId.HasValue)
                {
                    if (model.ActionIsAdd)
                    {
                        // ADD BUSINESS
                        var dbAllBusinessList = AdapterDb.Database.GetBusinessList(User);
                        var dbAddedBusiness = dbAllBusinessList.First(b => b.Id == model.ActionBusinessId.Value);
                        var addedBusiness = Mapper.Map<Business>(dbAddedBusiness);

                        if (model.CompareItems == null)
                            model.CompareItems = new List<CompareItem>();

                        model.CompareItems.Add(new CompareItem
                        {
                            ItemNumber = model.CompareItems.Count,
                            SelectedBusiness = addedBusiness
                        });
                    }
                }
                else if (model.ActionLineId.HasValue)
                {
                    var compareItem = model.CompareItems.First(i => i.ItemNumber == model.ActionItemNumber);

                    if (model.ActionIsToggle)
                    {
                        if (compareItem.SelectedLineList == null)
                            compareItem.SelectedLineList = new List<Line>();

                        if (compareItem.SelectedLineList.FirstOrDefault(l => l.Id == model.ActionLineId.Value) == null)
                        {
                            // ADD LINE
                            var dbFilteredLinesList = AdapterDb.Database.GetLineList(User, compareItem.SelectedBusiness.Id);
                            var dbAddedLine = dbFilteredLinesList.First(l => l.Id == model.ActionLineId.Value);
                            var addedLine = Mapper.Map<Line>(dbAddedLine);
                            compareItem.SelectedLineList.Add(addedLine);
                        }
                        else
                        {
                            // REMOVE LINE
                            var removeLine = compareItem.SelectedLineList.First(i => i.Id == model.ActionLineId.Value);
                            compareItem.SelectedLineList.Remove(removeLine);
                        }
                    }
                }
                else if (model.ActionEmployeeId.HasValue)
                {
                    var compareItem = model.CompareItems.First(i => i.ItemNumber == model.ActionItemNumber);

                    if (model.ActionIsToggle)
                    {
                        //if (compareItem.SelectedEmployeeList == null)
                        //    compareItem.SelectedEmployeeList = new List<Employee>();

                        if (compareItem.SelectedEmployee != null && compareItem.SelectedEmployee.Id == model.ActionEmployeeId.Value)
                        {
                            // UNSET EMPLOYEE
                            //var removeEmployee = compareItem.EmployeeList.First(i => i.Id == model.ActionEmployeeId.Value);
                            compareItem.SelectedEmployee = null;
                        }
                        else
                        {
                            // SET EMPLOYEE
                            var dbFilteredEmployeesList = AdapterDb.Database.GetEmployeeList(User, compareItem.SelectedLineList.Select(l => l.Id).ToList());
                            var dbAddedEmployee = dbFilteredEmployeesList.First(l => l.Id == model.ActionEmployeeId.Value);
                            var selectedEmployee = Mapper.Map<Employee>(dbAddedEmployee);
                            compareItem.SelectedEmployee = selectedEmployee;
                        }
                    }
                }
            }

            /* AGGREGATED DATA */
            DateTime clientTimeFrom;
            DateTime clientTimeTo;
            //DateTime utcTimeFrom;
            //DateTime utcTimeTo;
            if (model.TimeEntryType == TimeEntryType.Days)
            {
                clientTimeTo = TimeZoneHelper.ClientCurrentTime(ViewBag.TimeZoneOffset);

                if (model.TimeInDays > 0)
                    clientTimeFrom = clientTimeTo.AddDays(-model.TimeInDays);
                else
                    clientTimeFrom = clientTimeTo.Date;

                //utcTimeFrom = TimeZoneHelper.ClientToUtc(clientTimeFrom, ViewBag.TimeZoneOffset);
                //utcTimeTo = TimeZoneHelper.ClientToUtc(clientTimeTo, ViewBag.TimeZoneOffset);
            }
            else
            {
                clientTimeFrom = model.ClientTimeFrom;
                clientTimeTo = model.ClientTimeTo;

                //utcTimeFrom = TimeZoneHelper.ClientToUtc(clientTimeFrom, ViewBag.TimeZoneOffset);
                //utcTimeTo = TimeZoneHelper.ClientToUtc(clientTimeTo, ViewBag.TimeZoneOffset);
            }

            if (model.CompareItems != null && model.CompareItems.Count > 0)
            {
                foreach (var item in model.CompareItems)
                {
                    var lineIds = item.SelectedLineList == null ? null : item.SelectedLineList.Select(ln => ln.Id).ToList();
                    var employeeId = item.SelectedEmployee == null ? null : (int?)item.SelectedEmployee.Id;

                    item.AggregatedData =
                        AdapterDb.Database.GetAggregatedData(
                            businessId: item.SelectedBusiness.Id,
                            lineIds: lineIds,
                            employeeId: employeeId,
                            timeFrom: clientTimeFrom,
                            timeTo: clientTimeTo);
                }
            }

            // Refurbish model
            model.ActionIsAdd = false;
            model.ActionIsRemove = false;
            model.ActionIsToggle = false;
            model.ActionItemNumber = null;
            model.ActionBusinessId = null;
            model.ActionLineId = null;
            model.ActionEmployeeId = null;

            // ...business combo...
            var dbBusinessList = AdapterDb.Database.GetBusinessList(User);
            model.BusinessList = Mapper.Map<List<Business>>(dbBusinessList).ToList();

            // ...line combos...
            if (model.CompareItems != null && model.CompareItems.Count > 0)
            {
                foreach (var item in model.CompareItems)
                {
                    var dbLineList = AdapterDb.Database.GetLineList(User, item.SelectedBusiness.Id);
                    item.LineList = Mapper.Map<List<Line>>(dbLineList).ToList();

                    if (item.SelectedLineList != null && item.SelectedLineList.Count > 0)
                    {
                        // ...employee combos...
                        var lineIds = item.SelectedLineList.Select(l => l.Id).ToList();
                        var dbEmployeeList = AdapterDb.Database.GetEmployeeList(User, lineIds);
                        item.EmployeeList = Mapper.Map<List<Employee>>(dbEmployeeList).ToList();
                    }
                }
            }

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Sharp Media Group";

            return View();
        }

        [TimeZoneActionFilter]
        [Authorize(Roles = "Admin")]
        public ActionResult GetDiagnostics()
        {
            var model =
                new Diagnostics
                {
                    From = DateTime.Now.Date.AddDays(-1),
                    To = DateTime.Now.Date,
                    CacheOnly = true
                };

            return View("Diagnostics", model);
        }

        [HttpPost]
        [TimeZoneActionFilter]
        [Authorize(Roles = "Admin")]
        public ActionResult GetDiagnostics(Diagnostics diag)
        {
            var businessLineList = AdapterDb.Database.GetBusinessLineList(User);

            var businessIds = businessLineList.Select(b => b.Id).ToArray();

            var dbItems =
                AdapterDb.Database.GetDataItems(
                    businessIds: businessIds,
                    timeFrom: diag.From,
                    timeTo: diag.To);

            var businessDict = businessLineList.ToDictionary(b => b.Id, b => b.Name);
            var lineDict = businessLineList.SelectMany(b => b.Line).ToDictionary(l => l.Id, l => l.Name);
            var aggregatedData = (from i in dbItems
                                  group i by new { date = i.Entered.Value.Date, businessId = i.BusinessId, lineId = i.LineId } into g
                                  select new AdapterDb.AggregatedData
                                  {
                                      Date = g.Key.date,
                                      BusinessId = g.Key.businessId,
                                      BusinessName = businessDict[g.Key.businessId],
                                      LineId = g.Key.lineId,
                                      LineName = lineDict[g.Key.lineId],
                                      MinDate = g.Min(_ => _.Entered),
                                      MaxDate = g.Max(_ => _.Entered),
                                      AverageWaitTime = (int)(g.Count() > 0 ? g.Average(_ => _.WaitTimeSec) : 0),
                                      AverageServiceTime = (int)(g.Count() > 0 ? g.Average(_ => _.ServiceTimeSec) : 0),
                                      CustomersWaitingCount = g.Count(_ => _.Entered.HasValue && !_.Called.HasValue),
                                      CustomersBeingServicedCount = g.Count(_ => _.Called.HasValue && !_.Serviced.HasValue),
                                      CustomersServicedCount = g.Count(_ => _.Serviced.HasValue),
                                      CustomersCount = g.Count()
                                  });

            var sortedData = aggregatedData.OrderBy(ad => ad.Date)
                                           .ThenBy(ad => ad.BusinessName)
                                           .ThenBy(ad => ad.LineName);

            diag.Data = sortedData.ToArray();
            diag.BusinessList = businessDict.Select(i => i.Value).ToArray();
            diag.LineList = businessDict.Select(i => i.Value).ToArray();

            return View("Diagnostics", diag);
        }
    }
}
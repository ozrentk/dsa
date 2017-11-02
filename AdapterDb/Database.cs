using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;

namespace AdapterDb
{
    public class Database
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        public Database()
        {
        }

        public static Overview GetOverview()
        {
            var stats = new Overview();

            using (var db = new AdapterDbEntities())
            {
                Overview item = db.Database.SqlQuery<Overview>("GetOverview").FirstOrDefault();

                return item;
            }
        }

        public static List<ConfigItem> GetConfigItems()
        {
            using (var db = new AdapterDbEntities())
            {
                var items = from i in db.ConfigItem
                            select i;

                return items.ToList();
            }
        }

        public static bool SaveConfigItems(List<ConfigItem> configItems)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    // Update configuration items
                    foreach (var dbItem in db.ConfigItem)
                    {
                        var match = configItems.FirstOrDefault(i => i.Name.Equals(dbItem.Name));
                        if (match != null)
                        {
                            dbItem.Value = match.Value.Trim();
                        }
                    }

                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("SaveConfigItems error", ex);
                return false;
            }
        }

        public static bool DoCleanup(DateTime lastValidTime)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    SqlParameter lastValidTimeParam = new SqlParameter("@lastValidTime", lastValidTime);

                    log.DebugFormat("Deleting old invalid items from database...");
                    var resultset = db.Database.SqlQuery<int>("EXEC CleanupOldInvalidItems @lastValidTime", lastValidTimeParam).ToList();
                    log.DebugFormat("{0} old invalid items deleted!", resultset.First());
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("DoCleanup error", ex);
                return false;
            }
        }

        public static User GetUser(int id)
        {
            using (var db = new AdapterDbEntities())
            {
                var item = db.User.Find(id);
                return item;
            }
        }

        public static List<User> GetUsers()
        {
            using (var db = new AdapterDbEntities())
            {
                var items = from i in db.User//.Include(u => u.Roles)
                            select i;

                return items.ToList();
            }
        }

        public static bool RemoveUser(int id)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var user = db.User.Find(id);

                    foreach (var role in user.Roles.ToList())
                        user.Roles.Remove(role);

                    var membership = db.BusinessMember.Where(bm => bm.UserId == id);
                    db.BusinessMember.RemoveRange(membership);

                    db.Set<User>().Remove(user);

                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("RemoveUser error", ex);
                return false;
            }
        }

        public static List<ActivityRestriction> GetActivityRestrictions()
        {
            using (var db = new AdapterDbEntities())
            {
                var restrictions = db.Database.SqlQuery<ActivityRestriction>("SELECT [UserName],[Controller],[Action],[Method] FROM SEC.[AllowedActivities]");
                return restrictions.ToList();
            }
        }

        public static string GetActivityRestrictionsHash()
        {
            using (var db = new AdapterDbEntities())
            {
                var hash = db.Database.SqlQuery<string>("EXEC SEC.GetAllowedActivitiesChecksum");
                return hash.Single();
            }
        }

        public static int? GetLastLineQueueId(int lineId)
        {
            using (var db = new AdapterDbEntities())
            {
                var item =
                    db.DataItem
                        .Where(di => di.LineId == lineId)
                        .OrderByDescending(di => di.QueueId)
                        .FirstOrDefault();

                if (item == null)
                    return null;
                else
                    return item.Id;
            }
        }

        public static TPoco Find<TEntity, TPoco>(int id, Func<TEntity, TPoco> map) where TEntity : class
        {
            using (var db = new AdapterDbEntities())
            {
                var dbObj = db.Set<TEntity>().Find(id);
                var obj = map(dbObj);
                return obj;
            }
        }

        public static TPoco Last<TEntity, TKey, TPoco>(Expression<Func<TEntity, TKey>> expr, Func<TEntity, TPoco> map) where TEntity : class
        {
            using (var db = new AdapterDbEntities())
            {
                var dbObj = db.Set<TEntity>().OrderByDescending(expr).FirstOrDefault();
                if (dbObj == null)
                    return default(TPoco);

                var obj = map(dbObj);
                return obj;
            }
        }

        public static List<TPoco> GetAll<TEntity, TPoco>(Func<IEnumerable<TEntity>, IEnumerable<TPoco>> map) where TEntity : class
        {
            using (var db = new AdapterDbEntities())
            {
                var dbList = db.Set<TEntity>().ToList();
                var list = map(dbList).ToList();
                return list;
            }
        }

        #region Signup ticket
        public static List<UserSignupTicket> GetSignupTicketList()
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                return db.UserSignupTicket.Include("Business").ToList();
            }
        }

        public static bool CheckSignupTicketNumber(Guid guid)
        {
            using (var db = new AdapterDbEntities())
            {
                var item = db.UserSignupTicket.FirstOrDefault(t => t.Guid == guid);
                return item != null;
            }
        }

        public static object GetSignupTicket(int ticketId)
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                var item = db.UserSignupTicket.Include("Business").First(b => b.Id == ticketId);
                return item;
            }
        }

        public static bool SaveSignupTicket(UserSignupTicket signupTicket)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    signupTicket.Created = signupTicket.Created ?? DateTime.UtcNow;
                    db.UserSignupTicket.AddOrUpdate(signupTicket);

                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("SaveSignupTicket error", ex);
                return false;
            }
        }

        public static bool DeleteSignupTicket(int ticketId)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var ticket = db.UserSignupTicket.Find(ticketId);
                    foreach (var biz in ticket.Business)
                    {
                        biz.TicketId = null;
                        biz.UserSignupTicket = null;
                    }
                    db.UserSignupTicket.Remove(ticket);

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("DeleteSignupTicket error", ex);
                return false;
            }
        }

        public static bool AddBusinessToSignupTicket(int ticketId, List<int> selectedBusinesses)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var ticket = db.UserSignupTicket.Find(ticketId);
                    ticket.Business.Clear();
                    foreach (int bizId in selectedBusinesses)
                    {
                        var biz = db.Business.Find(bizId);
                        ticket.Business.Add(biz);
                    }

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("DeleteSignupTicket error", ex);
                return false;
            }
        }

        #endregion Signup ticket

        #region Business
        public static List<Business> GetBusinessList(IPrincipal user)
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                if (user == null || user.IsInRole("Admin"))
                {
                    var items = db.Business.ToList();

                    return items;
                }
                else
                {
                    var dbUser = db.User.Include("BusinessMember").Where(u => u.UserName == user.Identity.Name).First();
                    var ids = dbUser.BusinessMember.Select(bm => bm.BusinessId);

                    var items = db.Business.Where(b => ids.Contains(b.Id)).ToList();
                    return items;
                }
            }

        }

        public static List<Business> GetBusinessLineList(IPrincipal user)
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                if (user == null || user.IsInRole("Admin"))
                {
                    var items = db.Business.Include("Line").OrderBy(b => b.Name).ToList();

                    return items;
                }
                else
                {
                    var dbUser = db.User.Include("BusinessMember").Where(u => u.UserName == user.Identity.Name).First();
                    var ids = dbUser.BusinessMember.Select(bm => bm.BusinessId);

                    var items = db.Business.Include("Line").Where(b => ids.Contains(b.Id)).OrderBy(b => b.Name).ToList();

                    return items;
                }
            }

        }

        public static Business GetBusiness(int businessId)
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                var item = db.Business.Include("Line").First(b => b.Id == businessId);
                return item;
            }
        }

        public static List<int> GetBusinessIds(string userName)
        {
            using (var db = new AdapterDbEntities())
            {
                var item = db.User.Where(u => u.UserName == userName).First();
                var ids = item.BusinessMember.Select(bm => bm.BusinessId);

                return ids.ToList();
            }
        }

        public static List<Business> GetBusinessList(string userName)
        {
            using (var db = new AdapterDbEntities())
            {
                var user = db.User.Where(u => u.UserName == userName).First();
                var ids = user.BusinessMember.Select(bm => bm.BusinessId);

                var items = db.Business.Include("Line").Where(b => ids.Contains(b.Id));

                return items.ToList();
            }
        }

        public static bool HasMultipleBusiness(string userName)
        {
            using (var db = new AdapterDbEntities())
            {
                var item = db.User.Where(u => u.UserName == userName).First();
                return item.BusinessMember.Count > 1;
            }
        }

        public static bool SaveBusiness(Business dbBusiness)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    dbBusiness.Created = dbBusiness.Created ?? DateTime.UtcNow;
                    db.Business.AddOrUpdate(dbBusiness);

                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("SaveBusiness error", ex);
                return false;
            }
        }

        public static bool DeleteBusiness(int businessId)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var biz = db.Business.Find(businessId);
                    db.Line.RemoveRange(biz.Line);
                    db.Business.Remove(biz);

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("DeleteBusiness error", ex);
                return false;
            }
        }

        public static bool AssignTicketBusinesses(int userId, Guid guidTicketNumber)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var ticket = db.UserSignupTicket.First(t => t.Guid == guidTicketNumber);
                    var businessIdList = ticket.Business.Select(b => b.Id);

                    var user = db.User.Find(userId);
                    foreach (var bizId in businessIdList)
                    {
                        db.BusinessMember.AddOrUpdate(
                            new BusinessMember
                            {
                                BusinessId = bizId,
                                UserId = user.Id
                            });
                    }

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("AssignBusinesses error", ex);
                return false;
            }
        }

        public static bool AssignBusinessToUser(int userId, List<int> businessIdList)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var user = db.User.Find(userId);

                    var usersMembership = db.BusinessMember.Where(bm => bm.UserId == userId);
                    db.BusinessMember.RemoveRange(usersMembership);
                    foreach (var bizId in businessIdList)
                    {
                        db.BusinessMember.Add(
                            new BusinessMember
                            {
                                BusinessId = bizId,
                                UserId = user.Id
                            });
                    }

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("AssignBusinesses error", ex);
                return false;
            }
        }

        #endregion Business

        #region Line
        public static List<Line> GetLineList(IPrincipal user, int? businessId = null)
        {

            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                if (user == null || user.IsInRole("Admin"))
                {
                    var items = (from i in db.Line
                                 where !businessId.HasValue || (i.BusinessId == businessId.Value)
                                 select i).ToList();

                    return items;
                }
                else
                {
                    var dbUser = db.User.Include("BusinessMember").Where(u => u.UserName == user.Identity.Name).First();
                    var allowedBizIds = dbUser.BusinessMember.Select(bm => bm.BusinessId);

                    var items = (from i in db.Line
                                 where allowedBizIds.Contains(i.BusinessId) && (!businessId.HasValue || (i.BusinessId == businessId.Value))
                                 select i).ToList();
                    return items;
                }
            }
        }

        public static int GetLineCount(int? businessId = null)
        {

            using (var db = new AdapterDbEntities())
            {
                var items = from i in db.Line
                            where !businessId.HasValue || (i.BusinessId == businessId.Value)
                            select i;

                return items.Count();
            }
        }


        public static Line GetLine(int lineId)
        {
            using (var db = new AdapterDbEntities())
            {
                var item = db.Line.Include("Business").First(l => l.Id == lineId);
                return item;
            }
        }

        public static bool SaveLine(Line dbLine)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    dbLine.Created = dbLine.Created ?? DateTime.UtcNow;
                    db.Line.AddOrUpdate(dbLine);

                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("SaveLine error", ex);
                return false;
            }
        }

        public static bool DeleteLine(int lineId)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var ln = db.Line.Find(lineId);
                    db.Line.Remove(ln);

                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("DeleteLine error", ex);
                return false;
            }
        }
        #endregion Line

        #region Employees
        public static List<Employee> GetEmployeeList(IPrincipal user, List<int> lineIds)
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                if (user == null || user.IsInRole("Admin"))
                {
                    var items = (from i in db.EmployeeLine
                                 where lineIds.Count == 0 || lineIds.Contains(i.LineId)
                                 orderby i.Employee.Name
                                 select i.Employee).Distinct().ToList();

                    return items;
                }
                else
                {
                    var dbUser = db.User.Include("BusinessMember").Where(u => u.UserName == user.Identity.Name).First();
                    var allowedBizIds = dbUser.BusinessMember.Select(bm => bm.BusinessId);
                    var allowedLnIds = db.Line.Where(l => allowedBizIds.Contains(l.BusinessId)).Select(l => l.Id).ToList();
                    var filteredLnIds = allowedLnIds.Where(l => lineIds.Contains(l)).ToList();

                    var employees = db.EmployeeLine
                                      .Where(el => filteredLnIds.Contains(el.LineId))
                                      .Select(el => el.Employee)
                                      .OrderBy(e => e.Name)
                                      .ToList();

                    return employees;
                }
            }

        }

        public static List<Employee> GetEmployeeList(int businessId)
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                    var items = (from emp in db.Employee
                                 where emp.BusinessId == businessId
                                 orderby emp.Name
                                 select emp).ToList();

                    return items;
            }
        }

        public static List<Employee> GetCalledEmployees(IPrincipal user, int? businessId, DateTime timeFrom, DateTime timeTo)
        {
            using (var db = new AdapterDbEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;

                var businessIds = new List<int>();
                if (businessId.HasValue && user.IsInRole("Admin"))
                {
                    businessIds.Add(businessId.Value);
                }
                else if (businessId.HasValue && !user.IsInRole("Admin"))
                {
                    var dbUser = db.User.Include("BusinessMember").Where(u => u.UserName == user.Identity.Name).First();
                    var allowedBizIds = dbUser.BusinessMember.Select(bm => bm.BusinessId);
                    if (!allowedBizIds.Contains(businessId.Value))
                        return new List<Employee>();

                    businessIds.Add(businessId.Value);
                }
                else if (!businessId.HasValue && /*user == null || */user.IsInRole("Admin"))
                {
                    var allIds = db.Business.Select(b => b.Id).ToList();
                    businessIds.AddRange(allIds);
                }
                else //if (!businessId.HasValue && !user.IsInRole("Admin"))
                {
                    var dbUser = db.User.Include("BusinessMember").Where(u => u.UserName == user.Identity.Name).First();
                    var allowedBizIds = dbUser.BusinessMember.Select(bm => bm.BusinessId);
                    businessIds.AddRange(allowedBizIds);
                }

                // PARAMETERS
                var timeFromParam = new SqlParameter("@timeFrom", timeFrom);
                var timeToParam = new SqlParameter("@timeTo", timeTo);

                DataTable dtBusinesses = new DataTable();
                dtBusinesses.Columns.Add("Value", typeof(int));
                foreach (var id in businessIds)
                {
                    dtBusinesses.Rows.Add(id);
                }
                var bizIdentsParam = new SqlParameter("@businessIds", SqlDbType.Structured);
                bizIdentsParam.Value = dtBusinesses;
                bizIdentsParam.TypeName = "dbo.Integers";

                var employees = db.Database.SqlQuery<Employee>(
                    $"EXEC GetCalledEmployees @timeFrom, @timeTo, @businessIds",
                    timeFromParam, timeToParam, bizIdentsParam).ToList();

                return employees;
            }
        }

        public static List<EmployeeTimes> GetEmployeeTimes(int[] employeeIds, DateTime timeFrom, DateTime timeTo)
        {
            List<EmployeeTimes> itemList;
            using (var db = new AdapterDbEntities())
            {
                // PARAMETERS
                var timeFromParam = new SqlParameter("@timeFrom", timeFrom);
                var timeToParam = new SqlParameter("@timeTo", timeTo);

                DataTable dtEmployees = new DataTable();
                dtEmployees.Columns.Add("Value", typeof(int));
                foreach (var id in employeeIds)
                {
                    dtEmployees.Rows.Add(id);
                }
                var empIdentsParam = new SqlParameter("@employeeIds", SqlDbType.Structured);
                empIdentsParam.Value = dtEmployees;
                empIdentsParam.TypeName = "dbo.Integers";

                itemList = db.Database.SqlQuery<EmployeeTimes>(
                    $"EXEC GetEmployeeTimes @timeFrom, @timeTo, @employeeIds",
                    timeFromParam, timeToParam, empIdentsParam).ToList();
            }

            return itemList;
        }

        #endregion Employees

        #region Data items

        public static List<DataItem> GetDataItems(int businessId, int? lineId, int? employeeId, DateTime timeFrom, DateTime timeTo)
        {
            using (var db = new AdapterDbEntities())
            {
                var allItems = (from i in db.DataItem.Include("ServicingEmployee").Include("CallingEmployee").Include("Business").Include("Line")
                                where
                                    i.BusinessId == businessId &&
                                    (!lineId.HasValue || lineId.HasValue && lineId.Value == i.LineId) &&
                                    (!employeeId.HasValue || employeeId.HasValue && employeeId.Value == i.CalledById) &&
                                    i.Entered >= timeFrom &&
                                    i.Entered <= timeTo
                                orderby i.QueueId
                                select i).ToList();

                return allItems;
            }
        }

        public static List<DataItem> GetDataItems(int[] businessIds, DateTime timeFrom, DateTime timeTo)
        {
            using (var db = new AdapterDbEntities())
            {
                var allItems = (from i in db.DataItem
                                where
                                    businessIds.Contains(i.BusinessId) &&
                                    i.Entered >= timeFrom &&
                                    i.Entered <= timeTo
                                orderby i.QueueId
                                select i).ToList();

                return allItems;
            }
        }

        #endregion Data Items

        #region Aggregated data items

        public static List<AggregatedData> GetAggregatedDataForSingleBusiness(IPrincipal user, int businessId, DateTime timeFrom, DateTime timeTo)
        {
            var res = GetAggregatedDataForMultipleBusinesses(user, new int[] { businessId }, timeFrom, timeTo);
            return res;
        }

        public static List<AggregatedData> GetAggregatedDataForMultipleBusinesses(IPrincipal user, int[] businessIds, DateTime timeFrom, DateTime timeTo)
        {
            using (var db = new AdapterDbEntities())
            {
                var allBusinessList = GetBusinessList(user);
                var businessList = allBusinessList.Where(b => businessIds.Contains(b.Id)).ToList();
                var lineList = GetLineList(user);

                var list = new List<AggregatedData>();

                var lnItems = GetAggregatedData(db, "GetDataAggregatedByLine", timeFrom, timeTo, businessList, lineList);
                list.AddRange(lnItems);

                var bizItems = GetAggregatedData(db, "GetDataAggregatedByBusiness", timeFrom, timeTo, businessList, lineList);
                list.AddRange(bizItems);

                var summaryItem = GetAggregatedData(db, "GetAggregatedData", timeFrom, timeTo, businessList, lineList).Single();
                list.Add(summaryItem);

                return list;
            }
        }

        private static List<AggregatedData> GetAggregatedData(AdapterDbEntities db, string aggregationProcedureName, DateTime timeFrom, DateTime timeTo, List<Business> businessList, List<Line> lineList)
        {
            var timeFromParam = new SqlParameter("@timeFrom", timeFrom);
            var timeToParam = new SqlParameter("@timeTo", timeTo);
            DataTable dtBusinesses = new DataTable();
            dtBusinesses.Columns.Add("Value", typeof(int));
            foreach (var business in businessList)
            {
                dtBusinesses.Rows.Add(business.Id);
            }
            var bizIdentsParam = new SqlParameter("@businessIds", SqlDbType.Structured);
            bizIdentsParam.Value = dtBusinesses;
            bizIdentsParam.TypeName = "dbo.Integers";

            DataTable dtLines = new DataTable();
            dtLines.Columns.Add("Value", typeof(int));
            foreach (var line in lineList)
            {
                dtLines.Rows.Add(line.Id);
            }
            var lnIdentsParam = new SqlParameter("@lineIds", SqlDbType.Structured);
            lnIdentsParam.Value = dtLines;
            lnIdentsParam.TypeName = "dbo.Integers";

            return db.Database.SqlQuery<AggregatedData>(
                    $"EXEC {aggregationProcedureName} @timeFrom, @timeTo, @businessIds, @lineIds",
                   timeFromParam, timeToParam, bizIdentsParam, lnIdentsParam).ToList();
        }

        public static AggregatedData GetAggregatedData(DateTime timeFrom, DateTime timeTo, int businessId, List<int> lineIds, int? employeeId)
        {
            using (var db = new AdapterDbEntities())
            {
                var timeFromParam = new SqlParameter("@timeFrom", timeFrom);
                var timeToParam = new SqlParameter("@timeTo", timeTo);

                DataTable dtBiz = new DataTable();
                dtBiz.Columns.Add("Value", typeof(int));
                dtBiz.Rows.Add(businessId);
                var bizIdentsParam = new SqlParameter("@businessIdentifiers", SqlDbType.Structured);
                bizIdentsParam.Value = dtBiz;
                bizIdentsParam.TypeName = "dbo.Integers";

                DataTable dtLine = new DataTable();
                dtLine.Columns.Add("Value", typeof(int));
                if (lineIds != null)
                {
                    foreach (var id in lineIds)
                    {
                        dtLine.Rows.Add(id);
                    }
                }
                var lnIdentsParam = new SqlParameter("@lineIdentifiers", SqlDbType.Structured);
                lnIdentsParam.Value = dtLine;
                lnIdentsParam.TypeName = "dbo.Integers";

                List<AggregatedData> items;
                if (employeeId != null)
                {
                    var empIdParam = new SqlParameter("@employeeId", employeeId == null ? (object)DBNull.Value : employeeId);
                    items = db.Database.SqlQuery<AggregatedData>(
                        "EXEC GetAggregatedEmployeeData @timeFrom, @timeTo, @businessIdentifiers, @lineIdentifiers, @employeeId",
                       timeFromParam, timeToParam, bizIdentsParam, lnIdentsParam, empIdParam).ToList();
                }
                else
                {
                    items = db.Database.SqlQuery<AggregatedData>(
                        "EXEC GetAggregatedData @timeFrom, @timeTo, @businessIdentifiers, @lineIdentifiers",
                       timeFromParam, timeToParam, bizIdentsParam, lnIdentsParam).ToList();
                }

                return items.Single();
            }
        }

        #endregion Aggregated data items

        //public static bool CreateUser(string userName, List<int?> selectedRoleIds)
        //{
        //    try
        //    {
        //        using (var db = new AdapterDbEntities())
        //        {
        //            User dbUser = new User { UserName = userName };
        //            var selectedRoles = db.Roles.Where(p => selectedRoleIds.Contains(p.Id)).ToList();
        //            foreach (var sr in selectedRoles)
        //            {
        //                dbUser.Roles.Add(sr);
        //            }
        //            db.User.Add(dbUser);
        //            db.SaveChanges();

        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("CreateUser error", ex);
        //        return false;
        //    }
        //}

        //public static bool ModifyUser(User user, List<int?> selectedRoleIds)
        //{
        //    try
        //    {
        //        using (var db = new AdapterDbEntities())
        //        {
        //            var dbUser = db.User.Find(user.Id);
        //            dbUser.UserName = user.UserName;
        //            dbUser.Roles.Clear();

        //            var selectedRoles = db.Roles.Where(p => selectedRoleIds.Contains(p.Id)).ToList();
        //            foreach (var sr in selectedRoles)
        //            {
        //                dbUser.Roles.Add(sr);
        //            }

        //            db.SaveChanges();

        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("ModifyUser error", ex);
        //        return false;
        //    }
        //}

        public static bool UpdateUserActivity(int userId, bool isActive)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var dbUser = db.User.Find(userId);

                    if (dbUser.IsActive && !isActive ||
                        !dbUser.IsActive && isActive)
                    {
                        dbUser.IsActive = isActive;
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("UpdateUserActivity error", ex);
                return false;
            }
        }



        public static bool CreateRole(string roleName, List<int> selectedPermissionIds)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    Roles dbRole = new Roles { Name = roleName };
                    var selectedPermissions = db.Permission.Where(p => selectedPermissionIds.Contains(p.Id)).ToList();
                    foreach (var sp in selectedPermissions)
                    {
                        dbRole.Permission.Add(sp);
                    }
                    db.Roles.Add(dbRole);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("CreateRole error", ex);
                return false;
            }
        }

        public static bool ModifyRole(AdapterDb.Roles role, List<int> selectedPermissionIds)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var dbRole = db.Roles.Find(role.Id);
                    dbRole.Name = role.Name;
                    dbRole.Permission.Clear();

                    var selectedPermissions = db.Permission.Where(p => selectedPermissionIds.Contains(p.Id)).ToList();
                    foreach (var sp in selectedPermissions)
                    {
                        dbRole.Permission.Add(sp);
                    }

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("ModifyRole error", ex);
                return false;
            }
        }

        public static bool DeleteRole(int roleId)
        {
            try
            {
                using (var db = new AdapterDbEntities())
                {
                    var role = db.Roles.Find(roleId);
                    role.Permission.Clear();
                    db.Roles.Remove(role);

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("DeleteRole error", ex);
                return false;
            }
        }

        public static bool DeleteUser(int value)
        {
            throw new NotImplementedException();
        }

        #region Old code

        //public static void DoBackup(Func<int, int, List<DataItem>> itemSource)
        //{
        //    Batch newBatch;
        //    using (var db = new AdapterDbEntities())
        //    {
        //        log.Debug("creating a new batch...");
        //        newBatch = db.Batch.Add(new Batch() { Created = DateTime.UtcNow });
        //        db.SaveChanges();
        //        log.Debug("batch created");
        //    }

        //    var tblSyncItems = new DataTable();
        //    tblSyncItems.Columns.Add("BusinessId", typeof(int));
        //    tblSyncItems.Columns.Add("LineId", typeof(int));
        //    tblSyncItems.Columns.Add("ServiceId", typeof(int));
        //    tblSyncItems.Columns.Add("AnalyticId", typeof(int));
        //    tblSyncItems.Columns.Add("QueueId", typeof(int));
        //    tblSyncItems.Columns.Add("Name", typeof(string));
        //    tblSyncItems.Columns.Add("Verification", typeof(string));
        //    tblSyncItems.Columns.Add("Serviced", typeof(DateTime));
        //    tblSyncItems.Columns.Add("ServicedBy", typeof(string));
        //    tblSyncItems.Columns.Add("Called", typeof(DateTime));
        //    tblSyncItems.Columns.Add("CalledBy", typeof(string));
        //    tblSyncItems.Columns.Add("Entered", typeof(DateTime));
        //    tblSyncItems.Columns.Add("WaitTimeSec", typeof(int));
        //    tblSyncItems.Columns.Add("ServiceTimeSec", typeof(int));

        //    log.Debug("retrieving line/business configuration...");
        //    var dbLines = GetLineList(null);

        //    foreach (var dbLn in dbLines)
        //    {
        //        log.DebugFormat("retrieving items for line {0}, business {1}...", dbLn.Id, dbLn.BusinessId);
        //        var items = itemSource(dbLn.Id, dbLn.BusinessId);
        //        log.DebugFormat("{0} rows retrieved", items.Count);
        //        items.ForEach(i =>
        //        {
        //            tblSyncItems.Rows.Add(new object[14] {
        //                i.BusinessId,
        //                i.LineId,
        //                i.ServiceId,
        //                i.AnalyticId,
        //                i.QueueId,
        //                i.Name,
        //                i.Verification,
        //                i.Serviced,
        //                i.ServicedById,
        //                i.Called,
        //                i.CalledById,
        //                i.Entered,
        //                i.WaitTimeSec,
        //                i.ServiceTimeSec
        //            });
        //        });
        //    }

        //    using (var db = new AdapterDbEntities())
        //    {
        //        SqlParameter batchIdParam = new SqlParameter("@batchId", newBatch.Id);
        //        SqlParameter syncItemsParam = new SqlParameter("@items", tblSyncItems);
        //        syncItemsParam.TypeName = "DataItem";
        //        syncItemsParam.SqlDbType = SqlDbType.Structured;

        //        log.DebugFormat("merging {0} items with database...", tblSyncItems.Rows.Count);
        //        db.Database.ExecuteSqlCommand("EXEC MergeItems @batchId, @items", batchIdParam, syncItemsParam);
        //        log.DebugFormat("{0} items merged", tblSyncItems.Rows.Count);
        //    }

        //    using (var db = new AdapterDbEntities())
        //    {
        //        log.Debug("updating batch...");
        //        db.AttachAndModify(newBatch).Set(b => b.Collected, DateTime.UtcNow);
        //        db.SaveChanges();
        //        log.Debug("batch updated");
        //    }
        //}

        //public static void MergeDataItems(List<DataItem> dataItems)
        //{
        //    Batch newBatch;
        //    using (var db = new AdapterDbEntities())
        //    {
        //        log.Debug("creating a new batch...");
        //        newBatch = db.Batch.Add(new Batch() { Created = DateTime.UtcNow });
        //        db.SaveChanges();
        //        log.Debug("batch created");
        //    }

        //    var tblSyncItems = new DataTable();
        //    tblSyncItems.Columns.Add("BusinessId", typeof(int));
        //    tblSyncItems.Columns.Add("LineId", typeof(int));
        //    tblSyncItems.Columns.Add("ServiceId", typeof(int));
        //    tblSyncItems.Columns.Add("AnalyticId", typeof(int));
        //    tblSyncItems.Columns.Add("QueueId", typeof(int));
        //    tblSyncItems.Columns.Add("Name", typeof(string));
        //    tblSyncItems.Columns.Add("Verification", typeof(string));
        //    tblSyncItems.Columns.Add("Serviced", typeof(DateTime));
        //    tblSyncItems.Columns.Add("ServicedBy", typeof(string));
        //    tblSyncItems.Columns.Add("Called", typeof(DateTime));
        //    tblSyncItems.Columns.Add("CalledBy", typeof(string));
        //    tblSyncItems.Columns.Add("Entered", typeof(DateTime));
        //    tblSyncItems.Columns.Add("WaitTimeSec", typeof(int));
        //    tblSyncItems.Columns.Add("ServiceTimeSec", typeof(int));

        //    log.Debug("retrieving line/business configuration...");

        //    dataItems.ForEach(i =>
        //    {
        //        tblSyncItems.Rows.Add(new object[14] {
        //            i.BusinessId,
        //            i.LineId,
        //            i.ServiceId,
        //            i.AnalyticId,
        //            i.QueueId,
        //            i.Name,
        //            i.Verification,
        //            i.Serviced,
        //            i.ServicedById,
        //            i.Called,
        //            i.CalledById,
        //            i.Entered,
        //            i.WaitTimeSec,
        //            i.ServiceTimeSec
        //        });
        //    });

        //    using (var db = new AdapterDbEntities())
        //    {
        //        SqlParameter batchIdParam = new SqlParameter("@batchId", newBatch.Id);
        //        SqlParameter syncItemsParam = new SqlParameter("@items", tblSyncItems);
        //        syncItemsParam.TypeName = "DataItem";
        //        syncItemsParam.SqlDbType = SqlDbType.Structured;

        //        log.DebugFormat("merging {0} items with database...", tblSyncItems.Rows.Count);
        //        db.Database.ExecuteSqlCommand("EXEC MergeItems @batchId, @items", batchIdParam, syncItemsParam);
        //        log.DebugFormat("{0} items merged", tblSyncItems.Rows.Count);
        //    }

        //    using (var db = new AdapterDbEntities())
        //    {
        //        log.Debug("updating batch...");
        //        db.AttachAndModify(newBatch).Set(b => b.Collected, DateTime.UtcNow);
        //        db.SaveChanges();
        //        log.Debug("batch updated");
        //    }
        //}

        //public static List<DataItem> GetDataItems(int[] businessIds, Dictionary<int, int?> oldestQueueIds, DateTime timeFrom, DateTime timeTo)
        //{
        //    using (var db = new AdapterDbEntities())
        //    {
        //        var allItems = from i in db.DataItem
        //                       where
        //                           businessIds.Contains(i.BusinessId) &&
        //                           i.Entered >= timeFrom &&
        //                           i.Entered <= timeTo
        //                       select i;

        //        var allItemsList = allItems.ToList();

        //        var items = (from i in allItemsList
        //                     join qid in oldestQueueIds on i.BusinessId equals qid.Key
        //                     where qid.Value.HasValue && i.QueueId < qid.Value
        //                     select i).ToList();

        //        return items;
        //    }
        //}


        //public static List<TPoco> GetUsersWithActiveObjects<TPoco>(Func<IEnumerable<User>, IEnumerable<Roles>, IEnumerable<Business>, IEnumerable<TPoco>> map)
        //{
        //    using (var db = new AdapterDbEntities())
        //    {
        //        var dbUsers = db.Set<User>().Include(u => u.Roles).Include(u => u.BusinessMember).ToList();
        //        var dbRoles = db.Set<Roles>().ToList();
        //        var dbBusinessList = db.Set<Business>().ToList();
        //        var list = map(dbUsers, dbRoles, dbBusinessList).ToList();
        //        return list;
        //    }
        //}

        #endregion Old code
    }
}

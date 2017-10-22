using AdapterDb;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter
{
    public class ActivityRestrictions
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        private static ActivityRestrictions _instance;
        private static object singletonSyncLock = new object();
        private static object itemAccessSyncLock = new object();

        private HashSet<ActivityRestriction> items;
        public HashSet<ActivityRestriction> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
            }
        }

        protected ActivityRestrictions()
        {
            items = new HashSet<ActivityRestriction>();
        }

        public static ActivityRestrictions Instance
        {
            get
            {
                // 'Double checked locking' pattern - avoid locking each time the method is invoked
                // See: http://www.dofactory.com/net/singleton-design-pattern
                if (_instance == null)
                {
                    lock (singletonSyncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ActivityRestrictions();
                        }
                    }
                }

                return _instance;
            }
        }

        private int? lastHash = null;

        public void EnsureRestrictions()
        {
            var currentHash = Database.GetActivityRestrictionsHash();
            if (!lastHash.HasValue || lastHash != currentHash)
            {
                lock (itemAccessSyncLock)
                {
                    lastHash = currentHash;
                    var restrictions = Database.GetActivityRestrictions();
                    Items = new HashSet<ActivityRestriction>(restrictions);
                }
            }
        }

        public bool IsAllowed(string name, string controller, string action, string method)
        {
            EnsureRestrictions();

            var allowed = Instance.Items.Contains(new ActivityRestriction
            {
                UserName = name,
                Controller = controller,
                Action = action,
                Method = method
            });

            return allowed;
        }
    }

    public class ActivityRestrictionConfig
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        public static void Configure()
        {
            ConfigureActivityRestrictions();
        }

        private static void ConfigureActivityRestrictions()
        {
            log.DebugFormat("configuring activity restrictions");

            ActivityRestrictions.Instance.EnsureRestrictions();
        }
    }
}
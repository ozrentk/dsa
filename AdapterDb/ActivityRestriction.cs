using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterDb
{
    public class ActivityRestriction : IEquatable<ActivityRestriction>
    {
        public string UserName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Method { get; set; }

        public bool Equals(ActivityRestriction other)
        {
            if (other == null)
            {
                return false;
            }

            return
                StringComparer.Ordinal.Equals(UserName, other.UserName) &&
                StringComparer.Ordinal.Equals(Controller, other.Controller) &&
                StringComparer.Ordinal.Equals(Action, other.Action) &&
                StringComparer.Ordinal.Equals(Method, other.Method);
        }

        public override bool Equals(object obj)
        {
            return obj is ActivityRestriction && Equals((ActivityRestriction)obj);
        }

        public override int GetHashCode()
        {
            // See: https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            unchecked
            {
                int hash = 7;
                hash = hash * 11 + (UserName ?? String.Empty).GetHashCode();
                hash = hash * 11 + (Controller ?? String.Empty).GetHashCode();
                hash = hash * 11 + (Action ?? String.Empty).GetHashCode();
                hash = hash * 11 + (Method ?? String.Empty).GetHashCode();
                return hash;
            }
        }
    }
}

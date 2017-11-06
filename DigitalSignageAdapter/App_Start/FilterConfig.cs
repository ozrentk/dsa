using DigitalSignageAdapter.Filters;
using System.Web;
using System.Web.Mvc;

namespace DigitalSignageAdapter
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            //filters.Add(new EFExceptionHandlerAttribute());
            filters.Add(new RestrictedAccessAttribute());
        }
    }
}

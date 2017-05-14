using DigitalSignageAdapter.Models.TimeZone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalSignageAdapter.Controllers
{
    public class TimeZoneController : Controller
    {
        public ActionResult RefreshOffset(string returnUrl)
        {
            var model = new RefreshOffset()
            {
                ReturnUrl = returnUrl
            };

            return View("RefreshOffset", model);
        }

        [HttpPost]
        public ActionResult RefreshOffset(RefreshOffset model)
        {
            Session["timeZoneOffset"] = model.Offset;
            return Content("");
        }
    }
}
using DigitalSignageAdapter.Filters;
using System.Web.Mvc;

namespace DigitalSignageAdapter.Controllers
{
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult Http401()
        {
            ViewBag.Title = "Forbidden";
            ViewBag.Message = "You are not allowed to use this part of the application.";

            Response.StatusCode = 401;

            return View("Common");
        }

        [AllowAnonymous]
        public ActionResult Http403()
        {
            ViewBag.Title = "Forbidden";
            ViewBag.Message = "You are not allowed to use this part of the application.";

            Response.StatusCode = 403;

            return View("Common");
        }

        [AllowAnonymous]
        public ActionResult Http404()
        {
            ViewBag.Title = "Not found";
            ViewBag.Message = "It appears that this page doesn't exist. Sorry!";

            Response.StatusCode = 404;

            return View("Common");
        }

        [AllowAnonymous]
        public ActionResult Http500()
        {
            ViewBag.Title = "Unknown error";
            ViewBag.Message = "This shouldn't have happened. If you have time, feel free to inform the system administrator of this error.";

            Response.StatusCode = 500;

            return View("Common");
        }
    }
}
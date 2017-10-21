using System.Web.Mvc;

namespace DigitalSignageAdapter.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Forbidden";
            ViewBag.Message = "You are not allowed to use this part of the application.";

            return View();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSA.WEB.Controllers.Authenticate
{
    public class AuthenticateController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }
    }
}
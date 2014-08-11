using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BuggerOff.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            return View();
        }

        public ActionResult About()
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            return View();
        }

        public ActionResult Contact()
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            return View();
        }
    }
}
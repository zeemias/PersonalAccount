using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PersonalAccount.Controllers
{
    public class HomeController : Controller
    {
    	[AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

		[Authorize]
        public ActionResult Account()
        {

            return View();
        }
    }
}
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BackEnd.Controllers
{
    public class BackEndOrderController : Controller
    {
        // GET: BackEndOrder
        public ActionResult OrderQuery()
        {
            if (Session[CDictionary.SK_Logined_Member] == null ||
              (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("OrderQuery", "_BackEndLayoutPage");
        }
      
    }
}
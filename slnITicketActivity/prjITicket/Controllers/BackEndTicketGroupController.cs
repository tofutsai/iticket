using prjITicket.Models;
using prjITicket.ViewModel;
using prjITicket.ViewModel.BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace prjITicket.Controllers
{
    public class BackEndTicketGroupController : Controller
    {
        TicketSysEntities ticket = new TicketSysEntities();

        // GET: BackEndTicketGroup
        public ActionResult TicketGroupMaintain()
        {
            if (Session[CDictionary.SK_Logined_Member] == null ||
               (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("TicketGroupMaintain", "_BackEndLayoutPage");
        }

        public ActionResult TicketGroupDetail(int id)
        {
            if (Session[CDictionary.SK_Logined_Member] == null ||
              (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }

            CBackEndTicketGroupDetail TicketGroupDetail = new CBackEndTicketGroupDetail();
                       
            TicketGroupDetail.List = (from t in ticket.TicketGroups
                                      from td in ticket.TicketGroupDetail
                                      from a in ticket.Activity
                                      from s in ticket.Seller
                                      where t.TicketGroupId.Equals(td.TicketGroupId)
                                      && td.ActivityId.Equals(a.ActivityID)
                                     && a.SellerID.Equals(s.SellerID)
                                     && t.TicketGroupId == id
                                      select new CBackEndTicketGroupList
                                      {
                                          TicketGroups = t,
                                          Seller = s
                                      }).FirstOrDefault();

            TicketGroupDetail.activity = (from tg in ticket.TicketGroups
                                          from tgd in ticket.TicketGroupDetail
                                          from a in ticket.Activity

                                          where tg.TicketGroupId.Equals(tgd.TicketGroupId)
                                          && tgd.ActivityId.Equals(a.ActivityID)
                                          && tg.TicketGroupId == id

                                          select a).ToList();

            return View(TicketGroupDetail);
        }
    }
}
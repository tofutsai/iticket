using PagedList;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace prjITicket.Controllers
{
    
    public class SellerCenterController : Controller,IDisposable
    {
        TicketSysEntities db = new TicketSysEntities();
        // GET: SellerCenter

        //商家中心
        public ActionResult ManagementCenter()
        {

            int memberid = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            var sellerid=db.Seller.Where(x => x.MemberId == memberid).FirstOrDefault();

            if (sellerid.fPass != true)
            {
                return RedirectToAction("ActivityList", "Activity");
            }

            //var list = db.Activity.Where(s => s.SellerID == sellerid.SellerID);
            return View(/*list*/);
        }

        public ActionResult GetQueryResult(string txtQuery="",int page=1)
        {
            //-------
            int pagesize = 6;
            int pagecurrent = page < 1 ? 1 : page;
            //------------------
            ViewBag.Keyword = txtQuery;
            int memberid = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            var sellerid = db.Seller.Where(x => x.MemberId == memberid).FirstOrDefault();
            if (sellerid.fPass != true)
            {
                return RedirectToAction("ActivityList", "Activity");
            }
            IQueryable<Activity> list = null;
            if (txtQuery != "")
            {
               list = db.Activity.Where(s => s.SellerID == sellerid.SellerID&&s.ActivityName.Contains(txtQuery));
            }
            else
            {
                list= db.Activity.Where(s => s.SellerID == sellerid.SellerID);
            }
            var pagedlist = list.ToList().ToPagedList(pagecurrent, pagesize);

            return PartialView(pagedlist);
        }

        //套票中心

        public ActionResult PackageCenterM()
        {
            

            return View();
        }

        public ActionResult PackageCenter(string txtQuery = "", int page = 1)
        {
            //-------
            int pagesize = 5;
            int pagecurrent = page < 1 ? 1 : page;
            //------------------

            int memberid = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            var sellerid = db.Seller.Where(x => x.MemberId == memberid).FirstOrDefault();
            IQueryable<TicketGroups> total = null;
            if (sellerid.fPass != true)
            {
                return RedirectToAction("ActivityList", "Activity");
            }
            if (txtQuery != "")
            {
                total = db.TicketGroups.Where(x => x.TicketGroupDetail.FirstOrDefault().Activity.SellerID == sellerid.SellerID&&
                x.TicketGroupName.Contains(txtQuery));
            }
            else
            {
                total = db.TicketGroups.Where(x => x.TicketGroupDetail.FirstOrDefault().Activity.SellerID == sellerid.SellerID);
            }
            //var total=db.TicketGroups.Where(x => x.TicketGroupDetail.FirstOrDefault().Activity.SellerID == sellerid.SellerID);
            var pagedlist = total.ToList().ToPagedList(pagecurrent, pagesize);
            //return View(total);
            return PartialView(pagedlist);
        }
       

        public JsonResult PackageCenterDetailShow(int ticketGroupId)
        {
            
            var detail = db.TicketGroupDetail.Where(x => x.TicketGroupId == ticketGroupId).ToList();
            List<object> data = new List<object>();
            for (int i = 0; i < detail.Count; i++)
            {
                data.Add(new
                {
                    TicketGroupName = detail[i].TicketGroups.TicketGroupName,
                    ActivityName = detail[i].Activity.ActivityName,
                    Picture = detail[i].Activity.Picture,
                    UnitsInStock = detail[i].Activity.Tickets.FirstOrDefault().UnitsInStock


                });
            }
            
            return Json(data);
        }


























    }
}
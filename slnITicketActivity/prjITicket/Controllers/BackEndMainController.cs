using prjITicket.Models;
using prjITicket.ViewModel.BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace prjITicket.Controllers
{
    public class BackEndMainController : Controller
    {

        // GET: BackEndMain
        public ActionResult BackEndIndex()
        {
            TicketSysEntities ticket = new TicketSysEntities();

            if (Session[CDictionary.SK_Logined_Member] == null ||
              (Session[CDictionary.SK_Logined_Member] as Member).MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }

            CBackEndMain cBackEndMain = new CBackEndMain();
            DateTime t = DateTime.Today; //今天日期
            DateTime yesterday = DateTime.Today.AddDays(-1); //昨天日期
            DateTime Beforeyesterday = DateTime.Today.AddDays(-2); //前天日期

            //昨天訂單筆數
            int y = ticket.Orders.Count(o => yesterday <= o.OrderDate && o.OrderDate < t);
            cBackEndMain.YesterdayOrderCount = y;

            //前天訂單筆數
            int by = ticket.Orders.Count(o => Beforeyesterday <= o.OrderDate && o.OrderDate < yesterday);
            cBackEndMain.BeforeYesterdayOrderCount = by;

            //昨天訂單總金額
            //先找昨天的訂單
            List<Order_Detail> YesterdayOrderdetail = ticket.Order_Detail.Where(o => yesterday <= o.Orders.OrderDate && o.Orders.OrderDate < t).ToList();
            if (YesterdayOrderdetail.Count > 0)
            {
                //如果有訂單再做計算
                int yTotalPrice = YesterdayOrderdetail.Sum(o => o.Quantity * o.Tickets.Price);
                cBackEndMain.YesterDayTotalPrice = yTotalPrice;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.YesterDayTotalPrice = 0;
            }

            //前天訂單總金額
            List<Order_Detail> BeforeYesterdayOrderdetail = ticket.Order_Detail.Where(o => Beforeyesterday <= o.Orders.OrderDate && o.Orders.OrderDate < yesterday).ToList();
            if (BeforeYesterdayOrderdetail.Count > 0)
            {
                int byTotalPrice = BeforeYesterdayOrderdetail.Sum(o => o.Quantity * o.Tickets.Price);
                cBackEndMain.BeforeYesterDayTotalPrice = byTotalPrice;
            }
            else
            {
                cBackEndMain.BeforeYesterDayTotalPrice = 0;
            }

            //前一天新增的會員數
            int Member_Customer = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => yesterday <= m.fRegister_time && m.fRegister_time < t);
            cBackEndMain.Member_Customer = Member_Customer;

            //前一天新增的商家數
            int Member_Seller = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => yesterday <= m.fRegister_time && m.fRegister_time < t);
            cBackEndMain.Member_Seller = Member_Seller;

            //尚未審核的商家數
            int Seller_NoPass = ticket.Seller.Count(s => s.fPass == null);
            cBackEndMain.Seller_NoPass = Seller_NoPass;

            //尚未審核的活動數
            int Activity_NoPass = ticket.Activity.Count(a => a.ActivityStatusID == 0);
            cBackEndMain.Activity_NoPass = Activity_NoPass;

            //尚未審核的套票數
            int TicketGroup_NoPass = ticket.TicketGroups.Count(tg => tg.Status == false);
            cBackEndMain.GroupTicket_NoPass = TicketGroup_NoPass;

            //找出前三個熱門討論版(依留言數加總排序)
            var Take3_Forum = (from a in ticket.Article
                               join r in ticket.Reply
                               on a.ArticleID equals r.ArticleID
                               //join ac in ticket.ArticleCategories
                               //on a.ArticleCategoryID equals ac.ArticleCategoryID
                               group r by a into g
                               let count = g.Count()
                               orderby count descending
                               select new Take3_Forum
                               {
                                   CategoryName = g.Key.ArticleCategories.ArticleCategoryName,
                                   CommentNum = count
                               }).Take(3).ToList();
            cBackEndMain.Take3_Forum = Take3_Forum;

            //找出前三篇按讚數最多的文章
            var Take3_Article = (from a in ticket.Article
                                 join e in ticket.Article_Emotion
                                 on a.ArticleID equals e.ArticleId
                                 group e by a into g
                                 let count = g.Count()
                                 orderby count descending
                                 select new Take3_Article
                                 {
                                     CategoryName = g.Key.ArticleCategories.ArticleCategoryName,
                                     ArticleTitle = g.Key.ArticleTitle,
                                     ArticleLike = count,
                                     ArticleID = g.Key.ArticleID
                                 }).Take(3).ToList();
            cBackEndMain.Take3_Articles = Take3_Article;


            DateTime today = DateTime.Today;
            DateTime t_1 = today.AddDays(-1);
            DateTime t_2 = today.AddDays(-2);
            DateTime t_3 = today.AddDays(-3);
            DateTime t_4 = today.AddDays(-4);
            DateTime t_5 = today.AddDays(-5);
            DateTime t_6 = today.AddDays(-6);
            DateTime t_7 = today.AddDays(-7);
            DateTime t_8 = today.AddDays(-8);
            DateTime t_9 = today.AddDays(-9);
            DateTime t_10 = today.AddDays(-10);
            DateTime t_11 = today.AddDays(-11);
            DateTime t_12 = today.AddDays(-12);
            DateTime t_13 = today.AddDays(-13);
            DateTime t_14 = today.AddDays(-14);
            DateTime t_15 = today.AddDays(-15);

            //chart ordercount
            int OrderCount_1 = ticket.Orders.Count(o => today > o.OrderDate && o.OrderDate >= t_1);
            cBackEndMain.OrderCount_1 = OrderCount_1;

            int OrderCount_2 = ticket.Orders.Count(o => t_1 > o.OrderDate && o.OrderDate >= t_2);
            cBackEndMain.OrderCount_2 = OrderCount_2;

            int OrderCount_3 = ticket.Orders.Count(o => t_2 > o.OrderDate && o.OrderDate >= t_3);
            cBackEndMain.OrderCount_3 = OrderCount_3;

            int OrderCount_4 = ticket.Orders.Count(o => t_3 > o.OrderDate && o.OrderDate >= t_4);
            cBackEndMain.OrderCount_4 = OrderCount_4;

            int OrderCount_5 = ticket.Orders.Count(o => t_4 > o.OrderDate && o.OrderDate >= t_5);
            cBackEndMain.OrderCount_5 = OrderCount_5;

            int OrderCount_6 = ticket.Orders.Count(o => t_5 > o.OrderDate && o.OrderDate >= t_6);
            cBackEndMain.OrderCount_6 = OrderCount_6;

            int OrderCount_7 = ticket.Orders.Count(o => t_6 > o.OrderDate && o.OrderDate >= t_7);
            cBackEndMain.OrderCount_7 = OrderCount_7;

            int OrderCount_8 = ticket.Orders.Count(o => t_7 > o.OrderDate && o.OrderDate >= t_8);
            cBackEndMain.OrderCount_8 = OrderCount_8;

            int OrderCount_9 = ticket.Orders.Count(o => t_8 > o.OrderDate && o.OrderDate >= t_9);
            cBackEndMain.OrderCount_9 = OrderCount_9;

            int OrderCount_10 = ticket.Orders.Count(o => t_9 > o.OrderDate && o.OrderDate >= t_10);
            cBackEndMain.OrderCount_10 = OrderCount_10;

            int OrderCount_11 = ticket.Orders.Count(o => t_10 > o.OrderDate && o.OrderDate >= t_11);
            cBackEndMain.OrderCount_11 = OrderCount_11;

            int OrderCount_12 = ticket.Orders.Count(o => t_11 > o.OrderDate && o.OrderDate >= t_12);
            cBackEndMain.OrderCount_12 = OrderCount_12;

            int OrderCount_13 = ticket.Orders.Count(o => t_12 > o.OrderDate && o.OrderDate >= t_13);
            cBackEndMain.OrderCount_13 = OrderCount_13;

            int OrderCount_14 = ticket.Orders.Count(o => t_13 > o.OrderDate && o.OrderDate >= t_14);
            cBackEndMain.OrderCount_14 = OrderCount_14;

            int OrderCount_15 = ticket.Orders.Count(o => t_14 > o.OrderDate && o.OrderDate >= t_15);
            cBackEndMain.OrderCount_15 = OrderCount_15;

            //Chart orderprice
            List<Order_Detail> orderdetail_1 = ticket.Order_Detail.Where(o => today > o.Orders.OrderDate && o.Orders.OrderDate >= t_1).ToList();
            if (orderdetail_1.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_1 = orderdetail_1.Sum(o => (int)( o.Quantity * o.Tickets.Price *(1-o.Discount)));
                cBackEndMain.OrderPrice_1 = OrderPrice_1;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_1 = 0;
            }

            List<Order_Detail> orderdetail_2 = ticket.Order_Detail.Where(o => t_1 > o.Orders.OrderDate && o.Orders.OrderDate >= t_2).ToList();
            if (orderdetail_2.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_2 = orderdetail_2.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_2 = OrderPrice_2;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_2 = 0;
            }

            List<Order_Detail> orderdetail_3 = ticket.Order_Detail.Where(o => t_2 > o.Orders.OrderDate && o.Orders.OrderDate >= t_3).ToList();
            if (orderdetail_3.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_3 = orderdetail_3.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_3 = OrderPrice_3;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_3 = 0;
            }

            List<Order_Detail> orderdetail_4 = ticket.Order_Detail.Where(o => t_3 > o.Orders.OrderDate && o.Orders.OrderDate >= t_4).ToList();
            if (orderdetail_4.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_4 = orderdetail_4.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_4 = OrderPrice_4;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_4 = 0;
            }

            List<Order_Detail> orderdetail_5 = ticket.Order_Detail.Where(o => t_4 > o.Orders.OrderDate && o.Orders.OrderDate >= t_5).ToList();
            if (orderdetail_5.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_5 = orderdetail_5.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_5 = OrderPrice_5;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_5 = 0;
            }

            List<Order_Detail> orderdetail_6 = ticket.Order_Detail.Where(o => t_5 > o.Orders.OrderDate && o.Orders.OrderDate >= t_6).ToList();
            if (orderdetail_6.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_6 = orderdetail_6.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_6 = OrderPrice_6;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_6 = 0;
            }

            List<Order_Detail> orderdetail_7 = ticket.Order_Detail.Where(o => t_6 > o.Orders.OrderDate && o.Orders.OrderDate >= t_7).ToList();
            if (orderdetail_7.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_7 = orderdetail_7.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_7 = OrderPrice_7;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_7 = 0;
            }

            List<Order_Detail> orderdetail_8 = ticket.Order_Detail.Where(o => t_7 > o.Orders.OrderDate && o.Orders.OrderDate >= t_8).ToList();
            if (orderdetail_8.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_8 = orderdetail_8.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_8 = OrderPrice_8;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_8 = 0;
            }

            List<Order_Detail> orderdetail_9 = ticket.Order_Detail.Where(o => t_8 > o.Orders.OrderDate && o.Orders.OrderDate >= t_9).ToList();
            if (orderdetail_9.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_9 = orderdetail_9.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_9 = OrderPrice_9;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_9 = 0;
            }
            List<Order_Detail> orderdetail_10 = ticket.Order_Detail.Where(o => t_9 > o.Orders.OrderDate && o.Orders.OrderDate >= t_10).ToList();
            if (orderdetail_10.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_10 = orderdetail_10.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_10 = OrderPrice_10;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_10 = 0;
            }
            List<Order_Detail> orderdetail_11 = ticket.Order_Detail.Where(o => t_10 > o.Orders.OrderDate && o.Orders.OrderDate >= t_11).ToList();
            if (orderdetail_11.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_11 = orderdetail_11.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_11 = OrderPrice_11;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_11 = 0;
            }
            List<Order_Detail> orderdetail_12 = ticket.Order_Detail.Where(o => t_11 > o.Orders.OrderDate && o.Orders.OrderDate >= t_12).ToList();
            if (orderdetail_12.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_12 = orderdetail_12.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_12 = OrderPrice_12;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_12 = 0;
            }
            List<Order_Detail> orderdetail_13 = ticket.Order_Detail.Where(o => t_12 > o.Orders.OrderDate && o.Orders.OrderDate >= t_13).ToList();
            if (orderdetail_13.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_13 = orderdetail_13.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_13 = OrderPrice_13;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_13 = 0;
            }

            List<Order_Detail> orderdetail_14 = ticket.Order_Detail.Where(o => t_13 > o.Orders.OrderDate && o.Orders.OrderDate >= t_14).ToList();
            if (orderdetail_14.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_14 = orderdetail_14.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_14 = OrderPrice_14;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_14 = 0;
            }
            List<Order_Detail> orderdetail_15 = ticket.Order_Detail.Where(o => t_14 > o.Orders.OrderDate && o.Orders.OrderDate >= t_15).ToList();
            if (orderdetail_15.Count > 0)
            {
                //如果有訂單再做計算
                int OrderPrice_15 = orderdetail_15.Sum(o => (int)(o.Quantity * o.Tickets.Price * (1 - o.Discount)));
                cBackEndMain.OrderPrice_15 = OrderPrice_15;
            }
            else
            {
                //沒有訂單直接等於0
                cBackEndMain.OrderPrice_15 = 0;
            }

            //chart MemberCount
            int MemberCount_1 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => today > m.fRegister_time && m.fRegister_time >= t_1);
            cBackEndMain.MemberCount_1 = MemberCount_1;

            int MemberCount_2 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_1 > m.fRegister_time && m.fRegister_time >= t_2);
            cBackEndMain.MemberCount_2 = MemberCount_2;

            int MemberCount_3 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_2 > m.fRegister_time && m.fRegister_time >= t_3);
            cBackEndMain.MemberCount_3 = MemberCount_3;

            int MemberCount_4 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_3 > m.fRegister_time && m.fRegister_time >= t_4);
            cBackEndMain.MemberCount_4 = MemberCount_4;

            int MemberCount_5 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_4 > m.fRegister_time && m.fRegister_time >= t_5);
            cBackEndMain.MemberCount_5 = MemberCount_5;

            int MemberCount_6 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_5 > m.fRegister_time && m.fRegister_time >= t_6);
            cBackEndMain.MemberCount_6 = MemberCount_6;

            int MemberCount_7 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_6 > m.fRegister_time && m.fRegister_time >= t_7);
            cBackEndMain.MemberCount_7 = MemberCount_7;

            int MemberCount_8 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_7 > m.fRegister_time && m.fRegister_time >= t_8);
            cBackEndMain.MemberCount_8 = MemberCount_8;

            int MemberCount_9 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_8 > m.fRegister_time && m.fRegister_time >= t_9);
            cBackEndMain.MemberCount_9 = MemberCount_9;

            int MemberCount_10 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_9 > m.fRegister_time && m.fRegister_time >= t_10);
            cBackEndMain.MemberCount_10 = MemberCount_10;

            int MemberCount_11 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_10 > m.fRegister_time && m.fRegister_time >= t_11);
            cBackEndMain.MemberCount_11 = MemberCount_11;

            int MemberCount_12 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_11 > m.fRegister_time && m.fRegister_time >= t_12);
            cBackEndMain.MemberCount_12 = MemberCount_12;

            int MemberCount_13 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_12 > m.fRegister_time && m.fRegister_time >= t_13);
            cBackEndMain.MemberCount_13 = MemberCount_13;

            int MemberCount_14 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_13 > m.fRegister_time && m.fRegister_time >= t_14);
            cBackEndMain.MemberCount_14 = MemberCount_14;

            int MemberCount_15 = ticket.Member.Where(m => m.MemberRoleId == 2).Count(m => t_14 > m.fRegister_time && m.fRegister_time >= t_15);
            cBackEndMain.MemberCount_15 = MemberCount_15;

            //chart SellerCount
            int SellerCount_1 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => today > m.fRegister_time && m.fRegister_time >= t_1);
            cBackEndMain.SellerCount_1 = SellerCount_1;

            int SellerCount_2 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_1 > m.fRegister_time && m.fRegister_time >= t_2);
            cBackEndMain.SellerCount_2 = SellerCount_2;

            int SellerCount_3 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_2 > m.fRegister_time && m.fRegister_time >= t_3);
            cBackEndMain.SellerCount_3 = SellerCount_3;

            int SellerCount_4 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_3 > m.fRegister_time && m.fRegister_time >= t_4);
            cBackEndMain.SellerCount_4 = SellerCount_4;

            int SellerCount_5 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_4 > m.fRegister_time && m.fRegister_time >= t_5);
            cBackEndMain.SellerCount_5 = SellerCount_5;

            int SellerCount_6 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_5 > m.fRegister_time && m.fRegister_time >= t_6);
            cBackEndMain.SellerCount_6 = SellerCount_6;

            int SellerCount_7 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_6 > m.fRegister_time && m.fRegister_time >= t_7);
            cBackEndMain.SellerCount_7 = SellerCount_7;

            int SellerCount_8 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_7 > m.fRegister_time && m.fRegister_time >= t_8);
            cBackEndMain.SellerCount_8 = SellerCount_8;

            int SellerCount_9 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_8 > m.fRegister_time && m.fRegister_time >= t_9);
            cBackEndMain.SellerCount_9 = SellerCount_9;

            int SellerCount_10 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_9 > m.fRegister_time && m.fRegister_time >= t_10);
            cBackEndMain.SellerCount_10 = SellerCount_10;

            int SellerCount_11 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_10 > m.fRegister_time && m.fRegister_time >= t_11);
            cBackEndMain.SellerCount_11 = SellerCount_11;

            int SellerCount_12 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_11 > m.fRegister_time && m.fRegister_time >= t_12);
            cBackEndMain.SellerCount_12 = SellerCount_12;

            int SellerCount_13 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_12 > m.fRegister_time && m.fRegister_time >= t_13);
            cBackEndMain.SellerCount_13 = SellerCount_13;

            int SellerCount_14 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_13 > m.fRegister_time && m.fRegister_time >= t_14);
            cBackEndMain.SellerCount_14 = SellerCount_14;

            int SellerCount_15 = ticket.Member.Where(m => m.MemberRoleId == 3).Count(m => t_14 > m.fRegister_time && m.fRegister_time >= t_15);
            cBackEndMain.SellerCount_15 = SellerCount_15;


            return View(cBackEndMain);
        }
    }
}
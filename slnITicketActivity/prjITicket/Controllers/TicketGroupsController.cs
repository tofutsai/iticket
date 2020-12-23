using Newtonsoft.Json;
using PagedList;
using prjITicket.Models;
using prjITicket.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace prjITicket.Controllers
{
    public class TicketGroupsController : Controller
    {
        TicketSysEntities db = new TicketSysEntities();
        
        // 首頁按下套票後呼叫的Controller
        public ActionResult TicketGroupsList()
        {
            List<TicketGroups> ticketgroups = db.TicketGroups.Where(t=>t.Status==true).ToList();
            //if (ticketgroups.Count() == 0) return View("ProductNotFound","_Layout");
            // todo取出套票中各個活動最便宜的票價，然後加總乘以套票的優惠折扣，其中最貴的套票的價錢          
            int max = (int)ticketgroups.Max(ticketGroup => Math.Round(ticketGroup.TicketGroupDetail.Select(
                tgd => { return tgd.Activity.Tickets.Count() == 0 ? 0 : tgd.Activity.Tickets.Min(t => t.Price); }).Sum() * (1 - ticketGroup.TicketGroupDiscount), 0));

            VMTicketGroupsList vm = new VMTicketGroupsList()
            {
                ticketGroups = ticketgroups,
                maxPriceOfAll = max
            };
            return View(vm);
        }

        // 查看套票詳細資訊的Controller
        public ActionResult TicketGroupsDetail(int ticketgroupId)
        {
            ViewBag.TicketGroupName = db.TicketGroups.Where(n => n.TicketGroupId == ticketgroupId).FirstOrDefault().TicketGroupName + "內含有以下這些票：";
            List<TicketGroupDetail> ticketgroupdetails = db.TicketGroupDetail.Where(tgd => tgd.TicketGroupId == ticketgroupId).ToList();
            List<TicketCategory> ticketcategories = db.TicketCategory.ToList();
            List<TicketTimes> tickettimes = db.TicketTimes.ToList();            
            VMTicketGroupsDetail vm = new VMTicketGroupsDetail()
            {
                ticketGroupDetails = ticketgroupdetails,
                ticketCategory = ticketcategories,
                ticketTimes = tickettimes,                
            };
            return View(vm);
        }

        //讓Ajax調用的方法，在TicketGroupsDetail的View中根據票種和場次實時更新票的價格
        public string getTicketPrice(int ticketCategoryId, int ticketTimeId)
        {
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketCategoryId == ticketCategoryId && t.TicketTimeId == ticketTimeId);
            if(ticket == null)
            {
                return "暫無提供";
            }
            else if(ticket.UnitsInStock == 0)
            {
                return "已售完";
            }
            else
            {
                return JsonConvert.SerializeObject(new { ticket.Price, ticket.Discount, ticket.UnitsInStock });
            }
        }

        //tobeadd讓Ajax調用的方法，在TicketGroupsDetail的View中，每次按下數量加號，根據票種和場次判斷是否還有票
        public string isTicketsStillInStock(int ticketCategoryId, int ticketTimeId, int trCount)
        {
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketCategoryId == ticketCategoryId && t.TicketTimeId == ticketTimeId);
            if(ticket == null)
            {
                return "暫無提供";
            }
            else if (++trCount > ticket.UnitsInStock)
            {
                return "已售完";
            }
            else
            {
                return "還有票";
            }
        }

        //在TicketGroupsList頁面中Ajax調用這個方法取得活動分頁
        public ActionResult GetTicketGroupsPages(int page = 1, string orderMode = "pricedown", int minPrice = 0, int maxPrice = int.MaxValue, int priceFilter = 0)
        {
            int currentPage = page < 1 ? 1 : page;
            int pageSize = 4;
            //ViewBag紀錄此Action方法，才能執行正確的換頁
            ViewBag.ActionName = "GetTicketGroupsPages";
            //ViewBag紀錄此OrderMode，因為換頁時也會呼叫此方法
            ViewBag.OrderMode = orderMode;            
            List<TicketGroups> ticketgroups = db.TicketGroups.Where(t=>t.Status==true).ToList();
            if (priceFilter == 1)
            {
                ViewBag.PriceFilter = 1;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;

                ticketgroups = ticketgroups.Where(g => Math.Round(g.TicketGroupDetail.Select(
                        tgd => { return tgd.Activity.Tickets.Count() == 0 ? 0 : tgd.Activity.Tickets.Min(a => a.Price); }).Sum() * (1 - g.TicketGroupDiscount), 0) >= minPrice &&
                        Math.Round(g.TicketGroupDetail.Select(
                        tgd => { return tgd.Activity.Tickets.Count() == 0 ? 0 : tgd.Activity.Tickets.Min(a => a.Price); }).Sum() * (1 - g.TicketGroupDiscount), 0) <= maxPrice).ToList();
            }
            ticketgroups = OrderTicketGroupsByOrderMode(ticketgroups, orderMode);
            VMTicketGroupsList vm = new VMTicketGroupsList()
            {
                ticketGroups = ticketgroups
            };
            IPagedList<TicketGroups> pagedList = vm.ticketGroups.ToPagedList(currentPage, pageSize);
            return PartialView(pagedList);
        }

        //在TicketGroupsList頁面中Ajax調用這個方法，處理搜尋的request，並response取得的活動分頁
        public ActionResult GetTicketGroupsPagesByKeyword(string keyword, int page = 1, string orderMode= "pricedown", int minPrice = 0, int maxPrice = int.MaxValue, int priceFilter = 0)
        {
            int currentPage = page < 1 ? 1 : page;
            int pageSize = 4;
            //ViewBag紀錄此Action方法，才能執行正確的換頁
            ViewBag.ActionName = "GetTicketGroupsPagesByKeyword";
            //ViewBag紀錄搜尋的關鍵字，因為換頁時也會呼叫此方法
            ViewBag.Keyword = keyword;
            //ViewBag紀錄此OrderMode，因為換頁時也會呼叫此方法
            ViewBag.OrderMode = orderMode;
            List<Activity> activities = db.Activity.Where(a => a.ActivityStatusID == 1 && a.ActivityName.ToLower().Contains(keyword.ToLower())).ToList();
            List<int> activityIds = activities.Select(a => a.ActivityID).ToList();
            List<TicketGroups> groups = db.TicketGroups.Where(t=>t.Status==true).AsEnumerable().Where(tg =>
            {
                foreach (TicketGroupDetail tgd in tg.TicketGroupDetail)
                {
                    if (activityIds.Contains(tgd.ActivityId))
                    {
                        return true;
                    }
                }
                return false;
            }).ToList();
            if (priceFilter == 1)
            {
                ViewBag.PriceFilter = 1;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                
                groups = groups.Where(g => Math.Round(g.TicketGroupDetail.Select(
                        tgd => { return tgd.Activity.Tickets.Count() == 0 ? 0 : tgd.Activity.Tickets.Min(a => a.Price); }).Sum() * (1 - g.TicketGroupDiscount), 0) >= minPrice &&
                        Math.Round(g.TicketGroupDetail.Select(
                        tgd => { return tgd.Activity.Tickets.Count() == 0 ? 0 : tgd.Activity.Tickets.Min(a => a.Price); }).Sum() * (1 - g.TicketGroupDiscount), 0) <= maxPrice).ToList();
            }
            groups = OrderTicketGroupsByOrderMode(groups, orderMode);
            IPagedList<TicketGroups> pagedList = groups.ToPagedList(currentPage, pageSize);
            return PartialView("GetTicketGroupsPages",pagedList);
        }

        //根據OrderMode排序套票的方法
        public List<TicketGroups> OrderTicketGroupsByOrderMode(List<TicketGroups> ticketgroups, string ordermode)
        {
            switch (ordermode)
            {
                case "priceup":
                    return ticketgroups.OrderBy(t => Math.Round(t.TicketGroupDetail.Select(
                        tgd => { return tgd.Activity.Tickets.Count() == 0 ? 0 : tgd.Activity.Tickets.Min(a => a.Price); }).Sum() * (1 - t.TicketGroupDiscount), 0)).ToList();
                case "pricedown":
                    return ticketgroups.OrderByDescending(t => Math.Round(t.TicketGroupDetail.Select(
                        tgd => { return tgd.Activity.Tickets.Count() == 0 ? 0 : tgd.Activity.Tickets.Min(a => a.Price); }).Sum() * (1 - t.TicketGroupDiscount), 0)).ToList();
                default:
                    goto case "pricedown"; 
            }
        }
    }
}

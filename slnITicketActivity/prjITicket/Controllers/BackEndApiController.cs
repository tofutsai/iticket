using BackEnd.ViewModel;
using LinqKit;
using prjITicket.Models;
using prjITicket.ViewModel;
using prjITicket.ViewModel.BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BackEnd.Controllers
{
    public class formDataClass
    {
        private string statusID0;
        private string statusID1;
        private string statusID2;
        private string companyName;
        private string activityName;

        public string StatusID0 { get => statusID0; set => statusID0 = value; }
        public string StatusID1 { get => statusID1; set => statusID1 = value; }
        public string StatusID2 { get => statusID2; set => statusID2 = value; }
        public string CompanyName { get => companyName; set => companyName = value; }
        public string ActivityName { get => activityName; set => activityName = value; }
    }
    public class updateActivtyStatus
    {
        private int statusID;
        private int activityID;

        public int StatusID { get => statusID; set => statusID = value; }
        public int ActivityID { get => activityID; set => activityID = value; }
    }
    public class updateNoPassMessage
    {
        private int sellerID;
        private string message;

        public int SellerID { get => sellerID; set => sellerID = value; }
        public string Message { get => message; set => message = value; }
    }
    public class OrderQuery
    {
        private string orderStatus0;//未付款
        private string orderStatus1; //已付款
        private string orderGuid;//訂單編號
        private string username;//訂購人姓名
        private string useremail;//訂購人email
        private string startTime;
        private string endTime;

        public string OrderGuid { get => orderGuid; set => orderGuid = value; }
        public string OrderStatus0 { get => orderStatus0; set => orderStatus0 = value; }
        public string OrderStatus1 { get => orderStatus1; set => orderStatus1 = value; }
        public string Username { get => username; set => username = value; }
        public string UserEmail { get => useremail; set => useremail = value; }
        public string StartTime { get => startTime; set => startTime = value; }
        public string EndTime { get => endTime; set => endTime = value; }
    }
    public class OrderDetail
    {
        private string orderID;

        public string OrderID { get => orderID; set => orderID = value; }
    }
    public class TicketGroupList
    {
        private string ticketGroupName;//套票名稱
        private string companyName;//商家名稱
        private string ticketGroupStatus1;//套票審核狀態
        private string ticketGroupStatus2;//套票審核狀態

        public string TicketGroupName { get => ticketGroupName; set => ticketGroupName = value; }
        public string CompanyName { get => companyName; set => companyName = value; }
        public string TicketGroupStatus1 { get => ticketGroupStatus1; set => ticketGroupStatus1 = value; }
        public string TicketGroupStatus2 { get => ticketGroupStatus2; set => ticketGroupStatus2 = value; }
    }
    public class updateTicketGroupStatus
    {
        private bool status;
        private int ticketGroupID;

        public bool Status { get => status; set => status = value; }
        public int TicketGroupID { get => ticketGroupID; set => ticketGroupID = value; }
    }
    public class getTiketTimes
    {
        private int activityID;

        public int ActivityID { get => activityID; set => activityID = value; }
    }
    public class getOrdersCount
    {
        private DateTime yesterdayOrderTime;
        private DateTime beforeyesterdayOrderTime;

        public DateTime YesterdayOrderTime { get => yesterdayOrderTime; set => yesterdayOrderTime = value; }
        public DateTime BeforeyesterdayOrderTime { get => beforeyesterdayOrderTime; set => beforeyesterdayOrderTime = value; }
    }
    public class ArticleID
    {
        private int id;

        public int articleID { get => id; set => id = value; }
    }

    public class WebApiController : ApiController
    {

        TicketSysEntities ticket = new TicketSysEntities();

        //後臺活動查詢API
        [HttpPost]
        public List<CBackEndActivity> getActivity([FromBody]formDataClass formDataClass)
        {
            string StatusID0 = formDataClass.StatusID0;
            int AcStatusID0;
            int.TryParse(StatusID0, out AcStatusID0);

            string StatusID1 = formDataClass.StatusID1;
            int AcStatusID1;
            int.TryParse(StatusID1, out AcStatusID1);

            string StatusID2 = formDataClass.StatusID2;
            int AcStatusID2;
            int.TryParse(StatusID2, out AcStatusID2);

            string CompanyName = !string.IsNullOrEmpty(formDataClass.CompanyName) ? formDataClass.CompanyName.Trim() : null;
            string ActivityName = !string.IsNullOrEmpty(formDataClass.ActivityName) ? formDataClass.ActivityName.Trim() : null;

            IQueryable<CBackEndActivity> backEndActivities = null;


            Activity activity = new Activity();
            Seller seller = new Seller();

            var predicate_Activity = PredicateBuilder.New<Activity>(true);
            var predicate_seller = PredicateBuilder.New<Seller>(true);
            var predicate_Status = PredicateBuilder.New<ActivityStatus>(false);


            if (!string.IsNullOrEmpty(ActivityName))
                predicate_Activity = predicate_Activity.And(a => a.ActivityName.Contains(ActivityName));

            if (!string.IsNullOrEmpty(CompanyName))
                predicate_seller = predicate_seller.And(s => s.CompanyName.Contains(CompanyName));

            if (!string.IsNullOrEmpty(StatusID0))
                predicate_Status = predicate_Status.Or(st => st.ActivityStatusID.Equals(AcStatusID0));

            if (!string.IsNullOrEmpty(StatusID1))
                predicate_Status = predicate_Status.Or(st => st.ActivityStatusID.Equals(AcStatusID1));

            if (!string.IsNullOrEmpty(StatusID2))
                predicate_Status = predicate_Status.Or(st => st.ActivityStatusID.Equals(AcStatusID2));

            backEndActivities = from a in ticket.Activity.Where(predicate_Activity)

                                join s in ticket.Seller.Where(predicate_seller)
                                on a.SellerID equals s.SellerID

                                join status in ticket.ActivityStatus.Where(predicate_Status)
                                on a.ActivityStatusID equals status.ActivityStatusID

                                select new CBackEndActivity
                                {
                                    ActivityEntity = a,
                                    Seller = s,
                                    ActivityStatus = status,
                                };

            return backEndActivities.ToList();
        }

        //後台活動審核成功API
        [HttpPut]
        public string updateActivtyStatus([FromBody]updateActivtyStatus updateActivtyStatus)
        {
            try
            {
                int ActivityID = updateActivtyStatus.ActivityID;
                int StatusID = updateActivtyStatus.StatusID;

                var Activity = ticket.Activity.Where(ac => ac.ActivityID == ActivityID).FirstOrDefault();
                Activity.ActivityStatusID = StatusID;
                int num = ticket.SaveChanges();
                if (num == 1)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return (ex.Message);

            }

        }

        //後臺活動審核未通過，傳送通知給商家API
        [HttpPost]
        public void updateNoPassMessage([FromBody]updateNoPassMessage updateNoPassMessage)
        {

            int SellerID = updateNoPassMessage.SellerID;
            string Message = updateNoPassMessage.Message;

            ShortMessage shortMessage = new ShortMessage();

            var memberid = (
                from seller in ticket.Seller
                from m in ticket.Member
                where seller.MemberId == m.MemberID
                && seller.SellerID == SellerID
                select m.MemberID).FirstOrDefault();


            shortMessage.MemberID = memberid;
            shortMessage.MessageContent = Message;

            ticket.ShortMessage.Add(shortMessage);
            ticket.SaveChanges();
        }


        //後台訂單查詢API
        [HttpPost]
        public List<CBackEndOrders> getOrders([FromBody]OrderQuery orderQuery)
        {
            string OrderStatus0 = orderQuery.OrderStatus0;
            bool OrderStatus0_bool;
            bool.TryParse(OrderStatus0, out OrderStatus0_bool);

            string OrderStatus1 = orderQuery.OrderStatus1;
            bool OrderStatus1_bool;
            bool.TryParse(OrderStatus1, out OrderStatus1_bool);

            string StartTime = orderQuery.StartTime;
            DateTime startTime;
            DateTime.TryParse(StartTime, out startTime);

            string EndTime = orderQuery.EndTime;
            DateTime endTime;
            DateTime.TryParse(EndTime, out endTime);

            string OrderGuid = !string.IsNullOrEmpty(orderQuery.OrderGuid) ? orderQuery.OrderGuid.Trim() : null;
            string UserName = !string.IsNullOrEmpty(orderQuery.Username) ? orderQuery.Username.Trim() : null;
            string UserEmail = !string.IsNullOrEmpty(orderQuery.UserEmail) ? orderQuery.UserEmail.Trim() : null;
          
            IQueryable<CBackEndOrders> BackEndOrders = null;

            var predicate_OrdersStatus = PredicateBuilder.New<Orders>(false);
            var predicate_OrderGuid = PredicateBuilder.New<Orders>(true);
            var predicate_UserName = PredicateBuilder.New<Orders>(true);
            var predicate_UserEmail = PredicateBuilder.New<Orders>(true);
            var Predicate_OrderDate = PredicateBuilder.New<Orders>(true);


            if (!string.IsNullOrEmpty(OrderGuid))
                predicate_OrderGuid = predicate_OrderGuid.And(o => o.OrderGuid.Equals(OrderGuid));

            if (!string.IsNullOrEmpty(UserName))
                predicate_UserName = predicate_UserName.And(o => o.Name.Contains(UserName));

            if (!string.IsNullOrEmpty(UserEmail))
                predicate_UserEmail = predicate_UserEmail.And(o => o.Email.Contains(UserEmail));

            if (!string.IsNullOrEmpty(OrderStatus0))
                predicate_OrdersStatus = predicate_OrdersStatus.Or(o => o.OrderStatus.Equals(OrderStatus0_bool));

            if (!string.IsNullOrEmpty(OrderStatus1))
                predicate_OrdersStatus = predicate_OrdersStatus.Or(o => o.OrderStatus.Equals(OrderStatus1_bool));

            if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(EndTime))
                Predicate_OrderDate = Predicate_OrderDate.
                    And(o => (o.OrderDate.Year >= startTime.Year && o.OrderDate.Month >= startTime.Month && o.OrderDate.Day >= startTime.Day)
                    && (o.OrderDate.Year <= endTime.Year && o.OrderDate.Month <= endTime.Month && o.OrderDate.Day <= endTime.Day
                    ));
            
            BackEndOrders = from o in ticket.Orders.Where(predicate_OrdersStatus).Where(predicate_OrderGuid).Where(predicate_UserName).Where(predicate_UserEmail).Where(Predicate_OrderDate)
                            select new CBackEndOrders
                            {
                                Orders = o,
                                OrderPrice =o.Order_Detail.Sum(od=>(int)((od.Quantity * od.Tickets.Price)*(1-od.Discount)))
                            };


            return BackEndOrders.ToList();
        }

        //後臺訂單明細API
        [HttpPost]
        public List<CBackEndOrderDetail> getOrderDetail([FromBody]OrderDetail orderDetail)
        {
            string OrderID = orderDetail.OrderID;
            int Orderid;
            int.TryParse(OrderID, out Orderid);
   
            IQueryable<CBackEndOrderDetail> BackEndOrders = null;

            Orders Orders = new Orders();
            Order_Detail OrderDetail = new Order_Detail();
            Districts Districts = new Districts();
            Cities Cities = new Cities();
            Tickets Tickets = new Tickets();
            Activity Activity = new Activity();
            TicketTimes TicketTimes = new TicketTimes();
            TicketCategory TicketCategory = new TicketCategory();

            var predicate_Orders = PredicateBuilder.New<Orders>(true);
            var Predicate_OrderDetail = PredicateBuilder.New<Order_Detail>(true);
            var Predicate_Activity = PredicateBuilder.New<Activity>(true);
            var Predicate_Tickets = PredicateBuilder.New<Tickets>(true);
            var Predicate_TicketTimes = PredicateBuilder.New<TicketTimes>(true);
            var Predicate_TicletCategory = PredicateBuilder.New<TicketCategory>(true);
            var Predicate_Districts = PredicateBuilder.New<Districts>(true);
            var Predicate_Cities = PredicateBuilder.New<Cities>(true);

            if (!string.IsNullOrEmpty(OrderID))
                predicate_Orders = predicate_Orders.And(o => o.OrderID.Equals(Orderid));


            BackEndOrders = from o in ticket.Orders.Where(predicate_Orders)
                            join od in ticket.Order_Detail.Where(Predicate_OrderDetail)
                            on o.OrderID equals od.OrderID

                            join t in ticket.Tickets.Where(Predicate_Tickets)
                            on od.TicketId equals t.TicketID

                            join tt in ticket.TicketTimes.Where(Predicate_TicketTimes)
                            on t.TicketTimeId equals tt.TicketTimeId

                            join tc in ticket.TicketCategory.Where(Predicate_TicletCategory)
                            on t.TicketCategoryId equals tc.TicketCategoryId

                            join ac in ticket.Activity.Where(Predicate_Activity)
                            on tt.ActivityId equals ac.ActivityID

                            join d in ticket.Districts.Where(Predicate_Districts)
                            on o.DistrictId equals d.DistrictId

                            join c in ticket.Cities.Where(Predicate_Cities)
                            on d.CityId equals c.CityID

                            select new CBackEndOrderDetail
                            {
                                Orders = o,
                                OrderDetail = od,
                                Districts = d,
                                Cities = c,
                                Tickets = t,
                                TicketTimes = tt,
                                Activity = ac,
                                TicketCategory = tc
                            };

            return BackEndOrders.ToList();
        }

        //後台套票查詢API
        [HttpPost]
        public List<CBackEndTicketGroupList> getTicketGroupList([FromBody]TicketGroupList ticketGroupList)
        {
            string TGstatus1 = ticketGroupList.TicketGroupStatus1;
            bool Status1;
            bool.TryParse(TGstatus1, out Status1);

            string TGstatus2 = ticketGroupList.TicketGroupStatus2;
            bool Status2;
            bool.TryParse(TGstatus2, out Status2);

            string CompanyName = !string.IsNullOrEmpty(ticketGroupList.CompanyName) ? ticketGroupList.CompanyName.Trim() : null;
            string TicketGroupName = !string.IsNullOrEmpty(ticketGroupList.TicketGroupName) ? ticketGroupList.TicketGroupName.Trim() : null;


            IQueryable<CBackEndTicketGroupList> TicketGroupList = null;

            var predicate_TicketGroupName = PredicateBuilder.New<TicketGroups>(true);
            var predicate_CompanyName = PredicateBuilder.New<Seller>(true);
            var predicate_TGStatus = PredicateBuilder.New<TicketGroups>(false);

            if (!string.IsNullOrEmpty(CompanyName))
                predicate_CompanyName = predicate_CompanyName.And(s => s.CompanyName.Contains(CompanyName));

            if (!string.IsNullOrEmpty(TicketGroupName))
                predicate_TicketGroupName = predicate_TicketGroupName.And(tgn => tgn.TicketGroupName.Contains(TicketGroupName));

            if (!string.IsNullOrEmpty(TGstatus1))
                predicate_TGStatus = predicate_TGStatus.Or(tg => tg.Status.Equals(Status1));

            if (!string.IsNullOrEmpty(TGstatus2))
                predicate_TGStatus = predicate_TGStatus.Or(tg => tg.Status.Equals(Status2));

            TicketGroupList = (from tg in ticket.TicketGroups.Where(predicate_TicketGroupName).Where(predicate_TGStatus)

                               join tgd in ticket.TicketGroupDetail
                               on tg.TicketGroupId equals tgd.TicketGroupId

                               join a in ticket.Activity
                               on tgd.ActivityId equals a.ActivityID

                               join s in ticket.Seller.Where(predicate_CompanyName)
                               on a.SellerID equals s.SellerID


                               select new CBackEndTicketGroupList
                               {
                                   TicketGroups = tg,
                                   Seller = s
                               }).Distinct();

            return TicketGroupList.ToList();
        }

        //後台套票審核成功API
        [HttpPut]
        public string updateTicketGroupStatus([FromBody]updateTicketGroupStatus updateTicketGroupStatus)
        {
            try
            {
                bool status = updateTicketGroupStatus.Status;
                int TicketGroupID = updateTicketGroupStatus.TicketGroupID;

                var TicketGroup = ticket.TicketGroups.Where(tg => tg.TicketGroupId == TicketGroupID).FirstOrDefault();

                TicketGroup.Status = status;

                int num = ticket.SaveChanges();
                if (num == 1)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return (ex.Message);

            }

        }

        //後臺套票審核未通過，傳送通知給商家API
        [HttpPost]
        public void TicketGroupNoPassMessage([FromBody]updateNoPassMessage updateNoPassMessage)
        {
            int SellerID = updateNoPassMessage.SellerID;
            string Message = updateNoPassMessage.Message;

            ShortMessage shortMessage = new ShortMessage();

            var memberid = (
                from seller in ticket.Seller
                from m in ticket.Member
                where seller.MemberId == m.MemberID
                && seller.SellerID == SellerID
                select m.MemberID).FirstOrDefault();

            shortMessage.MemberID = memberid;
            shortMessage.MessageContent = Message;

            ticket.ShortMessage.Add(shortMessage);
            ticket.SaveChanges();
        }

        //套票->活動->場次
        [HttpPost]
        public List<DateTime> getTicketTimes([FromBody]getTiketTimes getTiketTimes)
        {
            int ActivityID = getTiketTimes.ActivityID;

            List<DateTime> ticketTimes = ticket.TicketTimes.Where(t => t.ActivityId == ActivityID).Select(s => s.TicketTime).ToList();

            return ticketTimes;
        }

        //文章內容
        [HttpPost]
        public CBackEndMainArticle Article([FromBody]ArticleID articleID)
        {
            int ArticleID = articleID.articleID;

            //Article article = new Article();
            CBackEndMainArticle CBackEndMainArticle = new CBackEndMainArticle();
            var ar = ticket.Article.Where(a => a.ArticleCategoryID.Equals(a.ArticleCategories.ArticleCategoryID) && a.MemberID.Equals(a.Member.MemberID) && a.ArticleID == ArticleID).FirstOrDefault();
            CBackEndMainArticle.Article = ar;
            CBackEndMainArticle.ArticleCategoryName = ar.ArticleCategories.ArticleCategoryName;
            CBackEndMainArticle.MemberName = ar.Article_Emotion.FirstOrDefault().Member.Name;

            return CBackEndMainArticle;
        }

    }
}

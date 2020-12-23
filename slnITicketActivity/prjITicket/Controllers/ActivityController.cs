using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using prjITicket.Models;
using prjITicket.ViewModel;
using Newtonsoft.Json;
using AllPay.Payment.Integration;
using System.Collections;
using ZXing.QrCode;
using System.IO;
using ZXing;
using System.Drawing;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Net.Mime;
using Newtonsoft.Json.Linq;

namespace prjITicket.Controllers
{
    //todo測試價格篩選環境下的換頁
    //todo 按價格區間篩選
    //todo 加入收藏
    //todo 點數
    public class ActivityController : Controller
    {
        TicketSysEntities db = new TicketSysEntities();
        //顯示活動一覽呼叫的controller
        public ActionResult ActivityList()
        {
            //抓到需要秀在左邊的類別
            List<Categories> categories = db.Categories.ToList();
            List<Activity> activities = db.Activity.Where(a => a.ActivityStatusID == 1).ToList();
            List<Activity> scrollActivities = db.Activity.Where(a=>a.ActivityStatusID==1).OrderByDescending(a => a.ActivityID).Take(3).ToList();
            //從subcategory抓出6個熱門搜索
            List<SubCategories> hotSearch = db.SubCategories.OrderByDescending(sc => sc.SearchedTime).Take(6).ToList();
            //抓出每個活動最便宜的票,再取這當中的最大值,準備塞給價格篩選器的最大值
            int maxPriceAll = (int)db.Tickets.GroupBy(t => t.ActivityID).Max(g => g.Min(t => t.Price * (1 - t.Discount)));
            //找出銷售量前10的活動塞入ViewModel
            List<ActivitySell> best10Activity = db.Activity.AsEnumerable().Where(a => a.ActivityStatusID == 1).Select(a => new ActivitySell()
            {
                ActivityId = a.ActivityID,
                ActivityName = a.ActivityName,
                TotalSell = a.Tickets.Sum(t => t.Order_Detail.Sum(od => od.Quantity))
            }).OrderByDescending(a => a.TotalSell).Take(10).ToList();
            //把資料塞進ViewModel
            VMActivityList vm = new VMActivityList()
            {
                Categories = categories,
                ScrollImgActivities = scrollActivities,
                HotSubCategories = hotSearch,
                MaxPriceAll = maxPriceAll,
                Best10 = best10Activity
            };
            return View(vm);
        }
        //調用顯示活動詳細資料的controller
        public ActionResult ActivityDetail(int activityId)
        {
            Activity activity = db.Activity.Where(a => a.ActivityStatusID == 1).FirstOrDefault(a => a.ActivityID == activityId);
            if (activity == null)
            {
                //todo回傳找不到活動的錯誤頁面
                return View("ProductNotFound");
            }
            //找出所有與這個Activity相關的group
            List<TicketGroups> groups = db.TicketGroups.Where(tg =>tg.Status&&tg.TicketGroupDetail.Select(tgd => tgd.ActivityId).Contains(activityId)).ToList();
            //找出(其他人還買了)的活動塞到ViewModel
            List<Activity> otherPeopleBuy = db.Orders.Where(o => o.Order_Detail.Any(od => od.Tickets.ActivityID == activityId))
                .SelectMany(o => o.Order_Detail).Where(od => od.Tickets.ActivityID != activityId)
                .GroupBy(od => od.Tickets.Activity).OrderByDescending(g => g.Count()).Select(g => g.Key).Where(a=>a.ActivityStatusID==1).Take(3).ToList();
            //塞入ViewModel傳到前端
            VMActivityDetail vm = new VMActivityDetail() { Activity = activity, Groups = groups,OtherPeopleBuy = otherPeopleBuy };
            return View(vm);
        }
        //顯示購物車清單的controller
        public ActionResult ShoppingCartList()
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            //db讀取會員的殘餘點數
            int memberId = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            int point = db.Member.FirstOrDefault(m => m.MemberID == memberId).Point;
            ViewBag.MaxPoint = point;
            return View();
        }
        //驗證購物車清單是否合法的方法
        public bool CheckShoppingCartItem(List<CShoppingCart> shoppingCart, out string result)
        {
            bool flag = true;
            int count = shoppingCart.Count();
            result = "購物失敗\\n";
            int index = 0;
            for (int i = 0; i < count; i++)
            {
                Tickets ticket = db.Tickets.ToList().FirstOrDefault(t => t.TicketID == shoppingCart[index].TicketID);
                if (ticket.Activity.ActivityStatusID != 1)
                {
                    //OK
                    shoppingCart.Remove(shoppingCart[index]);
                    result += $"{ticket.Activity.ActivityName}已經下架,已從購物車移除\\n";
                    flag = false;
                    continue;
                }
                else if (ticket.UnitsInStock == 0)
                {
                    //OK
                    shoppingCart.Remove(shoppingCart[index]);
                    result += $"{ticket.Activity.ActivityName}{ticket.TicketCategory.TicketCategoryName}{ticket.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm:ss")}售完,已從購物車移除\\n";
                    flag = false;
                    continue;
                }
                else if (ticket.UnitsInStock < shoppingCart[index].Quantity)
                {
                    //OK
                    shoppingCart[index].Quantity = ticket.UnitsInStock;
                    result += $"{ticket.Activity.ActivityName}{ticket.TicketCategory.TicketCategoryName}{ticket.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm:ss")}超過購買量,已調整\\n";
                    flag = false;
                }
                index++;
            }
            return flag;
        }
        //去歐付寶假付款
        public string GoToPay(CGoToPay input)
        {
            if (Session[CDictionary.SK_Logined_Member] == null || Session[CDictionary.ShoppingCart] == null)
            {
                //ShoppingCart為空,交易失敗並且跳轉 //OK
                //todo 測試Member為空
                return $"<script>alert('交易失敗,返回首頁');location.href='{Url.Action("ActivityList")}'</script>";
            }
            else if ((Session[CDictionary.ShoppingCart] as List<CShoppingCart>).Count() == 0)
            {
                //OK
                return $"<script>alert('購物車沒有物品,返回首頁');location.href='{Url.Action("ActivityList")}'</script>";
            }
            else
            {
                //檢查購物車是否合法
                string resultString = "";
                List<CShoppingCart> shoppingCarts = Session[CDictionary.ShoppingCart] as List<CShoppingCart>;
                //不合法,就調整完跳轉回主頁  //OK
                if (!CheckShoppingCartItem(shoppingCarts, out resultString))
                {
                    return $"<script>alert('{resultString}');location.href='{Url.Action("ActivityList")}'</script>";
                }
                //合法,就造出Order跟OrderDetail      
                int memberId = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
                string orderGuid = String.Concat(Guid.NewGuid().ToString().Where(c => c != '-')).Substring(0, 20);
                Orders order = new Orders()
                {
                    Name = input.name,
                    Email = input.email,
                    DistrictId = input.districtId,
                    Address = input.address,
                    MemberID = memberId,
                    OrderDate = DateTime.Now,
                    OrderStatus = false,
                    OrderGuid = orderGuid,
                    PayPoint = input.point
                };
                db.Orders.Add(order);
                db.SaveChanges();
                int totalPrice = 0;
                foreach (CShoppingCart item in shoppingCarts)
                {
                    Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketID == item.TicketID);
                    totalPrice = totalPrice + (int)Math.Round(ticket.Price * (1 - ticket.Discount), 0) * item.Quantity;
                    Order_Detail od = new Order_Detail()
                    {
                        TicketId = ticket.TicketID,
                        OrderID = order.OrderID,
                        Quantity = item.Quantity,
                        Discount = ticket.Discount
                    };
                    db.Order_Detail.Add(od);
                }
                db.SaveChanges();
                //一旦產生訂單就清空購物車 //OK
                Session[CDictionary.ShoppingCart] = null;
                //執行歐付寶程式碼
                List<string> enErrors = new List<string>();
                try
                {
                    using (AllInOne oPayment = new AllInOne())
                    {
                        /* 服務參數 */
                        oPayment.ServiceMethod = HttpMethod.HttpPOST;
                        oPayment.ServiceURL = "https://payment-stage.opay.tw/Cashier/AioCheckOut/V5";
                        oPayment.HashKey = "5294y06JbISpM5x9";
                        oPayment.HashIV = "v77hoKGq4kWxNNIS";
                        oPayment.MerchantID = "2000132";
                        /* 基本參數 */
                        //Azure版
                        oPayment.Send.ReturnURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("CatchFinishPayData");
                        oPayment.Send.OrderResultURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay", new { orderId = order.OrderID });
                        //本地版
                        //oPayment.Send.ReturnURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay");
                        //oPayment.Send.OrderResultURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay");
                        //////////
                        oPayment.Send.MerchantTradeNo = orderGuid;
                        oPayment.Send.MerchantTradeDate = DateTime.Now;
                        oPayment.Send.TotalAmount = totalPrice-input.point;
                        oPayment.Send.TradeDesc = "Itcket購票網";                       
                        oPayment.Send.Remark = $"點數折抵{input.point}元";
                        //oPayment.Send.ChooseSubPayment = PaymentMethodItem.None;
                        //oPayment.Send.NeedExtraPaidInfo = ExtraPaymentInfo.Yes;
                        //oPayment.Send.HoldTrade = HoldTradeType.No;
                        //oPayment.Send.DeviceSource = DeviceType.PC;
                        //oPayment.Send.UseRedeem = UseRedeemFlag.Yes; //購物金/紅包折抵
                        //oPayment.Send.IgnorePayment = "<<您不要顯示的付款方式>>"; // 例如財付通:Tenpay
                        // 加入選購商品資料。
                        foreach (Order_Detail od in db.Order_Detail.Where(od => od.OrderID == order.OrderID))
                        {
                            oPayment.Send.Items.Add(new Item()
                            {
                                Name = od.Tickets.Activity.ActivityName + od.Tickets.TicketCategory.TicketCategoryName + od.Tickets.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm:ss"),
                                Price = Math.Round(od.Tickets.Price * (1 - od.Discount), 0) * od.Quantity,
                                Currency = "台幣",
                                Quantity = od.Quantity,
                                URL = "test"
                            });
                        }
                        /* 產生訂單 */
                        enErrors.AddRange(oPayment.CheckOut());
                        /* 產生產生訂單 Html Code 的方法 */
                        string szHtml = String.Empty;
                        enErrors.AddRange(oPayment.CheckOutString(ref szHtml));
                        return szHtml;
                    }
                }
                catch (Exception ex)
                {
                    // 例外錯誤處理。
                    enErrors.Add(ex.Message);
                    return String.Concat(enErrors);
                }
                finally
                {
                    // 顯示錯誤訊息。
                    if (enErrors.Count() > 0)
                    {
                        string szErrorMessage = String.Join("\\r\\n", enErrors);
                    }
                }
            }
        }
        //根據orderId顯示訂單,付款
        public ActionResult OrderDetailByOrderId(int orderId)
        {
            Orders order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            return View(order);
        }
        //在未付款訂單畫面點付款會用到的函數
        [HttpPost]
        public string OrderDetailByOrderIdGoToPay(int orderId,int totalPrice)
        {
            Orders order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            //執行歐付寶程式碼
            List<string> enErrors = new List<string>();
            try
            {
                using (AllInOne oPayment = new AllInOne())
                {
                    /* 服務參數 */
                    oPayment.ServiceMethod = HttpMethod.HttpPOST;
                    oPayment.ServiceURL = "https://payment-stage.opay.tw/Cashier/AioCheckOut/V5";
                    oPayment.HashKey = "5294y06JbISpM5x9";
                    oPayment.HashIV = "v77hoKGq4kWxNNIS";
                    oPayment.MerchantID = "2000132";
                    /* 基本參數 */
                    //Azure版
                    oPayment.Send.ReturnURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("CatchFinishPayData");
                    oPayment.Send.OrderResultURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay", new { orderId = order.OrderID });
                    //本地版
                    //oPayment.Send.ReturnURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay");
                    //oPayment.Send.OrderResultURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay");
                    //////////
                    oPayment.Send.MerchantTradeNo = order.OrderGuid;
                    oPayment.Send.MerchantTradeDate = DateTime.Now;
                    oPayment.Send.TotalAmount = totalPrice - order.PayPoint;
                    oPayment.Send.TradeDesc = "Itcket購票網";
                    //oPayment.Send.ChoosePayment = PaymentMethod.ALL;
                    oPayment.Send.Remark = $"點數折抵{order.PayPoint}元";
                    //oPayment.Send.ChooseSubPayment = PaymentMethodItem.None;
                    //oPayment.Send.NeedExtraPaidInfo = ExtraPaymentInfo.Yes;
                    //oPayment.Send.HoldTrade = HoldTradeType.No;
                    //oPayment.Send.DeviceSource = DeviceType.PC;
                    //oPayment.Send.UseRedeem = UseRedeemFlag.Yes; //購物金/紅包折抵
                    //oPayment.Send.IgnorePayment = "<<您不要顯示的付款方式>>"; // 例如財付通:Tenpay
                    // 加入選購商品資料。
                    foreach (Order_Detail od in db.Order_Detail.Where(od => od.OrderID == order.OrderID))
                    {
                        oPayment.Send.Items.Add(new Item()
                        {
                            Name = od.Tickets.Activity.ActivityName + od.Tickets.TicketCategory.TicketCategoryName + od.Tickets.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            Price = Math.Round(od.Tickets.Price * (1 - od.Discount), 0) * od.Quantity,
                            Currency = "台幣",
                            Quantity = od.Quantity,
                            URL = "test"
                        });
                    }
                    /* 產生訂單 */
                    enErrors.AddRange(oPayment.CheckOut());
                    /* 產生產生訂單 Html Code 的方法 */
                    string szHtml = String.Empty;
                    enErrors.AddRange(oPayment.CheckOutString(ref szHtml));
                    return szHtml;
                }
            }
            catch (Exception ex)
            {
                // 例外錯誤處理。
                enErrors.Add(ex.Message);
                return String.Concat(enErrors);
            }
            finally
            {
                // 顯示錯誤訊息。
                if (enErrors.Count() > 0)
                {
                    string szErrorMessage = String.Join("\\r\\n", enErrors);
                }
            }
        }
        //從套票點付款會進來的controller,show出套票訂單
        public ActionResult OrderDetailByTicketGroup(COrderDetailByTicketGroup input)
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var ticketsInput = (JArray)JsonConvert.DeserializeObject(input.Tickets);
            List<TicketDesc> ticketDescs = ticketsInput.Select(j => new TicketDesc { TicketCategoryId = j.First.First.Value<int>(), TicketTimeId = j.Last.First.Value<int>() }).ToList();
            List<Tickets> tickets = ticketDescs.Select(td => db.Tickets.FirstOrDefault(t => t.TicketCategoryId == td.TicketCategoryId && t.TicketTimeId == td.TicketTimeId)).ToList();           
            //驗證選擇的套票是否合法
            bool disAvailible = tickets.Any(t => t == null||t.UnitsInStock==0||input.Quantity>t.UnitsInStock);
            if (disAvailible)
            {
                return View("ProductNotFound");
            }
            int maxPoint = (Session[CDictionary.SK_Logined_Member] as Member).Point;
            VMOrderDetailByTicketGroup vm = new VMOrderDetailByTicketGroup()
            {
                Tickets = tickets,
                MaxPoint = maxPoint,
                Quantity = input.Quantity,
                Discount = input.Discount
            };
            return View(vm);
            //////////////////////////////////////////////////
            //***************添加到組長View的javascript代碼**************************/
            /*
             //按下付款要執行的動作
        $("#frmGoToPay").submit(function () {
            let tickets = [];
            $("#shoppingCartDetail tr").each(function (i, e) {
                let ticketCategoryId = $(e).find(".selTicketCategory").val();
                let ticketTimeId = $(e).find(".selTicketTime").val();
                tickets.push(new Ticket(ticketCategoryId, ticketTimeId));
            });
            let discount = @Model.ticketGroupDetails[0].TicketGroups.TicketGroupDiscount;
            let quantity = count;
            tickets = JSON.stringify(tickets);
            $("input[name='Discount']").val(discount);
            $("input[name='Quantity']").val(quantity);
            $("input[name='Tickets']").val(tickets);
        });
        //ticket物件
        function Ticket(ticketCategoryId, ticketTimeId) {
            this.TicketCategoryId = ticketCategoryId||-1;
            this.TicketTimeId = ticketTimeId||-1;
        }
        <div class="mt-3 d-flex justify-content-end">   
                    <form id="frmGoToPay" action="@Url.Action("OrderDetailByTicketGroup","Activity")" method="post">
                        <input type="hidden" name="Discount"/>
                        <input type="hidden" name="Quantity"/>
                        <input type="hidden" name="Tickets"/>
                        <input type="submit" class="btn btn-primary" value="購買">
                    </form>
                </div>
            /*************************************************/
            ////////////////////////////////////////////////////
        }
        //套票去歐福寶付款
        public string GoToPayTicketGroup(CGoToPayTicketGroup input)
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            if (member == null)
            {
                //todo 測試
                return $"<script>alert('交易失敗,返回首頁');location.href='{Url.Action("Index","Home")}'</script>";
            }
            //找出所選擇的套票
            int[] ticketIds = (int[])JsonConvert.DeserializeObject(input.ticketIds, typeof(int[]));
            List<Tickets> tickets = ticketIds.Select(ti => db.Tickets.FirstOrDefault(t => t.TicketID == ti)).ToList();
            //驗證套票庫存,上架狀態是否合法
            foreach (Tickets ticket in tickets)
            {
                if (ticket.Activity.ActivityStatusID == 0)
                {
                    //todo 測試
                    return $"<script>alert('{ticket.Activity.ActivityName}已經下架,交易失敗,返回首頁');location.href='{Url.Action("Index", "Home")}'</script>";
                }
                if (ticket.UnitsInStock == 0)
                {
                    //todo 測試
                    return $"<script>alert('{ticket.Activity.ActivityName}{ticket.TicketCategory.TicketCategoryName}{ticket.TicketTimes.TicketTime.ToString("yyyy-MM-dd HH:mm")}已經售完,交易失敗,返回首頁');location.href='{Url.Action("Index", "Home")}'</script>";
                }
                if (input.quantity > ticket.UnitsInStock)
                {
                    //todo 測試
                    return $"<script>alert('{ticket.Activity.ActivityName}{ticket.TicketCategory.TicketCategoryName}{ticket.TicketTimes.TicketTime.ToString("yyyy-MM-dd HH:mm")}庫存量不足,交易失敗,返回首頁');location.href='{Url.Action("Index", "Home")}'</script>";
                }
            }
            //驗證通過,就產生訂單
            int memberId = member.MemberID;
            string orderGuid = String.Concat(Guid.NewGuid().ToString().Where(c => c != '-')).Substring(0, 20);
            Orders order = new Orders()
            {
                Name = input.name,
                Email = input.email,
                DistrictId = input.districtId,
                Address = input.address,
                MemberID = memberId,
                OrderDate = DateTime.Now,
                OrderStatus = false,
                OrderGuid = orderGuid,
                PayPoint = input.point
            };
            db.Orders.Add(order);
            db.SaveChanges();
            int totalPrice = 0;
            foreach (Tickets ticket in tickets)
            {               
                totalPrice = totalPrice + (int)Math.Round(ticket.Price * (1 - input.discount), 0) * input.quantity;
                Order_Detail od = new Order_Detail()
                {
                    TicketId = ticket.TicketID,
                    OrderID = order.OrderID,
                    Quantity = input.quantity,
                    Discount = input.discount
                };
                db.Order_Detail.Add(od);
            }
            db.SaveChanges();
            //去歐福寶付款
            //執行歐付寶程式碼
            List<string> enErrors = new List<string>();
            try
            {
                using (AllInOne oPayment = new AllInOne())
                {
                    /* 服務參數 */
                    oPayment.ServiceMethod = HttpMethod.HttpPOST;
                    oPayment.ServiceURL = "https://payment-stage.opay.tw/Cashier/AioCheckOut/V5";
                    oPayment.HashKey = "5294y06JbISpM5x9";
                    oPayment.HashIV = "v77hoKGq4kWxNNIS";
                    oPayment.MerchantID = "2000132";
                    /* 基本參數 */
                    //Azure版
                    oPayment.Send.ReturnURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("CatchFinishPayData");
                    oPayment.Send.OrderResultURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay", new { orderId = order.OrderID });
                    //本地版
                    //oPayment.Send.ReturnURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay");
                    //oPayment.Send.OrderResultURL = Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Action("FinishPay");
                    //////////
                    oPayment.Send.MerchantTradeNo = orderGuid;
                    oPayment.Send.MerchantTradeDate = DateTime.Now;
                    oPayment.Send.TotalAmount = totalPrice - input.point;
                    oPayment.Send.TradeDesc = "Itcket購票網";
                    //oPayment.Send.ChoosePayment = PaymentMethod.ALL;
                    oPayment.Send.Remark = $"點數折抵{input.point}元";
                    //oPayment.Send.ChooseSubPayment = PaymentMethodItem.None;
                    //oPayment.Send.NeedExtraPaidInfo = ExtraPaymentInfo.Yes;
                    //oPayment.Send.HoldTrade = HoldTradeType.No;
                    //oPayment.Send.DeviceSource = DeviceType.PC;
                    //oPayment.Send.UseRedeem = UseRedeemFlag.Yes; //購物金/紅包折抵
                    //oPayment.Send.IgnorePayment = "<<您不要顯示的付款方式>>"; // 例如財付通:Tenpay
                    // 加入選購商品資料。
                    foreach (Order_Detail od in db.Order_Detail.Where(od => od.OrderID == order.OrderID))
                    {
                        oPayment.Send.Items.Add(new Item()
                        {
                            Name = od.Tickets.Activity.ActivityName + od.Tickets.TicketCategory.TicketCategoryName + od.Tickets.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            Price = Math.Round(od.Tickets.Price * (1 - od.Discount), 0) * od.Quantity,
                            Currency = "台幣",
                            Quantity = od.Quantity,
                            URL = "test"
                        });
                    }
                    /* 產生訂單 */
                    enErrors.AddRange(oPayment.CheckOut());
                    /* 產生產生訂單 Html Code 的方法 */
                    string szHtml = String.Empty;
                    enErrors.AddRange(oPayment.CheckOutString(ref szHtml));
                    return szHtml;
                }
            }
            catch (Exception ex)
            {
                // 例外錯誤處理。
                enErrors.Add(ex.Message);
                return String.Concat(enErrors);
            }
            finally
            {
                // 顯示錯誤訊息。
                if (enErrors.Count() > 0)
                {
                    string szErrorMessage = String.Join("\\r\\n", enErrors);
                }
            }           
        }
        /*******************************
        上傳Azure時,需要把歐付寶背景回傳資料處理程式CatchFinishPayData與回傳完成付款頁面分開,
        否則會執行2次
        本地時,要把FinishPay與CatchFinishPayData合併
        ********************************/
        //本地版付款程式碼
        //public ActionResult FinishPay()
        //{
        //    List<string> enErrors = new List<string>();
        //    Hashtable htFeedback = null;
        //    using (AllInOne oPayment = new AllInOne())
        //    {
        //        oPayment.HashKey = "5294y06JbISpM5x9";
        //        oPayment.HashIV = "v77hoKGq4kWxNNIS";
        //        /* 取回付款結果 */
        //        enErrors.AddRange(oPayment.CheckOutFeedback(ref htFeedback));
        //    }
        //    //抓到成功訂單的Guid
        //    string orderGuid = htFeedback["MerchantTradeNo"].ToString();
        //    Orders order = db.Orders.FirstOrDefault(o => o.OrderGuid == orderGuid);
        //    //把訂單的付款狀態改為true
        //    order.OrderStatus = true;
        //    //為OrderDetail產生QRCode,計算應獲得點數為總成交價格的1%
        //    decimal total = 0;
        //    foreach (Order_Detail od in order.Order_Detail.ToList())
        //    {
        //        total += od.Tickets.Price * od.Quantity * (1 - od.Discount);
        //        for (int i = 0; i < od.Quantity; i++)
        //        {
        //            TicketQRCodes qrCode = new TicketQRCodes()
        //            {
        //                QRCode = Guid.NewGuid().ToString(),
        //                OrderDetailId = od.OrderDetailID
        //            };
        //            db.TicketQRCodes.Add(qrCode);
        //        }
        //    }
        //    //todo 發送系統通知,通知獲得點數
        //    int earnPoint = (int)(total - order.PayPoint) / 100;
        //    if (earnPoint != 0)
        //    {
        //        order.Member.Point += earnPoint;
        //        db.ShortMessage.Add(new ShortMessage()
        //        {
        //            MemberID = order.MemberID,
        //            MessageContent = $"訂單號碼{order.OrderGuid}完成,獲得點數{earnPoint}點"
        //        });
        //    }
        //    db.SaveChanges();
        //    //發送Email
        //    string emailAddress = order.Email;
        //    SendFinishPayEmailToMember(emailAddress, orderGuid);
        //    return View(order);
        //}        
        //Azure版付款程式碼
        //顯示購物完成的頁面
        public ActionResult FinishPay(int orderId)
        {
            Orders order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            return View(order);
        }
        //購物完成接收資料寫入資料庫
        public void CatchFinishPayData()
        {
            List<string> enErrors = new List<string>();
            Hashtable htFeedback = null;
            using (AllInOne oPayment = new AllInOne())
            {
                oPayment.HashKey = "5294y06JbISpM5x9";
                oPayment.HashIV = "v77hoKGq4kWxNNIS";
                /* 取回付款結果 */
                enErrors.AddRange(oPayment.CheckOutFeedback(ref htFeedback));
            }
            //抓到成功訂單的Guid
            string orderGuid = htFeedback["MerchantTradeNo"].ToString();
            Orders order = db.Orders.FirstOrDefault(o => o.OrderGuid == orderGuid);
            //把訂單的付款狀態改為true
            order.OrderStatus = true;
            //為OrderDetail產生QRCode,計算應獲得點數為總成交價格的1%
            decimal total = 0;
            foreach (Order_Detail od in order.Order_Detail.ToList())
            {
                total += od.Tickets.Price * od.Quantity * (1 - od.Discount);
                for (int i = 0; i < od.Quantity; i++)
                {
                    TicketQRCodes qrCode = new TicketQRCodes()
                    {
                        QRCode = Guid.NewGuid().ToString(),
                        OrderDetailId = od.OrderDetailID
                    };
                    db.TicketQRCodes.Add(qrCode);
                }
            }
            //todo 發送系統通知,通知獲得點數
            int earnPoint = (int)(total - order.PayPoint) / 100;
            if (earnPoint != 0)
            {
                order.Member.Point += earnPoint;
                db.ShortMessage.Add(new ShortMessage()
                {
                    MemberID = order.MemberID,
                    MessageContent = $"訂單號碼{order.OrderGuid}完成,獲得點數{earnPoint}點"
                });
            }
            db.SaveChanges();
            //發送Email
            string emailAddress = order.Email;
            SendFinishPayEmailToMember(emailAddress, orderGuid);
        }
        //上傳產品的頁面
        public ActionResult UpLoadActivity()
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            if (member == null || member.MemberRoleId != 3)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        //修改產品的頁面
        public ActionResult EditActivity(int activityId)
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            if (member == null || member.MemberRoleId != 3)
            {
                return RedirectToAction("Login", "Login");
            }
            Activity activity = db.Activity.FirstOrDefault(a => a.ActivityID == activityId);
            return View(activity);
        }
        //設定活動票的價格和庫存量的頁面
        public ActionResult AddPriceAndUnitsInStock(int activityId)
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            if (member == null || member.MemberRoleId != 3)
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.ActivityId = activityId;
            Activity activity = db.Activity.FirstOrDefault(a => a.ActivityID == activityId);
            return View(activity);
        }
        //上傳套票的頁面
        public ActionResult UploadTicketGroup()
        {
            //商家後臺layout連結填寫這裡
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            if (member == null || member.MemberRoleId != 3)
            {
                return RedirectToAction("Login", "Login");
            }
            List<Activity> activities = member.Seller.FirstOrDefault().Activity.Where(a=>a.ActivityStatusID==1).ToList();
            return View(activities);
        }
        //在ActivityList頁面中Ajax調用這個方法取得活動分頁
        public ActionResult GetActivityPages(int currentPage = 1, string orderMode = "scoredown", int minPrice = 0, int maxPrice = int.MaxValue, int priceFilter = 0)
        {
            currentPage = currentPage < 1 ? 1 : currentPage;
            int pageSize = 6;
            //ViewBag紀錄搜索模式
            ViewBag.ActionName = "GetActivityPages";
            ViewBag.OrderMode = orderMode;
            List<Activity> activities = db.Activity.Where(a => a.ActivityStatusID == 1).ToList();
            if (priceFilter == 1)
            {
                ViewBag.PriceFilter = 1;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                activities = activities.Where(a => a.Tickets.Count() != 0 && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) >= minPrice && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) <= maxPrice).ToList();
            }
            activities = OrderActivitiesByOrderMode(activities, orderMode);
            IPagedList<Activity> pages = activities.ToPagedList(currentPage, pageSize);
            if (pages.Count() == 0)
            {
                //todo回傳找不到產品的頁面
                return PartialView("ZeroActivity");
            }
            return PartialView(pages);
        }
        //在ActivityList頁面中Ajax調用這個方法取得活動分頁
        public ActionResult GetActivityPagesByKeyword(string keyword, int currentPage = 1, string orderMode = "scoredown", int minPrice = 0, int maxPrice = int.MaxValue, int priceFilter = 0)
        {
            currentPage = currentPage < 1 ? 1 : currentPage;
            int pageSize = 6;
            //ViewBag紀錄搜索模式與參數
            ViewBag.ActionName = "GetActivityPagesByKeyword";
            ViewBag.Keyword = keyword;
            ViewBag.OrderMode = orderMode;
            List<Activity> activities = db.Activity.Where(a => a.ActivityStatusID == 1 && a.ActivityName.ToLower().Contains(keyword.ToLower())).ToList();
            if (priceFilter == 1)
            {
                ViewBag.PriceFilter = 1;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                activities = activities.Where(a => a.Tickets.Count() != 0 && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) >= minPrice && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) <= maxPrice).ToList();
            }
            activities = OrderActivitiesByOrderMode(activities, orderMode);
            IPagedList<Activity> pages = activities.ToPagedList(currentPage, pageSize);
            if (pages.Count() == 0)
            {
                //todo回傳找不到產品的頁面
                return PartialView("ZeroActivity");
            }
            return PartialView("GetActivityPages", pages);
        }
        //在ActivityList頁面中Ajax調用這個方法取得活動分頁
        public ActionResult GetActivityPagesByCategoryId(int categoryId, int currentPage = 1, string orderMode = "scoredown", int minPrice = 0, int maxPrice = int.MaxValue, int priceFilter = 0)
        {
            currentPage = currentPage < 1 ? 1 : currentPage;
            int pageSize = 6;
            //ViewBag紀錄搜索模式與參數
            ViewBag.ActionName = "GetActivityPagesByCategoryId";
            ViewBag.CategoryId = categoryId;
            ViewBag.OrderMode = orderMode;
            List<Activity> activities = db.Activity.Where(a => a.ActivityStatusID == 1 && a.SubCategories.Categories.CategoryID == categoryId).ToList();
            if (priceFilter == 1)
            {
                ViewBag.PriceFilter = 1;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                activities = activities.Where(a => a.Tickets.Count() != 0 && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) >= minPrice && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) <= maxPrice).ToList();
            }
            activities = OrderActivitiesByOrderMode(activities, orderMode);
            IPagedList<Activity> pages = activities.ToPagedList(currentPage, pageSize);
            if (pages.Count() == 0)
            {
                //todo回傳找不到產品的頁面
                return PartialView("ZeroActivity");
            }
            return PartialView("GetActivityPages", pages);
        }
        //在ActivityList頁面中Ajax調用這個方法取得活動分頁
        public ActionResult GetActivityPagesBySubCategoryId(int subCategoryId, int currentPage = 1, string orderMode = "scoredown", int minPrice = 0, int maxPrice = int.MaxValue, int priceFilter = 0)
        {
            currentPage = currentPage < 1 ? 1 : currentPage;
            int pageSize = 6;
            //ViewBag紀錄搜索模式與參數
            ViewBag.ActionName = "GetActivityPagesBySubCategoryId";
            ViewBag.SubCategoryId = subCategoryId;
            ViewBag.OrderMode = orderMode;
            List<Activity> activities = db.Activity.Where(a => a.ActivityStatusID == 1 && a.SubCategoryId == subCategoryId).ToList();
            if (priceFilter == 1)
            {
                ViewBag.PriceFilter = 1;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                activities = activities.Where(a => a.Tickets.Count() != 0 && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) >= minPrice && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) <= maxPrice).ToList();
            }
            activities = OrderActivitiesByOrderMode(activities, orderMode);
            IPagedList<Activity> pages = activities.ToPagedList(currentPage, pageSize);
            if (pages.Count() == 0)
            {
                //todo回傳找不到產品的頁面
                return PartialView("ZeroActivity");
            }
            return PartialView("GetActivityPages", pages);
        }
        //ActicityList中Ajax調用這個方法獲得按照地區搜索的活動
        public ActionResult GetActivityPagesByDistrictId(int districtId, int currentPage = 1, string orderMode = "scoredown", int minPrice = 0, int maxPrice = int.MaxValue, int priceFilter = 0)
        {
            currentPage = currentPage < 1 ? 1 : currentPage;
            int pageSize = 6;
            //ViewBag紀錄搜索模式與參數
            ViewBag.ActionName = "GetActivityPagesByDistrictId";
            ViewBag.DistrictId = districtId;
            ViewBag.OrderMode = orderMode;
            //這裡districtID實際上有存cityID,當小於0的時候是cityId的相反數,大於0的時候是districtId
            List<Activity> activities = null;
            if (districtId < 0)
            {
                activities = db.Activity.Where(a => a.ActivityStatusID == 1 && a.Districts.CityId == districtId * -1).ToList();
            }
            else
            {
                activities = db.Activity.Where(a => a.ActivityStatusID == 1 && a.DistrictId == districtId).ToList();
            }
            if (priceFilter == 1)
            {
                ViewBag.PriceFilter = 1;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                activities = activities.Where(a => a.Tickets.Count() != 0 && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) >= minPrice && a.Tickets.Min(t => Math.Round(t.Price * (1 - t.Discount), 0)) <= maxPrice).ToList();
            }
            activities = OrderActivitiesByOrderMode(activities, orderMode);
            IPagedList<Activity> pages = activities.ToPagedList(currentPage, pageSize);
            if (pages.Count() == 0)
            {
                //todo回傳找不到產品的頁面
                return PartialView("ZeroActivity");
            }
            return PartialView("GetActivityPages", pages);
        }
        //根據OrderMode排序Activity的方法
        public List<Activity> OrderActivitiesByOrderMode(List<Activity> activities, string orderMode)
        {
            //如果沒有價格或者沒有評論,就排最後
            switch (orderMode)
            {
                case "scoreup":
                    return activities.OrderBy(a => { if (a.Comment.Count() == 0) return 6; else return a.Comment.Average(c => c.CommentScore); }).ToList();
                case "scoredown":
                    return activities.OrderByDescending(a => { if (a.Comment.Count() == 0) return -1; else return a.Comment.Average(c => c.CommentScore); }).ToList();
                case "priceup":
                    return activities.OrderBy(a => { if (a.Tickets.Count() == 0) return int.MaxValue; else return a.Tickets.Min(t => t.Price * (1 - t.Discount)); }).ToList();
                case "pricedown":
                    return activities.OrderByDescending(a => { if (a.Tickets.Count() == 0) return -1; else return a.Tickets.Min(t => t.Price * (1 - t.Discount)); }).ToList();
                default:
                    goto case "scoredown";
            }
        }
        //增加子類別被搜索次數的函數
        public void AddSubCategorySearchedTime(int subCategoryId)
        {
            SubCategories subCategory = db.SubCategories.FirstOrDefault(sc => sc.SubCategoryId == subCategoryId);
            if (subCategory != null)
            {
                subCategory.SearchedTime++;
                db.SaveChanges();
            }
        }
        //在ActivityList頁面中Ajax調用這個方法取得活動的次詳細分頁
        public ActionResult GetActivitySubDetailPage(int activityId)
        {
            Activity activity = db.Activity.Where(a => a.ActivityStatusID == 1).FirstOrDefault(a => a.ActivityID == activityId);
            if (activity == null)
            {
                //todo 回傳活動是空值的錯誤頁面
                return PartialView("ZeroActivity");
            }
            return PartialView("GetActivitySubDetailPage", activity);
        }
        //在GetActivitySubDetailPage中Ajax調用這個方法獲得評論分頁
        public ActionResult GetActivityCommentPage(int activityId, int currentPage = 1)
        {
            currentPage = currentPage < 1 ? 1 : currentPage;
            int pageSize = 4;
            List<Comment> comments = db.Comment.Where(c => c.CommentActivityId == activityId).OrderByDescending(c => c.CommentDate).ToList();
            IPagedList<Comment> pages = comments.ToPagedList(currentPage, pageSize);
            ViewBag.ActivityId = activityId;
            return PartialView("GetActivityCommentPage", pages);
        }
        //GetActivitySubDetailPage中Ajax調用上傳評論的方法
        [HttpPost]
        public string AddActivityComment(int activityId, string content, int starCount)
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
            {
                return "必須先登錄";
            }
            if (string.IsNullOrEmpty(content.Trim())) return "不准上傳空值";
            Comment comment = new Comment()
            {
                CommentMemberID = (Session[CDictionary.SK_Logined_Member] as Member).MemberID,
                CommentActivityId = activityId,
                CommentContent = content.Trim(),
                CommentDate = DateTime.Now,
                CommentScore = starCount
            };
            db.Comment.Add(comment);
            try
            {
                db.SaveChanges();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //ActivityDetail的View中實時得到票價的Ajax調用的方法
        public string getTicketPrice(int ticketCategoryId, int ticketTimeId)
        {
            string result1 = "";
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketCategoryId == ticketCategoryId && t.TicketTimeId == ticketTimeId);
            if (ticket == null)
            {
                result1= "暫無提供";
            }
            else if (ticket.UnitsInStock == 0)
            {
                result1= "已售完";
            }
            else
            {
                result1 = JsonConvert.SerializeObject(new { ticket.Price, ticket.Discount, ticket.UnitsInStock });
            }
            string result2 = getTicketTable(ticketCategoryId);
            List<string> result = new List<string>() { result1, result2 };
            return JsonConvert.SerializeObject(result);
        }
        //獲取場次表的函數
        public string getTicketTable(int ticketCategoryId)
        {
            var tickets = db.TicketCategory.FirstOrDefault(tc => tc.TicketCategoryId == ticketCategoryId).Activity.TicketTimes.
                Select(tt =>
                {
                    Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketCategoryId == ticketCategoryId && t.TicketTimeId == tt.TicketTimeId);
                    if (ticket == null)
                    {
                        return new { Time = tt.TicketTime.ToString("yyyy/MM/dd HH:mm"), Price = "暫無提供", UnitsInStock = "暫無提供" };
                    }
                    else
                    {
                        return new { Time = tt.TicketTime.ToString("yyyy/MM/dd HH:mm"), Price = Math.Round(ticket.Price * (1 - ticket.Discount), 0).ToString(), UnitsInStock=ticket.UnitsInStock.ToString() };
                    }
                }).ToList();
            return JsonConvert.SerializeObject(tickets);
        }
        //ActivityList中Ajax調用獲得城市資訊的函數
        public string getAllCity()
        {
            var cities = db.Cities.Select(c => new { c.CityID, c.CityName }).ToList();
            return JsonConvert.SerializeObject(cities);
        }
        public string getDistrictsByCityId(int cityId)
        {
            var districts = db.Districts.Where(d => d.CityId == cityId).Select(d => new { d.DistrictId, d.DistrictName }).ToList();
            return JsonConvert.SerializeObject(districts);
        }
        //ActivityList中Ajax調用獲得活動類別資訊的函數
        public string getAllCategory()
        {
            var categories = db.Categories.Select(c => new { c.CategoryID, c.CategoryName }).ToList();
            return JsonConvert.SerializeObject(categories);
        }
        public string getSubcategoriesByCategoryId(int categoryId)
        {
            var subCategories = db.SubCategories.Where(sc => sc.CategoryId == categoryId).Select(sc => new { sc.SubCategoryId, sc.SubCategoryName }).ToList();
            return JsonConvert.SerializeObject(subCategories);
        }
        //Ajax調用根據區來獲得郵遞區號
        public string getPostCodeByDistrictId(int districtId)
        {
            return db.Districts.FirstOrDefault(d => d.DistrictId == districtId).PostCode;
        }
        //ActivityDetail加入購物車Ajax調用的方法
        public string AddToShoppingCart(int activityId, int ticketCategoryId, int ticketTimeId)
        {
            //加入購物車之前强迫他登錄
            if (Session[CDictionary.SK_Logined_Member] == null)
            {
                return "toLogin";
            }
            //加入購物車前判斷產品是否被下架
            if (db.Activity.Where(a => a.ActivityStatusID == 1).FirstOrDefault(a => a.ActivityID == activityId) == null)
            {
                return "activityOver";
            }
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.ActivityID == activityId && t.TicketCategoryId == ticketCategoryId && t.TicketTimeId == ticketTimeId);
            //判斷票是否存在與庫存量
            if (ticket == null)
            {
                return "noSupplly";
            }
            else if (ticket.UnitsInStock == 0)
            {
                return "noUnitsInStock";
            }
            int ticketId = ticket.TicketID;
            List<CShoppingCart> shoppingCart = Session[CDictionary.ShoppingCart] as List<CShoppingCart>;
            //如果還沒有購物車就new一個給他
            if (shoppingCart == null)
            {
                shoppingCart = new List<CShoppingCart>();
                shoppingCart.Add(new CShoppingCart() { TicketID = ticketId, Quantity = 1 });
                Session[CDictionary.ShoppingCart] = shoppingCart;
            }
            else
            {
                CShoppingCart sameItem = shoppingCart.FirstOrDefault(sc => sc.TicketID == ticketId);
                if (sameItem == null)
                {
                    shoppingCart.Add(new CShoppingCart() { TicketID = ticketId, Quantity = 1 });
                }
                else
                {
                    //如果他要加的產品已經存在購物車裏面就幫他數量+1
                    //但是如果加超過庫存量,就幫他更新成產品的最大庫存量
                    //如果大於99個就不能加
                    int count = sameItem.Quantity;
                    if (count + 1 > ticket.UnitsInStock)
                    {
                        sameItem.Quantity = ticket.UnitsInStock;
                        return "overCount";
                    }
                    else if (count + 1 > 99)
                    {
                        return "overMaxCount";
                    }
                    else
                    {
                        sameItem.Quantity = count + 1;
                    }
                }
                Session[CDictionary.ShoppingCart] = shoppingCart;
            }
            //從ticketId轉成可以顯示的資料傳回前端
            return ToShoppingCartJson(shoppingCart);
        }
        //Ajax請求獲得購物車資料的方法
        public string GetShoppingCart()
        {
            List<CShoppingCart> shoppingCart = Session[CDictionary.ShoppingCart] as List<CShoppingCart>;
            if (shoppingCart == null || shoppingCart.Count() == 0)
            {
                return "noShoppingCart";
            }
            else
            {
                return ToShoppingCartJson(shoppingCart);
            }
        }
        //購物車Session資料轉換詳細資料的函數
        public string ToShoppingCartJson(List<CShoppingCart> shoppingCart)
        {
            var cartItems = shoppingCart.Select(sc =>
            {
                Tickets scticket = db.Tickets.FirstOrDefault(t => t.TicketID == sc.TicketID);
                return new
                {
                    scticket.Activity.ActivityName,
                    scticket.TicketCategory.TicketCategoryName,
                    Time = scticket.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm"),
                    Price = Math.Round(scticket.Price * (1 - scticket.Discount), 0),
                    sc.Quantity,
                    sc.TicketID,
                    scticket.Activity.Picture
                };
            });
            return JsonConvert.SerializeObject(cartItems);
        }
        //Ajax調用刪除購物車條目的方法
        public string DeleteShoppingCartItem(int ticketId)
        {
            List<CShoppingCart> shoppingCart = Session[CDictionary.ShoppingCart] as List<CShoppingCart>;
            if (shoppingCart == null)
            {
                return "noShoppingCart";
            }
            CShoppingCart deleteItem = shoppingCart.FirstOrDefault(sc => sc.TicketID == ticketId);
            if (deleteItem != null)
            {
                shoppingCart.Remove(deleteItem);
            }
            //remove完之後如果數量是0,就告訴前端,讓前端顯示空購物車的畫面
            if (shoppingCart.Count() == 0)
            {
                return "noShoppingCart";
            }
            else
            {
                return ToShoppingCartJson(shoppingCart);
            }
        }
        //購物車列表頁加號按下去Ajax會調用的函數
        public string ShoppingCartItemCountPlus(int ticketId)
        {
            List<CShoppingCart> shoppingCart = Session[CDictionary.ShoppingCart] as List<CShoppingCart>;
            if (shoppingCart == null)
            {
                return "noShoppingCart";
            }
            CShoppingCart plusCountItem = shoppingCart.FirstOrDefault(sc => sc.TicketID == ticketId);
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketID == ticketId);
            if (ticket.UnitsInStock == 0)
            {
                shoppingCart.Remove(plusCountItem);
                return "outOfStock";
            }
            int count = plusCountItem.Quantity;
            if (count + 1 > 99)
            {
                return "overEdge";
            }
            else if (count + 1 > ticket.UnitsInStock)
            {
                //如果加之後數量大於最大庫存數量,就改成最大庫存數量
                plusCountItem.Quantity = ticket.UnitsInStock;
                Session[CDictionary.ShoppingCart] = shoppingCart;
                return "overCount";
            }
            else
            {
                plusCountItem.Quantity = count + 1;
                Session[CDictionary.ShoppingCart] = shoppingCart;
                return ToShoppingCartJson(shoppingCart);
            }
        }
        //購物車列表頁減號按下去Ajax會調用的函數
        public string ShoppingCartItemCountMinus(int ticketId)
        {
            List<CShoppingCart> shoppingCart = Session[CDictionary.ShoppingCart] as List<CShoppingCart>;
            if (shoppingCart == null)
            {
                return "noShoppingCart";
            }
            CShoppingCart minusCountItem = shoppingCart.FirstOrDefault(sc => sc.TicketID == ticketId);
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketID == ticketId);
            if (ticket.UnitsInStock == 0)
            {
                shoppingCart.Remove(minusCountItem);
                return "outOfStock";
            }
            int count = minusCountItem.Quantity;
            if (count - 1 < 1)
            {
                return "overEdge";
            }
            else if (count - 1 > ticket.UnitsInStock)
            {
                //如果減完之後數量大於最大數量(數量被賣家或管理員臨時砍掉的情況),就改成最大數量
                minusCountItem.Quantity = ticket.UnitsInStock;
                Session[CDictionary.ShoppingCart] = shoppingCart;
                return "overCount";
            }
            else
            {
                minusCountItem.Quantity = count - 1;
                Session[CDictionary.ShoppingCart] = shoppingCart;
                return ToShoppingCartJson(shoppingCart);
            }
        }
        //Ajax調用獲得QRCode的方法
        public string ShowQRCode(int orderDetailId)
        {
            List<string> qrCodes = db.TicketQRCodes.Where(qr => qr.OrderDetailId == orderDetailId).Select(qr => qr.QRCode).ToList();
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = new QrCodeEncodingOptions() { DisableECI = true, CharacterSet = "UTF-8", Height = 200, Width = 200, Margin = 1 };
            List<string> qrCodesBase64 = new List<string>();
            foreach (string qrCodeStr in qrCodes)
            {
                var qrCode = writer.Write(qrCodeStr);
                MemoryStream ms = new MemoryStream();
                qrCode.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] data = ms.GetBuffer();
                qrCodesBase64.Add(Convert.ToBase64String(data));
            }
            Order_Detail orderDetail = db.Order_Detail.FirstOrDefault(od => od.OrderDetailID == orderDetailId);
            return JsonConvert.SerializeObject(new
            {
                orderDetail.Tickets.Activity.ActivityName,
                orderDetail.Tickets.TicketCategory.TicketCategoryName,
                Time = orderDetail.Tickets.TicketTimes.TicketTime.ToString("yyyy/MM/dd HH:mm:ss"),
                QRCodes = qrCodesBase64
            });
        }
        //Ajax調用檢查活動名稱是否有重複的方法
        public string CheckActivityName(string activityName)
        {
            Activity activity = db.Activity.FirstOrDefault(a => a.ActivityName == activityName);
            if (activity == null)
            {
                return "OK";
            }
            else
            {
                return "NotAvailible";
            }
        }
        //Ajax調用上傳活動資訊的方法
        [ValidateInput(false)]   //不寫這句話無法傳送html標簽
        public string AddActivity(CActivity input)
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
            {
                return "loginOverTime";
            }
            if (CheckActivityName(input.activityName) != "OK")
            {
                return "sameActivityName";
            }
            //todo ActiivityStatusId要調回0
            int sellerId = db.Seller.AsEnumerable().FirstOrDefault(s => s.MemberId == (Session[CDictionary.SK_Logined_Member] as Member).MemberID).SellerID;
            string pictureName = Guid.NewGuid().ToString() + ".png";
            Activity activity = new Activity()
            {
                ActivityName = input.activityName,
                SellerID = sellerId,
                Description = input.description,
                Address = input.address,
                Picture = pictureName,
                Information = input.information,
                SubCategoryId = input.subcategoryId,
                Hostwords = input.hostwords,
                Map = input.map,
                ActivityStatusID = 0,
                DistrictId = input.districtId
            };
            db.Activity.Add(activity);
            foreach (string ticketCategoryName in input.ticketCategories)
            {
                TicketCategory tc = new TicketCategory()
                {
                    TicketCategoryName = ticketCategoryName,
                    ActivityId = activity.ActivityID
                };
                db.TicketCategory.Add(tc);
            }
            foreach (string ticketTime in input.times)
            {
                TicketTimes tt = new TicketTimes()
                {
                    TicketTime = Convert.ToDateTime(ticketTime),
                    ActivityId = activity.ActivityID
                };
                db.TicketTimes.Add(tt);
            }
            try
            {
                string pictureCode = input.picture.Substring(input.picture.IndexOf(",") + 1);  //把"data:image/png;base64,"這部分捨棄,取逗號後面的值
                byte[] imgData = Convert.FromBase64String(pictureCode);
                MemoryStream ms = new MemoryStream(imgData);
                Bitmap bitmap = new Bitmap(ms);
                bitmap.Save(Server.MapPath("~/images/Activity/" + pictureName), System.Drawing.Imaging.ImageFormat.Png);
                db.SaveChanges();
                return activity.ActivityID.ToString();
            }
            catch (Exception ex)
            {
                return "error";
            }
        }
        //Ajax呼叫修改活動的方法
        [ValidateInput(false)]   //不寫這句話無法傳送html標簽
        public string UpdateActivity(CActivity input)
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
            {
                return "loginOverTime";
            }
            Activity activityNow = db.Activity.FirstOrDefault(a => a.ActivityID == input.activityId);
            if (input.activityName!=activityNow.ActivityName&&CheckActivityName(input.activityName) != "OK")
            {
                return "sameActivityName";
            }          
            activityNow.ActivityName = input.activityName;
            activityNow.Description = input.description;
            activityNow.Address = input.address;
            activityNow.Information = input.information;
            activityNow.SubCategoryId = input.subcategoryId;
            activityNow.Hostwords = input.hostwords;
            activityNow.Map = input.map;
            activityNow.ActivityStatusID = 0;
            activityNow.DistrictId = input.districtId;
            try
            {
                if (input.picture != null)
                {
                    string pictureName = activityNow.Picture;
                    string pictureCode = input.picture.Substring(input.picture.IndexOf(",") + 1);  //把"data:image/png;base64,"這部分捨棄,取逗號後面的值
                    byte[] imgData = Convert.FromBase64String(pictureCode);
                    MemoryStream ms = new MemoryStream(imgData);
                    Bitmap bitmap = new Bitmap(ms);
                    bitmap.Save(Server.MapPath("~/images/Activity/" + pictureName), System.Drawing.Imaging.ImageFormat.Png);
                }               
                db.SaveChanges();
                return "OK";
            }
            catch (Exception ex)
            {
                return "error";
            }
        }
        //添加票的庫存量,價格,折扣的頁面中Ajax調用這個方法獲得表格資料
        public string getAllTicketInfo(int activityId)
        {
            Activity activity = db.Activity.FirstOrDefault(a => a.ActivityID == activityId);
            var ticketsData = new
            {
                TicketCategoty = activity.TicketCategory.Select(tc => new { tc.TicketCategoryId, tc.TicketCategoryName }),
                TicketTime = activity.TicketTimes.Select(tt => new { tt.TicketTimeId, Time = tt.TicketTime.ToString("yyyy/MM/dd HH:mm") }),
                Tickets = activity.Tickets.Select(t => new { t.TicketID, t.TicketCategoryId, t.TicketTimeId, t.Price, t.UnitsInStock, t.Discount })
            };
            return JsonConvert.SerializeObject(ticketsData);
        }
        //同上頁面中Ajax調用設定庫存量,價格,折扣的方法
        public string SetTicketProperties(Tickets input)
        {
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketCategoryId == input.TicketCategoryId && t.TicketTimeId == input.TicketTimeId);
            if (ticket == null)
            {
                db.Tickets.Add(input);
            }
            else
            {
                ticket.Price = input.Price;
                ticket.UnitsInStock = input.UnitsInStock;
                ticket.Discount = input.Discount;
            }
            try
            {
                db.SaveChanges();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //同上頁面中Ajax調用刪除票的方法
        public string deleteTicket(int ticketCategoryId, int ticketTimeId)
        {
            Tickets ticket = db.Tickets.FirstOrDefault(t => t.TicketCategoryId == ticketCategoryId && t.TicketTimeId == ticketTimeId);
            if (ticket != null)
            {
                db.Tickets.Remove(ticket);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return "已經存在訂單,無法刪除";
                }
            }
            return "OK";
        }
        //Ajax叫用加入收藏的方法,如果已經收藏就移除收藏
        public string addToFavourite(int activityId)
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
            {
                return "noLogin";
            }
            int memberId = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            ActivityFavourite activityFavourite = db.ActivityFavourite.FirstOrDefault(af => af.MemberId == memberId && af.ActivityId == activityId);
            if (activityFavourite == null)
            {
                activityFavourite = new ActivityFavourite()
                {
                    MemberId = memberId,
                    ActivityId = activityId
                };
                db.ActivityFavourite.Add(activityFavourite);
                db.SaveChanges();
                return "addOK";
            }
            else
            {
                db.ActivityFavourite.Remove(activityFavourite);
                db.SaveChanges();
                return "removeOK";
            }
        }
        public void SendFinishPayEmailToMember(string emailAddress, string orderGuid)
        {
            //製造email內文的部分
            string email = "<table style='border:1px solid black;width:80%;border-collapse:collapse;margin-left:auto;margin-right:auto'><tr>";
            //設定qrcode產生器
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = new QrCodeEncodingOptions() { DisableECI = true, CharacterSet = "UTF-8", Height = 200, Width = 200, Margin = 1 };
            //循環每一個orderDetail準備產生qrCode塞入Email
            Orders order = db.Orders.FirstOrDefault(o => o.OrderGuid == orderGuid);
            //紀錄資料放在表格中的列,一行滿3列就換到下一行
            int colCount = 0;
            MailMessage mail = new MailMessage();
            foreach (Order_Detail orderDetail in order.Order_Detail)
            {
                string activityName = orderDetail.Tickets.Activity.ActivityName;
                string ticketCategoryName = orderDetail.Tickets.TicketCategory.TicketCategoryName;
                string ticketTime = orderDetail.Tickets.TicketTimes.TicketTime.ToString("yyyy-MM-dd HH:mm");
                foreach (TicketQRCodes ticketQRCode in orderDetail.TicketQRCodes)
                {
                    string qrCodeContent = ticketQRCode.QRCode;
                    var qrCode = writer.Write(qrCodeContent);
                    MemoryStream ms = new MemoryStream();
                    qrCode.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    ms.Seek(0L, SeekOrigin.Begin);
                    string codeName = Guid.NewGuid().ToString();
                    //內嵌qrCode圖片到email供使用者查看下載
                    Attachment attachment = new Attachment(ms, $"{activityName}_{ticketCategoryName}_{ticketTime}");
                    attachment.Name = $"{activityName}_{ticketCategoryName}_{ticketTime}";
                    attachment.ContentId = codeName;
                    attachment.NameEncoding = Encoding.GetEncoding("utf-8");
                    attachment.ContentType = new ContentType(MediaTypeNames.Image.Jpeg);
                    // 設定該附件為一個內嵌附件(Inline Attachment)
                    attachment.ContentDisposition.Inline = true;
                    attachment.ContentDisposition.DispositionType = System.Net.Mime.DispositionTypeNames.Inline;
                    mail.Attachments.Add(attachment);
                    string detailCard = "<td style='border:1px solid black;padding:10px;text-align:center;background-color:white'>" +
                       $"<h2>{activityName}</h2>" +
                       $"<p>{ticketCategoryName} {ticketTime}</p>" +
                       $"<img src='cid:{attachment.ContentId}'/>" +
                       "</td>";
                    email += detailCard;
                    colCount++;
                    colCount %= 3;
                    if (colCount == 0)
                        email += "</tr><tr>";
                }
            }
            email += "</tr></table>";
            ////從所有活動找出推薦的活動給他
            //當前order的活動
            List<int> activityIds = order.Order_Detail.Select(od => od.Tickets.ActivityID).ToList();
            //找出所有order中有買上面找出來的活動的order,並整合成orderDetail的集合
            List<Orders> linkOrders = db.Orders.Where(o => o.Order_Detail.Any(od => activityIds.Contains(od.Tickets.ActivityID))).ToList();
            List<Order_Detail> linkOrderDetails = new List<Order_Detail>();
            foreach(Orders linkOrder in linkOrders)
            {
                foreach(Order_Detail order_Detail in linkOrder.Order_Detail)
                {
                    linkOrderDetails.Add(order_Detail);
                }
            }
            Activity[] activities = linkOrderDetails.Where(od=>!activityIds.Contains(od.Tickets.Activity.ActivityID)).GroupBy(lod => lod.Tickets.Activity).OrderByDescending(g => g.Count()).Select(g => g.Key).Where(a=>a.ActivityStatusID==1).Take(3).ToArray();
            if (activities.Length != 0)
            {
                email += "<h2 style='color:red;text-align:center;margin-top:50px'>推薦活動</h2>";
                email += "<ul style='width:80%;border:1px solid black;margin:20px auto;list-style:none;padding-left:0'>";
                foreach (Activity activity in activities)
                {
                    string codeName = Guid.NewGuid().ToString();
                    //內嵌圖片到email供使用者查看
                    string filePath = Server.MapPath("~/images/Activity/" + Server.UrlEncode(activity.Picture));
                    Attachment attachment = new Attachment(filePath);
                    attachment.Name = $"{activity.ActivityName}";
                    attachment.ContentId = codeName;
                    attachment.NameEncoding = Encoding.GetEncoding("utf-8");
                    attachment.ContentType = new ContentType(MediaTypeNames.Image.Jpeg);
                    // 設定該附件為一個內嵌附件(Inline Attachment)
                    attachment.ContentDisposition.Inline = true;
                    attachment.ContentDisposition.DispositionType = System.Net.Mime.DispositionTypeNames.Inline;
                    mail.Attachments.Add(attachment);
                    string li = "<li style='margin-left:0;border:1px solid black;overflow:auto;padding:10px'>" +
                   $"<img style='width:50%;float:left;margin-right:20px' src='cid:{codeName/*Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8)) + Url.Content("~/images/Activity/" + activity.Picture)*/}'>" +
                    "<div>" +
                        $"<h2><a href='{Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf("/", 8))+Url.Action("ActivityDetail",new { activityId=activity.ActivityID})}'>{activity.ActivityName}</a></h2>" +
                           $"<p style='text-align:justify'>{activity.Description}" +
                            "</p>" +
                        "</div>" +
                    "</li>";
                    email += li;
                }
                email += "</ul>";
            }
            //發送email的部分
            string userName = "apikey";
            string password = "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";
            try
            {
                mail.From = new MailAddress("iticket128@gmail.com");
                mail.Subject = "iTicket訂單成功,查看電子票QRCode";
                mail.Body = $@"<h1 style='text-align:center;color:#ff0000'>iTicket 訂購成功</h1>{email}";
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;
                mail.To.Add(emailAddress);
                using (SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net"))
                {
                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new NetworkCredential(userName, password);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                }
            }
            catch (Exception ex)
            {

            }


            //try
            //{
            //    mail.From = new MailAddress("iticket128@gmail.com");
            //    mail.Subject = "iTicket訂單成功,查看電子票QRCode";
            //    mail.Body = $@"<h1 style='text-align:center;color:#ff0000'>iTicket 訂購成功</h1>{email}";
            //    mail.IsBodyHtml = true;
            //    mail.Priority = MailPriority.High;
            //    mail.To.Add(emailAddress);
            //    using (SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com"))
            //    {
            //        SmtpServer.Port = 587;
            //        SmtpServer.Credentials = new NetworkCredential("iticket128@gmail.com", "!@#qweasd");
            //        SmtpServer.EnableSsl = true;
            //        SmtpServer.Send(mail);
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }
        //前端調用驗證套票名稱是否可用
        public string CheckTicketGroupNameAvailible(string ticketGroupName)
        {
            return db.TicketGroups.Any(tg => tg.TicketGroupName == ticketGroupName) ? "false" : "true";
        }
        //把套票資料塞入db的函數
        public string AddTicketGroup(string ticketGroupName,decimal discount,int[] activityIds)
        {
            //todo正式版Status調成false供後台審核
            TicketGroups ticketGroup = new TicketGroups()
            {
                TicketGroupName = ticketGroupName,
                TicketGroupDiscount = discount,
                Status = false
            };
            db.TicketGroups.Add(ticketGroup);
            foreach(int activityId in activityIds)
            {
                TicketGroupDetail ticketGroupDetail = new TicketGroupDetail()
                {
                    TicketGroupId = ticketGroup.TicketGroupId,
                    ActivityId = activityId
                };
                db.TicketGroupDetail.Add(ticketGroupDetail);
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "OK";
        }
        public ActionResult CustomerSupport()
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            if (member == null || member.MemberRoleId != 3) return RedirectToAction("Login", "Login");
            return View();
        }
        public void ReportComment(int commentId,int memberId,string reason)
        {
            CommentReport report = new CommentReport()
            {
                CommentId = commentId,
                MemberId = memberId,
                ReportReason = reason,
            };
            db.CommentReport.Add(report);
            db.SaveChanges();
        }
        public bool CheckRepeatReport(int commentId, int memberId)
        {
            return db.CommentReport.Any(cr => cr.CommentId == commentId && cr.MemberId == memberId);
        }
        //上架/下架活動
        public string UpDownActivity(int activityId)
        {
            Activity activity = db.Activity.FirstOrDefault(a => a.ActivityID == activityId);
            if (activity.ActivityStatusID == 1)
            {
                activity.ActivityStatusID = 2;
            }
            else if (activity.ActivityStatusID == 2)
            {
                activity.ActivityStatusID = 0;
            }
            db.SaveChanges();
            return activity.ActivityStatusID.ToString();
        }
        //刪除套票
        public ActionResult DeleteTicketGroup(int groupId)
        {
            TicketGroups ticketGroups = db.TicketGroups.FirstOrDefault(tg => tg.TicketGroupId == groupId);
            db.TicketGroupDetail.RemoveRange(ticketGroups.TicketGroupDetail);
            db.TicketGroups.Remove(ticketGroups);
            db.SaveChanges();
            return RedirectToAction("PackageCenterM", "SellerCenter");
        }
        //產生修改套票的頁面
        public ActionResult UpDateTicketGroup(int groupId)
        {
            //商家後臺修改套票連到這裡
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            if (member == null || member.MemberRoleId != 3)
            {
                return RedirectToAction("Login", "Login");
            }
            List<Activity> activities = member.Seller.FirstOrDefault().Activity.Where(a => a.ActivityStatusID == 1).ToList();
            TicketGroups ticketGroup = db.TicketGroups.FirstOrDefault(tg => tg.TicketGroupId == groupId);
            VMUpdateTicketGroup vm = new VMUpdateTicketGroup()
            {
                Activities = activities,
                TicketGroup = ticketGroup
            };
            return View(vm);
        }
        //修改套票的程式碼
        public string MofifyTicketGroup(string ticketGroupName, decimal discount, int[] activityIds,int ticketGroupId)
        {
            //todo正式版Status調成false供後台審核
            TicketGroups ticketGroup = db.TicketGroups.FirstOrDefault(tg => tg.TicketGroupId == ticketGroupId);
            ticketGroup.TicketGroupName = ticketGroupName;
            ticketGroup.TicketGroupDiscount = discount;
            ticketGroup.Status = false;
            List<TicketGroupDetail> oldDetail = db.TicketGroupDetail.Where(tgd => tgd.TicketGroupId == ticketGroupId).ToList();
            db.TicketGroupDetail.RemoveRange(oldDetail);
            foreach (int activityId in activityIds)
            {
                TicketGroupDetail ticketGroupDetail = new TicketGroupDetail()
                {
                    TicketGroupId = ticketGroup.TicketGroupId,
                    ActivityId = activityId
                };
                db.TicketGroupDetail.Add(ticketGroupDetail);
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "OK";
        }
    }
}
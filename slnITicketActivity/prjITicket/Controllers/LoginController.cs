using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PagedList;
using prjITicket.Models;
using prjITicket.Service;
using prjITicket.ViewModel;



namespace prjITicket.Controllers
{
    public class LoginController : Controller,IDisposable
    {
        // GET: Login
        TicketSysEntities db = new TicketSysEntities();
        //實體化MailService，以引用內部方法進行Email驗證
        MailService mailService = new MailService();
        #region 第三方登入
        //第三方登入=======
        public string FBLogin(string returnUrl)
        {
            facebook ms = JsonConvert.DeserializeObject<facebook>(returnUrl);

            var member = db.Member.Where(x => x.Email == ms.email).FirstOrDefault();
            if (member == null)
            {
                Member m = new Member();
                m.Email = ms.email;
                m.Name = ms.name;
                m.NickName = ms.name;
                m.MemberRoleId = 2;
                m.Point = 0;
                m.fRegister_time = DateTime.Now;
                m.providerFB = true;
                db.Member.Add(m);
                db.SaveChanges();
               var Nmember = db.Member.Where(x => x.Email == ms.email).FirstOrDefault();
                Session[CDictionary.SK_Logined_Member] = Nmember;
                
                (Session[CDictionary.SK_Logined_Member] as Member).provider = "facebook";
               
            }
            else
            {
                if (member.providerFB !=true)
                {
                    member.providerFB = true;
                    db.SaveChanges();
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "facebook";
                }
                else 
                {
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "facebook";
                }
               
            }


            return "驗證成功";//RedirectToAction("ActivityList", "Activity");

        }

        public string GoLogin(string returnUrl)
        {
            google ms = JsonConvert.DeserializeObject<google>(returnUrl);

            var member = db.Member.Where(x => x.Email == ms.du).FirstOrDefault();
            if (member == null)
            {
                Member m = new Member();
                m.Email = ms.du;
                m.Name = ms.Ad;
                m.NickName = ms.Ad;
                m.MemberRoleId = 2;
                m.Point = 0;
                m.providerGO = true;
                m.fRegister_time = DateTime.Now;
                db.Member.Add(m);
                db.SaveChanges();
                member = db.Member.Where(x => x.Email == ms.du).FirstOrDefault();
                Session[CDictionary.SK_Logined_Member] = member;
                
                (Session[CDictionary.SK_Logined_Member] as Member).provider = "google";
            }
            else
            {
                if (member.providerGO != true)
                {
                    member.providerGO = true;
                    db.SaveChanges();
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "google";
                }
                else
                {
                    Session[CDictionary.SK_Logined_Member] = member;
                    (Session[CDictionary.SK_Logined_Member] as Member).provider = "google";
                }

            }


            return "驗證成功";//RedirectToAction("ActivityList", "Activity");

        }
        //第三方登入End=======
        #endregion

        #region 登入與登出
        //登入
        public ActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult Login(string Email, string Password,FormCollection form)
        {
            
            var isVerify = new GoogleReCaptcha().GetCaptchaResponse(form["g-recaptcha-response"]);
            if (isVerify)
            {
                var member = db.Member
                    .Where(m => m.Email == Email && m.Password == Password)
                    .FirstOrDefault();
                //var banmember = db.BanLIst.Where(x => x.BanMemberId == member.MemberID&&x.EndTime> DateTime.Now).FirstOrDefault();               
                //若member為null，表示會員未註冊
                if (member == null)
                {
                    ViewBag.Message = "帳密錯誤，登入失敗";
                    return View();
                }
                else if (member.MemberRoleId == 1)
                {
                    ViewBag.Message = "尚未進行信箱驗證，請至信箱進行驗證作業";
                    return View();
                }

                
                 Session[CDictionary.SK_Logined_Member] = member;
               
            }
            else {
                ViewBag.Message = "請勾選驗證機器人";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }
        //登出
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region 收取驗證信
        //接收驗證信連接密碼修改頁面
        //==================================================
        public ActionResult PasswordValidate(string UserName, string RegisterCheckCode)
        {
            Member member = db.Member.Where(m => m.Email == UserName).FirstOrDefault();
            //CMember s = new CMember() { entity = member };
            ViewBag.email = UserName;
            return View();
        }
        [HttpPost,ActionName("PasswordValidate")]
        public ActionResult PasswordValidatePost(string Password,string email)
        {
            
            Member member = db.Member.Where(m => m.Email == email).FirstOrDefault();
            member.Password = Password;
            db.SaveChanges();
            
            return RedirectToAction("Login","Login");
        }

        //接收驗證信連接傳進來的Action
        //==================================================
        public ActionResult EmailValidate(string UserName, string RegisterCheckCode)
        {
            Member member = db.Member.Where(m => m.Email == UserName).FirstOrDefault();
            UserName = member.Email;
            ViewBag.EmailValidate = mailService.EmailValidate(UserName, RegisterCheckCode);
            return View();
        }
        #endregion

        #region 忘記密碼
        public ActionResult Forget()
        {
            return View();
        }
        [HttpPost]
        public string Forget1(QMember formData)
        {
           var mem= db.Member.Where(x => x.Email == formData.Email).FirstOrDefault();
            if (mem == null) 
            {
                return "無此帳號";
            }
            else 
            {
                //取得信箱驗證碼
                //======================================
                string RegisterCheckCode = mailService.GetValidateCode();
                //取得寫好的驗證範本內容
                string TempMail = System.IO.File.ReadAllText(
                    Server.MapPath("~/Views/Shared/ForgetEmail.html"));
                //宣告驗證Email驗證用的Url
                UriBuilder ValidateUrl = new UriBuilder(Request.Url)
                {
                    Path = Url.Action("PasswordValidate", "Login",
                    new { UserName = formData.Email, RegisterCheckCode = RegisterCheckCode })
                };
                //藉由Service將使用者資料填入驗證信範本中
                string MailBody = mailService.GetRegisterMailBody(TempMail,
                    formData.Email, ValidateUrl.ToString().Replace("%3F", "?"));
                //呼叫Service寄出驗證信
                mailService.SendRegisterMail(MailBody, formData.Email);
            }
  
            return "成功";//RedirectToAction("Login", "Login");
        }

        #endregion

        #region 註冊會員與檢查email有無衝突(ajax)
        //註冊會員
        public ActionResult Register()
        {            
            return View();
        }

        [HttpPost]
        public ActionResult Register(QMember formData)
        {

            //若模型沒有通過驗證則顯示目前的View
            if (ModelState.IsValid == false)
            {
                ViewBag.Message = "請確認是否欄位輸入正確";
                return View();
            }
            else if (formData.agreeterm == false)
            {

                ViewBag.Message = "請勾選";
                return View();
            }
            else if (formData.Password == null)
            {

                ViewBag.Message = "請輸入密碼";
                return View();
            }

            // 依帳號取得會員並指定給member
            var member = db.Member
                .Where(m => m.Email == formData.Email)
                .FirstOrDefault();

            //取得信箱驗證碼
            //======================================
            string RegisterCheckCode = mailService.GetValidateCode();

            //若member為null，表示會員未註冊
            //======================================
            if (member == null)
            {
                Member m = new Member();
                m.Email = formData.Email;
                m.Password = formData.Password;
                m.Name = "Guest";
                m.NickName = "Guest";
                m.MemberRoleId = 1;
                m.Point = 0;
                m.RegisterCheckCode = RegisterCheckCode;
                m.fRegister_time = DateTime.Now;
                db.Member.Add(m);
                db.SaveChanges();
                //取得寫好的驗證範本內容
                string TempMail = System.IO.File.ReadAllText(
                    Server.MapPath("~/Views/Shared/RegisterEmailTemplate.html"));
                //宣告驗證Email驗證用的Url
                UriBuilder ValidateUrl = new UriBuilder(Request.Url)
                {
                    Path = Url.Action("EmailValidate", "Login",
                    new { UserName = formData.Email, RegisterCheckCode = RegisterCheckCode })
                };
                //藉由Service將使用者資料填入驗證信範本中
                string MailBody = mailService.GetRegisterMailBody(TempMail,
                    formData.Email, ValidateUrl.ToString().Replace("%3F", "?"));
                //呼叫Service寄出驗證信
                mailService.SendRegisterMail(MailBody, formData.Email);



                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號己有人使用，註冊失敗";
            return View();


        }

        public string IsHasMember(string Email)
        {
            var member = db.Member
                .Where(m => m.Email == Email)
                .FirstOrDefault();
            //[a-zA-Z0-9._%+-]+@[a-zA-z0-9.-]+\.[a-zA-Z]{2,}$
            if (!Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                return "請輸入email格式";
            }
            if (Email == "")
            {
                return "請輸入帳號";
            }
            else if (member == null)
            {
                return "\u2705 可以使用此帳號";
            }
            else
            {
                return "此帳號己有人使用";
            }

        }
        #endregion

        #region 企業註冊會員
        public ActionResult BussRegister()
        {
          
         return View();
          
        }
        [HttpPost]
        public ActionResult BussRegister(QBussMember formData, HttpPostedFileBase FileSave)
        {
            //若模型沒有通過驗證則顯示目前的View
            if (ModelState.IsValid == false)
            {
                return View();
            }
            else if (formData.Password == null)
            {

                ViewBag.Message = "請輸入密碼";
                return View();
            }
            else if (formData.agreeterm == false)
            {

                ViewBag.Message = "請勾選";
                return View();
            }
            
            else if (FileSave == null) 
            {
                ViewBag.Message = "請上傳檔案";
                return View();
            }

            // 依帳號取得會員並指定給member
            var bussmember = db.Member
                .Where(m => m.Email == formData.Email)
                .FirstOrDefault();

            //取得信箱驗證碼
            //======================================
            string RegisterCheckCode = mailService.GetValidateCode();


            //若member為null，表示會員未註冊
            if (bussmember == null)
            {
                
                Member m = new Member();
                m.Email = formData.Email;
                m.Password = formData.Password;
                m.Name = "Guest";
                m.NickName = "Guest";
                m.RegisterCheckCode = RegisterCheckCode;
                m.MemberRoleId = 1;
                m.Point = 0;
                m.fRegister_time = DateTime.Now;
                
                db.Member.Add(m);
                db.SaveChanges();

                Seller s = new Seller();
                s.MemberId = m.MemberID;
                s.SellerPhone = formData.SellerPhone;
                s.CompanyName = formData.CompanyName;
                s.TaxIDNumber = formData.TaxIDNumber;
                s.fFileName = $"abc{m.MemberID:D4}" + FileSave.FileName;
                db.Seller.Add(s);
                db.SaveChanges();
                FileSave.SaveAs(Server.MapPath("~/Content/Login/SellerImage") + "/" + $"abc{m.MemberID:D4}" + FileSave.FileName);
                //取得寫好的驗證範本內容
                string TempMail = System.IO.File.ReadAllText(
                    Server.MapPath("~/Views/Shared/RegisterEmailTemplate.html"));
                //宣告驗證Email驗證用的Url
                UriBuilder ValidateUrl = new UriBuilder(Request.Url)
                {
                    Path = Url.Action("EmailValidate", "Login",
                    new { UserName = formData.Email, RegisterCheckCode = RegisterCheckCode })
                };
                //藉由Service將使用者資料填入驗證信範本中
                string MailBody = mailService.GetRegisterMailBody(TempMail,
                    formData.Email, ValidateUrl.ToString().Replace("%3F", "?"));
                //呼叫Service寄出驗證信
                mailService.SendRegisterMail(MailBody, formData.Email);

                return RedirectToAction("Login");
            }

            ViewBag.Message = "此帳號己有人使用，註冊失敗";
            return View();
        }
        #endregion

        #region 提供企業下載範例檔案
        //提供下載檔案
        public ActionResult DemoDownload()
        {           
            //string path2 = Server.MapPath("~/Content/Login/DemoFile") + "\\" + "企業合同確認書.docx";
            //string path = @"F:\slnITicketActivity\prjITicket\Content\Login\DemoFile\企業合同確認書.docx";          
            //我要下載的檔案位置
            string filepath = Server.MapPath("~/Content/Login/DemoFile/企業合同確認書.docx");
            //取得檔案名稱
            string filename = System.IO.Path.GetFileName(filepath);
            //讀成串流
            Stream iStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //回傳出檔案
            return File(iStream, "application/msword", filename);
            //contentType The content type (MIME type)副檔名
        }
        #endregion

        #region (會員中心)一般會員與企業會員呈現目前個人資料
        public ActionResult MemberEdit(string mode = "")
        {
            if (Session[CDictionary.SK_Logined_Member] != null)
            {
                string email = (Session[CDictionary.SK_Logined_Member] as Member).Email;
                var member = db.Member.Where(x => x.Email == email).FirstOrDefault();
                member.Password = "";
                CMember c = new CMember { entity = member };
                ViewBag.Mode = mode;
                return View(c);
            }
            ViewBag.Mode = mode;
            return View();
        }


        public ActionResult BussEdit(string mode = "")
        {
            if (Session[CDictionary.SK_Logined_Member] != null)
            {
                string email = (Session[CDictionary.SK_Logined_Member] as Member).Email;
                int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
                var member = db.Member.Where(x => x.Email == email).FirstOrDefault();
                var seller = db.Seller.Where(x => x.MemberId == id).FirstOrDefault();
                member.Password = "";
                CBussMember buss = new CBussMember() { entity = member, BussEntity = seller };
                ViewBag.Mode = mode;
                return View(buss);
            }
            ViewBag.Mode = mode;
            return View();

        }
        #endregion

        #region 企業會員資料修改與審核不通過再次上傳檔案(前後端ajax)
        public string BussMemberSave(QBussMember b)
        {
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;

            Seller prod = db.Seller.FirstOrDefault(t => t.MemberId == id);
            if (prod != null)
            {
                if (b.CompanyName == null || b.TaxIDNumber == null || b.SellerPhone == null)
                {
                    return "必填欄位未填寫，修改失敗";
                }
                prod.CompanyName = b.CompanyName;
                prod.TaxIDNumber = b.TaxIDNumber;
                prod.SellerHomePage = b.SellerHomePage;
                prod.SellerDeccription = b.SellerDeccription;
                prod.SellerPhone = b.SellerPhone;
                db.SaveChanges();
                return "修改成功";
            }
            
            return "修改失敗";   

        }
        public string FileSave(HttpPostedFileBase FileSave)
        {
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            Seller prod = db.Seller.FirstOrDefault(t => t.MemberId == id);
            if (FileSave != null)
            {
                prod.fPass = null;
                prod.fFileName = FileSave.FileName;
                db.SaveChanges();
                FileSave.SaveAs(Server.MapPath("~/Content/Login/SellerImage") + "/" + FileSave.FileName);
                return "上傳成功";
            }
            return "上傳失敗";
        }
        public string UpdatePass()
        {
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            var seller=db.Seller.Where(x => x.MemberId == id).FirstOrDefault();
            if (seller.fPass == null)
            {
                return "審核中";
            }
            else if (seller.fPass == true)
            {
                return "審核通過";
            }
            return "審核失敗";
        }
        #endregion

        #region (ajax)會員資料修改與密碼修改
        public string MemberSave(QMember b)
        {          
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            Member prod = db.Member.FirstOrDefault(t => t.MemberID == id);
            //if (ModelState.IsValid == false)
            //{
            //    return "修改失敗";
            //}
            if (prod != null)
            {
                prod.Name = b.Name;
                prod.NickName = b.NickName;
                prod.Address = b.Address==null?null:b.Address;
                prod.BirthDate = b.BirthDate == null ? null : b.BirthDate;
                prod.IDentityNumber = b.IDentityNumber == null ? null : b.IDentityNumber;
                prod.Passport = b.Passport == null ? null : b.Passport;
                prod.Phone = b.Phone == null ? null : b.Phone;
                prod.DistrictId = b.DistrictId == null ? null : b.DistrictId;
                prod.Sex = b.Sex;//== null ? null : b.Sex;
                db.SaveChanges();
                (Session[CDictionary.SK_Logined_Member] as Member).Name = b.Name;
                return "修改成功";
            }
            return "修改失敗";

        }
        
        

        //todo 12/13
        public string MemberPassSave(QMember b)
        {
            int id = (Session[CDictionary.SK_Logined_Member] as Member).MemberID;
            string password = (Session[CDictionary.SK_Logined_Member] as Member).Password;
            Member prod = db.Member.FirstOrDefault(t => t.MemberID == id && t.Password == b.Password);
            if (prod != null && b.NPassword != null)
            {
                prod.Password = b.NPassword;
                db.SaveChanges();
                password = "";
                return "密碼修改成功，下次請用新密碼登入";
            }
            password = "";
            if (prod == null && b.NPassword == null)
            {
                return "請輸入密碼，修改失敗";
            }
            return "原密碼輸入錯誤，修改失敗";

        }
        #endregion


        #region 上傳會員照片
        //上傳會員照片
        //=====================================================
        public void CreateImage(string input)
        {
            //已登入
            if (Session[CDictionary.SK_Logined_Member] != null)
            {
                string email = (Session[CDictionary.SK_Logined_Member] as Member).Email;
                var member = db.Member.Where(m => m.Email == email).FirstOrDefault();
                input = input.Substring(input.IndexOf(",") + 1);  //把"data:image/png;base64,"這部分捨棄,取逗號後面的值 
                byte[] imgData = Convert.FromBase64String(input); //轉成2進位
                MemoryStream ms = new MemoryStream(imgData); //儲存資料流
                Bitmap bitmap = new Bitmap(ms);
                string photoName = Guid.NewGuid().ToString() + ".png"; //產生亂數檔名及副檔名
                bitmap.Save(Server.MapPath("~/images/Login/Upload/" + photoName), System.Drawing.Imaging.ImageFormat.Png);
                member.Icon = photoName; //儲存Icon
                db.SaveChanges();
                (Session[CDictionary.SK_Logined_Member] as Member).Icon = photoName;
            }
        }
        #endregion

        #region 載入會員修改頁面抓取City、District、PostCode欄位
        //載入所有城市至Cities資料庫
        //====================================================
        public void loadAllCities()
        {
            string[] cities = { "台北市", "基隆市", "新北市", "宜蘭縣", "桃園市", "新竹市", "新竹縣", "苗栗縣", "台中市", "彰化縣", "南投縣", "嘉義市", "嘉義縣", "雲林縣", "台南市", "高雄市", "澎湖縣", "金門縣", "屏東縣", "台東縣", "花蓮縣", "連江縣" };
            Cities city = new Cities();
            for (int i = 0; i < cities.Length; i++)
            {
                city.CityName = cities[i];
                db.Cities.Add(city);
                db.SaveChanges();
            }
        }
        //載入所有地區至Districts資料庫
        public void loadAllDistricts()
        {
            string[] cities = { "台北市", "基隆市", "新北市", "宜蘭縣", "桃園市", "新竹市", "新竹縣", "苗栗縣", "台中市", "彰化縣", "南投縣", "嘉義市", "嘉義縣", "雲林縣", "台南市", "高雄市", "澎湖縣", "金門縣", "屏東縣", "台東縣", "花蓮縣", "連江縣" };
            string[,] districts = new string[,]{{"中正區", "大同區", "中山區", "松山區", "大安區", "萬華區", "信義區", "士林區", "北投區", "內湖區", "南港區", "文山區" },
        { "100", "103", "104", "105", "106", "108", "110", "111", "112", "114", "115", "116"} };
            Districts districts1 = new Districts();
            for (int i = 0; i < districts.GetUpperBound(0); i++)
            {

            }
        }

        //載入會員修改頁面抓取City欄位
        //====================================================
        public string getAllCities()
        {
            //取得Cities資料庫內的資料
            var city = db.Cities.Select(c => new { c.CityID, c.CityName }).ToList();

            return JsonConvert.SerializeObject(city);
        }

        //藉由CityId取得Districts
        //====================================================
        public string getDistrictsByCityId(int cityId)
        {
            //取得Districts資料庫內的資料
            var districts = db.Districts.Where(d => d.CityId == cityId).Select(d => new { d.DistrictId, d.DistrictName }).ToList();
            return JsonConvert.SerializeObject(districts);
        }


        //藉由districtId取得postCode
        //====================================================
        public string getPostCodeByDistrictId(int districtId)
        {
            var postCode = db.Districts.FirstOrDefault(d => d.DistrictId == districtId).PostCode;
            return postCode;
        }
        #endregion

        #region 會員訂單管理查詢
        //會員訂單管理查詢
        public ActionResult getOrderbyMemberId(int memberId, int page = 1)
        {
            int pagesize = 5;
            int pagecurrent = page < 1 ? 1 : page;
            List<Orders> order = db.Orders.OrderByDescending(o => o.OrderDate).Where(o => o.MemberID == memberId).ToList();
            IPagedList<Orders> pagelist = order.ToPagedList(pagecurrent, pagesize);
            ViewBag.MemberId = memberId;
            return PartialView("getOrderbyMemberId", pagelist);
        }

        

        //會員訂單管理showQRCode
        public string getQRCodeByOrderId(int orderId)
        {
            List<Order_Detail> order_Details = db.Orders.FirstOrDefault(o => o.OrderID == orderId).Order_Detail.ToList();
            List<string> datas = new List<string>();
            foreach (Order_Detail order_Detail in order_Details)
            {
                datas.Add(new ActivityController().ShowQRCode(order_Detail.OrderDetailID));
            }
            return JsonConvert.SerializeObject(datas);
        }
        #endregion

        #region 會員收藏管理
        //會員我的收藏查詢
        public ActionResult getActivityFavouriteByMemberId(int MemberId, int page = 1)
        {
            int pagesize = 6;
            int pagecurrent = page < 1 ? 1 : page;
            List<ActivityFavourite> favourite = db.ActivityFavourite.Where(a => a.MemberId == MemberId).ToList();
            IPagedList<ActivityFavourite> pagelist = favourite.ToPagedList(pagecurrent, pagesize);
            ViewBag.MemberId = MemberId;
            return PartialView("getActivityFavouriteByMemberId", pagelist);
        }

        //會員我的收藏圈圈數字改變
        public int changeActivityFavouriteByMemberId(int memberId)
        {
            int number = db.ActivityFavourite.Where(a => a.MemberId == memberId).Count();
            return number;
        }


        //會員我的收藏刪除
        public string deleteActivityFavouriteByActivityId(int memberId, int activityId)
        {
            ActivityFavourite af = db.ActivityFavourite.FirstOrDefault(a => a.ActivityId == activityId && a.MemberId == memberId);
            db.ActivityFavourite.Remove(af);
            db.SaveChanges();

            return "刪除成功";
        }
        #endregion

        #region 會員後台訊息管理
        //取得後台系統訊息 ShoppingCartList.cshtml參考
        public ActionResult getShortMassageByMemberId(int MemberId, int page = 1)
        {
            int pagesize = 5;
            int pagecurrent = page < 1 ? 1 : page;
            List<ShortMessage> massage = db.ShortMessage.OrderByDescending(s => s.ShortMessageID).Where(s => s.MemberID == MemberId).ToList();
            IPagedList<ShortMessage> pagelist = massage.ToPagedList(pagecurrent, pagesize);
            ViewBag.MemberId = MemberId;
            return PartialView("getShortMassageByMemberId", pagelist);
        }

        
        //會員我的訊息刪除
        public string deleteShortmessage(int memberId, int shortmessageId)
        {
            ShortMessage sm = db.ShortMessage.FirstOrDefault(s => s.MemberID == memberId && s.ShortMessageID == shortmessageId);
            db.ShortMessage.Remove(sm);
            db.SaveChanges();
            return "刪除成功";
        }

        
        //會員我的訊息圈圈數字改變
        public int changeShortMessageNumber(int memberId)
        {
            int number = db.ShortMessage.Where(s => s.MemberID == memberId).Count();
            return number;
        }
        #endregion
    }
}
using prjITicket.Models;
using prjITicket.Models.Forum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Web.UI;
using prjITicket.ViewModel;

namespace 期末專題_討論版.Controllers
{
    
    public class ForumController : Controller
    {
       

        //=====↓↓↓文章增刪修區域↓↓↓=====

        //新增文章前，判斷是否登入，如果沒有則導入登入畫面；如果有被停權則彈出視窗；如果以登入且沒有被停權，則導入新增文章頁面。
        public string before_Add_article()
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            TicketSysEntities db = new TicketSysEntities();

            if (member == null || member.MemberRoleId == 1)
            {
                return "未登入";
            }
            else if (db.BanLIst.Where(n => n.EndTime > DateTime.Now).Select(n => n.BanMemberId).Contains(member.MemberID))
            {
                return "被停權";
            }
            else
            {
                return "可發文";
            }
        }
    
        //新增文章，用VM傳入必要資訊如：
        public ActionResult Add_article()
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            TicketSysEntities db = new TicketSysEntities();
            VMforum_mainblock vMforum_Mainblock = new VMforum_mainblock();
            vMforum_Mainblock.activities = db.Activity.Where(n => n.Seller.MemberId == member.MemberID).ToList();
            vMforum_Mainblock.ArticleCategories = db.ArticleCategories.ToList();

            return View(vMforum_Mainblock);
        }
        [ValidateInput(false)]//關閉保護html傳送
        [HttpPost]
        public string Add_article(string title, string content, int ArticleCategoryID, string picPath, int[] Activities)
        {
            if (Session[CDictionary.SK_Logined_Member] == null)
                return "Fail";

            try
            {
                if (string.IsNullOrEmpty(title))
                    return "標題不得空白";
                if (string.IsNullOrEmpty(content))
                    return "內文不得空白";
                //其他部分
                TicketSysEntities db = new TicketSysEntities();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;//如果轉型失敗，回傳null;
                Article article = new Article();
                article.MemberID = member.MemberID;
                article.Date = DateTime.Now;
                article.ArticleCategoryID = ArticleCategoryID;
                article.ArticleTitle = title;
                article.ArticleContent = content;
                if (!string.IsNullOrEmpty(picPath))
                    article.Picture = picPath;
                //article 的活動
                if (Activities != null)
                {
                    foreach (var item in Activities)
                    {
                        Ad_Article_Activity ad_Article_Activity = new Ad_Article_Activity();
                        ad_Article_Activity.ActivityID = item;
                        ad_Article_Activity.ArticleID = article.ArticleID;
                        db.Ad_Article_Activity.Add(ad_Article_Activity);
                    }
                }


                db.Article.Add(article);
                db.SaveChanges();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //剪裁圖片
        [HttpPost]
        public JsonResult CropImage(string id, int? x1, int? x2, int? y1, int? y2, int? imgWidth)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                result.Add("result", "error");
                result.Add("msg", "沒有輸入資料編號");
                return Json(result);
            }
            if (!x1.HasValue || !x2.HasValue || !y1.HasValue || !y2.HasValue)
            {
                result.Add("result", "error");
                result.Add("msg", "裁剪圖片區域值有缺少");
                return Json(result);
            }
            string id2 = id.Remove(0, 23);
            byte[] arr = Convert.FromBase64String(id2);
            MemoryStream instance = new MemoryStream(arr);
            Bitmap oBitmap = new Bitmap(instance);
            CropImageUtility cropUtils = new CropImageUtility();
            //不可以唯讀啊！
            int X1 = x1.Value;
            int X2 = x2.Value;
            int Y1 = y1.Value;
            int Y2 = y2.Value;
            if (imgWidth > 700)
            {
                X1 = X1 * imgWidth.Value / 700;
                X2 = X2 * imgWidth.Value / 700;
                Y1 = Y1 * imgWidth.Value / 700;
                Y2 = Y2 * imgWidth.Value / 700;
            }
            Dictionary<string, string> processResult = cropUtils.ProcessImageCrop
            (
                oBitmap,
                new int[] { X1, X2, Y1, Y2 }
            );
            if (processResult["result"].Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                result.Add("result", "OK");
                result.Add("msg", "");
                result.Add("CropImage", processResult["CropImage"].Remove(0, 1));
            }
            else
            {
                result.Add("result", processResult["result"]);
                result.Add("msg", processResult["msg"]);
            }
            return Json(result);

        }
        
        //編輯文章，用VM傳入必要資訊如：
        public ActionResult Edit_article(int? articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = from n in db.Article
                    where n.ArticleID == articleID
                    select n;
            var p = db.ArticleCategories.Select(n => n);
            Article article = q.FirstOrDefault();
            VMforum_mainblock vMforum_Mainblock = new VMforum_mainblock();
            vMforum_Mainblock.Article = new List<Article>();
            vMforum_Mainblock.ArticleCategories = new List<ArticleCategories>();
            vMforum_Mainblock.Article.Add(article);
            vMforum_Mainblock.ArticleCategories = p.ToList();
            vMforum_Mainblock.activities = db.Activity.Where(n => n.Seller.MemberId == article.MemberID).ToList();
            
            return View(vMforum_Mainblock);
        }
        [ValidateInput(false)]
        [HttpPost]
        public string Edit_article(string title, string content, int ArticleID, string picPath, int[] Activities)
        {
            TicketSysEntities db = new TicketSysEntities();
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            Article article = db.Article.Where(n => n.ArticleID == ArticleID).FirstOrDefault();
            if (member == null || (member.MemberRoleId != 4 && member.MemberID != article.MemberID))
                return "您沒有登入或是權限不足";


            if (article != null)
            {
                if (string.IsNullOrEmpty(title))
                    return "標題不得空白";
                if (string.IsNullOrEmpty(content))
                    return "內文不得空白";
                article.ArticleTitle = title;
                article.ArticleContent = content;
                article.Date = DateTime.Now;
                if (!string.IsNullOrEmpty(picPath))
                    article.Picture = picPath;
                if (Activities != null)
                {
                    //把先前的紀錄清空
                    foreach (var items in db.Ad_Article_Activity.Where(n => n.ArticleID == ArticleID))
                    {
                        db.Ad_Article_Activity.Remove(items);
                    }
                    //加入新的活動連結
                    foreach (var item in Activities)
                    {
                        Ad_Article_Activity ad_Article_Activity = new Ad_Article_Activity();
                        ad_Article_Activity.ActivityID = item;
                        ad_Article_Activity.ArticleID = ArticleID;
                        db.Ad_Article_Activity.Add(ad_Article_Activity);
                    }
                }
                db.SaveChanges();
            }
            return "OK";
        }
        //刪除文章
        public void  Delete(int articleID)
        {
            
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            TicketSysEntities db = new TicketSysEntities();

            //是一般會員且是作者，或是權限為管理員，就可以編輯
            Article article = db.Article.Where(n => n.ArticleID == articleID).FirstOrDefault();
            if (member != null && (member.MemberRoleId==4 || article.MemberID==member.MemberID))
            {
                var q = db.Article.Where(n => n.ArticleID == articleID).FirstOrDefault();
                var reply = db.Reply.Where(n => n.ArticleID == articleID);
                foreach (var item in reply)
                {
                    var qq = db.Reply_Emotion.Where(n => n.ReplyId == item.ReplyID);
                    var qqq = db.Reply_Report.Where(n => n.ReplyId == item.ReplyID);
                    foreach (var items in qq)
                    {
                        db.Reply_Emotion.Remove(items);
                    }
                    foreach (var itemss in qqq)
                    {
                        db.Reply_Report.Remove(itemss);
                    }

                }
                
                foreach (var item in reply)
                {
                    db.Reply.Remove(item);

                }
                foreach (var item in db.Article_Report.Where(n => n.ArticleId == articleID))
                {
                    db.Article_Report.Remove(item);
                }
                foreach (var item in db.Article_Emotion.Where(n => n.ArticleId == articleID))
                {
                    db.Article_Emotion.Remove(item);
                }
                foreach (var item in db.Ad_Article_Activity.Where(n=>n.ArticleID == articleID))
                {
                    db.Ad_Article_Activity.Remove(item);
                }
                db.Article.Remove(q);
                db.SaveChanges();
                Response.Write("<script>" +
                    "alert('刪除成功！即將跳轉至討論版首頁');" +
                    $"window.location.href='{Url.Action("forum_mainblock", "Forum")}';</script> ");
            }
            else if (member != null && (member.MemberRoleId==2 || member.MemberRoleId==1))
            {
                
                    Response.Write("<script>alert('您非該文章作者，禁止刪除！即將跳轉至討論版首頁');" +
                   $"window.location.href='{Url.Action("forum_mainblock", "Forum")}';</script> ");
            }
            else
            {
                Response.Write("<script>alert('您尚未登入！即將跳轉至登入頁面');" +
                $"window.location.href='{Url.Action("Login", "Login")}';</script> ");
            }          
           
        }
        //=====↑↑↑文章增刪修↑↑↑=====

        
        public ActionResult forum_homepage()
        {
            return RedirectToAction("forum_mainblock");
        }
        //初次載入調用，之後都用不到惹
        public ActionResult forum_mainblock(string searchText)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = (from n in db.Article
                     orderby n.Date descending
                     select n).ToList();
            var p = db.ArticleCategories.Select(n => n).ToList();
            int maxPage = (q.Count -1) / 4;
            var memberlist = db.Member.ToList();
            var qq = new VMforum_mainblock { Article = q, ArticleCategories = p, maxpage = maxPage ,searchWord = searchText, Memberlist = memberlist, editor ="", list ="", page=0 };

            //刪除沒有用到的圖片(就是剪裁失敗的)
            CleanClear cleanClear = new CleanClear();
            //cleanClear.cleanData();
            return View("forum_mainblock", "_ForumLayout", qq);
        }
        //======↓↓↓留言增刪修↓↓↓=====
        //載入留言內容
        public ActionResult forum_reply(int? articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = from n in db.Article
                    where n.ArticleID == articleID
                    select n;
            Article article = q.FirstOrDefault();
            List<Report> report = db.Report.ToList();

            return PartialView(new VMReport() { Article = article, Report = report });
        }
        //載入活動資訊
        public ActionResult forum_Activity(int? articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = from n in db.Ad_Article_Activity
                    where n.ArticleID == articleID
                    select n;
            List<Ad_Article_Activity> Ads = q.ToList();
            List<Activity> Activitys = new List<Activity>();
            foreach (var item in Ads)
            {
                Activity activity = db.Activity.Where(n => n.ActivityID == item.ActivityID).FirstOrDefault();
                Activitys.Add(activity);
            }
            Article article = db.Article.Where(n => n.ArticleID == articleID).FirstOrDefault();

            return PartialView(Activitys);

        }
        //載入文章 & 新增留言
        public ActionResult forum_content(int? articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = from n in db.Article
                    where n.ArticleID == articleID
                    select n;
            Article article = q.FirstOrDefault();
            List<Report> report = db.Report.ToList();
            return View(new VMReport() { Article = article, Report = report });
        }
        [ValidateInput(false)]
        [HttpPost]
        public string forum_content(string content, int articleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            try
            {
                if (member == null)
                {
                    return "您尚未登入！即將跳轉至登入頁面";
                }
                else if (db.BanLIst.Where(n => n.EndTime > DateTime.Now).Select(n => n.BanMemberId).Contains(member.MemberID))
                {
                    return "您已被停權！無法操作此功能";
                }
                else
                {
                
                Reply rp = new Reply();
                rp.MemberID = member.MemberID;
                rp.ReplyDate = DateTime.Now;
                rp.ArticleID = articleID;
                rp.ReplyContent = content;
                rp.Readed = false;
                db.Reply.Add(rp);
                db.SaveChanges();
                return "成功";

                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //刪除留言
        public ActionResult Reply_Delete(int replyID, int ArticleID)
        {
            TicketSysEntities db = new TicketSysEntities();
            var q = db.Reply.Where(n => n.ReplyID == replyID).FirstOrDefault();
            foreach (var item in db.Reply_Report.Where(n => n.ReplyId == q.ReplyID))
            {
                db.Reply_Report.Remove(item);
            }
            foreach (var item in db.Reply_Emotion.Where(n => n.ReplyId == q.ReplyID))
            {
                db.Reply_Emotion.Remove(item);

            }
            db.Reply.Remove(q);
            db.SaveChanges();
            return RedirectToAction("forum_content", "Forum", new { articleID = ArticleID });
        }
        //編輯留言
        [ValidateInput(false)]
        public string Reply_Edit(string rpcontent, int replyID)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                Reply reply = db.Reply.Where(n => n.ReplyID == replyID).FirstOrDefault();
                if (reply != null)
                {
                    reply.ReplyContent = rpcontent;
                    reply.ReplyDate = DateTime.Now;

                    db.SaveChanges();
                }


                return "OK";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }
        //======↑↑↑留言增刪修↑↑↑=====
        //======↓↓↓舉報增刪修↓↓↓=====
        public string Reply_report(int ReportID, int ReplyID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Reply_Report report = new Reply_Report();

                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                report.MemberId = member.MemberID;
                report.ReplyId = ReplyID;
                report.ReportId = ReportID;
                db.Reply_Report.Add(report);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string Article_report(int ReportID, int ArticleID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Article_Report report = new Article_Report();

                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                report.MemberId = member.MemberID;
                report.ArticleId = ArticleID;
                report.ReportId = ReportID;
                db.Article_Report.Add(report);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //======↑↑↑舉報增刪修↑↑↑=====

        //文章搜尋
        public ActionResult SearchArticle(string searchText = "", int Page = 0, int CategoryID = 0, int searchType = 30, string list = "ByTime" ,string editor = "")
        {
            Member member = Session[CDictionary.SK_Logined_Member] as Member;
            TicketSysEntities db = new TicketSysEntities();
            CForum_ArticleList_Factory al = new CForum_ArticleList_Factory();

            List<Article> articles = db.Article.ToList();

            //有選擇討論版
            if (CategoryID != 0)
                articles = al.Article_Category(articles, CategoryID);
            if (searchText == "我的文章")
            {
                searchText = member.NickName;
            }
            //搜尋有文字
            if (!string.IsNullOrEmpty(searchText))
            {
                //searchType =2...有勾作者
                //searchType =3...有勾標題
                //searchType =5...有勾內文  
                List<Article> q = new List<Article>();
                if (searchType % 2 == 0)
                    q = q.Union(al.Article_Search_Editor(articles, searchText)).ToList();
                if (searchType % 3 == 0)
                    q = q.Union(al.Article_Search_Title(articles, searchText)).ToList();
                if (searchType % 5 == 0)
                    q = q.Union(al.Article_Search_Content(articles, searchText)).ToList();
                articles = q;
            }
            //如果按到的是專欄作家
            if (!string.IsNullOrEmpty(editor))
            {
                articles = articles.Where(n => n.Member.NickName == editor).ToList();
            }
            //
            //第幾頁
            int maxPage = ((articles.Count() - 1) / 4);
            //todo:按日期檢索
            ///
            //如果有選擇按照讚數排列
            if (list == "ByGood")
            {
                articles = articles.OrderByDescending(n => n.Article_Emotion.Count(k => k.ActionId == 1) - n.Article_Emotion.Count(k => k.ActionId == 2)).Skip(Page * 4).ToList();
            }
            else
            {
                articles = articles.OrderByDescending(n => n.Date).Skip(Page * 4).ToList();//這個頁數不能一起算欸！放最後篩好了
            }
            
            
            
            var p = db.ArticleCategories.Select(n => n).ToList();
            var qq = new VMforum_mainblock { Article = articles, ArticleCategories = p, searchWord = searchText, page = Page,maxpage = maxPage, ArticleCategoryID = CategoryID,editor = editor,list = list};
            return PartialView(qq);


        }

        //⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇貼文、留言Emotion⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇⬇//

        public string Reply_EmotionAction(int ReplyID, int ActionID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Reply_Emotion RE = new Reply_Emotion();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;

                RE.MemberId = member.MemberID;
                RE.ReplyId = ReplyID;
                RE.ActionId = ActionID;
                db.Reply_Emotion.Add(RE);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Reply_EmotionAction_Cancel(int ReplyID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                var q = db.Reply_Emotion.Where(n => n.ReplyId == ReplyID && n.MemberId == member.MemberID).FirstOrDefault();
                db.Reply_Emotion.Remove(q);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Article_EmotionAction(int ArticleID, int ActionID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Article_Emotion AE = new Article_Emotion();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;

                AE.MemberId = member.MemberID;
                AE.ArticleId = ArticleID;
                AE.ActionId = ActionID;
                db.Article_Emotion.Add(AE);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string Article_EmotionAction_Cancel(int ArticleID)
        {
            try
            {
                if (Session[CDictionary.SK_Logined_Member] == null)
                    return "您尚未登入！即將跳轉至登入頁面";
                TicketSysEntities db = new TicketSysEntities();
                Member member = Session[CDictionary.SK_Logined_Member] as Member;
                var q = db.Article_Emotion.Where(n => n.ArticleId == ArticleID && n.MemberId == member.MemberID).FirstOrDefault();
                db.Article_Emotion.Remove(q);
                db.SaveChanges();
                return "成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //在ActivityList頁面中Ajax調用這個方法取得活動的次詳細分頁
        public ActionResult GetActivitySubDetailPage(int activityId)
        {
            TicketSysEntities db = new TicketSysEntities();
            Activity activity = db.Activity.Where(a => a.ActivityStatusID == 1).FirstOrDefault(a => a.ActivityID == activityId);
            if (activity == null)
            {
                //todo 回傳活動是空值的錯誤頁面
            }
            return PartialView("GetActivitySubDetailPage", activity);
        }
        //已讀留言
        public void reply_readed(int Article)
        {
            TicketSysEntities db = new TicketSysEntities();
            Article article = db.Article.FirstOrDefault(n => n.ArticleID == Article);
            foreach (var item in db.Reply.Where(n=>n.ArticleID==article.ArticleID))
            {
                item.Readed = true;
            }
            db.SaveChanges();
        }
    }
}
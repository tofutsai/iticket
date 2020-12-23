using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using prjITicket.Models;

namespace prjITicket.Controllers
{
    public class BackEndArticleController : Controller
    {
        TicketSysEntities db = new TicketSysEntities();

        public ActionResult ArticleList()
        {
            if ((Session[CDictionary.SK_Logined_Member] as Member)?.MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(db.ArticleCategories);
        }

        [HttpPost]
        public JsonResult ArticleList(ArticleAjax m)
        {
            if (m.Type == 0)
            {
                IEnumerable<Article> articles;
                if (m.Sort == 1)
                {
                    articles = db.Article_Report.AsEnumerable()
                        .GroupBy(x => x.ArticleId)
                        .Where(g => g.Count() >= m.Report)
                        .Select(g => g.First().Article);

                    if (m.Report == 0)
                    {
                        int[] vs = db.Article_Report.Select(x => x.ArticleId).Distinct().ToArray();
                        articles = articles.Concat(
                            db.Article.Where(x => !vs.Contains(x.ArticleID))
                        );
                    }
                    articles = articles.OrderByDescending(x => x.Date).ThenByDescending(x => x.ArticleID);
                }
                else
                {
                    articles = db.Article_Report.AsEnumerable()
                        .GroupBy(x => x.ArticleId)
                        .Where(g => g.Count() >= m.Report)
                        .OrderByDescending(g => g.Count())
                        .ThenByDescending(g => g.First().ArticleId)
                        .Select(g => g.First().Article);

                    if (m.Report == 0)
                    {
                        int[] vs = db.Article_Report.Select(x => x.ArticleId).Distinct().ToArray();
                        articles = articles.Concat(
                            db.Article.Where(x => !vs.Contains(x.ArticleID)).OrderByDescending(x => x.ArticleID)
                        );
                    }
                }
                articles = articles
                    .Where(x => m.Cate == 0 || x.ArticleCategoryID == m.Cate)
                    .Where(x => m.Date == 0 || (DateTime.Now - x.Date).TotalDays <= m.Date)
                    .Where(x =>
                        string.IsNullOrEmpty(m.Keyword) ||
                        (
                            m.Keyword.StartsWith("author:") ?
                            (
                                x.Member.Email.Split('@')[0].ToLower() == m.Keyword.Split(':')[1].Trim()
                            ):
                            (
                                x.ArticleTitle.ToLower().Contains(m.Keyword) ||
                                x.Member.Email.Split('@')[0].ToLower().Contains(m.Keyword)
                            )
                        )
                    );

                List<ArticleJson> data = new List<ArticleJson>();
                if (articles.Count() == 0)
                {
                    data.Add(new ArticleJson
                    {
                        MaxPage = 1,
                        ChangePage = 1
                    });
                }
                else
                {
                    int skip = m.PageSize * (m.PageCurrent - 1);
                    int take = m.PageSize;
                    int maxpage = (int)Math.Ceiling((decimal)articles.Count() / m.PageSize);
                    int changepage = m.PageCurrent > maxpage ? maxpage : 0;
                    data.Add(new ArticleJson
                    {
                        MaxPage = maxpage,
                        ChangePage = changepage
                    });
                    skip = changepage == 0 ? skip : take * (changepage - 1);
                    articles = articles.Skip(skip).Take(take).ToList();
                    data.AddRange(articles.Select(article => new ArticleJson
                    {
                        article = article,
                        reportA = db.Article_Report
                    }));
                }
                return Json(data);
            }
            else
            {
                IEnumerable<Reply> replies;
                if (m.Sort == 1)
                {
                    replies = db.Reply_Report.AsEnumerable()
                        .GroupBy(x => x.ReplyId)
                        .Where(g => g.Count() >= m.Report)
                        .Select(g => g.First().Reply);

                    if (m.Report == 0)
                    {
                        int[] vs = db.Reply_Report.Select(x => x.ReplyId).Distinct().ToArray();
                        replies = replies.Concat(
                            db.Reply.Where(x => !vs.Contains(x.ReplyID))
                        );
                    }
                    replies = replies.OrderByDescending(x => x.ReplyDate).ThenByDescending(x => x.ReplyID);
                }
                else
                {
                    replies = db.Reply_Report.AsEnumerable()
                        .GroupBy(x => x.ReplyId)
                        .Where(g => g.Count() >= m.Report)
                        .OrderByDescending(g => g.Count())
                        .ThenByDescending(g => g.First().ReplyId)
                        .Select(g => g.First().Reply);

                    if (m.Report == 0)
                    {
                        int[] vs = db.Reply_Report.Select(x => x.ReplyId).Distinct().ToArray();
                        replies = replies.Concat(
                            db.Reply.Where(x => !vs.Contains(x.ReplyID)).OrderByDescending(x => x.ReplyID)
                        );
                    }
                }
                replies = replies
                    .Where(x => m.Cate == 0 || x.Article.ArticleCategoryID == m.Cate)
                    .Where(x => m.Date == 0 || (DateTime.Now - x.ReplyDate).TotalDays <= m.Date)
                    .Where(x =>
                        string.IsNullOrEmpty(m.Keyword) ||
                        (
                            m.Keyword.StartsWith("author:") ?
                            (
                                x.Member.Email.Split('@')[0].ToLower() == m.Keyword.Split(':')[1].Trim()
                            ) :
                            (
                                x.Article.ArticleTitle.ToLower().Contains(m.Keyword) ||
                                x.Member.Email.Split('@')[0].ToLower().Contains(m.Keyword)
                            )
                        )
                    );

                List<ArticleJson> data = new List<ArticleJson>();
                if (replies.Count() == 0)
                {
                    data.Add(new ArticleJson
                    {
                        MaxPage = 1,
                        ChangePage = 1
                    });
                }
                else
                {
                    int skip = m.PageSize * (m.PageCurrent - 1);
                    int take = m.PageSize;
                    int maxpage = (int)Math.Ceiling((decimal)replies.Count() / m.PageSize);
                    int changepage = m.PageCurrent > maxpage ? maxpage : 0;
                    data.Add(new ArticleJson
                    {
                        MaxPage = maxpage,
                        ChangePage = changepage
                    });
                    skip = changepage == 0 ? skip : take * (changepage - 1);
                    replies = replies.Skip(skip).Take(take).ToList();
                    data.AddRange(replies.Select(reply => new ArticleJson
                    {
                        reply = reply,
                        reportR = db.Reply_Report
                    }));
                }
                return Json(data);
            }
        }

        [HttpPost]
        public JsonResult ArticleDetail(int id, bool isArticle)
        {
            List<ArticleDetail> data = new List<ArticleDetail>();
            if (isArticle)
            {
                Article article = db.Article.FirstOrDefault(x => x.ArticleID == id);
                Seller seller = db.Seller.FirstOrDefault(x => x.MemberId == article.MemberID);
                IEnumerable<BanLIst> banlist = db.BanLIst
                    .Where(x => x.BanMemberId == article.MemberID)
                    .OrderByDescending(x => x.EndTime);

                data.Add(new ArticleDetail
                {
                    article = article,
                    seller = seller,
                    banlist = banlist
                });

                IEnumerable<Article_Report> reports = db.Article_Report
                    .Where(x => x.ArticleId == id)
                    .OrderByDescending(x => x.Article_ReportId);

                data.AddRange(reports.Select(reportA => new ArticleDetail
                {
                    reportA = reportA
                }));
            }
            else
            {
                Reply reply = db.Reply.FirstOrDefault(x => x.ReplyID == id);
                Seller seller = db.Seller.FirstOrDefault(x => x.MemberId == reply.MemberID);
                IEnumerable<BanLIst> banlist = db.BanLIst
                    .Where(x => x.BanMemberId == reply.MemberID)
                    .OrderByDescending(x => x.EndTime);

                data.Add(new ArticleDetail
                {
                    reply = reply,
                    seller = seller,
                    banlist = banlist
                });

                IEnumerable<Reply_Report> reports = db.Reply_Report
                    .Where(x => x.ReplyId == id)
                    .OrderByDescending(x => x.Reply_ReportId);

                data.AddRange(reports.Select(reportR => new ArticleDetail
                {
                    reportR = reportR
                }));
            }
            return Json(data);
        }

        public async Task<string> DeleteArticleOrReply(BanTaskInfo m)
        {
            if (m.BTtype == "article")
            {
                List<Article_Report> reportsA = db.Article_Report.Where(x => x.ArticleId == m.BTxid).ToList();
                foreach (Article_Report reportA in reportsA)
                {
                    db.Entry(reportA).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                int[] repliesIDs = db.Reply.Where(x => x.ArticleID == m.BTxid).Select(x => x.ReplyID).ToArray();
                foreach (int rid in repliesIDs)
                {
                    List<Reply_Report> reportsR = db.Reply_Report.Where(x => x.ReplyId == rid).ToList();
                    foreach (Reply_Report reportR in reportsR)
                    {
                        db.Entry(reportR).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
                    Reply reply = db.Reply.FirstOrDefault(x => x.ReplyID == rid);
                    db.Entry(reply).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                Article article = db.Article.FirstOrDefault(x => x.ArticleID == m.BTxid);
                db.Entry(article).State = EntityState.Deleted;
                db.SaveChanges();
            }
            else
            {
                List<Reply_Report> reports = db.Reply_Report.Where(x => x.ReplyId == m.BTxid).ToList();
                foreach (Reply_Report report in reports)
                {
                    db.Entry(report).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                Reply reply = db.Reply.FirstOrDefault(x => x.ReplyID == m.BTxid);
                db.Entry(reply).State = EntityState.Deleted;
                db.SaveChanges();
            }

            string msgType = m.BTtype == "article" ? "文章" : "回覆";
            if (db.Member.FirstOrDefault(x => x.MemberID == m.BTid).MemberRoleId != 4)
            {
                if (m.BTban)
                {
                    BanLIst banlist = new BanLIst
                    {
                        BanMemberId = m.BTid,
                        AdminMemberId = (Session[CDictionary.SK_Logined_Member] as Member).MemberID,
                        Reason = m.BTmessage,
                        EndTime = m.BTendtime
                    };
                    db.Entry(banlist).State = EntityState.Added;
                    db.SaveChanges();
                }
                await BackEndFactory.SendMessageToMember(m.BTid, m.BTmain, m.BTmessage, m.BTban, m.BTendtime);
                return $"已完成刪除{msgType}與發送通知兼站外寄信!";
            }
            else
            {
                await BackEndFactory.SendMessageToMember(m.BTid, m.BTmain, m.BTmessage, false, m.BTendtime);
                return $"已完成刪除{msgType}與發送通知兼站外寄信!{(m.BTban ? " (管理者身份無法被停權)" : "")}";
            }
        }
    }
}
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
    public class BackEndCommentController : Controller
    {
        TicketSysEntities db = new TicketSysEntities();

        public ActionResult CommentList()
        {
            if ((Session[CDictionary.SK_Logined_Member] as Member)?.MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(db.Categories);
        }

        [HttpPost]
        public JsonResult CommentList(CommentAjax m)
        {
            IEnumerable<Comment> comments;
            if (m.Sort == 1)
            {
                comments = db.CommentReport.AsEnumerable()
                    .GroupBy(x => x.CommentId)
                    .Where(g => g.Count() >= m.Report)
                    .Select(g => g.First().Comment);

                if (m.Report == 0)
                {
                    int[] vs = db.CommentReport.Select(x => x.CommentId).Distinct().ToArray();
                    comments = comments.Concat(
                        db.Comment.Where(x => !vs.Contains(x.CommentID))
                    );
                }
                comments = comments.OrderByDescending(x => x.CommentDate).ThenByDescending(x => x.CommentID);
            }
            else
            {
                comments = db.CommentReport.AsEnumerable()
                    .GroupBy(x => x.CommentId)
                    .Where(g => g.Count() >= m.Report)
                    .OrderByDescending(g => g.Count())
                    .ThenByDescending(g => g.First().CommentId)
                    .Select(g => g.First().Comment);

                if (m.Report == 0)
                {
                    int[] vs = db.CommentReport.Select(x => x.CommentId).Distinct().ToArray();
                    comments = comments.Concat(
                        db.Comment.Where(x => !vs.Contains(x.CommentID)).OrderByDescending(x => x.CommentID)
                    );
                }
            }
            comments = comments
                .Where(x => m.Cate == 0 || x.Activity.SubCategories.CategoryId == m.Cate)
                .Where(x => m.SubCate == 0 || x.Activity.SubCategoryId == m.SubCate)
                .Where(x => m.Date == 0 || (DateTime.Now - x.CommentDate).TotalDays <= m.Date)
                .Where(x => m.ShowBan == 1 || x.IsBaned == false)
                .Where(x =>
                    string.IsNullOrEmpty(m.Keyword) ||
                    (
                        m.Keyword.StartsWith("author:") ? 
                        (
                            x.Member.Email.Split('@')[0].ToLower() == m.Keyword.Split(':')[1].Trim()
                        ): 
                        (
                            x.Activity.ActivityName.ToLower().Contains(m.Keyword) ||
                            x.Member.Email.Split('@')[0].ToLower().Contains(m.Keyword)
                        )
                    )
                );

            List<CommentJson> data = new List<CommentJson>();
            if (comments.Count() == 0)
            {
                data.Add(new CommentJson
                {
                    MaxPage = 1,
                    ChangePage = 1
                });
            }
            else
            {
                int skip = m.PageSize * (m.PageCurrent - 1);
                int take = m.PageSize;
                int maxpage = (int)Math.Ceiling((decimal)comments.Count() / m.PageSize);
                int changepage = m.PageCurrent > maxpage ? maxpage : 0;
                data.Add(new CommentJson
                {
                    MaxPage = maxpage,
                    ChangePage = changepage
                });
                skip = changepage == 0 ? skip : take * (changepage - 1);
                comments = comments.Skip(skip).Take(take).ToList();
                data.AddRange(comments.Select(comment => new CommentJson
                {
                    comment = comment,
                    reports = db.CommentReport
                }));
            }
            return Json(data);
        }

        [HttpPost]
        public JsonResult GetSubCateOption(int id)
        {
            IEnumerable<SubCateOption> data = db.SubCategories
                .Where(x => id == 0 || x.CategoryId == id)
                .Select(x => new SubCateOption { subCategories = x });
            return Json(data);
        }

        [HttpPost]
        public JsonResult CommentDetail(int id)
        {
            List<CommentDetail> data = new List<CommentDetail>();
            Comment comment = db.Comment.FirstOrDefault(x => x.CommentID == id);
            Seller seller = db.Seller.FirstOrDefault(x => x.MemberId == comment.CommentMemberID);
            IEnumerable<BanLIst> banlist = db.BanLIst
                .Where(x => x.BanMemberId == comment.CommentMemberID)
                .OrderByDescending(x => x.EndTime);

            data.Add(new CommentDetail 
            { 
                comment = comment,
                seller = seller,
                banlist = banlist
            });

            IEnumerable<CommentReport> reports = db.CommentReport
                .Where(x => x.CommentId == id)
                .OrderByDescending(x => x.CommentReportId);

            data.AddRange(reports.Select(report => new CommentDetail
            {
                report = report
            }));
            return Json(data);
        }

        [HttpPost]
        public async Task<string> BanComment(BanTaskInfo m)
        {
            Comment comment = db.Comment.FirstOrDefault(x => x.CommentID == m.BTxid);
            comment.IsBaned = true;
            db.SaveChanges();

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
                return "已完成隱藏評論與發送通知兼站外寄信!";
            }
            else
            {
                await BackEndFactory.SendMessageToMember(m.BTid, m.BTmain, m.BTmessage, false, m.BTendtime);
                return $"已完成隱藏評論與發送通知兼站外寄信!{(m.BTban ? " (管理者身份無法被停權)" : "")}";
            }
        }

        [HttpPost]
        public async Task<string> DismissComment(DismissTaskInfo m)
        {
            Comment comment = db.Comment.FirstOrDefault(x => x.CommentID == m.DTxid);
            comment.IsBaned = false;
            db.SaveChanges();

            if (m.DTunban)
            {
                List<BanLIst> banlists = db.BanLIst.Where(x => x.BanMemberId == m.DTid && x.EndTime > DateTime.Now).ToList();
                foreach (BanLIst banlist in banlists)
                {
                    db.Entry(banlist).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }
            await BackEndFactory.SendMessageToMember(m.DTid, m.DTmain, m.DTmessage, false, DateTime.Now);
            return "已完成解除隱藏評論與發送通知兼站外寄信!";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using prjITicket.Models;

namespace prjITicket.Controllers
{
    public class BackEndMemberController : Controller
    {
        TicketSysEntities db = new TicketSysEntities();

        public ActionResult MemberList()
        {
            if ((Session[CDictionary.SK_Logined_Member] as Member)?.MemberRoleId != 4)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public JsonResult MemberList(MemberAjax m)
        {
            IEnumerable<Member> members = db.Member.Where(x => 
                !m.Veri || (x.MemberRoleId == 3 && db.Seller.FirstOrDefault(s => s.MemberId == x.MemberID).fPass == null)
            );
            switch (m.RoleId)
            {
                case 3:
                    members = members.Where(x => x.MemberRoleId == 3);
                    break;
                case 2:
                    members = members.Where(x => x.MemberRoleId == 2);
                    break;
                case 1:
                    members = members.Where(x => x.MemberRoleId == 1);
                    break;
                case 0:
                    int[] vs = db.BanLIst.Where(x => x.EndTime > DateTime.Now).Select(x => x.BanMemberId).Distinct().ToArray();
                    members = members.Where(x => vs.Contains(x.MemberID));
                    break;
            }
            members = members.AsEnumerable().Where(x =>
                string.IsNullOrEmpty(m.Keyword) ||
                x.Email.Split('@')[0].ToLower().Contains(m.Keyword) ||
                x.NickName.ToLower().Contains(m.Keyword) ||
                x.Name.ToLower().Contains(m.Keyword) ||
                (x.Phone != null && x.Phone.Contains(m.Keyword))
            );

            List<MemberJson> data = new List<MemberJson>();
            if (members.Count() == 0)
            {
                data.Add(new MemberJson
                {
                    MaxPage = 1,
                    ChangePage = 1
                });
            }
            else
            {
                int skip = m.PageSize * (m.PageCurrent - 1);
                int take = m.PageSize;
                int maxpage = (int)Math.Ceiling((decimal)members.Count() / m.PageSize);
                int changepage = m.PageCurrent > maxpage ? maxpage : 0;
                data.Add(new MemberJson
                {
                    MaxPage = maxpage,
                    ChangePage = changepage
                });
                skip = changepage == 0 ? skip : take * (changepage - 1);
                switch (m.Sort)
                {
                    case "1d":
                        members = members.OrderByDescending(x => x.Email).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    case "1a":
                        members = members.OrderBy(x => x.Email).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    case "2d":
                        members = members.OrderByDescending(x => x.NickName).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    case "2a":
                        members = members.OrderBy(x => x.NickName).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    case "3d":
                        members = members.OrderByDescending(x => x.Name).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    case "3a":
                        members = members.OrderBy(x => x.Name).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    case "4d":
                        members = members.OrderByDescending(x => x.Phone).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    case "4a":
                        members = members.OrderBy(x => x.Phone).ThenByDescending(x => x.MemberID)
                            .Skip(skip).Take(take).ToList();
                        break;
                    default:
                        members = members.OrderByDescending(x => x.MemberID).Skip(skip).Take(take).ToList();
                        break;
                }
                data.AddRange(members.Select(member => new MemberJson
                {
                    member = member,
                    seller = db.Seller.FirstOrDefault(x => x.MemberId == member.MemberID),
                    banlist = db.BanLIst.Where(x => x.BanMemberId == member.MemberID && x.EndTime > DateTime.Now)
                        .OrderByDescending(x => x.EndTime)
                }));
            }
            return Json(data);
        }

        [HttpPost]
        public JsonResult MemberDetail(int id)
        {
            Member member = db.Member.FirstOrDefault(x => x.MemberID == id);
            MemberDetail data = new MemberDetail
            {
                member = member,
                seller = db.Seller.FirstOrDefault(x => x.MemberId == id),
                banlist = db.BanLIst.Where(x => x.BanMemberId == member.MemberID)
                    .OrderByDescending(x => x.EndTime)
            };
            return Json(data);
        }

        [HttpPost]
        public JsonResult MemberEmailCollection(int[] BackEndMemberListCollection)
        {
            var data = BackEndMemberListCollection.Select(id => new
                {
                    email = db.Member.FirstOrDefault(x => x.MemberID == id).Email
                });
            return Json(data);
        }

        [HttpPost]
        public async Task<string> SendMessageAsync(int[] BackEndMemberListCollection, string message)
        {
            List<Task> tasks = new List<Task>();
            foreach (int id in BackEndMemberListCollection)
            {
                tasks.Add(Task.Run(() =>
                    BackEndFactory.SendMessageToMember(id, "一般系統通知", message, false, DateTime.Now)
                ));
            }
            await Task.WhenAll(tasks);
            return "系統通知發送完畢!";
        }

        [HttpPost]
        public async Task<string> BanMemberAsync(BanTaskInfo m)
        {
            if (db.Member.FirstOrDefault(x => x.MemberID == m.BTid).MemberRoleId != 4)
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
                await BackEndFactory.SendMessageToMember(m.BTid, m.BTmain, m.BTmessage, true, m.BTendtime);
                return "已完成停權會員與發送通知兼站外寄信!";
            }
            else
            {
                return "管理者身份無法被停權!!!";
            }
        }

        [HttpPost]
        public async Task<string> UnBanMemberAsync(int id)
        {
            List<BanLIst> banlists = db.BanLIst.Where(x => x.BanMemberId == id && x.EndTime > DateTime.Now).ToList();
            foreach (BanLIst banlist in banlists)
            {
                db.Entry(banlist).State = EntityState.Deleted;
                db.SaveChanges();
            }
            string message = "iTicket 管理者已解除您的停權處分";
            await BackEndFactory.SendMessageToMember(id, "解除停權通知", message, false, DateTime.Now);
            return "已完成解除停權與發送通知兼站外寄信!";
        }

        [HttpPost]
        public async Task<string> VerifyAsync(int id, bool fPass, string message = null)
        {
            Seller seller = db.Seller.FirstOrDefault(x => x.MemberId == id);
            seller.fPass = fPass;
            db.SaveChanges();

            string main = fPass ? "商家審核通過" : "商家審核不通過";
            message = fPass ? "iTicket 管理者已通過您申請的商家審核" : message;
            await BackEndFactory.SendMessageToMember(id, main, message, false, DateTime.Now);
            return fPass ? "審核通過" : $"以「{message}」為理由不給予通過";
        }

        [HttpPost]
        public string DataDownloadCheck(int id)
        {
            try
            {
                string filename = db.Seller.FirstOrDefault(x => x.MemberId == id).fFileName;
                string filepath = Server.MapPath($"~/Content/Login/SellerImage/{filename}");
                FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return "Success";
            }
            catch
            {
                return "Failure";
            }
        }

        [HttpGet]
        public ActionResult DataDownload(int id)
        {
            if ((Session[CDictionary.SK_Logined_Member] as Member)?.MemberRoleId != 4)
            {
                return Content($"<script>window.close()</script>");
            }
            try
            {
                string filename = db.Seller.FirstOrDefault(x => x.MemberId == id).fFileName;
                string filepath = Server.MapPath($"~/Content/Login/SellerImage/{filename}");
                FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(stream, "application/msword", filename);
            }
            catch
            {
                return Content($"<script>window.close()</script>");
            }
        }
    }
}
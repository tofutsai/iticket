using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace prjITicket.Models
{
    public static class BackEndFactory
    {
        public static async Task SendMessageToMember(int id, string main, string message, bool ban, DateTime endtime)
        {
            try
            {
                TicketSysEntities db = new TicketSysEntities();
                ShortMessage shortmessage = new ShortMessage
                {
                    MemberID = id,
                    MessageContent = $"[{main}] {message}"
                };
                db.Entry(shortmessage).State = EntityState.Added;
                db.SaveChanges();

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("iticket128@gmail.com"),
                    Subject = "iTicket 發送了一則系統通知給您",
                    Body = $@"
                        <h2 style='color: #ff8c00'>iTicket 發送了一則系統通知給您</h2>
                        <p style='color: #f00'>{(ban ? $"iTicket 管理者以下述理由將您停權至 {endtime:yyyy-MM-dd}" : "")}</p>
                        <p><strong>系統通知標題: {main}</strong><br>系統通知內容: {message}</p>",
                    IsBodyHtml = true,
                    Priority = MailPriority.High
                };
                mail.To.Add(db.Member.FirstOrDefault(x => x.MemberID == id).Email);

                using (SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net"))
                {
                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new NetworkCredential(
                        "apikey", "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");
                    SmtpServer.EnableSsl = true;
                    await SmtpServer.SendMailAsync(mail);
                }
            }
            catch { }
        }
    }

    public class BanTaskInfo
    {
        public string BTtype { get; set; }
        public int BTxid { get; set; }
        public int BTid { get; set; }
        public string BTmain { get; set; }
        public string BTmessage { get; set; }
        public bool BTban { get; set; }
        public DateTime BTendtime { get; set; }
    }

    public class DismissTaskInfo
    {
        public int DTxid { get; set; }
        public int DTid { get; set; }
        public string DTmain { get; set; }
        public string DTmessage { get; set; }
        public bool DTunban { get; set; }
    }

    public class SubCateOption
    {
        public SubCategories subCategories { private get; set; }
        public int SubCategoryId => subCategories.SubCategoryId;
        public string SubCategoryName => subCategories.SubCategoryName;
    }

    public class CommentAjax
    {
        public int Cate { get; set; }
        public int SubCate { get; set; }
        public int Date { get; set; }
        public int Report { get; set; }
        public int ShowBan { get; set; }
        public int PageCurrent { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }
        public int Sort { get; set; }
    }

    public class CommentJson
    {
        public int? MaxPage { get; set; }
        public int? ChangePage { get; set; }

        public Comment comment { private get; set; }
        public int? CommentID => comment?.CommentID;
        public string Author => comment?.Member.Email;
        public string Activity => comment?.Activity.ActivityName;
        public int? Score => comment?.CommentScore;
        public string Date => comment?.CommentDate.ToString("yyyy-MM-dd HH:mm:ss");
        public bool? IsBaned => comment?.IsBaned;

        public IEnumerable<CommentReport> reports { private get; set; }
        public int? ReportCount => reports?.Count(x => x.CommentId == comment.CommentID);
    }

    public class CommentDetail
    {
        public Comment comment { private get; set; }
        public string ActivityName => comment?.Activity.ActivityName;
        public string ActivityDescription => comment?.Activity.Description;

        public int? CommentID => comment?.CommentID;
        public string CommentContent => comment?.CommentContent;
        public int? CommentScore => comment?.CommentScore;
        public bool? IsBaned => comment?.IsBaned;

        public int? MemberID => comment?.CommentMemberID;
        public string MemberEmail => comment?.Member.Email;
        public string MemberName => comment?.Member.Name;
        public string MemberIDentityNumber => comment?.Member.IDentityNumber;
        public string MemberPassport => comment?.Member.Passport;
        public string MemberNickName => comment?.Member.NickName;
        public string MemberBirthDate => comment?.Member.BirthDate?.ToString("yyyy-MM-dd");
        public string MemberPhone => comment?.Member.Phone;
        public int? MemberPoint => comment?.Member.Point;
        public string MemberAddress => comment?.Member.Address;
        public string MemberRoleName => comment?.Member.MemberRole.MemberRoleName;
        public string MemberSex => comment?.Member.Sex == null ? null : (bool)comment?.Member.Sex ? "女性" : "男性";
        public string MemberDistrict => comment?.Member.DistrictId == null ?
            null : $"{comment?.Member.Districts.DistrictName} (${comment?.Member.Districts.Cities.CityName})";

        public Seller seller { private get; set; }
        public int? SellerID => seller?.SellerID;
        public string SellerCompanyName => seller?.CompanyName;
        public string SellerTaxIDNumber => seller?.TaxIDNumber;
        public string SellerHomePage => seller?.SellerHomePage;
        public string SellerPhone => seller?.SellerPhone;
        public string SellerDiscription => seller?.SellerDeccription;
        public string fPass => seller?.fPass == null ? "尚未審核" : (bool)seller?.fPass ? "審核通過" : "審核不通過";

        public IEnumerable<BanLIst> banlist { private get; set; }
        public IEnumerable<string> Reasons => banlist?.Select(x => x.Reason);
        public IEnumerable<string> EndTimes => banlist?.Select(x => x.EndTime.ToString("yyyy-MM-dd"));

        public CommentReport report { private get; set; }
        public string ReportEmail => report?.Member.Email;
        public string ReportReason => report?.ReportReason;
    }

    public class ArticleAjax
    {
        public int Cate { get; set; }
        public int Date { get; set; }
        public int Report { get; set; }
        public int Type { get; set; }
        public int PageCurrent { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }
        public int Sort { get; set; }
    }

    public class ArticleJson
    {
        public int? MaxPage { get; set; }
        public int? ChangePage { get; set; }

        public Article article { private get; set; }
        public Reply reply { private get; set; }
        public bool IsArticle => article != null;
        public int? ARxID => article?.ArticleID ?? reply?.ReplyID;

        public string ARxPicture => article?.Picture ?? reply?.Article.Picture;
        public string ARxAuthor => article?.Member.Email ?? reply?.Member.Email;
        public string ARxArticle => article?.ArticleTitle ?? reply?.Article.ArticleTitle;
        public string ARxDate => 
            article?.Date.ToString("yyyy-MM-dd HH:mm:ss") ?? 
            reply?.ReplyDate.ToString("yyyy-MM-dd HH:mm:ss");

        public IEnumerable<Article_Report> reportA { private get; set; }
        public IEnumerable<Reply_Report> reportR { private get; set; }
        public int? ARxReportCount =>
            reportA?.Count(x => x.ArticleId == article.ArticleID) ??
            reportR?.Count(x => x.ReplyId == reply.ReplyID);
    }

    public class ArticleDetail
    {
        public Article article { private get; set; }
        public Reply reply { private get; set; }
        public int? ArticleID => article?.ArticleID;
        public string ArticleTitle => article?.ArticleTitle;
        public int? ReplyID => reply?.ReplyID;
        public int? ReplyArticleID => reply?.ArticleID;
        public string ReplyArticleTitle => reply?.Article.ArticleTitle;
        public string XContent => article?.ArticleContent ?? reply?.ReplyContent;

        public int? MemberID => article?.MemberID ?? reply?.MemberID;
        public string MemberEmail => article?.Member.Email ?? reply?.Member.Email;
        public string MemberName => article?.Member.Name ?? reply?.Member.Name;
        public string MemberIDentityNumber => article?.Member.IDentityNumber ?? reply?.Member.IDentityNumber;
        public string MemberPassport => article?.Member.Passport ?? reply?.Member.Passport;
        public string MemberNickName => article?.Member.NickName ?? reply?.Member.NickName;
        public string MemberBirthDate =>
            article?.Member.BirthDate?.ToString("yyyy-MM-dd") ??
            reply?.Member.BirthDate?.ToString("yyyy-MM-dd");
        public string MemberPhone => article?.Member.Phone ?? reply?.Member.Phone;
        public int? MemberPoint => article?.Member.Point ?? reply?.Member.Point;
        public string MemberAddress => article?.Member.Address ?? reply?.Member.Address;
        public string MemberRoleName => article?.Member.MemberRole.MemberRoleName ?? reply?.Member.MemberRole.MemberRoleName;
        public string MemberSex =>
            article?.Member.Sex != null ? ((bool)article?.Member.Sex ? "女性" : "男性") :
            reply?.Member.Sex != null ? ((bool)reply?.Member.Sex ? "女性" : "男性") : null;
        public string MemberDistrict =>
            article?.Member.DistrictId != null ?
            $"{article?.Member.Districts.DistrictName} (${article?.Member.Districts.Cities.CityName})" :
            reply?.Member.DistrictId != null ?
            $"{reply?.Member.Districts.DistrictName} (${reply?.Member.Districts.Cities.CityName})" : null;

        public Seller seller { private get; set; }
        public int? SellerID => seller?.SellerID;
        public string SellerCompanyName => seller?.CompanyName;
        public string SellerTaxIDNumber => seller?.TaxIDNumber;
        public string SellerHomePage => seller?.SellerHomePage;
        public string SellerPhone => seller?.SellerPhone;
        public string SellerDiscription => seller?.SellerDeccription;
        public string fPass => seller?.fPass == null ? "尚未審核" : (bool)seller?.fPass ? "審核通過" : "審核不通過";

        public IEnumerable<BanLIst> banlist { private get; set; }
        public IEnumerable<string> Reasons => banlist?.Select(x => x.Reason);
        public IEnumerable<string> EndTimes => banlist?.Select(x => x.EndTime.ToString("yyyy-MM-dd"));

        public Article_Report reportA { private get; set; }
        public Reply_Report reportR { private get; set; }
        public string ReportEmail => reportA?.Member.Email ?? reportR?.Member.Email;
        public string ReportReason => reportA?.Report.ReportReason ?? reportR?.Report.ReportReason;
    }

    public class MemberAjax
    {
        public int PageCurrent { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }
        public string Sort { get; set; }
        public int RoleId { get; set; }
        public bool Veri { get; set; }
    }

    public class MemberJson
    {
        public int? MaxPage { get; set; }
        public int? ChangePage { get; set; }

        public Member member { private get; set; }
        public int? MemberID => member?.MemberID;
        public string MemberEmail => member?.Email;
        public string MemberNickName => member?.NickName;
        public string MemberName => member?.Name;
        public string MemberPhone => member?.Phone;
        public int? MemberRoleId => member?.MemberRoleId;
        public string MemberRoleName => member?.MemberRole.MemberRoleName;
        
        public Seller seller { private get; set; }
        public int? SellerID => seller?.SellerID;
        public string fPass => seller?.fPass == null ? "尚未審核" : (bool)seller?.fPass ? "審核通過" : "審核不通過";
        public string fFileName => seller?.fFileName;

        public IEnumerable<BanLIst> banlist { private get; set; }
        public IEnumerable<string> Reasons => banlist?.Select(x => x.Reason);
        public IEnumerable<string> EndTimes => banlist?.Select(x => x.EndTime.ToString("yyyy-MM-dd"));
    }

    public class MemberDetail
    {
        public Member member { private get; set; }
        public int? MemberID => member?.MemberID;
        public string MemberEmail => member?.Email;
        public string MemberName => member?.Name;
        public string MemberIDentityNumber => member?.IDentityNumber;
        public string MemberPassport => member?.Passport;
        public string MemberNickName => member?.NickName;
        public string MemberBirthDate => member?.BirthDate?.ToString("yyyy-MM-dd");
        public string MemberPhone => member?.Phone;
        public int? MemberPoint => member?.Point;
        public string MemberAddress => member?.Address;
        public string MemberRoleName => member?.MemberRole.MemberRoleName;
        public string MemberSex => member?.Sex == null ? null : (bool)member?.Sex ? "女性" : "男性";
        public string MemberDistrict => member?.DistrictId == null ? null :
            $"{member?.Districts.DistrictName} ({member?.Districts.Cities.CityName})";

        public Seller seller { private get; set; }
        public int? SellerID => seller?.SellerID;
        public string SellerCompanyName => seller?.CompanyName;
        public string SellerTaxIDNumber => seller?.TaxIDNumber;
        public string SellerHomePage => seller?.SellerHomePage;
        public string SellerPhone => seller?.SellerPhone;
        public string SellerDiscription => seller?.SellerDeccription;
        public string fPass => seller?.fPass == null ? "尚未審核" : (bool)seller?.fPass ? "審核通過" : "審核不通過";
        public string fFileName => seller?.fFileName;

        public IEnumerable<BanLIst> banlist { private get; set; }
        public IEnumerable<string> Reasons => banlist?.Select(x => x.Reason);
        public IEnumerable<string> EndTimes => banlist?.Select(x => x.EndTime.ToString("yyyy-MM-dd"));
    }
}

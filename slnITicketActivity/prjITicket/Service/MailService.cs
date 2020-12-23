using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using prjITicket.Models;

namespace prjITicket.Service
{
    public class MailService
    {
        TicketSysEntities db = new TicketSysEntities();

        private string gmail_account = "iticket128";//Gmail帳號
        private string gmail_password = "!@#qweasd";//Gmail密碼
        private string gmail_mail = "iticket128@gmail.com";//Gmail信箱

        //產生驗證碼方法
        public string GetValidateCode()
        {
            //設定驗證碼字元的陣列
            string[] Code = {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","P","Q",
            "R","S","T","U","V","W","X","Y","Z","1","2","3","4","5","6","7","8","9","a","b",
            "c","d","e","f","g","h","i","j","k","l","m","n","p","q","r","s","t","u","v","w",
            "x","y","z"};
            //宣告初始為空的驗證字串    
            string ValidateCode = string.Empty;
            //宣告可產生隨機數值的物件
            Random rd = new Random();
            //使用迴圈產生出驗證碼
            for (int i = 0; i < 10; i++)
            {
                ValidateCode += Code[rd.Next(Code.Count())];
            }
            //回傳驗證碼
            return ValidateCode;
        }

        //將使用者資料填入驗證信範本中
        public string GetRegisterMailBody(string Tempstring, string UserName, string ValidateUrl)
        {
            //將使用者資料填入
            Tempstring = Tempstring.Replace("{{UserName}}", UserName);
            Tempstring = Tempstring.Replace("{{ValidateUrl}}", ValidateUrl);
            //回傳最後結果
            return Tempstring;
        }

        //寄驗證信的方法
        public void SendRegisterMail(string MailBody, string ToEmail)
        {
            string userName = "apikey";
            string password = "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";
            //建立寄信用Smtp物件，這裡使用Gmail為例
            SmtpClient SmtpServer = new SmtpClient("smtp.sendgrid.net");
            //設定使用的Port，這裡設定Gmail所使用的
            SmtpServer.Port = 587;
            //建立使用者憑據，這裡要設定Gmail帳戶
            SmtpServer.Credentials = new System.Net.NetworkCredential(userName,password);
            //開啟SSL
            SmtpServer.EnableSsl = true;
            //宣告信件內容物件
            MailMessage mail = new MailMessage();
            //設定來源信箱
            mail.From = new MailAddress(gmail_mail);
            //設定收信者信箱
            mail.To.Add(ToEmail);
            //設定信件主旨
            mail.Subject = "會員註冊確認信";
            //設定信件內容
            mail.Body = MailBody;
            //設定信件內容為HTML格式
            mail.IsBodyHtml = true;
            //送出信件
            SmtpServer.Send(mail);
        }

        //信箱驗證碼驗證方法
        public string EmailValidate(string Email, string RegisterCheckCode)
        {
            //取得傳入帳號的會員資料
            Member ValidateMember = db.Member.Where(m => m.Email == Email).FirstOrDefault();
            //取得Seller 與 member的 memberId
            Seller SellerMember = db.Seller.FirstOrDefault(s => s.MemberId == ValidateMember.MemberID);
            //宣告驗證後訊息字串
            string ValidateStr = string.Empty;
            if (ValidateMember != null)
            {
                //判斷傳入驗證碼與資料庫中是否相同
                if (ValidateMember.RegisterCheckCode == RegisterCheckCode.Split('?')[0] && SellerMember != null)
                {
                    //將資料庫中的驗證碼設為空
                    ValidateMember.RegisterCheckCode = string.Empty;
                    //將會員資料表中的會員腳色改為商家
                    ValidateMember.MemberRoleId = 3;
                    db.SaveChanges();
                    ValidateStr = "帳號信箱驗證成功，現在可以登入了";
                }
                else if(ValidateMember.RegisterCheckCode == RegisterCheckCode.Split('?')[0])
                {
                    //將資料庫中的驗證碼設為空
                    ValidateMember.RegisterCheckCode = string.Empty;
                    //將會員資料表中的會員腳色改為普通會員
                    ValidateMember.MemberRoleId = 2;
                    db.SaveChanges();
                    ValidateStr = "帳號信箱驗證成功，現在可以登入了";
                }
                else
                {
                    ValidateStr = "驗證碼錯誤，請重新確認或再註冊";
                }
            }
            else
            {
                ValidateStr = "驗證資料錯誤，請重新確認或再註冊";
            }
            //回傳驗證訊息
            return ValidateStr;

        }
    }
}
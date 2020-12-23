using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.Models.Forum
{
    public class Authority
    {
        //此類別寫入判斷瀏覽者權限的方法
        //1. 未登入
        //2. 一般會員
        //3. 商家會員
        //4. 管理者

        public List<string> Authorityjudgment(Member member)
        {
            List<string> author = new List<string>();
            TicketSysEntities db = new TicketSysEntities();
            if (member == null)
            {
                author.Add("未登入");
            }            
            else if (member.MemberRoleId==4)
            {
                author.Add("管理員");
            }
            else if (member.MemberID==3)
            {
                author.Add("商家會員");
            }
            else if (member.MemberRoleId==1)
            {
                author.Add("一般會員");
            }
            return author;

        }
    }
}
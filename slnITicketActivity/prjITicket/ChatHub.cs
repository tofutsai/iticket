using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using prjITicket.Models;

namespace prjITicket
{
    public class ChatHub : Hub
    {
        static List<User> Users = new List<User>();
        TicketSysEntities db = new TicketSysEntities();
        public void Send(string msg,int senderId,int recieverId,string senderType)
        {
            User reciever = Users.FirstOrDefault(u => u.MemberId == recieverId);
            User sender = Users.FirstOrDefault(u => u.MemberId == senderId);
            if (reciever == null || sender == null)
            {
                return;
            }
            string icon = db.Member.FirstOrDefault(m => m.MemberID == senderId).Icon ?? "default.png";
            if (senderType == "customer"&&reciever!=null)
            {
                Clients.Clients(new List<string>() { reciever.ConId }).getMsgFromCustomer(msg,sender.MemberId,sender.MemberName,icon);
            }
            else if(senderType=="seller"&&reciever!=null)
            {
                Clients.Clients(new List<string>() { reciever.ConId }).getMsgFromSeller(msg, sender.CompanyName);
            }
        }
        public void Join(int memberId,string memberName,string companyName = "非商家")
        {
            User userNow = Users.FirstOrDefault(u => u.MemberId == memberId);
            if (userNow == null)
            {
                userNow = new User()
                {
                    MemberId = memberId,
                    ConId = Context.ConnectionId,
                    MemberName = memberName,
                    CompanyName = companyName
                };
                Users.Add(userNow);
            }
            else
            {
                userNow.ConId = Context.ConnectionId;
            }
        }
    }
    public class User
    {
        public int MemberId { get; set; }
        public string ConId { get; set; }
        public string MemberName { get; set; }
        public string CompanyName { get; set; }
    }
}
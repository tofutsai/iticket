using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.Models
{
    //ActivityController中給GotoPay方法用的裝B的類
    public class CGoToPay
    {
        public string name { get; set; }
        public string email { get; set; }
        public int districtId { get; set; }
        public string address { get; set; }
        public int point { get; set; }
    }
    //套票去歐服寳付款用來接收資料的類
    public class CGoToPayTicketGroup : CGoToPay
    {
        public string ticketIds { get; set; }
        public int quantity { get; set; }
        public decimal discount { get; set; }
    }
}
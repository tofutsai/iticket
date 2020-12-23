using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.Models
{
    //套票訂單controller用來接收資料的裝B的類
    public class COrderDetailByTicketGroup
    {
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public string Tickets { get; set; }
    }
    public class TicketDesc
    {
        public int TicketCategoryId { get; set; }
        public int TicketTimeId { get; set; }
    }
}
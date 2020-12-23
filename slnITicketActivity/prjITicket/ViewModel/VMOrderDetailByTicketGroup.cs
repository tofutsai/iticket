using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using prjITicket.Models;

namespace prjITicket.ViewModel
{
    public class VMOrderDetailByTicketGroup
    {
        public List<Tickets> Tickets { get; set; }
        public int MaxPoint { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }
}
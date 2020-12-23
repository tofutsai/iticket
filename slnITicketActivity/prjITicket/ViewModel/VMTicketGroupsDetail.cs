using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class VMTicketGroupsDetail
    {
        public List<TicketGroupDetail> ticketGroupDetails { get; set; }
        public List<TicketCategory> ticketCategory { get; set; }
        public List<TicketTimes> ticketTimes { get; set; }
        public List<Tickets> tickets { get; set; }
    }
}
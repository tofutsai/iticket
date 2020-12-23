using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class VMTicketGroupsList
    {

        public List<TicketGroups> ticketGroups { get; set; }
        public int maxPriceOfAll { get; set; }
    }
}
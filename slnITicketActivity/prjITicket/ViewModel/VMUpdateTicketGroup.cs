using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using prjITicket.Models;

namespace prjITicket.ViewModel
{
    public class VMUpdateTicketGroup
    {
        public List<Activity> Activities { get; set; }
        public TicketGroups TicketGroup { get; set; }
    }
}
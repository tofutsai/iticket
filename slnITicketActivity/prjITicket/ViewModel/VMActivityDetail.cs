using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using prjITicket.Models;

namespace prjITicket.ViewModel
{
    public class VMActivityDetail
    {
        public Activity Activity { get; set; }
        public List<TicketGroups> Groups { get; set; }
        public List<Activity> OtherPeopleBuy { get; set; }
    }
}
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel.BackEnd
{
    public class CBackEndActivityTimes
    {

        public TicketTimes TicketTimes { get; set; }

        public DateTime Tickettime { get {return this.TicketTimes.TicketTime; } }
    }
}
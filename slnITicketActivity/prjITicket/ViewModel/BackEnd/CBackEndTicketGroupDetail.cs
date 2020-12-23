using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel.BackEnd
{
    public class CBackEndTicketGroupDetail
    {
        public CBackEndTicketGroupList List = new CBackEndTicketGroupList();

        public List<Activity> activity = new List<Activity>();
    }
}
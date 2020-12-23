using prjITicket.ViewModel.BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class CBackEndActivityDetailModel
    {
        public List<CBackEndActivityReason> FailedReason = new List<CBackEndActivityReason>();

        public CBackEndActivityDetail Detail = new CBackEndActivityDetail();

        public List<CBackEndActivityTimes> ActivityTimes = new List<CBackEndActivityTimes>();

        public string SubCategoryName { get; set; }
        public string CategoryName { get; set; }
    }
}
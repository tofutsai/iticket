using Newtonsoft.Json;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class CBackEndActivityReason
    {
        [JsonIgnore]
        public ActivityFailedReason failedReason { get; set; }

        public int FailedReasonID { get { return this.failedReason.ActivityFailedReasonID; } }
        public string FailedReason { get { return this.failedReason.FailedReason; } }
    }
}
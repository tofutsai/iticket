using Newtonsoft.Json;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel
{
    public class CBackEndActivityDetail
    {

        [JsonIgnore]
        public Activity ActivityEntity { get; set; }

        [JsonIgnore]
        public Seller Seller { get; set; }

        [JsonIgnore]
        public ActivityStatus ActivityStatus { set; get; }

        [JsonIgnore]
        public TicketTimes TicketTimes { get; set; }

        public int ActivityID { get { return this.ActivityEntity.ActivityID; } }
        public string ActivityName { get { return this.ActivityEntity.ActivityName; } }
        public string ActivityDescription { get { return this.ActivityEntity.Description; } }
        public string ActivityInfo { get { return this.ActivityEntity.Information; } }
        public string ActivityHost { get { return this.ActivityEntity.Hostwords; } }
        public string ActivityAddress { get { return this.ActivityEntity.Address; } }
        public string ActivityPicture { get { return this.ActivityEntity.Picture; } }
        public string ActivityMap { get { return this.ActivityEntity.Map; } }
        public string CompanyName { get { return this.Seller.CompanyName; } }
        public string StatusName { get { return this.ActivityStatus.ActivityStatusName; } }
        public int StatusID { get { return this.ActivityStatus.ActivityStatusID; } }
        public int SellerID { get { return this.ActivityEntity.SellerID; } }

        public DateTime TicketTime { get { return this.TicketTimes.TicketTime; } }

        
        
    }
}